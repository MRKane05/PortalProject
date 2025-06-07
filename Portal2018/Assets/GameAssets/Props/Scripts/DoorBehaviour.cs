using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic door. Opens, closes, gets locked/unlocked, opens on command, it'll be added to as the game develops
public class DoorBehaviour : MonoBehaviour {
	public bool bStateLocked = false;
	public bool bDoorOpen = false;
	public float playerTriggerDistance = 3f;    //How close before our door will automatically open?
	public Vector3 doorOffsetPosition = new Vector3(0.8f, 0, 0);    //What do we lerp to when we're open?
	Vector3 doorRightOffsetPosition = new Vector3(-0.8f, 0, 0);
	public float doorLerpSpeed = 5f;
	float lerpTime = 0;

	OcclusionPortal OcclusionLeft, OcclusionRight;

	public GameObject DoorLeft, DoorRight;

	void Start()
    {
		doorRightOffsetPosition = new Vector3(-doorOffsetPosition.x, 0, 0); //Flip our stated door position
		OcclusionLeft = DoorLeft.GetComponent<OcclusionPortal>();
		OcclusionRight = DoorRight.GetComponent<OcclusionPortal>();
    }

	//Given we're playing with physics this could be a bad idea...
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			SetDoorState(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			SetDoorState(false);
		}
	}

	public void SetDoorState(bool bOpen)
	{
		if (bOpen == bDoorOpen) { return; } //Don't need to do anything
		bDoorOpen = bOpen;
		if (bOpen)
		{
			OcclusionLeft.open = true;
			OcclusionRight.open = true;
		}
	}

	public void SetDoorLocked(bool bIsUnlocked)	//This will always be true when the button depresses
    {
		bStateLocked = !bIsUnlocked;
	}

	void Update()
    {
		if (bDoorOpen && !Mathf.Approximately(lerpTime, 1f) && !bStateLocked)
        {
			lerpTime = Mathf.Lerp(lerpTime, 1f, Time.deltaTime * doorLerpSpeed);
			DoorLeft.transform.localPosition = doorOffsetPosition * lerpTime;
			DoorRight.transform.localPosition = doorRightOffsetPosition * lerpTime;
		}
		if ((!bDoorOpen || bStateLocked) && !Mathf.Approximately(lerpTime, 0f))
        {
			lerpTime = Mathf.Lerp(lerpTime, 0f, Time.deltaTime * doorLerpSpeed);
			DoorLeft.transform.localPosition = doorOffsetPosition * lerpTime;
			DoorRight.transform.localPosition = doorRightOffsetPosition * lerpTime;
			if (lerpTime < 0.01f)	//Check to see if we should turn our occlusion off
            {
				OcclusionLeft.open = false;
				OcclusionRight.open = false;
			}
		}
    }
}
