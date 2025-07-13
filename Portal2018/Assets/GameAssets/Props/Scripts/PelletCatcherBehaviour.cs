using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletCatcherBehaviour : MonoBehaviour {
	public List<Animation> ArmAnimations = new List<Animation>();

    public GameObject projectionPlane;
    public GameObject holdPoint;
    public bool bHasPellet = false;
    public BoolUnityEvent OnCatcherTriggered;

    //This will be called by our pellet
    public void DockPellet(GameObject thisPellet, PelletProjectile pelletScript)
    {
        bHasPellet = true;
        if (thisPellet.name.Contains("(Clone)"))
        {   //We don't want to grab the clone, but instead grab the original
            string objectName = thisPellet.name.Replace("(Clone)", "");
            //Destroy(thisPellet);
            //PROBLEM: Need to remove the pellet clone from the pool, not that it should matter given the low occurance of this
            thisPellet = GameObject.Find(objectName);
            //rigidbody = obj.GetComponent<Rigidbody>();
            //Debug.LogError("Opted to get original instead of Clone");
        }

        if (thisPellet)
        {
            thisPellet.transform.position = holdPoint.transform.position;
            //And play our animations/sound!
            for (int i = 0; i < ArmAnimations.Count; i++)
            {
                ArmAnimations[i].Rewind();
                ArmAnimations[i].Play(ArmAnimations[i].clip.name);
            }
            //Should there be a trigger delay?
            StartCoroutine(eventTriggerDelay());
        }
    }

    IEnumerator eventTriggerDelay()
    {
        yield return new WaitForSeconds(1.5f);
        //Play affirmative sound
        OnCatcherTriggered.Invoke(true);
    }
}
