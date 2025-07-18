using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Behaviour for the two instances of the stationary portal gun
public class StationaryPortalGunBehavior : MonoBehaviour
{
	//Directly linking to something is always a terrible idea...
	public float gunTurnTime = 3f;
	public float gunPauseTime = 3f;
	float lastActionTime = 0;
	Vector3 setRotation = Vector3.zero;
	float startDelay = 2f;
	public GameObject gunBase;
	public GameObject gunStand;

	public GameObject outPortalLocation;

	public Animation StandAnimator;

	// Use this for initialization
	IEnumerator Start()
	{
		yield return new WaitForSeconds(startDelay);
		lastActionTime = Time.time;
		//Start with a shot
		LevelController.Instance.playersPortalGun.DoScriptFire(SpawnPortalOnClick.Polarity.Left, gameObject.transform.position, gameObject.transform.forward);
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.time - lastActionTime > gunPauseTime)  //We need to rotate our gun 90deg to the left
		{
			gunBase.transform.eulerAngles = Vector3.MoveTowards(gunBase.transform.eulerAngles, setRotation + new Vector3(0, -90, 0), Time.deltaTime * 90f / gunTurnTime);
			gunStand.transform.eulerAngles = gunBase.transform.eulerAngles - new Vector3(0, 90, 0);
		}
		if (Time.time - lastActionTime > gunPauseTime + gunTurnTime)
		{
			lastActionTime = Time.time;
			gunBase.transform.eulerAngles = setRotation + new Vector3(0, -90, 0); //Make sure this is strictly correct
			setRotation = gunBase.transform.eulerAngles;
			LevelController.Instance.playersPortalGun.DoScriptFire(SpawnPortalOnClick.Polarity.Left, gunBase.transform.position, gunBase.transform.forward);
		}
	}

	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.name == "Player")
		{
			LevelController.Instance.playersPortalGun.PlacePortal(SpawnPortalOnClick.Polarity.Right, outPortalLocation.transform.position, outPortalLocation.transform.forward);
		}
	}


	public void PlayStandCloseAnimation()
	{
		StandAnimator.Play();
	}
}
