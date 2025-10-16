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

    
    // public void PopulateSlot(string playername, int id, int actornum)
    // {
    //     int roundcount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);
    //
    //     // Check if the round count is within bounds of the slotsMatch array
    //     if (roundcount > 0 && roundcount < slotsMatch.Count)
    //     {
    //         foreach (var item in slotsMatch[0].slots)
    //         {
    //             item.loadoldids();  // Ensure this handles any possible null or error cases
    //         }
    //     }
    //
    //     // Check if the player already exists in the current round's slots
    //     var currentRoundSlots = slotsMatch[roundcount].slots;
    //     if (currentRoundSlots != null && currentRoundSlots.Find(x => x.myname == playername) == null)
    //     {
    //         // Update the slot with the player's name and actor number
    //         if (id >= 0 && id < currentRoundSlots.Count)  // Ensure the id is within bounds
    //         {
    //             currentRoundSlots[id].UpdateName(playername, actornum);
    //         }
    //         else
    //         {
    //             Debug.LogError($"Invalid slot ID: {id}");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Player {playername} already exists in the current round.");
    //     }
    // }
    
    public void PopulateSlotManual(string playername, int id, int actornum, int roundIndex)
    {
        // Check if the roundIndex is within bounds of the slotsMatch array
        if (roundIndex >= 0 && roundIndex < slotsMatch.Count)
        {
            // foreach (var item in slotsMatch[roundIndex].slots)
            // {
            //     item.loadoldids();  // Ensure this handles any possible null or error cases
            // }

            var manualRoundSlots = slotsMatch[roundIndex].slots;

            // Check if the player already exists in the selected round's slots
            if (manualRoundSlots != null && manualRoundSlots.Find(x => x.myname == playername) == null)
            {
                if (id >= 0 && id < manualRoundSlots.Count)
                {
                    manualRoundSlots[id].UpdateName(playername, actornum);
                }
                else
                {
                    Debug.LogError($"Invalid slot ID: {id}");
                }
            }
            else
            {
                Debug.LogWarning($"Player {playername} already exists in round {roundIndex}.");
            }
        }
        else
        {
            Debug.LogError($"Invalid round index: {roundIndex}");
        }
        
        
    }

    
    public void ClearSlot(int roundcount, int id)
    {
        // Ensure the roundcount is within bounds
        if (roundcount >= 0 && roundcount < slotsMatch.Count)
        {
            var currentRoundSlots = slotsMatch[roundcount].slots;
        
            // Ensure the slot ID is valid
            if (id >= 0 && id < currentRoundSlots.Count)
            {
                // Clear the slot data
                currentRoundSlots[id].ClearSlot();

                // Optionally, log the clearing of the slot for debugging
                Debug.Log($"Slot {id} in round {roundcount} has been cleared.");
            }
            else
            {
                Debug.LogError($"Invalid slot ID: {id}");
            }
        }
        else
        {
            Debug.LogError($"Invalid round count: {roundcount}");
        }
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
            // foreach (var item in slotsMatch[0].slots)
            // {
            //     item.loadoldids();
            // }

        }
        if (slotsMatch[roundcount].slots.Find(x => x.actornum == actornum) == null)
        {
            Debug.Log("haveskin 3");
            //   slotsMatch1[id].name = playername+"  actor : "+actornum;
            slotsMatch[roundcount].slots[id].UpdateSprite(sp);
        }
    }
}
