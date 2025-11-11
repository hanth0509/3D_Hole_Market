using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    public GameObject hole;
    public int score = 0;
    public int scaleStep = 5;
    private int lastScoreChecked = 0;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (score >= scaleStep && score % scaleStep == 0 && lastScoreChecked != score)
        {
            hole.transform.localScale += new Vector3(1f, hole.transform.localScale.y,1f);
            lastScoreChecked = score;
        }
    }
    public void AddScore(int amount)
    {
        score += amount;
        print("Score: " + score);
    }

}