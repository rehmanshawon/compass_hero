using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnMouseOverGame : MonoBehaviour
{
    [SerializeField] private GameObject explanationDialog;
    [SerializeField] private Text explanationText;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void UnitGlowHover()
    {
        explanationDialog.SetActive(true);
        explanationText.text = "Unit Glow makes your units glow so you can find them easier.";
    }

    public void UnitGlowExit()
    {
        explanationDialog.SetActive(false);
    }

    public void SmartMapHover()
    {
        explanationDialog.SetActive(true);
        explanationText.text = "Smart Map allows the Game Map to move with you.";
    }

    public void SmartMapExit()
    {
        explanationDialog.SetActive(false);
    }

    public void CarvenHover()
    {
        explanationDialog.SetActive(true);
        explanationText.text = "Entrance to underwater cavern around Volcano";
    }

    public void CarvenExit()
    {
        explanationDialog.SetActive(false);
    }

    public void MapShotHover()
    {
        explanationDialog.SetActive(true);
        explanationText.text = "MAP SHOT button is used to take pictures of hidden objects Pinged by Sub Sonar";
    }

    public void MapShotExit()
    {
        explanationDialog.SetActive(false);
    }

    public void PicturesHover()
    {
        explanationDialog.SetActive(true);
        explanationText.text = "PICTURES button is pressed to see the small pictures appear again";
    }

    public void PicturesExit()
    {
        explanationDialog.SetActive(false);
    }
}
