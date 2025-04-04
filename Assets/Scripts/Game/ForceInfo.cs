using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceInfo : MonoBehaviour
{
    public int tutorialid;
    public int teamIndex;
    public int forceType;
    public int movementRange;
    public int fireRange;
    public int controlTimes;
    public int legionIndex;
    public int xCoordinate;
    public int yCoordinate;


    public Vector2 startpos;
    private void Awake()
    {

        startpos = transform.position;

    }
    public void resetme()
    {
        transform.position = startpos;
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
