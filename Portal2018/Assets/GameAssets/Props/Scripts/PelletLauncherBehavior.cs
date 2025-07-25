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

    IEnumerator Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
        if (bCanFire)
        {
            yield return new WaitForSeconds(3f);
            DoFirePellet();
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
