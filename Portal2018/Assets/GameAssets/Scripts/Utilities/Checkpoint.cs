using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is called whenever we pass through a checkpoint, and is a reference location to return the player to
public class Checkpoint : PlayerActionTrigger {
	public enum enOverridePortal { NONE, LEFT, RIGHT }  //A flag to see if we need to change portal gun behavior for this checkpoint
	public enOverridePortal OverridePortal = enOverridePortal.NONE;

	public string CheckpointName = "";
	public string CheckpointTitle = "00"; //This is what's shown on the main menu

	public int CheckpointIndex = -1; //This is used when there are multiple chambers in a map and relates to unlocking. The first is zero, the second is 1 and so on. -1 is a null point


	public ElevatorHandler entryElevatorSystem;


	public override void DoTriggerAction()
    {
		if (GameDataHandler.Instance && CheckpointIndex >= 0)
		{
			GameDataHandler.Instance.UnlockedChamber(CheckpointName);
		}

		//But always we'll keep a note of what we are and where we are so that we can have the player continue from here
		//This feels like a playerprefs thing

		PlayerPrefs.SetString("CheckpointLevel", gameObject.scene.name);
		PlayerPrefs.SetString("CheckpointObject", gameObject.name);
		PlayerPrefs.SetString("CheckpointTitle", CheckpointTitle);
		PlayerPrefs.Save();

		HUDManager.Instance.DisplayMessage(DisplayMessage);
	}

	public void SetupPortalGun()
    {
		if (OverridePortal != enOverridePortal.NONE)
        {
			switch (OverridePortal)
            {				
				case enOverridePortal.LEFT:
					//This'll do for the moment, but it's far from correct
					LevelController.Instance.playersPortalGun.RevealHidePortalGun(true);
					LevelController.Instance.playerCollectPortalGun();
					LevelController.Instance.reticuleHandler.ChangeStartState(true);
					break;
				case enOverridePortal.RIGHT:
					break;

			}
        }
    }
}
