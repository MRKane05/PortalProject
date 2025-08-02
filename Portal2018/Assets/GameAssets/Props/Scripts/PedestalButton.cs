using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestalButton : MonoBehaviour {
    public BoolUnityEvent OnButtonTriggered;  //This can't be public...
    float retriggerTime = 0;
    public Animation buttonAnimation;
    AudioSource ourAudio;
    public AudioClip buttonClick;
    private void Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
    }
    public void PlayerInteract()
    {
        if (Time.time > retriggerTime)
        {
            retriggerTime = Time.time + 1f;//So we cannot spam this button
            OnButtonTriggered.Invoke(true);
            buttonAnimation.Play();
            if (ourAudio)
            {
                ourAudio.PlayOneShot(buttonClick);
            }
        }
    }
}
