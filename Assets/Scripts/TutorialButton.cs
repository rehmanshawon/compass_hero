using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        TutorialSystem.instance.NextStep();
    }
    public void MovetoNextStep()
    {
        TutorialSystem.instance.NextStep();
    }
}
