using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using Random = UnityEngine.Random;



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

    // THÊM ENUM CHO HÌNH DẠNG
    public enum shapeType
    {
        SQUARE,
        RECTANGLE,
        TRIANGLE,
        CIRCLE,
        DIAMOND
    }

    [Header("Prefab Settings")]
    public GameObject objectPrefab;
    [Header("Shape Settings")]
    public shapeType defaultShape = shapeType.SQUARE;

    //  PHƯƠNG THỨC TẠO LEVEL VỚI HÌNH DẠNG ĐA DẠNG
    [ContextMenu(" Generate Level với Đa Dạng Hình Dạng")]
    public void GenerateLevelWithShapes()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        int nextIndex = GetNextLevelIndex(folderPath);
        LevelData level = GenerateLevelDataWithShapes(nextIndex);

        string fileName = $"level_{nextIndex}.json";
        string fullPath = Path.Combine(folderPath, fileName);

        ES3.Save("LevelData", level, fullPath);
        Debug.Log($"✅ Level {nextIndex} với đa dạng hình dạng đã được tạo!");

        // COPY VÀO RESOURCES
        CopyToResourcesFolder(fullPath, nextIndex);
    }

    private LevelData GenerateLevelDataWithShapes(int levelIndex)
    {
        LevelData level = new LevelData();
        level.levelIndex = levelIndex;

        //  TĂNG ĐỘ KHÓ THEO LEVEL
        // level.targetScore = 800 + (levelIndex * 120);
        // level.moveLimit = Mathf.Max(15, 25 - (levelIndex / 4));
        //  HỆ THỐNG TARGET MỚI - ĐỘ KHÓ TĂNG DẦN
        level.target = CalculateLevelTarget(levelIndex);
        level.timeLimit = CalculateTimeLimit(levelIndex);
        level.moveLimit = CalculateMoveLimit(levelIndex);

        //  RANDOM SHAPE TYPE CHO LEVEL
        Array shapeTypes = Enum.GetValues(typeof(shapeType));
        level.levelType = ((shapeType)shapeTypes.GetValue(UnityEngine.Random.Range(0, shapeTypes.Length))).ToString();

        for (int g = 0; g < numberOfGroups; g++)
        {
            FormationGroup group = new FormationGroup();
            group.groupName = "Group_" + g;

            //  MỖI GROUP CÓ SHAPE KHÁC NHAU
            shapeType groupShape = (shapeType)shapeTypes.GetValue(UnityEngine.Random.Range(0, shapeTypes.Length));
            group.shapeType = groupShape.ToString();

            // 🔥 THÊM KÍCH THƯỚC OBJECT & ĐIỂM
            group.objectSize = GetRandomSizeForLevel(levelIndex);
            group.points = GetPointsForSize(group.objectSize);
            //

            // TẠO POSITIONS THEO SHAPE
            Vector3 groupOffset = new Vector3(g * 12f, 0, 0);

            switch (groupShape)
            {
                case shapeType.SQUARE:
                    // CreateSquareFormation(group, groupOffset, 4, 4);
                    CreateSmartSquareFormation(group, groupOffset, 4, 4);
                    break;
                case shapeType.RECTANGLE:
                    // CreateRectangleFormation(group, groupOffset, 3, 6);
                    CreateSmartRectangleFormation(group, groupOffset, 3, 6);
                    break;
                case shapeType.TRIANGLE:
                    // CreateTriangleFormation(group, groupOffset, 5);
                    CreateSmartTriangleFormation(group, groupOffset, 5);
                    break;
                case shapeType.CIRCLE:
                    // CreateCircleFormation(group, groupOffset, 12, 2f);
                    CreateSmartCircleFormation(group, groupOffset, 12, 2f);
                    break;
                case shapeType.DIAMOND:
                    // CreateDiamondFormation(group, groupOffset, 4);
                    CreateSmartDiamondFormation(group, groupOffset, 4);
                    break;
            }

            level.groups.Add(group);
        }
        Debug.Log($"🎯 Level {levelIndex}: {level.target.smallObjects}S {level.target.mediumObjects}M {level.target.largeObjects}L - Time: {level.timeLimit}s");
        return level;
    }
    // 🔥 THÊM CÁC HÀM HỖ TRỢ MỚI
    private LevelTarget CalculateLevelTarget(int levelIndex)
    {
        LevelTarget target = new LevelTarget();

        // 🔥 CÔNG THỨC ĐỘ KHÓ TĂNG DẦN:
        // - Vật nhỏ: tăng đều theo level
        // - Vật trung: bắt đầu từ level 3  
        // - Vật lớn: bắt đầu từ level 5
        // - Vật đặc biệt: mỗi 5 level
        target.smallObjects = 5 + (levelIndex * 2);
        target.mediumObjects = Mathf.Max(0, (levelIndex - 2) * 2); // Bắt đầu từ level 3
        target.largeObjects = Mathf.Max(0, levelIndex - 4);        // Bắt đầu từ level 5
        target.specialObjects = (levelIndex % 5 == 0) ? 1 : 0;     // Mỗi 5 level

        // 🔥 ĐẢM BẢO LEVEL 1-2 CHỈ CÓ VẬT NHỎ
        if (levelIndex <= 2)
        {
            target.mediumObjects = 0;
            target.largeObjects = 0;
            target.specialObjects = 0;
        }

        return target;
    }

    private int CalculateTimeLimit(int levelIndex)
    {
        // Level càng cao, thời gian càng ít
        // Level 1: 90s, Level 10: 70s, Level 20: 50s, ...
        return Mathf.Max(30, 90 - (levelIndex * 2));
    }

    private int CalculateMoveLimit(int levelIndex)
    {
        // Level càng cao, số move càng ít  
        // Level 1: 25 moves, Level 10: 20 moves, Level 20: 15 moves, ...
        return Mathf.Max(10, 25 - (levelIndex / 2));
    }

    private string GetRandomSizeForLevel(int levelIndex)
    {
        string[] sizes = { "SMALL", "MEDIUM", "LARGE", "SPECIAL" };

        // 🔥 TỈ LỆ XUẤT HIỆN THAY ĐỔI THEO LEVEL
        int[] weights;

        if (levelIndex <= 2)
        {
            weights = new int[] { 100, 0, 0, 0 }; // Chỉ vật nhỏ
        }
        else if (levelIndex <= 4)
        {
            weights = new int[] { 70, 30, 0, 0 }; // Vật nhỏ + trung
        }
        else if (levelIndex <= 8)
        {
            weights = new int[] { 60, 25, 15, 0 }; // + Vật lớn
        }
        else
        {
            weights = new int[] { 50, 20, 25, 5 }; // Tất cả + đặc biệt
        }

        int totalWeight = 0;
        foreach (int weight in weights) totalWeight += weight;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            currentWeight += weights[i];
            if (randomValue < currentWeight)
                return sizes[i];
        }

        return "SMALL";
    }

    private int GetPointsForSize(string size)
    {
        switch (size)
        {
            case "SMALL": return 10;
            case "MEDIUM": return 30;
            case "LARGE": return 100;
            case "SPECIAL": return 200;
            default: return 10;
        }
    }
    //  CÁC PHƯƠNG THỨC TẠO HÌNH DẠNG MỚI
    private void CreateSquareFormation(FormationGroup group, Vector3 offset, int sizeX, int sizeZ)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                Vector3 pos = offset + new Vector3(x * spacing, 0, z * spacing);
                group.positions.Add(pos);
            }
        }
    }

    private void CreateRectangleFormation(FormationGroup group, Vector3 offset, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = offset + new Vector3(x * spacing * 1.2f, 0, z * spacing);
                group.positions.Add(pos);
            }
        }
    }

    private void CreateTriangleFormation(FormationGroup group, Vector3 offset, int layers)
    {
        for (int layer = 0; layer < layers; layer++)
        {
            for (int i = 0; i <= layer; i++)
            {
                float x = i * spacing - (layer * spacing * 0.5f);
                float z = layer * spacing;
                Vector3 pos = offset + new Vector3(x, 0, z);
                group.positions.Add(pos);
            }
        }
    }

    private void CreateCircleFormation(FormationGroup group, Vector3 offset, int points, float radius)
    {
        for (int i = 0; i < points; i++)
        {
            float angle = i * Mathf.PI * 2 / points;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 pos = offset + new Vector3(x, 0, z);
            group.positions.Add(pos);
        }
    }

    private void CreateDiamondFormation(FormationGroup group, Vector3 offset, int size)
    {
        for (int layer = 0; layer < size * 2 - 1; layer++)
        {
            int objectsInLayer = (layer < size) ? layer + 1 : size * 2 - 1 - layer;

            for (int i = 0; i < objectsInLayer; i++)
            {
                float x = (i - (objectsInLayer - 1) * 0.5f) * spacing;
                float z = (layer - (size - 1)) * spacing;
                Vector3 pos = offset + new Vector3(x, 0, z);
                group.positions.Add(pos);
            }
        }
    }

    // 🔥 SMART FORMATIONS - PHÂN BỐ SIZE THEO VỊ TRÍ
    private void CreateSmartSquareFormation(FormationGroup group, Vector3 offset, int sizeX, int sizeZ)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                Vector3 pos = offset + new Vector3(x * spacing, 0, z * spacing);
                group.positions.Add(pos);

                // 🔥 PHÂN BỐ SIZE: TRUNG TÂM LỚN, NGOÀI NHỎ
                float distanceFromCenter = GetDistanceFromCenter(x, z, sizeX, sizeZ);
                string sizeType = GetSizeBasedOnDistance(distanceFromCenter);
                group.positionSizes.Add(sizeType); // Cần thêm field mới
            }
        }
    }

    private void CreateSmartRectangleFormation(FormationGroup group, Vector3 offset, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = offset + new Vector3(x * spacing * 1.2f, 0, z * spacing);
                group.positions.Add(pos);

                float distanceFromCenter = GetDistanceFromCenter(x, z, width, height);
                string sizeType = GetSizeBasedOnDistance(distanceFromCenter);
                group.positionSizes.Add(sizeType);
            }
        }
    }

    private void CreateSmartTriangleFormation(FormationGroup group, Vector3 offset, int layers)
    {
        for (int layer = 0; layer < layers; layer++)
        {
            for (int i = 0; i <= layer; i++)
            {
                float x = i * spacing - (layer * spacing * 0.5f);
                float z = layer * spacing;
                Vector3 pos = offset + new Vector3(x, 0, z);
                group.positions.Add(pos);

                // 🔥 ĐỈNH TAM GIÁC NHỎ, ĐÁY LỚN
                string sizeType = (layer == 0) ? "SMALL" : (layer < layers / 2) ? "MEDIUM" : "LARGE";
                group.positionSizes.Add(sizeType);
            }
        }
    }

    private void CreateSmartCircleFormation(FormationGroup group, Vector3 offset, int points, float radius)
    {
        for (int i = 0; i < points; i++)
        {
            float angle = i * Mathf.PI * 2 / points;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 pos = offset + new Vector3(x, 0, z);
            group.positions.Add(pos);

            // 🔥 VÒNG TRÒN: TÂM LỚN, NGOÀI NHỎ
            string sizeType = (radius < 1.5f) ? "LARGE" : (radius < 2.5f) ? "MEDIUM" : "SMALL";
            group.positionSizes.Add(sizeType);
        }
    }

    private void CreateSmartDiamondFormation(FormationGroup group, Vector3 offset, int size)
    {
        for (int layer = 0; layer < size * 2 - 1; layer++)
        {
            int objectsInLayer = (layer < size) ? layer + 1 : size * 2 - 1 - layer;

            for (int i = 0; i < objectsInLayer; i++)
            {
                float x = (i - (objectsInLayer - 1) * 0.5f) * spacing;
                float z = (layer - (size - 1)) * spacing;
                Vector3 pos = offset + new Vector3(x, 0, z);
                group.positions.Add(pos);

                // 🔥 KIM CƯƠNG: TRUNG TÂM LỚN, NGOÀI NHỎ
                string sizeType = (layer == size - 1 && i == objectsInLayer / 2) ? "LARGE" :
                                (Mathf.Abs(layer - (size - 1)) <= 1) ? "MEDIUM" : "SMALL";
                group.positionSizes.Add(sizeType);
            }
        }
    }

    // 🔥 HELPER METHODS
    private float GetDistanceFromCenter(int x, int y, int maxX, int maxY)
    {
        float centerX = (maxX - 1) / 2f;
        float centerY = (maxY - 1) / 2f;
        return Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
    }

    private string GetSizeBasedOnDistance(float distance)
    {
        if (distance <= 1f) return "LARGE";
        else if (distance <= 2f) return "MEDIUM";
        else return "SMALL";
    }

    // 🔥 COPY FILE VÀO RESOURCES
    private void CopyToResourcesFolder(string sourcePath, int levelIndex)
    {
        try
        {
            string resourcesPath = Path.Combine(Application.dataPath, "Resources", "Levels");
            if (!Directory.Exists(resourcesPath))
                Directory.CreateDirectory(resourcesPath);

            string destPath = Path.Combine(resourcesPath, $"level_{levelIndex}.json");
            File.Copy(sourcePath, destPath, true);

            Debug.Log($"📦 Đã copy vào Resources: {destPath}");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Lỗi khi copy file: {ex.Message}");
        }
    }



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

        // foreach (var group in levelData.groups)
        // {
        //     Debug.Log($"Spawning {group.groupName} - {group.positions.Count} object");

        //     foreach (var position in group.positions)
        //     {
        //         SpawnCubeAtPosition(position, group.groupName);
        //         total++;
        //     }
        // }
        foreach (var group in levelData.groups)
        {
            string shapeInfo = string.IsNullOrEmpty(group.shapeType) ? "DEFAULT" : group.shapeType;
            Debug.Log($"   👥 {group.groupName} - {group.positions.Count} objects - Shape: {shapeInfo}");

            // foreach (var position in group.positions)
            // {
            //     SpawnObjectAtPosition(position, group.groupName);
            // }
            for (int i = 0; i < group.positions.Count; i++)
            {
                string sizeType = (group.positionSizes != null && i < group.positionSizes.Count)
                    ? group.positionSizes[i]
                    : group.objectSize; // Fallback to group size

                SpawnObjectAtPosition(group.positions[i], group.groupName, sizeType);
            }
        }

        Debug.Log($"✅ ĐÃ SPAWN XONG {total} TRONG SCENE {currentScene}!");
    }


    // void  SpawnObjectAtPosition(Vector3 position, string groupName)
    // // void SpawnCubeAtPosition(Vector3 position, string groupName)
    // {
    //     float prefabHeight = GetPrefabHeight();
    //     Debug.Log($"Prefab height: {prefabHeight}");
    //     Vector3 adjustedPosition = new Vector3(
    //         position.x,
    //         position.y + prefabHeight / 2f, 
    //         position.z
    //     );
    //     if (objectPrefab != null)
    //     {
    //         GameObject spawnedObject = Instantiate(objectPrefab, adjustedPosition, Quaternion.identity);
    //         spawnedObject.name = $"{groupName}_{objectPrefab.name}";

    //         Debug.Log($"Đã tạo {objectPrefab.name}: {groupName} tại {position}");
    //     }
    //     else
    //     {
    //         GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //         cube.transform.position = position;
    //         cube.name = $"{groupName}_Cube";
    //         // THÊM OFFSET NHỎ ĐỂ TRÁNH CHỒNG LẤN
    //         Vector3 offset = new Vector3(
    //             Random.Range(-0.1f, 0.1f),
    //             Random.Range(-0.1f, 0.1f),
    //             Random.Range(-0.1f, 0.1f)
    //         );

    //         cube.transform.position = position + offset;
    //         cube.name = $"{groupName}_Cube";
    //         cube.transform.localScale = Vector3.one * 0.3f;
    //         if (GetComponent<Renderer>() != null)
    //         {
    //             if (groupName.Contains("0")) GetComponent<Renderer>().material.color = Color.red;
    //             else if (groupName.Contains("1")) GetComponent<Renderer>().material.color = Color.green;
    //             else if (groupName.Contains("2")) GetComponent<Renderer>().material.color = Color.blue;
    //             else GetComponent<Renderer>().material.color = Color.yellow;
    //         }

    //         Debug.Log($" Đã tạo : {groupName} tại {position}");
    //     }

    // }
    void SpawnObjectAtPosition(Vector3 position, string groupName, string sizeType = "SMALL")
    {
        float prefabHeight = GetPrefabHeight();
        Debug.Log($"Prefab height: {prefabHeight}");
        Vector3 adjustedPosition = new Vector3(
            position.x,
            position.y + prefabHeight / 2f,
            position.z
        );

        GameObject spawnedObject = null;

        if (objectPrefab != null)
        {
            spawnedObject = Instantiate(objectPrefab, adjustedPosition, Quaternion.identity);
            spawnedObject.name = $"{groupName}_{objectPrefab.name}";
            Debug.Log($"Đã tạo {objectPrefab.name}: {groupName} tại {position}");
        }
        else
        {
            // Tạo primitive cube nếu không có prefab
            spawnedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // THÊM OFFSET NHỎ ĐỂ TRÁNH CHỒNG LẤN
            Vector3 offset = new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f)
            );

            spawnedObject.transform.position = position + offset;
            spawnedObject.name = $"{groupName}_Cube";

            // SET MÀU THEO GROUP
            Renderer renderer = spawnedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (groupName.Contains("0")) renderer.material.color = Color.red;
                else if (groupName.Contains("1")) renderer.material.color = Color.green;
                else if (groupName.Contains("2")) renderer.material.color = Color.blue;
                else renderer.material.color = Color.yellow;
            }

            Debug.Log($" Đã tạo : {groupName} tại {position}");
        }

        // 🔥 🔥 🔥 PHẦN QUAN TRỌNG: THÊM OBJECT SIZE COMPONENT
        if (spawnedObject != null)
        {
            // THÊM HOẶC LẤY ObjectSize COMPONENT
            ObjectSize objSize = spawnedObject.GetComponent<ObjectSize>();
            if (objSize == null)
                objSize = spawnedObject.AddComponent<ObjectSize>();

            // SET SIZE TYPE VÀ POINTS
            objSize.size = sizeType;
            objSize.points = GetPointsForSize(sizeType);

            // 🔥 ĐIỀU CHỈNH SCALE VISUAL THEO SIZE
            float scaleMultiplier = GetVisualScaleForSize(sizeType);
            spawnedObject.transform.localScale = Vector3.one * 0.3f * scaleMultiplier;

            Debug.Log($"✅ Object created: {groupName} | Size: {sizeType} | Scale: {scaleMultiplier}");
        }
    }

    // 🔥 THÊM HELPER METHOD (thêm vào cuối class, trước các method khác)
    private float GetVisualScaleForSize(string sizeType)
    {
        switch (sizeType)
        {
            case "SMALL": return 0.7f;
            case "MEDIUM": return 1.0f;
            case "LARGE": return 1.4f;
            case "SPECIAL": return 2.0f;
            default: return 1.0f;
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
        // GameObject[] allObjects = FindObjectsOfType<GameObject>();
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);


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
        // LevelData level = GenerateLevelData(nextIndex);
        LevelData level = GenerateLevelDataWithShapes(nextIndex);

        string fileName = $"level_{nextIndex}.json";
        string fullPath = Path.Combine(folderPath, fileName);

        ES3.Save("LevelData", level, fullPath);
        Debug.Log($"Level {nextIndex} generated & saved to:\n{fullPath}");

        CopyToResourcesFolder(fullPath, nextIndex);
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
