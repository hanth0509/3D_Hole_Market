using UnityEngine;
using UnityCommunity.UnitySingleton;
using System.Collections.Generic;

public class MasterData : MonoSingleton<MasterData>
{
    public TextAsset currentLevelFile;
    public LevelData CurrentLevelData { get; private set; }

    [SerializeField]
    public SerializableLevelData currentLevelDataInspector;

    [System.Serializable]
    public class SerializableLevelData
    {
        public int levelIndex;
        public List<SerializableFormationGroup> groups = new List<SerializableFormationGroup>();
    }

    [System.Serializable]
    public class SerializableFormationGroup
    {
        public string groupName;
        public List<SerializableVector3> positions = new List<SerializableVector3>();
    }

    [System.Serializable]
    public class SerializableVector3
    {
        public float x, y, z;
        public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 ToVector3() { return new Vector3(x, y, z); }
    }

    // Them moi truong luu tru tien trinh level
    [System.Serializable]
    public class LevelProgress
    {
        public int levelNumber;
        public bool isUnlocked;
        public bool isCompleted;
        public int highScore;
        public int stars;
    }
    [SerializeField] public List<LevelProgress> levelProgressList = new List<LevelProgress>();

    //Đảm bảo tồn tại xuyên scene
    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("MasterData - DontDestroyOnLoad enabled");
        }
    }

    void Start()
    {
        Debug.Log("🔄 MasterData Start - Kiểm tra trạng thái:");
        Debug.Log($"   Instance: {Instance != null}");
        Debug.Log($"   CurrentLevelData: {CurrentLevelData != null}");
        Debug.Log($"   CurrentLevelFile: {currentLevelFile != null}");

        InitializeLevelProgress();
    }

    public void InitializeLevelProgress()
    {
        if (levelProgressList.Count == 0)
        {
            for (int i = 1; i <= 48; i++)
            {
                levelProgressList.Add(new LevelProgress
                {
                    levelNumber = i,
                    isUnlocked = (i == 1),
                    isCompleted = false,
                    highScore = 0,
                    stars = 0
                });
            }
            SaveGameProgress();
        }
    }
    public void UpdateLevelProgress(int levelNumber, bool completed, int score, int stars)
    {
        LevelProgress progress = levelProgressList.Find(p => p.levelNumber == levelNumber);
        
        if (progress != null)
        {
            progress.isCompleted = completed;
            
            if (score > progress.highScore)
                progress.highScore = score;
                
            if (stars > progress.stars)
                progress.stars = stars;
            
            // MỞ KHÓA LEVEL TIẾP THEO
            if (completed && levelNumber < 48)
            {
                LevelProgress nextLevel = levelProgressList.Find(p => p.levelNumber == levelNumber + 1);
                if (nextLevel != null) 
                    nextLevel.isUnlocked = true;
            }
            
            SaveGameProgress();
            Debug.Log($"💾 Đã lưu progress Level {levelNumber}");
        }
    }

    void SaveGameProgress()
    {
        ES3.Save("levelProgress", levelProgressList);
        Debug.Log("✅ Đã lưu game progress");
    }

    void LoadGameProgress()
    {
        if (ES3.KeyExists("levelProgress"))
        {
            levelProgressList = ES3.Load<List<LevelProgress>>("levelProgress");
            Debug.Log("✅ Đã load game progress");
        }
    }
    public bool LoadLevelData(int levelIndex)
    {
        Debug.Log($"=== LOAD LEVEL {levelIndex} ===");

        // KIỂM TRA INSTANCE TRƯỚC
        if (Instance == null)
        {
            Debug.LogError("MasterData Instance là null!");
            return false;
        }
        //tạo đường dẫn tới file JSON level
        string resourcePath = $"Levels/level_{levelIndex}";
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

        if (jsonAsset == null)
        {
            Debug.LogError($"Không tìm thấy file: {resourcePath}");
            return false;
        }

        Debug.Log($"Tìm thấy file: {jsonAsset.name}");

        // PARSE DATA
        //chuyển chuỗi JSON thành dữ liệu level
        bool success = ParseLevelData(jsonAsset.text, levelIndex);

        if (success)
        {
            currentLevelFile = jsonAsset;
            Debug.Log($"LOAD THÀNH CÔNG Level {CurrentLevelData.levelIndex}");

            // CẬP NHẬT INSPECTOR
            UpdateInspectorData();

            return true;
        }
        else
        {
            Debug.LogError($"LOAD THẤT BẠI Level {levelIndex}");
            return false;
        }
    }

    private bool ParseLevelData(string jsonText, int expectedLevel)
    {
        Debug.Log("Bắt đầu parse level data...");

        try
        {
            // TÌM PHẦN "value" TRONG JSON
            int valueIndex = jsonText.IndexOf("\"value\"");
            if (valueIndex < 0)
            {
                Debug.LogError("❌ Không tìm thấy key 'value' trong JSON");
                return false;
            }

            // TÌM DẤU { SAU "value"
            int startBrace = jsonText.IndexOf('{', valueIndex);
            if (startBrace < 0)
            {
                Debug.LogError("❌ Không tìm thấy { sau 'value'");
                return false;
            }

            // TÌM DẤU } TƯƠNG ỨNG
            int endBrace = FindMatchingBrace(jsonText, startBrace);
            if (endBrace <= startBrace)
            {
                Debug.LogError("❌ Không tìm thấy } tương ứng");
                return false;
            }

            // TRÍCH XUẤT JSON
            string valueJson = jsonText.Substring(startBrace, endBrace - startBrace + 1);
            Debug.Log("Đã trích xuất JSON");

            // PARSE JSON (chuyển kiểu json thành object)
            CurrentLevelData = JsonUtility.FromJson<LevelData>(valueJson);

            if (CurrentLevelData == null)
            {
                Debug.LogError(" JsonUtility.FromJson trả về null");
                return false;
            }

            // KIỂM TRA DATA
            Debug.Log($"Parse result - LevelIndex: {CurrentLevelData.levelIndex}, Groups: {CurrentLevelData.groups?.Count ?? 0}");

            if (CurrentLevelData.groups != null)
            {
                foreach (var group in CurrentLevelData.groups)
                {
                    Debug.Log($"   Group: {group.groupName} - {group.positions?.Count ?? 0} positions");
                }
            }

            Debug.Log("PARSE THÀNH CÔNG!");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi parse: {ex.Message}");
            return false;
        }
    }

    private int FindMatchingBrace(string text, int startIndex)
    {
        int count = 1;
        for (int i = startIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '{') count++;
            else if (text[i] == '}') count--;

            if (count == 0) return i;
        }
        return -1;
    }

    private void UpdateInspectorData()
    {
        if (CurrentLevelData == null)
        {
            currentLevelDataInspector = null;
            return;
        }

        currentLevelDataInspector = new SerializableLevelData();
        currentLevelDataInspector.levelIndex = CurrentLevelData.levelIndex;

        if (CurrentLevelData.groups != null)
        {
            foreach (var group in CurrentLevelData.groups)
            {
                var serializableGroup = new SerializableFormationGroup();
                serializableGroup.groupName = group.groupName;

                if (group.positions != null)
                {
                    foreach (var pos in group.positions)
                    {
                        serializableGroup.positions.Add(new SerializableVector3(pos));
                    }
                }

                currentLevelDataInspector.groups.Add(serializableGroup);
            }
        }

        Debug.Log($"Updated Inspector: Level {currentLevelDataInspector.levelIndex}, {currentLevelDataInspector.groups.Count} groups");
    }

    // METHOD KIỂM TRA TRẠNG THÁI
    [ContextMenu("Check State")]
    public void CheckState()
    {
        Debug.Log($"=== MASTER DATA STATE ===");
        Debug.Log($"Instance: {Instance != null}");
        Debug.Log($"This: {this != null}");
        Debug.Log($"CurrentLevelData: {CurrentLevelData != null}");

        if (CurrentLevelData != null)
        {
            Debug.Log($"Level Index: {CurrentLevelData.levelIndex}");
            Debug.Log($"Groups Count: {CurrentLevelData.groups?.Count ?? 0}");
        }

        Debug.Log($"CurrentLevelFile: {currentLevelFile != null}");
        Debug.Log($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log("=== STATE END ===");
    }

    // METHOD LẤY LEVEL DATA AN TOÀN
    public LevelData GetLevelDataSafe()
    {
        if (CurrentLevelData != null)
        {
            return CurrentLevelData;
        }

        Debug.LogWarning("CurrentLevelData là null, trả về null");
        return null;
    }

    // METHOD KIỂM TRA DATA READY
    public bool IsLevelDataReady()
    {
        bool ready = (CurrentLevelData != null);
        Debug.Log($"LevelData Ready: {ready}");
        return ready;
    }

#if UNITY_EDITOR
    private void ForceInspectorUpdate()
    {
        if (!Application.isPlaying) return;
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
#endif
}