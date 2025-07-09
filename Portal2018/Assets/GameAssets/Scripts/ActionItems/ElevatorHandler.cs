using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This will most likely become an interpolation class that'll also be used for the likes of platforms, but for now lets just get elevators working
public class ElevatorHandler : MonoBehaviour {
    public enum enElevatorState { NULL, WAITING, DOORCLOSING, MOVING, DOOROPENING, FINISHED}
    public enElevatorState ElevatorState = enElevatorState.WAITING;

    public GameObject ElevatorShuttle; //Our bit wot does the moving
    public GameObject ElevatorBase, ElevatorDestination; //Our two exterior pieces that we'll move between
    public ElevatorDoorsHandler ElevatorDoors;

    public float ElevatorMoveSpeed = 3f;

    public bool bBaseDoorsOpen = false;
    public float baseDoorAlpha = 0f;

    public void SetPlayerState(bool bPlayerInVolume)
    {
        if (bPlayerInVolume)
        {
            PlayerSteppedIntoElevator();
        }
    }

    public void PlayerEnteredOpenTrigger()
    {
        bBaseDoorsOpen = true;
    }

    public void PlayerSteppedIntoElevator()
    {
        if (ElevatorState == enElevatorState.WAITING)   //Do our door close thing
        {
            ElevatorState = enElevatorState.DOORCLOSING;
        }
    }
    float doorOpenSpeed = 3f;

    public void LateUpdate()
    {
        //Door behavior
        if (bBaseDoorsOpen && baseDoorAlpha < 1f)
        {
            baseDoorAlpha = Mathf.MoveTowards(baseDoorAlpha, 1f, Time.deltaTime * doorOpenSpeed);
            ElevatorDoors.setDoorAlpha(baseDoorAlpha);
        } 
        
        if (!bBaseDoorsOpen && baseDoorAlpha > 0f)
        {
            baseDoorAlpha = Mathf.MoveTowards(baseDoorAlpha, 0f, Time.deltaTime * doorOpenSpeed);
            ElevatorDoors.setDoorAlpha(baseDoorAlpha);
        }

        switch (ElevatorState)
        {
            case enElevatorState.WAITING:
                break;
            case enElevatorState.DOORCLOSING:   //Just jump straight to the moving
                bBaseDoorsOpen = false;
                if (baseDoorAlpha < 0.001f)
                {
                    ElevatorState = enElevatorState.MOVING;
                }
                break;
            case enElevatorState.MOVING:
                DoElevatorMove();
                break;
            case enElevatorState.DOOROPENING:
                break;
            case enElevatorState.FINISHED:
                break;
        }
    }

    void DoElevatorMove()
    {
        //What we really need is something that will smoothly transition. What we're going to get for the moment is something that moves!
        ElevatorShuttle.transform.position = Vector3.MoveTowards(ElevatorShuttle.transform.position, ElevatorDestination.transform.position, ElevatorMoveSpeed*Time.deltaTime);
        
        //Break from our state when we get close enough to the top. I'm sure that this could be made dramatic
        if (Mathf.Approximately(Vector3.SqrMagnitude(ElevatorShuttle.transform.position-ElevatorDestination.transform.position), 0f))
        {
            bBaseDoorsOpen = true;
            ElevatorState = enElevatorState.DOOROPENING;
        }
    }
}
