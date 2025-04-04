using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class customButtonLobby : MonoBehaviour,IPointerDownHandler, IPointerUpHandler
{

    public CanvasGroup btn;

    

    public MainUI ui;
    public GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetSelected(bool check)
    {

        if(check) 
        {
            btn.alpha = 1;
        panel.SetActive(true);
        }
        else
        {
            btn.alpha=0.5f;
        
            panel.SetActive(false);
        
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ui.selectButton(this);
       
    }
 

}
