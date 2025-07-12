using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletProjectile : MonoBehaviour
{
	public float moveSpeed = 10f;
	Vector3 moveDirection = Vector3.up;
	Rigidbody rb;
	public MeshRenderer effectsSphere;
	Material effectsMaterial;

	public Vector2 effectsPan = Vector2.zero;
	public GameObject bounceEffect;

	public List<AudioClip> bounceSounds;
	public List<AudioClip> dissolveSounds;
	public GameObject pelletDissolveEffect;

	public float lifeSpan = 15f;
	public float lifeStart = 0f;

	AudioSource ourAudio;

	public PelletLauncherBehavior ourPelletLauncher;
	bool bPelletCaught = false;

	void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();
		ourAudio = gameObject.GetComponent<AudioSource>();
	}

	// Use this for initialization
	IEnumerator Start()
	{
		if (false)
		{
			setMoveDir(Vector3.up);
			yield return new WaitForSeconds(0.5f);
		}
		//turn our collider back on now that we've left the area of our projector
		//gameObject.GetComponent<Collider>().enabled = true;
		effectsMaterial = new Material(effectsSphere.material);
		effectsSphere.material = effectsMaterial;
		
	}

	//This also starts the projectile
	public void setMoveDir(Vector3 newMoveDir)
	{
		if (bPelletCaught) { return; }

		lifeStart = Time.time;
		if (!rb)
		{
			rb = gameObject.GetComponent<Rigidbody>();
		}
		rb.velocity = newMoveDir * moveSpeed;

		moveDirection = newMoveDir;

		StartCoroutine(setPelletActives());
	}

	IEnumerator setPelletActives()
    {
		yield return new WaitForSeconds(0.5f);
		gameObject.GetComponent<Collider>().enabled = true; //Coliders back on
	}

	private Vector3 ReflectProjectile(Vector3 velocityIn, Vector3 reflectVector)
	{
		return Vector3.Reflect(velocityIn, reflectVector).normalized;
	}

	void Update()
	{
		if (effectsMaterial)
		{
			effectsMaterial.SetTextureOffset("_MainTex", wrapVector(effectsPan * Time.time));   //In theory there's a breakdown point with this...
			if (!bPelletCaught)
			{
				effectsMaterial.SetColor("_Color", Color.Lerp(Color.white, Color.black, (Time.time - lifeStart) / (lifeSpan + 5f)));
			}
		}
		if (Time.time > lifeStart + lifeSpan && !bPelletCaught)
        {
			DoPelletDie();
        }
	}

	void DoPelletDie()
    {
		GameObject pelletDissolve = Instantiate(pelletDissolveEffect, transform.position, transform.rotation) as GameObject;
		pelletDissolve.GetComponent<AudioSource>().PlayOneShot(dissolveSounds[Random.Range(0, dissolveSounds.Count)]);
		Destroy(pelletDissolve, 3f);	//Remove our effect
		ourPelletLauncher.PelletDied();
		gameObject.GetComponent<Collider>().enabled = false;
	}

	Vector2 wrapVector(Vector2 thisVector)
    {
		return new Vector2(Mathf.Repeat(thisVector.x, 2f), Mathf.Repeat(thisVector.y, 2f));
    }

	void SpawnSplashParticles(Vector3 position, Vector3 direction, Color color)
	{
		if (!bounceEffect) { return; } //Don't do anything if we don't have a prefab
		GameObject obj = Instantiate(bounceEffect);
		obj.transform.position = position;
		obj.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

		ParticleSystem particles = obj.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = particles.main;
		main.startColor = color;
		Destroy(obj, 2f); //Problem: Tiny gain to be made by having splash particles in a buffer
	}
	void OnCollisionEnter(Collision collision)
	{
		//Look to see if we've hit out pellet catcher
		PelletCatcherBehaviour catcherScript = collision.gameObject.GetComponent<PelletCatcherBehaviour>();
		if (catcherScript)
		{
			if (!catcherScript.bHasPellet)
			{
				bPelletCaught = true;
				//Disable our collider, rigid body, and our dock pellet should set our position
				Teleportable teleportableScript = gameObject.GetComponent<Teleportable>();
				Destroy(teleportableScript);
				gameObject.GetComponent<Collider>().enabled = false;
				Destroy(rb);
				
				//Tell our launcher that we've docked
				ourPelletLauncher.PelletDocked();
				catcherScript.DockPellet(gameObject, this);
				//Set our brightness for the user to see
				effectsMaterial.SetColor("_Color", Color.white);
				//Turn down our audio
				ourAudio.volume = 0.05f;
			}
		}
		
		if (!bPelletCaught)
		{

			SpawnSplashParticles(collision.contacts[0].point, collision.contacts[0].normal, Color.white);

			if (ourAudio && bounceSounds.Count > 0)
			{
				ourAudio.PlayOneShot(bounceSounds[Random.Range(0, bounceSounds.Count)]);
			}
		}
    }

	void OnCollisionExit(Collision collision)
    {
		//Quick bit of math to see if we need to add more to our speed
		if (rb.velocity.magnitude < moveSpeed)
        {
			//rb.AddForce(rb.velocity.normalized * (moveSpeed - rb.velocity.magnitude));
			rb.velocity = rb.velocity.normalized * moveSpeed;
        }
    }
}
