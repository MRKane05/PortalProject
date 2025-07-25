using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningCreditsHandler : MonoBehaviour {
    public List<CanvasGroup> DisplayPanels;
    public CanvasGroup AuthentificationFailedPanel;
    float cycleTime = 2f;
    float fadeTime = 0.5f;
    float fadeStartTime = 0;

    int bChecksumPass = 0;

    IEnumerator Start()
    {
        //Set initial state
        for (int i=0; i<DisplayPanels.Count; i++)
        {
            DisplayPanels[i].alpha = i == 0 ? 1f : 0f;
        }

        while (!SaveUtility.Instance)
        {
            yield return null;
        }

        string FileChecksum = SaveUtility.Instance.CalculateFileChecksum("portal_pak_dir.vpk", "MD5");
        if (FileChecksum == null)
        {
            bChecksumPass = 2;
        }
        else
        {
            bChecksumPass = (FileChecksum == "efb124c25f2d3dabd4a377f19dc931e6") ? 1 : 2;
        }

        //So we want to cycle through our panels and then load the main menu when we're done
        //Begin by fading out our first panel to the second, and expand this script as we get new stuff...
        fadeStartTime = Time.time;
        while (Time.time < fadeStartTime + fadeTime)
        {
            float fadeFactor = ((fadeStartTime + fadeTime) - Time.time) / fadeTime;
            DisplayPanels[0].alpha = fadeFactor;
            DisplayPanels[1].alpha = 1f - fadeFactor;
            yield return null;
            CheckBreak();
        }

        DisplayPanels[0].alpha = 0f;
        DisplayPanels[1].alpha = 1f;

        //Our pause
        fadeStartTime = Time.time;
        while (fadeStartTime + 2f > Time.time)
        {
            yield return null;
            CheckBreak();
        }
        
        while (bChecksumPass == 0)
        {
            yield return null;
        }

        if (bChecksumPass == 2)
        {
            fadeStartTime = Time.time;
            while (Time.time < fadeStartTime + fadeTime)
            {
                float fadeFactor = ((fadeStartTime + fadeTime) - Time.time) / fadeTime;
                AuthentificationFailedPanel.alpha = 1f - fadeFactor;
                yield return null;
            }

            while (true)
            {
                yield return null;
            }
        }

        LoadNextLevel();
    }

    void LoadNextLevel()
    {
        if (bChecksumPass == 1)
        {
            SceneManager.LoadScene("Menu_Start");
        }
    }
    void CheckBreak()
    {
        if (Input.GetButton("Start"))
        {
            if (bChecksumPass == 1)
            {
                LoadNextLevel();
            }
        }
    }
}
