using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is called whenever we pass through a checkpoint, and is a reference location to return the player to
public class Checkpoint : MonoBehaviour {
	public string CheckpointName = "";

	public int CheckpointIndex = -1; //This is used when there are multiple chambers in a map and relates to unlocking. The first is zero, the second is 1 and so on. -1 is a null point

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			if (GameDataHandler.Instance && CheckpointIndex >= 0)
            {
				GameDataHandler.Instance.UnlockedChamber(CheckpointName);
            }

			//But always we'll keep a note of what we are and where we are so that we can have the player continue from here
			//This feels like a playerprefs thing

			PlayerPrefs.SetString("CheckpointLevel", gameObject.scene.name);
			PlayerPrefs.SetString("CheckpointObject", gameObject.name);
			PlayerPrefs.Save();

			HUDManager.Instance.DisplayMessage("Checkpoint");
		}
	}

}
