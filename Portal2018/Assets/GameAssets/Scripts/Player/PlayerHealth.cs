using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles all player health functions
public class PlayerHealth : MonoBehaviour {
	public GameObject CameraObject;
	public GameObject BodyBase;
	public float Health = 100;
	public float HealthRegenDelay = 3f;
	public float HealthRegenSpeed = 33f;
	bool bPlayerAlive = true;
	Vector3 cameraLocalGrounded = new Vector3(0f, -0.6f, 0f);   //Where the camera will drop to
	public bool bIsInvincible = false;

	public Animator portalGunAnimator;


	InputManager ourInputManager;
	void Start () {
		ourInputManager = gameObject.GetComponent<InputManager>();
	}

	public virtual void TakeDamage(float DamageAmount, string DamageType)
    {
		//Put some effect over the screen, even better if we can make it so that it indicates where damage is coming from
		if (bIsInvincible) { return; }

		Health -= DamageAmount;
		if (Health <=0 && bPlayerAlive)
        {
			bPlayerAlive = false;
			PlayerDie(DamageType);
        }
    }

	public virtual void PlayerDie(string DamageType)
    {
		//For the moment lets just make our camera drop
		BodyBase.SetActive(false); //turn off the players body
		ourInputManager._movementEnabled = false;   //Disable player input and movement
		portalGunAnimator.SetTrigger("PlayerDie");

	}

	void LateUpdate () {
		if (!bPlayerAlive)	//Make the camera drop
        {
			//Close enough for the moment
			CameraObject.transform.localPosition = Vector3.MoveTowards(CameraObject.transform.localPosition, cameraLocalGrounded, 3f * Time.deltaTime);
        }	
	}
}
