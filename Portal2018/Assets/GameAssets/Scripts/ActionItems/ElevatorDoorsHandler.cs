using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoorsHandler : MonoBehaviour {
    public GameObject RightDoor, LeftDoor;
    public float openRotation = 40f;
    public void setDoorAlpha(float toThis)
    {
        RightDoor.transform.localEulerAngles = new Vector3(0, toThis * openRotation, 0);
        LeftDoor.transform.localEulerAngles = new Vector3(0, -toThis * openRotation, 0);
    }
}
