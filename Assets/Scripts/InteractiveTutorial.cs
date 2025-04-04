using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractiveTutorial : MonoBehaviour {

    public static InteractiveTutorial Instance;
    public static bool TutorialActive;
  
    public CustomBuildType buildType;

    public GameObject gameUI;
    public GameObject pref_pointer;

    public GameObject lastpointer;
    public static int CurrentStep;

    public static Transform currenttarget;

    public GameObject pointer_ui_move;
    public GameObject pointer_ui_fire;
    public GameObject pointer_ui_sub;
    public GameObject pointer_ui_sonar;
    public GameObject pointer_ui_ping;

    public GameObject[] pl_generals;

    public TextMeshProUGUI txt_header;
    public GameObject tutorialheader;
    public CameraController cameraController;
    public GameObject tutorialended;

    public GameObject[] mousetutorial;
    public GameObject mousetutorialparent;
    public GameObject[] uitodisable;

    public GameObject[] todisable;
    public GameObject[] toenable;
    public int[] tutorialpositionlist;

    public Button[] buttonstoset;
    int currposindex = -1;



    public GameObject singlegamebtns;
    public GameObject normalgamebtns;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        currenttarget = null;
        //TutorialActive = true;
        CurrentStep = 0;
        InteractiveTutorial.TutorialActive = true;
    
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in uitodisable)
        {
            item.SetActive(false);
        }

        switch (buildType)
        {
            case CustomBuildType.Full:
                singlegamebtns.SetActive(false);
                normalgamebtns.SetActive(true);

                break;
            case CustomBuildType.Single:

                singlegamebtns.SetActive(true);
                normalgamebtns.SetActive(false);

          

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
    public int getPositionIndex()
    {
        currposindex++;

        if (currposindex < tutorialpositionlist.Length)
        {
            return tutorialpositionlist[currposindex];
        }
        else
        {
            return -1;
        }
    }
    [HideInInspector]public bool resetstepnow = false;
    public void resetstep()
    {
        ClearPointer();
        cameraController.lockcam = false;
    resetstepnow= true;
    }
    public void NextStep(int val, float t)
    {
        StartCoroutine(gotoNextStep(val, t));
    }
    public IEnumerator gotoNextStep(int val, float t)
    {
        yield return new WaitForSeconds(t);
        NextStep(val);
    }
    public void UpdateLastPos()
    {
        if (lastpointer != null)
        {
            lastpointer.transform.position = pl_generals[lastgeneral].transform.position;
            cameraController.setcampos(pl_generals[lastgeneral].transform);
        }
    }
    public void MouseTutorialstart()
    {
        mousetutorialparent.gameObject.SetActive(true);
        ShowMouseTutorial(-1);
    }
    public void MouseTutorialcomplete()
    {
        if (TutorialActive)
        {
            cameraController.lockcam = true;
            NextStep(0);
        }
    }
    int currMouseT = -1;
    public void ShowMouseTutorial(int n)
    {
        if (currMouseT == n)
        {
            currMouseT++;
            for (int i = 0; i < mousetutorial.Length; i++)
            {
                mousetutorial[i].SetActive(i == currMouseT);
            }
        }
    }
    public void setbutton(int n)
    {
        for (int i = 0; i < buttonstoset.Length; i++)
        {
            buttonstoset[i].interactable = i == n;
        }
    }
      public void UpdateTarget()
    {
        ClearPointer();

        if (pl_generals[lastgeneral] == null || !pl_generals[lastgeneral].activeSelf)
        {
            lastgeneral++;
        }

        spawnPoint(pl_generals[lastgeneral].transform);

    }
    public void NextStep(int val)
    {
        
        if (resetstepnow)
        {

           
            return;
        }
        if (CurrentStep == val)
        {
       
            CurrentStep++;
     

            if (CurrentStep == 1)
            {
              
                cameraController.lockcam = true;
                spawnAtGenral(0);
            }
            else if (CurrentStep == 2)
            {
               setbutton(0);
                setPointerMove(true);
            }
            else if (CurrentStep == 3)
            {
             
                cameraController.lockcam = false;
                setPointerMove(false);
                spawnAtGenral(0);
                setTutorialHeader("Choose from available positions!");
            }
            else if (CurrentStep == 4)
            {
                ClearPointer();
                setTutorialHeader();
                setbutton(1);
            }
            else if (CurrentStep == 5)
            {
                spawnAtGenral(0);

            }
            else if (CurrentStep == 6)
            {
                ClearPointer();
                setPointerFire(true);

            }
            else if (CurrentStep == 7)
            {
                cameraController.lockcam = false;
                setPointerFire(false);
                spawnAtGenral(0);
                setTutorialHeader("Choose from available positions to attack!");
            }
            else if (CurrentStep == 8)
            {
                cameraController.lockcam = false;
                ClearPointer();
                setTutorialHeader();
            }
            else if (CurrentStep == 9)
            {
                cameraController.lockcam = true;
                spawnAtGenral(1);
            }
            else if (CurrentStep == 10)
            {
                setbutton(3);
                setPointerSonar(true);
            }
            else if (CurrentStep == 11)
            {
                setPointerMove(false);
                setPointerPing(true, 3);
                //   spawnAtGenral(1);
                setTutorialHeader();
                cameraController.lockcam = false;
                setbutton(4);
            }
            else if (CurrentStep == 12)
            {
                //     TutorialEnded();
                foreach (var item in todisable)
                {
                    item.SetActive(false);
                }
                foreach (var item in toenable)
                {
                    item.SetActive(true);
                }
                cameraController.lockcam = false;
                // Invoke(nameof(TutorialEnded), 15);
            }
            else
            {

            }

        }

    }
    public void SkipTutorial()
    {





        switch (buildType)
        {
            case CustomBuildType.Full:
                SceneManager.LoadScene("Tutorial");
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



        switch (buildType)
        {
            case CustomBuildType.Full:
                SceneManager.LoadScene("Main");
                break;
            case CustomBuildType.Single:
                SceneManager.LoadScene("Tutorial");
                break;
            case CustomBuildType.Tutorial:
                GoBack();
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
    public void ReplayTutorial()
    {
        SceneManager.LoadScene("InteractiveTutorial");
    }
    public void Backtotutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowAdditionalTutorials()
    {
        SceneManager.LoadScene("AdditionalTutorial");
    }
    public void TutorialEnded()
    {
        mousetutorialparent.SetActive(false);
        tutorialended.SetActive(true);
    }
    public void setTutorialHeader(string str = "")
    {
        if (str == "" || str == string.Empty)
        {
            tutorialheader.gameObject.SetActive(false);
        }
        else
        {
            txt_header.text = str;
            tutorialheader.gameObject.SetActive(true);

        }

    }
    // Update is called once per frame
    void Update()
    {
        gameUI.transform.localScale = new Vector3(Screen.width / 1366f, Screen.height / 768f, 1);



        if (currMouseT == 0)
        {
            float val = Input.GetAxis("Mouse ScrollWheel");
            if (val != 0)
            {
                ShowMouseTutorial(0);
            }
        }
        else if (currMouseT == 1)
        {
            if (Input.GetMouseButtonUp(0))
            {
                ShowMouseTutorial(1);
                mousetutorialparent.GetComponent<Animator>().enabled = true;
                Invoke(nameof(endMousetutorial), 1);
            }
        }
    }
    public void endMousetutorial()
    {
        mousetutorialparent.SetActive(false);
        MouseTutorialcomplete();

    }
    int lastgeneral;
    public void spawnAtGenral(int n)
    {
  

        lastgeneral = n;
        if (pl_generals[n] == null || !pl_generals[n].activeSelf)
        {
            n++;
        }

        spawnPoint(pl_generals[n].transform); 
    }
    public void spawnPoint(Transform pos)
    {
        InteractiveTutorial.currenttarget = pos;

        if (lastpointer != null)
        {
            Destroy(lastpointer);
        }
        var sp=Instantiate(pref_pointer,null);
        sp.transform.position = pos.position;
    lastpointer = sp;
        cameraController.setcampos(pos);
    }
    public void ClearPointer()
    {
        if (lastpointer != null)
        {
            Destroy(lastpointer);
        }
    }

    public void setPointerMove(bool check)
    {
        ClearPointer();
        pointer_ui_move.SetActive(check);
    }
    public void setPointerFire(bool check)
    {
        ClearPointer();
        pointer_ui_fire.SetActive(check);
    }
    public void setPointerSub(bool check)
    {
        ClearPointer();
        pointer_ui_sub.SetActive(check);
    }
    public void setPointerSonar(bool check)
    {
        ClearPointer();
        pointer_ui_sonar.SetActive(check);
    }
    public void setPointerPing(bool check)
    {
        ClearPointer();
        pointer_ui_ping.SetActive(check);
    }
    public void setPointerPing(bool check,float t)
    {
        StartCoroutine(setpointerping(check, t));
    }
    private IEnumerator setpointerping(bool check,float t)
    {
        ClearPointer();
        yield return new WaitForSeconds(t);
    
        pointer_ui_ping.SetActive(check);

    }
}
