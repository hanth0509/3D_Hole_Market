using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PanelSwitcher : MonoBehaviour
{
    public List<GameObject> panels;
    public Button buttonPrev;
    public Button buttonNext;

    int currentIndex = 0;

    void Start()
    {
       UpdatPanels();
    }

    void UpdatPanels()
    {
        // Deactivate all panels
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        // Activate the current panel
        panels[currentIndex].SetActive(true);

        // Update button visibility
        buttonPrev.gameObject.SetActive(currentIndex > 0);
        buttonNext.gameObject.SetActive(currentIndex < panels.Count - 1);

    }
    public void ShowNextPanel()
    {
        if(currentIndex < panels.Count -1)
        {
            currentIndex ++;
            UpdatPanels();
        }
    }
    public void ShowPreviousPanel()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatPanels();
        }
    }
}
