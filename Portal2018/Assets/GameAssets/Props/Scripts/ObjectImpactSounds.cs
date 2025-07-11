using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectImpactSounds : MonoBehaviour {
	AudioSource ourAudio;
    public List<AudioClip> bumpSounds;

	void Start () {
		ourAudio = gameObject.GetComponent<AudioSource>();
	}

    void OnCollisionEnter(Collision collision)
    {
        if (ourAudio && bumpSounds.Count > 0)
        {
            ourAudio.PlayOneShot(bumpSounds[Random.Range(0, bumpSounds.Count)]);
        }
    }
}
