using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectImpactSounds : MonoBehaviour {
	AudioSource ourAudio;
    public List<AudioClip> bumpSounds;
    public bool bSilentFirstHit = true;

    Rigidbody rb;
    Vector3 maxAngularVelocity = Vector3.zero;

	void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
		ourAudio = gameObject.GetComponent<AudioSource>();
	}

    void OnCollisionEnter(Collision collision)
    {
        if (ourAudio && bumpSounds.Count > 0)
        {
            if (bSilentFirstHit)
            {
                bSilentFirstHit = false;
                return;
            }
            ourAudio.PlayOneShot(bumpSounds[Random.Range(0, bumpSounds.Count)]);
        }
        //Annul our angular velocity as we're only checking that for a "slap"
        maxAngularVelocity = Vector3.zero;
    }
}
