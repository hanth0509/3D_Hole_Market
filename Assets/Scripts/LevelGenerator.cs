// using UnityEngine;
// using System.Collections.Generic;
// using System.IO;

// [System.Serializable]
// public class FormationGroup
// {
//     public string groupName;
//     public List<Vector3> positions = new List<Vector3>();
// }

// [System.Serializable]
// public class LevelData
// {
//     public int levelIndex;
//     public List<FormationGroup> groups = new List<FormationGroup>();
// }

// public class LevelGenerator : MonoBehaviour
// {
//     [Header("Level Settings")]
//     public int numberOfGroups = 3;
//     public Vector2Int groupSizeRange = new Vector2Int(3, 6);
//     public float spacing = 1.5f;

//     [Header("Save Settings")]
//     public string saveFolderName = "Levels";

//     [ContextMenu("Generate & Save Level")]
//     public void GenerateAndSaveLevel()
//     {
//         // 1️⃣ Chuẩn bị đường dẫn thư mục lưu
//         string folderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
//         if (!Directory.Exists(folderPath))
//             Directory.CreateDirectory(folderPath);

//         // 2️⃣ Tìm số thứ tự tiếp theo
//         int nextIndex = GetNextLevelIndex(folderPath);

//         // 3️⃣ Tạo dữ liệu level
//         LevelData level = GenerateLevelData(nextIndex);

//         // 4️⃣ Lưu file JSON bằng Easy Save
//         string fileName = $"level_{nextIndex}.json";
//         string fullPath = Path.Combine(folderPath, fileName);

//         ES3.Save("LevelData", level, fullPath);

//         Debug.Log($"✅ Level {nextIndex} generated & saved to:\n{fullPath}");
//     }

//     private LevelData GenerateLevelData(int levelIndex)
//     {
//         LevelData level = new LevelData();
//         level.levelIndex = levelIndex;

//         for (int g = 0; g < numberOfGroups; g++)
//         {
//             FormationGroup group = new FormationGroup();
//             group.groupName = "Group_" + g;

//             int rows = Random.Range(groupSizeRange.x, groupSizeRange.y + 1);
//             int cols = Random.Range(groupSizeRange.x, groupSizeRange.y + 1);

//             Vector3 groupOffset = new Vector3(g * 10f, 0, 0);

//             for (int r = 0; r < rows; r++)
//             {
//                 for (int c = 0; c < cols; c++)
//                 {
//                     Vector3 pos = groupOffset + new Vector3(c * spacing, 0, r * spacing);
//                     group.positions.Add(pos);
//                 }
//             }

//             level.groups.Add(group);
//         }

//         return level;
//     }

//     private int GetNextLevelIndex(string folderPath)
//     {
//         // Lấy tất cả file trong thư mục
//         string[] files = Directory.GetFiles(folderPath, "level_*.json");
//         int maxIndex = 0;

//         foreach (string file in files)
//         {
//             string name = Path.GetFileNameWithoutExtension(file);
//             // Cắt ra phần số
//             string[] parts = name.Split('_');
//             if (parts.Length == 2 && int.TryParse(parts[1], out int index))
//                 maxIndex = Mathf.Max(maxIndex, index);
//         }

//         return maxIndex + 1;
//     }

//     [ContextMenu("Load & Print Last Level")]
//     public void LoadAndPrintLastLevel()
//     {
//         string folderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
//         int lastIndex = GetNextLevelIndex(folderPath) - 1;
//         if (lastIndex <= 0)
//         {
//             Debug.LogWarning("⚠️ No level files found to load.");
//             return;
//         }

//         string fullPath = Path.Combine(folderPath, $"level_{lastIndex}.json");

//         if (File.Exists(fullPath))
//         {
//             LevelData loaded = ES3.Load<LevelData>("LevelData", fullPath);
//             Debug.Log($"📖 Loaded Level {loaded.levelIndex} with {loaded.groups.Count} groups");
//         }
//         else
//         {
//             Debug.LogWarning("⚠️ Last level file not found.");
//         }
//     }
// }





