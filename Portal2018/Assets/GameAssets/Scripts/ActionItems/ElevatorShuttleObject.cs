using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Because the trigger class will be a volume within this space we need to keep an eye on this for handling everything
public class ElevatorShuttleObject : MonoBehaviour {
	public ElevatorHandler ourParentHandler;
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			ourParentHandler.SetPlayerState(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			ourParentHandler.SetPlayerState(false);
		}
	}
}
