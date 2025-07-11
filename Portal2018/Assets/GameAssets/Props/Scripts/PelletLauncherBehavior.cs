using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletLauncherBehavior : MonoBehaviour {
	public List<Animation> ArmAnimations = new List<Animation>();
    float pelletFireDelay = 0.5f;
    public GameObject pelletPrefab; //Might have to be a rigidbody, I'm not sure
    GameObject ourPellet;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        DoFirePellet();
    }

	public void DoFirePellet()
    {
        //this'll have to be a co-routing to delay before the pellet is fired
        //But first animate the joints
        for (int i = 0; i < ArmAnimations.Count; i++)
        {
            ArmAnimations[i].Play();
        }
        StartCoroutine(firePellet());
    }

    public void FirePellet()
    {
        if (ourPellet == null)
        {
            ourPellet = Instantiate(pelletPrefab, transform.position, Quaternion.identity) as GameObject;
        }
        PelletProjectile newProjectile = ourPellet.GetComponent<PelletProjectile>();
        newProjectile.transform.position = gameObject.transform.position;
        newProjectile.setMoveDir(transform.up);
        //Set the pellet script pointing forward
        //Set the pellet timer
    }

    IEnumerator firePellet()
    {
        yield return new WaitForSeconds(pelletFireDelay);
        FirePellet();
    }
}
