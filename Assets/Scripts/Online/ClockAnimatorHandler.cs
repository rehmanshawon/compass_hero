using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Called from Animation Event at final frame of clock animation
public class ClockAnimatorHandler : MonoBehaviourPun
{
    // Set in inspector or find MultiEngine at runtime
    public MultiEngine multi;

    // This method name should match the Animation Event name
    public void TimerAnimation_OnEnd()
    {
        if (multi == null) multi = MultiEngine.Instance;
        if (multi == null) {
            Debug.LogWarning("TimerAnimation_OnEnd: MultiEngine not found.");
            return;
        }

        // Route through central handler so master ends authoritatively and non-master requests master
        multi.HandleTimerTurnEnd();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
