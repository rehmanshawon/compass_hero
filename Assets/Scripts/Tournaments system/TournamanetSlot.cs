using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamanetSlot : MonoBehaviour
{
    public string myname;
    public int actornum;
    public TextMeshProUGUI txt_name;
    public Image mySp;
    public string myid;
    public void loadoldids()
    {
        string oldname = PlayerPrefs.GetString(myid + "oldname", "---");

        txt_name.text = oldname;
    }
    public void ClearSlot()
    {
        txt_name.text = "---";
        myname = "";
        actornum = -1;
        mySp.gameObject.SetActive(false);
    }
    public void UpdateName(string name,int num)
    {
        txt_name.text = name;
        actornum = num;
        mySp.gameObject.SetActive(true);
        PlayerPrefs.SetString(myid + "oldname", name);
        PlayerPrefs.Save();
    }
    public void UpdateSprite(Sprite sp)
    {
        Debug.Log("haveskin 4");
        mySp.gameObject.SetActive(true);
        mySp.sprite = sp;
    }
    public void clearsp()
    {
        mySp.gameObject.SetActive(false);
    }
}
