
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour {
    
    void Start() {
        // Initialization code here
    }
    void Update() {
        // Frame update code here
    }
    public void LoadSceneHome() {
        SceneManager.LoadScene("Home");
    }
    public void LoadSceneLevel() {
        SceneManager.LoadScene("Level");
    }
    public void LoadSceneGamePlay() {
        SceneManager.LoadScene("GamePlay");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
