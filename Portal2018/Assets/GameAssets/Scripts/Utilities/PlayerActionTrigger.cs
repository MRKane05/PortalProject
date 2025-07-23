using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A basic trigger that does something when our player walks through it
//Expands into Checkpoints
public class PlayerActionTrigger : MonoBehaviour
{
	public string DisplayMessage = "Checkpoint";

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			DoTriggerAction();
		}
	}

	public virtual void DoTriggerAction()
	{
		HUDManager.Instance.DisplayMessage(DisplayMessage);
	}
}
