using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Similar to the simple move object this is a door that can be timed or commanded, or just stay open
public class SimpleDoor : MonoBehaviour {
    public enum enDoorNature { NULL, TIMED, TRIGGERED }
    public enDoorNature DoorNature = enDoorNature.TIMED;

    public enum enDoorType { NULL, SLIDING, SWINGING }
    public enDoorType DoorType = enDoorType.SLIDING;

    public float TimedDuration = 4f;
    public float MoveSpeed = 5f;


    public Vector3 OpenOffset = Vector3.zero;
    Vector3 basePosition;
    bool bDoorOpen = false;
    bool bDoingMove = false;

    AudioSource ourAudio;
    public AudioClip doorOpenSound, doorCloseSound;

    float lastTriggerTime = 0;

    void Start()
    {
        //PROBLEM: Need to add rotational door behaviours
        if (DoorType == enDoorType.SLIDING)
        {
            basePosition = transform.position;
        }
    }

    void Update()
    {
        if (lastTriggerTime + TimedDuration > Time.time && bDoorOpen && DoorNature == enDoorNature.TIMED)
        {
            TriggerCloseDoor();
        }
    }

    void FixedUpdate()
    {
        if (bDoingMove)
        {
            switch (DoorType) {
                case enDoorType.SLIDING:
                    if (bDoingMove)
                    {
                        if ((Vector3.SqrMagnitude(basePosition + (bDoorOpen ? OpenOffset : Vector3.zero) - gameObject.transform.position) < 0.01f))
                        {
                            bDoingMove = false;
                        }
                        else
                        {
                            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, basePosition + (bDoorOpen ? OpenOffset : Vector3.zero), MoveSpeed * Time.fixedDeltaTime);
                        }
                    }
                    break;
            }
        }
    }

    public void TriggerOpenDoor()
    {
        bDoingMove = true;
        bDoorOpen = true;
        lastTriggerTime = Time.time;
        if (ourAudio)
        {
            ourAudio.PlayOneShot(doorOpenSound);
        }
    }

    public void TriggerCloseDoor()
    {
        bDoingMove = true;
        bDoorOpen = false;
        lastTriggerTime = Time.time;
        if (ourAudio)
        {
            ourAudio.PlayOneShot(doorCloseSound);
        }
    }
}
