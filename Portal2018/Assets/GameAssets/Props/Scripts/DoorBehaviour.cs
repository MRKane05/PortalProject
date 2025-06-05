using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic door. Opens, closes, gets locked/unlocked, opens on command, it'll be added to as the game develops
public class DoorBehaviour : MonoBehaviour {
	public bool bStateLocked = false;
	public bool bDoorOpen = false;
	public float playerTriggerDistance = 3f;    //How close before our door will automatically open?
	public Animation ourAnimation;

	void Start()
    {
		if (!ourAnimation) { //see if we can get it on this object
			ourAnimation = gameObject.GetComponent<Animation>();
		}
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
		switch (bDoorOpen)
        {
			case true:
				ourAnimation.Play("DoorOpen");
				break;
			case false:
				ourAnimation.Play("DoorClose");
				break;
        }
	}
}
