using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletLauncherBehavior : MonoBehaviour {
	public List<Animation> ArmAnimations = new List<Animation>();
    float pelletFireDelay = 0.5f;
    public GameObject pelletPrefab; //Might have to be a rigidbody, I'm not sure
    GameObject ourPellet;

    public GameObject launchPoint;

    public List<AudioClip> launchSounds;

    AudioSource ourAudio;
    public bool bCanFire = false;
    bool bPelletDocked = false;
    public ParticleSystem ScorchParticles;

    IEnumerator Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
        if (bCanFire)
        {
            yield return new WaitForSeconds(3f);
            DoFirePellet();
        }
    }

    public void AddScorchMark(Vector3 position, Vector3 normal)
    {
        if (ScorchParticles)
        {
            ParticleSystem.Particle newParticle = new ParticleSystem.Particle();
            newParticle.position = position + normal * 0.1f;
            Quaternion particleRotation = Quaternion.LookRotation(normal); //Get our particle direction

            //I don't know why this isn't working correctly. Particles are getting a funny angle on them
            //particleRotation *= Quaternion.AngleAxis(Random.Range(0f, 360f), normal); //Rotate around the direction of impact for some randomization

            newParticle.rotation3D = particleRotation.ToEuler() * 57.295779f; // Because ToEuler is always in radians and our particles are in degrees

            newParticle.startLifetime = 20f; // ScorchParticles.startLifetime;
            newParticle.remainingLifetime = 20f;// ScorchParticles.startLifetime;
            newParticle.startSize3D = Vector3.one;
            //newParticle.startColor = new Color(((float)Random.Range(0, 16)) / 16f, 1, 1, 0.75f); // ScorchParticles.startColor.a);
            newParticle.startColor = new Color(((float)Random.Range(0, 16)) / 15f, 1, 1, 0.75f);
            ScorchParticles.Emit(newParticle);
        }
    }

	public void DoFirePellet()
    {
        if (bPelletDocked) { return; }  //Make sure that we cannot fire another pellet
        //this'll have to be a co-routing to delay before the pellet is fired
        //But first animate the joints
        if (ourAudio && launchSounds.Count > 0)
        {
            ourAudio.PlayOneShot(launchSounds[Random.Range(0, launchSounds.Count)]);
        }
        PlayFireAnimations();
        StartCoroutine(firePellet());
    }

    public void PlayFireAnimations()
    {
        for (int i = 0; i < ArmAnimations.Count; i++)
        {
            ArmAnimations[i].Rewind();
            ArmAnimations[i].Play(ArmAnimations[i].clip.name);
        }
    }

    public void FirePellet()
    {
        if (ourPellet == null)
        {
            ourPellet = Instantiate(pelletPrefab, launchPoint.transform.position, Quaternion.identity) as GameObject;
            ourPellet.name = pelletPrefab.name;
            ourPellet.name += Random.Range(0, 1234); //Make this into an original name for our clone handling
        }
        ourPellet.SetActive(true);
        ourPellet.transform.position = launchPoint.transform.position;

        PelletProjectile newProjectile = ourPellet.GetComponent<PelletProjectile>();
        newProjectile.ourPelletLauncher = this;
        newProjectile.setMoveDir(launchPoint.transform.up);
    }

    public void PelletDied()
    {
        if (bPelletDocked) { return; }  //Make sure that we cannot fire another pellet
        StartCoroutine(RespawnPellet());
        //ourPellet.SetActive(false);
        //ourPellet.GetComponent<Collider>().enabled = false;
    }

    IEnumerator RespawnPellet()
    {
        yield return new WaitForSeconds(3);
        PlayFireAnimations();
        yield return new WaitForSeconds(0.5f);
        FirePellet();
    }

    public void PelletDocked()
    {
        bPelletDocked = true;
    }

    IEnumerator firePellet()
    {
        yield return new WaitForSeconds(pelletFireDelay);
        FirePellet();
    }
}
