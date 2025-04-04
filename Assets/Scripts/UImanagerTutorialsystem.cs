using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UImanagerTutorialsystem : MonoBehaviour
{
    public GameObject maincanvas;
    public GameObject[] tutorialslist;

    
    public GameObject endinfo;


    public CustomBuildType buildType;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SkipTutorial()
    {
   
       
        switch (buildType)
        {
            case CustomBuildType.Full:
                SceneManager.LoadScene("Main");
                break;
            case CustomBuildType.Single:
                SceneManager.LoadScene("Single");
                break;
            case CustomBuildType.Tutorial:

                SceneManager.LoadScene("Tutorial");
                break;
            default:
                break;
        }
    }
 
    public void Backtomain()
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void UpdateEndTutorial()
    {
        endinfo.SetActive(true);
    }
    public void EndTutorial()
    {
        maincanvas.SetActive(true);
        foreach (var tutorial in tutorialslist)
        {
           tutorial.SetActive(false);
        }
    }
    public void chooseATutorial(int n)
    {
        if (n >= 0 && n < tutorialslist.Length)
        {
            for (int i = 0; i < tutorialslist.Length; i++)
            {
                tutorialslist[i].SetActive(i == n);
            }
        }
        maincanvas.SetActive(!(n >= 0 && n < tutorialslist.Length));
        endinfo.SetActive(false);
    }
}
