using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialSystem : MonoBehaviour
{
   int currentTutorial=0;
    public static TutorialSystem instance;
    public static bool TutorialActive;
    public UImanagerTutorialsystem ui;
    public List<tutorialData> tutorials;
    public GameObject handpointer;




    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        TutorialActive = true;
    }
    bool startset = false;
    private void OnEnable()
    {
        if (startset)
        {


            instance = this;
            currentTutorial = 0;
            var objs = GameObject.FindObjectsOfType<ForceInfo>();
            foreach (var item in objs)
            {
                item.resetme();
            }
            foreach (var tutorial in tutorials)
            {
                tutorial.resetme();

            }
            CameraController.instance.lockcam = true;
            initTutorial();
        } 
    }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        currentTutorial = 0;

        var objs = GameObject.FindObjectsOfType<ForceInfo>();
        foreach (var item in objs)
        {
            item.resetme();
        }
        foreach (var tutorial in tutorials)
        {
            tutorial.resetme();

        }
        CameraController.instance.lockcam = true;
       initTutorial();
        startset = true;
    }
    public void Backtomain()
    {
        SceneManager.LoadScene("Main");
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void initTutorial()
    {
        tutorials[currentTutorial].NextStep();
       
    }
    public void NextStep()
    {
        var result = tutorials[currentTutorial].NextStep();
        if (result.Item2)
        {

            if (result.Item1)
            {
                Invoke("NextStep", result.time_val);
            }
            else
            {
                
                    

                
            }
            Debug.Log("mesgae her1");
        } 
        else
        {
            Debug.Log("mesgae her2");
            CameraController.instance.lockcam = false;
            ui.UpdateEndTutorial();
           // Invoke(nameof(endnow), 30);
            //if (NextTutorial())
            //{

            //}
            //else
            //{
               

            //    // tutorials ended
            //}
        }
    }
    public void endnow()
    {
        ui.EndTutorial();
    }
   public bool NextTutorial()
    {
        currentTutorial++;
        if (currentTutorial < tutorials.Count)
        {
            tutorials[currentTutorial].NextStep();
            return true;
        }
        else
        {
            return false;
        }


    }
}
[System.Serializable]
public class tutorialData
{
    public AudioSource audi;

    int currentstep = -1;
    public GameObject[] toenableonreset;
    public GameObject[] todisableonreset;
    public tutorialSteps[] Steps;
    public void resetme()
    {
        currentstep = -1;

        foreach (var item in toenableonreset)
        {
            item.SetActive(true);
        }
        foreach (var item in todisableonreset)
        {
            item.SetActive(false);
        }
    }

    public (bool istimed,bool ,float time_val) NextStep()
    {
        if (currentstep >= Steps.Length)
        {
            return (false,false,0);
        }
        if (currentstep >= 0)
        {

            foreach (var step in Steps[currentstep].stepobj)
            {
                step.SetActive(false);
            }
        }

        currentstep++;
        Debug.Log($"mesgae her  {currentstep}    : {Steps.Length}");
        if (currentstep < Steps.Length)
        {
            bool camset = false;
            foreach (var step in Steps[currentstep].stepobj)
            {
                step.SetActive(true);


            }

            if (Steps[currentstep].setCamPos)
            {
                CameraController.instance.setcampos(Steps[currentstep].positions[Steps[currentstep].campostarget].transform);
                camset = true;
            }
            bool istimed = false;
            if (Steps[currentstep].moveobjectonclick)
            {
                Steps[currentstep].objecttomove.transform.DOMove(Steps[currentstep].objectotmoveendpos.transform.position, 2);
            }
            if (Steps[currentstep].todestroy!=null && Steps[currentstep].todestroy.Length>0)
            {
                foreach (var item in Steps[currentstep].todestroy)
                {
                    item.gameObject.SetActive(false);
                 
                }
            }

            foreach (var item in Steps[currentstep].btns)
            {

                item.btn.interactable = item.enable;

            }

            if (Steps[currentstep].audioclip != null)
            {
                if (Steps[currentstep].isoneshot)
                {
                    audi.loop = true;
                    audi.PlayOneShot(Steps[currentstep].audioclip);
                }
                else
                {
                    audi.clip = Steps[currentstep].audioclip;
                    audi.loop = true;
                    audi.Play();
                }
            }
            else
            {
                audi.Stop();
            }

            CameraController.instance.lockcam = !Steps[currentstep].canmovecam;
            TutorialSystem.instance.handpointer.SetActive(camset);

            if(Steps[currentstep].moveobjectonclick)
            {
                istimed = true;
            }

            if(Steps[currentstep].istimed)
            {
                istimed=true;
            }
            if (currentstep >= Steps.Length - 1)
            {
                TutorialSystem.instance.handpointer.SetActive(false);
                return (false, false, 0);
            }
            else
            {
                return (istimed, true, Steps[currentstep].timeval);
            }

            

          
        }
        else
        {
            TutorialSystem.instance.handpointer.SetActive(false);
            return (false, false, 0);
        }

    }


}

[System.Serializable]
public class tutorialSteps
{
    public bool setCamPos;
    public bool canmovecam;
    public bool istimed;
    public float timeval=2;
    public int campostarget;
    public AudioClip audioclip;
 
    public bool isoneshot;
    public List<buttonsstate> btns;
    public bool moveobjectonclick;
    public GameObject objecttomove;
    public Transform objectotmoveendpos;
    public GameObject[] todestroy;
    public GameObject[] positions;
    public GameObject[] stepobj;
}
[System.Serializable]
public class buttonsstate
{
    public Button btn;
    public bool enable;
}
