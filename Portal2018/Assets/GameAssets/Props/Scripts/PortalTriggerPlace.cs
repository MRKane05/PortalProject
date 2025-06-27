using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTriggerPlace : MonoBehaviour {
	public GameObject outPortalLocation;
	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.name == "Player")
		{
			LevelController.Instance.playersPortalGun.PlacePortal(SpawnPortalOnClick.Polarity.Right, outPortalLocation.transform.position, outPortalLocation.transform.forward);
		}
	}
}
