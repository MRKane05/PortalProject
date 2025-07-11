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

	void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();
	}

	// Use this for initialization
	IEnumerator Start()
	{

		setMoveDir(Vector3.up);
		yield return new WaitForSeconds(0.5f);
		//turn our collider back on now that we've left the area of our projector
		gameObject.GetComponent<Collider>().enabled = true;
		effectsMaterial = new Material(effectsSphere.material);
		effectsSphere.material = effectsMaterial;

	}

	public void setMoveDir(Vector3 newMoveDir)
	{

		if (!rb)
		{
			rb = gameObject.GetComponent<Rigidbody>();
		}
		rb.velocity = newMoveDir * moveSpeed;

		moveDirection = newMoveDir;
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
		}
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
		SpawnSplashParticles(collision.contacts[0].point, collision.contacts[0].normal, Color.white);
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
