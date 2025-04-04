using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentsMatchSlots : MonoBehaviour
{
    public List< SlotList> slotsMatch;
 

    [System.Serializable]
    public class SlotList
    {
        public List<TournamanetSlot> slots;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void clearslots()
    {
        foreach (var slota in slotsMatch)
        {
            foreach (var slot in slota.slots)
            {

                slot.ClearSlot();
            }

        }   
    }

    
    public void PopulateSlot(string playername,int id,int actornum)
    {
        int roundcount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);


        if(roundcount>0)
        {
            foreach (var item in slotsMatch[0].slots)
            {
                item.loadoldids();
            }

        }

        if (slotsMatch[roundcount].slots.Find(x => x.myname == playername) == null)
        {
            slotsMatch[roundcount].slots[id].UpdateName(playername  , actornum);

        }
        //if (roundcount == 0)
        //{

        //}else if(roundcount == 1)
        //{
        //    if (slotsMatch2.Find(x => x.myname == playername) == null)
        //    {
        //        slotsMatch2[id].UpdateName(playername + "  actor : " + actornum, actornum);

        //    }
        //}


    }
    public void clearSkinSlot(int id, int actornum)
    {
        int roundcount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);

        if (slotsMatch[roundcount].slots.Find(x => x.actornum == actornum) == null)
        {
            //   slotsMatch1[id].name = playername+"  actor : "+actornum;
            slotsMatch[roundcount].slots[id].clearsp();
        }
    }
    public void UpdateSlotSkin(Sprite sp,int id ,int actornum)
    {


        int roundcount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);
        if (roundcount > 0)
        {
            foreach (var item in slotsMatch[0].slots)
            {
                item.loadoldids();
            }

        }
        if (slotsMatch[roundcount].slots.Find(x => x.actornum == actornum) == null)
        {
            Debug.Log("haveskin 3");
            //   slotsMatch1[id].name = playername+"  actor : "+actornum;
            slotsMatch[roundcount].slots[id].UpdateSprite(sp);
        }
        //if (roundcount == 0)
        //{

        //}
        //else if (roundcount == 1)
        //{
        //    if (slotsMatch2.Find(x => x.actornum == actornum) == null)
        //    {
        //        Debug.Log("haveskin 3");
        //        //   slotsMatch1[id].name = playername+"  actor : "+actornum;
        //        slotsMatch2[id].UpdateSprite(sp);
        //    }
        //}

    }
}
