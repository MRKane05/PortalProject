using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Dedicated class for playing music when triggered/set
public class MusicManager : MonoBehaviour {
    private static MusicManager instance = null;
    public static MusicManager Instance { get { return instance; } }

    public AudioSource ourAudio;

    void Awake()
    {
        if (instance)
        {
            Debug.Log("Runtime Error: More than one LevelController instance present");
        }

        instance = this;    //This should be the correct one. We won't be doing additive or seamless loads
        
    }

    public void playMusic(AudioClip thisAudio, bool bLooping)
    {
        if (ourAudio)
        {
            if (ourAudio.isPlaying) //Crossfade I assume
            {

            } else
            {
                ourAudio.clip = thisAudio;
                ourAudio.loop = bLooping;
                ourAudio.Play();
            }
        }
    }
}
