using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FormationGroup
{
    // public string groupName;
    // public List<Vector3> positions = new List<Vector3>();
    // public string shapeType;
    // // Vật thể trong nhóm
    // public string objectSize = "SMALL";
    // public int points = 10;
    //
    public string groupName;
    public string shapeType;
    public string objectSize; // Size chung của group
    public int points;
    public List<Vector3> positions = new List<Vector3>();

    // 🔥 THÊM FIELD MỚI - size cho từng vị trí cụ thể
    public List<string> positionSizes = new List<string>();
}
[System.Serializable]
public class LevelTarget
{
    public int smallObjects = 10;   // Vật nhỏ (làm hole to lên)
    public int mediumObjects = 5;   // Vật trung
    public int largeObjects = 3;    // Vật lớn (mục tiêu chính)
    public int specialObjects = 2;  // Vật đặc biệt (bonus)
}
[System.Serializable]
public class LevelData
{
    public int levelIndex;
    public List<FormationGroup> groups = new List<FormationGroup>();

    //
    public LevelTarget target = new LevelTarget();
    public int timeLimit = 60;
    public int moveLimit = 20;
    //

    public int targetScore = 1000;
    // public int moveLimit = 20;
    // public float timeLimit = 0f;
    public string levelType = "SQUARE";
}