// using UnityEngine;

// public class UILevelButton: MonoBehaviour
// {
//     public int levelIndex;

//     public void OnLevelButtonClicked()
//     {
//         // Gọi hàm LoadLevelData từ MasterData và lưu kết quả trả về
//         MasterData.Instance.LoadLevelData(levelIndex);


//     }

// }


using UnityEngine;
using UnityEngine.SceneManagement;

public class UILevelButton : MonoBehaviour
{
    public int levelIndex = 1;

    public void OnLevelButtonClicked()
    {
        Debug.Log($"===CLICK LEVEL {levelIndex} ===");
        
        if (MasterData.Instance == null)
        {
            Debug.LogError(" MasterData.Instance là null!");
            return;
        }

        // CHỈ LOAD LEVEL DATA, KHÔNG SPAWN Ở ĐÂY
        bool success = MasterData.Instance.LoadLevelData(levelIndex);

        if (success)
        {
            Debug.Log($"Load Level {levelIndex} thành công!");

            //CHỈ LOAD SCENE GAMEPLAY, KHÔNG GỌI SPAWN
            SceneManager.LoadScene("Gameplay");
            Debug.Log("Đang chuyển sang scene Gameplay...");
        }
        else
        {
            Debug.LogError($"Load Level {levelIndex} thất bại");
        }
    }

}