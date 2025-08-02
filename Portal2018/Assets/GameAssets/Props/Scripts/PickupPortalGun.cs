using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupPortalGun : MonoBehaviour {
	public StationaryPortalGunBehavior stationaryBehaviour;
	public bool bPickupLeft = true;
	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.name == "Player")
		{
			stationaryBehaviour.enabled = false;    //Turn off our stationary behavior
			LevelController.Instance.playersPortalGun.RevealHidePortalGun(true);
			LevelController.Instance.playerCollectPortalGun();
			LevelController.Instance.reticuleHandler.ChangeStartState(bPickupLeft);
			gameObject.SetActive(false); //Hide the model for this object
			stationaryBehaviour.PlayStandCloseAnimation();			
		}
	}
}
