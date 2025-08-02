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
	public List<float> gunPositions = new List<float>();
	public int currentMoveAngle = 0;
	public GameObject gunBase;
	public GameObject gunStand;

	public GameObject outPortalLocation;

	public Animation StandAnimator;

	bool bIsActive = false;

	public SpawnPortalOnClick.Polarity portalPolartiy = SpawnPortalOnClick.Polarity.Left;
	public ParticleSystem FlareParticles;
	AudioSource ourAudio;
	Animation ourAnimation;
	public AudioClip RotateSound;
    public AudioClip FireSound;
	public AudioClip ChargeSound;

    void Start()
    {
		ourAudio = gameObject.GetComponent<AudioSource>();
		ourAnimation = gameObject.GetComponent<Animation>();
    }


	// Use this for initialization
	public void DoActionStart()
	{
		bIsActive = true;
		ourAnimation.Play();
	}

	public void DoGunFlare()
    {
		//Sound effects
		//Visual effect of gun flaring up
		if (ourAudio && ChargeSound)
		{
			ourAudio.PlayOneShot(ChargeSound);
		}
		if (FlareParticles)
        {
			FlareParticles.Emit(1);
		}
	}

	public void DoGunFire()
    {
		LevelController.Instance.playersPortalGun.DoScriptFire(portalPolartiy, gunBase.transform.position, gunBase.transform.forward);
		if (ourAudio && FireSound)
		{
			ourAudio.PlayOneShot(FireSound);
		}
	}

	public void DoRotate()
    {
		if (ourAudio && RotateSound)
        {
			ourAudio.PlayOneShot(RotateSound);
        }
    }

	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.name == "Player")
		{
			//Place out portal
			if (outPortalLocation)
			{
				LevelController.Instance.playersPortalGun.PlacePortal(false, outPortalLocation.transform, outPortalLocation.transform.position, outPortalLocation.transform.forward);
			}
		}
	}


	public void PlayStandCloseAnimation()
	{
		//We want to kill our animation too
		ourAnimation.Stop();
		StandAnimator.Play();
	}
}
