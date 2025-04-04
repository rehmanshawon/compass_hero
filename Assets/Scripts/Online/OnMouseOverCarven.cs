using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnMouseOverCarven : MonoBehaviour
{
    [SerializeField] private GameObject explanationDialog;
    [SerializeField] private Text explanationText;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        if (explanationDialog != null)
        {
            explanationDialog.SetActive(true);
            explanationText.text = "Entrance to underwater cavern around Volcano";
        }
    }

    private void OnMouseExit()
    {
        if(explanationDialog != null)
        explanationDialog.SetActive(false);
    }
}
