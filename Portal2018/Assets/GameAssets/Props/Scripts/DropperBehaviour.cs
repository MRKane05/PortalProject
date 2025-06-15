using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A basic behavior method for our cube dropper. This'll also have to include spawning/respawning a cube after it's been destroyed. For the moment lets start simple
public class DropperBehaviour : MonoBehaviour {
    public List<Animation> ArmAnimations = new List<Animation>();

    public void triggerAnimation(bool bState)
    {
        if (bState)
        {
            for (int i=0; i<ArmAnimations.Count; i++)
            {
                ArmAnimations[i].Play();
            }
        }
    }
}
