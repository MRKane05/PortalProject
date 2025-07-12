using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A base class for something that moves between points
//Of course there'll have to be some sort of clever going on with this because we've a lot of variation with lifts in this game
public class MovingPlatform : MonoBehaviour {
    public List<GameObject> pathPoints;
    public int startPoint = 1;
    public int accessablePoint = 0;    //If we're dealing with a basic lift
    public float moveSpeed = 3f;
    public GameObject platformObject;

    public enum enLiftType { NULL, UPDOWN, PATH };
    public enLiftType LiftType = enLiftType.UPDOWN;

    public bool bIsLocked = true;
    public float triggerTime = 0;   //The idea here is that the lift will return to it's base position after a set delay

    void Start()
    {
        //Soon to become a switch...
        if (LiftType == enLiftType.UPDOWN)
        {
            platformObject.transform.position = pathPoints[startPoint].transform.position;
        }
    }

    void Update()
    {
        triggerTime -= Time.deltaTime;
        if (!bIsLocked && triggerTime <= 0)
        {
            platformObject.transform.position = Vector3.MoveTowards(platformObject.transform.position, pathPoints[accessablePoint].transform.position, moveSpeed * Time.deltaTime);
        } else if (triggerTime > 0)
        {
            platformObject.transform.position = Vector3.MoveTowards(platformObject.transform.position, pathPoints[startPoint].transform.position, moveSpeed * Time.deltaTime);
        }
    }

    public void SetTriggerLock(bool bToState)
    {
        bIsLocked = !bToState;
    }

    public void SetTriggerTime()
    {
        triggerTime = 5f;   //This should make our lift go up
    }
}
