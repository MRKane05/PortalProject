using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateBehaviour : MonoBehaviour {

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name != "Player")
		{
			//We need to do the destroy on the object we were holding
			Teleportable ourTeleportable = other.gameObject.GetComponent<Teleportable>();
			if (ourTeleportable)
            {
				ourTeleportable.DoGateDissolve();
            }
		}
	}
}
