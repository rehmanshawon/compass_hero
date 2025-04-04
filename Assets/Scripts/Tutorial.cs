using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject[] tutorialPages;
    [SerializeField] private GameObject[] videoPlayer;
    [SerializeField] private GameObject[] blinkingText;
    public int pageIndex;

    public CustomBuildType buildType;

    public GameObject singlegamebtns;
    public GameObject normalgamebtns;
    public Image progressBar;
    public TextMeshProUGUI progressText;
    void Start()
    {
        pageIndex = 0;


        switch (buildType)
        {
            case CustomBuildType.Full:
               singlegamebtns.SetActive(false);
                normalgamebtns.SetActive(true);
                
                break;
            case CustomBuildType.Single:

                singlegamebtns.SetActive(true);
                normalgamebtns.SetActive(false);

                Invoke(nameof(PlayInteractive), 1.5f);

                break;
            case CustomBuildType.Tutorial:
                singlegamebtns.SetActive(false);
                normalgamebtns.SetActive(true);

                break;
            default:
                singlegamebtns.SetActive(false);
                normalgamebtns.SetActive(true);

                break;

        }
    }


    void Update()
    {
        tutorialUI.transform.localScale = new Vector3(Screen.width / 1366f, Screen.height / 768f, 1f);
    }
    public void PlayInteractive()
    {
       // InteractiveTutorial.InterativeTutorialActive = true;
       // SceneManager.LoadScene("InteractiveTutorial");
        StartCoroutine(LoadSceneAsync("InteractiveTutorial"));
    }
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent auto-switch until fully loaded

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // Normalize 0-1
            progressBar.fillAmount = progress;
            progressText.text = (progress * 100).ToString("F0") + "%";

            if (operation.progress >= 0.9f) // Scene is loaded, allow activation
            {
              
                    operation.allowSceneActivation = true;
                
            }
            yield return null;
        }
    }


    public void NextBtnClick()
    {
        for(int i = 0; i < videoPlayer.Length; i++)
        {
            videoPlayer[i].SetActive(false);
            blinkingText[i].SetActive(true);
        }
        
        if(pageIndex < 21)
        {
            pageIndex++;
            tutorialPages[pageIndex - 1].SetActive(false);
            tutorialPages[pageIndex].SetActive(true);
        }
        else
        {
            PlayInteractive();
        }                
    }

    public void PreviousBtnClick()
    {
        for (int i = 0; i < videoPlayer.Length; i++)
        {
            videoPlayer[i].SetActive(false);
            blinkingText[i].SetActive(true);
        }

        if (pageIndex > 0)
        {
            pageIndex--;
            tutorialPages[pageIndex].SetActive(true);
            tutorialPages[pageIndex + 1].SetActive(false);
        }
    }

    public void ExitBtnClick()
    {
 

        switch (buildType)
        {
            case CustomBuildType.Full:
                SceneManager.LoadScene("Main");
                break;
            case CustomBuildType.Single:
                GoBack();
                break;
            case CustomBuildType.Tutorial:

                GoBack();
                break;
            default:
                break;
        }
    }
    public void SkipTutorial()
    {
     
       SceneManager.LoadScene("Single");
        // SceneManager.LoadScene("Tutorial");
        //    ExitBtnClick();

        switch (buildType)
        {
            case CustomBuildType.Full:
                SceneManager.LoadScene("Tutorial");
                break;
            case CustomBuildType.Single:
                SceneManager.LoadScene("Single");
                break;
            case CustomBuildType.Tutorial:
                break;
            default:
                break;
        }
    }
    public void GoBack() // to make exit close the gameplay for tutorial only
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("history.back()");
        #endif
    }
    
}
[System.Serializable]
public enum CustomBuildType
{
    Full,
    Single,
    Tutorial

}