using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

//lưu tên nhóm và vị trí spawn các object (Vector3)
[System.Serializable]
public class FormationGroup
{
    public string groupName;
    public List<Vector3> positions = new List<Vector3>();
}
//lưu số level và danh sách các group
[System.Serializable]
public class LevelData
{
    public int levelIndex;
    public List<FormationGroup> groups = new List<FormationGroup>();
}

public class LevelGenerator : MonoBehaviour
{
    // SINGLETON PATTERN
    public static LevelGenerator Instance { get; private set; }

    [Header("Level Settings")]
    public int numberOfGroups = 3;
    public Vector2Int groupSizeRange = new Vector2Int(3, 6);
    public float spacing = 1.5f;

    [Header("Save Settings")]
    public string saveFolderName = "Levels";

    [Header("Prefab Settings")]
    public GameObject objectPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ LevelGenerator Singleton created");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // CHỈ SPAWN KHI VÀO SCENE GAMEPLAY
        if (scene.name == "GamePlay")
        {
            // Debug.Log("Phát hiện scene Gameplay, bắt đầu spawn level...");
            StartCoroutine(DelayedSpawn());
        }
        else
        {
            // Debug.Log("Không phải scene Gameplay, không spawn level.");
        }
    }

    System.Collections.IEnumerator DelayedSpawn()
    {
        // ĐỢI 2 FRAME 
        yield return null;
        yield return null;

        SpawnCurrentLevel();
    }

    public void SpawnCurrentLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Kiểm tra spawn điều kiện: Scene={currentScene}");

        if (currentScene != "GamePlay")
        {
            // Debug.LogError($"KHÔNG được spawn ở scene {currentScene}, chỉ spawn trong GamePlay!");
            return;
        }

        if (MasterData.Instance == null)
        {
            Debug.LogError("MasterData.Instance là null!");
            return;
        }

        if (MasterData.Instance.CurrentLevelData == null)
        {
            Debug.LogError("CurrentLevelData là null!");
            return;
        }

        Debug.Log("Đang clear level objects cũ...");
        ClearLevelObjects();

        LevelData levelData = MasterData.Instance.CurrentLevelData;
        Debug.Log($"Đang spawn Level {levelData.levelIndex}...");

        int total = 0;

        foreach (var group in levelData.groups)
        {
            Debug.Log($"Spawning {group.groupName} - {group.positions.Count} object");

            foreach (var position in group.positions)
            {
                SpawnCubeAtPosition(position, group.groupName);
                total++;
            }
        }

        Debug.Log($"✅ ĐÃ SPAWN XONG {total} TRONG SCENE {currentScene}!");
    }

    void SpawnCubeAtPosition(Vector3 position, string groupName)
    {
        float prefabHeight = GetPrefabHeight();
        Debug.Log($"Prefab height: {prefabHeight}");
        Vector3 adjustedPosition = new Vector3(
            position.x,
            position.y + prefabHeight / 2f, 
            position.z
        );
        if (objectPrefab != null)
        {
            GameObject spawnedObject = Instantiate(objectPrefab, adjustedPosition, Quaternion.identity);
            spawnedObject.name = $"{groupName}_{objectPrefab.name}";

            Debug.Log($"Đã tạo {objectPrefab.name}: {groupName} tại {position}");
        }
        else
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.name = $"{groupName}_Cube";
            // THÊM OFFSET NHỎ ĐỂ TRÁNH CHỒNG LẤN
            Vector3 offset = new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f)
            );

            cube.transform.position = position + offset;
            cube.name = $"{groupName}_Cube";
            cube.transform.localScale = Vector3.one * 0.3f;
            if (GetComponent<Renderer>() != null)
            {
                if (groupName.Contains("0")) GetComponent<Renderer>().material.color = Color.red;
                else if (groupName.Contains("1")) GetComponent<Renderer>().material.color = Color.green;
                else if (groupName.Contains("2")) GetComponent<Renderer>().material.color = Color.blue;
                else GetComponent<Renderer>().material.color = Color.yellow;
            }

            Debug.Log($" Đã tạo : {groupName} tại {position}");
        }

    }
    private float GetPrefabHeight()
    {
        if (objectPrefab == null) return 1f; // Default height

        // tính chiều cao
        Renderer renderer = objectPrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }

        // dùng Collider nếu có
        Collider collider = objectPrefab.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.size.y;
        }

        // dùng scale y
        return objectPrefab.transform.localScale.y;
    }
    [ContextMenu("Clear Level Objects")]
    public void ClearLevelObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int destroyedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.name.Contains("Group_") && obj.name.Contains("Cube"))
            {
                DestroyImmediate(obj);
                destroyedCount++;
            }
        }

        Debug.Log($"Đã xóa {destroyedCount} ");
    }

    // THÊM METHOD ĐẢM BẢO SPAWN TRONG GamePlay
    [ContextMenu("FORCE Spawn in GamePlay")]
    public void ForceSpawnInGamePlay()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"FORCE SPAWN in {currentScene}");

        if (currentScene == "GamePlay")
        {
            SpawnCurrentLevel();
        }
        else
        {
            Debug.LogError($"Phải ở trong scene GamePlay! Hiện đang ở {currentScene}");
        }
    }



    // DEBUG METHOD - CHỈ SPAWN KHI NHẤN NÚT
    [ContextMenu("Debug Manual Spawn")]
    public void DebugManualSpawn()
    {
        Debug.Log("=== DEBUG MANUAL SPAWN ===");

        if (MasterData.Instance == null)
        {
            Debug.LogError("MasterData NULL");
            return;
        }

        if (MasterData.Instance.CurrentLevelData == null)
        {
            Debug.LogError("CurrentLevelData NULL");
            return;
        }

        SpawnCurrentLevel();
    }


    [ContextMenu("Generate & Save Level")]
    public void GenerateAndSaveLevel()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        int nextIndex = GetNextLevelIndex(folderPath);
        LevelData level = GenerateLevelData(nextIndex);

        string fileName = $"level_{nextIndex}.json";
        string fullPath = Path.Combine(folderPath, fileName);

        ES3.Save("LevelData", level, fullPath);
        Debug.Log($"Level {nextIndex} generated & saved to:\n{fullPath}");
    }

    private LevelData GenerateLevelData(int levelIndex)
    {
        LevelData level = new LevelData();
        level.levelIndex = levelIndex;

        for (int g = 0; g < numberOfGroups; g++)
        {
            FormationGroup group = new FormationGroup();
            group.groupName = "Group_" + g;

            int rows = Random.Range(groupSizeRange.x, groupSizeRange.y + 1);
            int cols = Random.Range(groupSizeRange.x, groupSizeRange.y + 1);

            Vector3 groupOffset = new Vector3(g * 10f, 0, 0);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector3 pos = groupOffset + new Vector3(c * spacing, 0, r * spacing);
                    group.positions.Add(pos);
                }
            }

            level.groups.Add(group);
        }

        return level;
    }

    private int GetNextLevelIndex(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "level_*.json");
        int maxIndex = 0;

        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string[] parts = name.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out int index))
                maxIndex = Mathf.Max(maxIndex, index);
        }

        return maxIndex + 1;
    }

    [ContextMenu("Load & Print Last Level")]
    public void LoadAndPrintLastLevel()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
        int lastIndex = GetNextLevelIndex(folderPath) - 1;

        if (lastIndex <= 0)
        {
            Debug.LogWarning("No level files found to load.");
            return;
        }

        string fullPath = Path.Combine(folderPath, $"level_{lastIndex}.json");

        if (File.Exists(fullPath))
        {
            LevelData loaded = ES3.Load<LevelData>("LevelData", fullPath);
            Debug.Log($"Loaded Level {loaded.levelIndex} with {loaded.groups.Count} groups");
        }
        else
        {
            Debug.LogWarning("Last level file not found.");
        }
    }
}
