using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletCatcherBehaviour : MonoBehaviour {
	public List<Animation> ArmAnimations = new List<Animation>();

    public GameObject projectionPlane;

    public GameObject holdPoint;

    public bool bHasPellet = false;

    //This will be called by our pellet
    public void DockPellet(GameObject thisPellet, PelletProjectile pelletScript)
    {
        bHasPellet = true;
        thisPellet.transform.position = holdPoint.transform.position;
        //And play our animations/sound!
        for (int i = 0; i < ArmAnimations.Count; i++)
        {
            ArmAnimations[i].Rewind();
            ArmAnimations[i].Play(ArmAnimations[i].clip.name);
        }
    }
}
