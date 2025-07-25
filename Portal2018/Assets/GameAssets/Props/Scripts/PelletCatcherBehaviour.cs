using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletCatcherBehaviour : MonoBehaviour {
	public List<Animation> ArmAnimations = new List<Animation>();

    public GameObject projectionPlane;
    public GameObject holdPoint;
    public bool bHasPellet = false;
    public BoolUnityEvent OnCatcherTriggered;
    public GameObject CatcherGlowPoint;
    public GameObject CatcherRayLaunchPoint;
    public LayerMask GlowPointProjectionMask;
    

    IEnumerator Start()
    {
        while(LevelController.Instance == null && LevelController.Instance.playersPortalGun)
        {
            yield return null;
        }

        LevelController.Instance.playersPortalGun.GunFired.AddListener(DoProjectorSet);
        TryToSetProjector();
    }

    void DoProjectorSet(bool bWasLeft)
    {
        TryToSetProjector();
        StartCoroutine(ProjectorDelaySet());
    }

    IEnumerator ProjectorDelaySet()
    {
        yield return new WaitForSeconds(0.5f);
        TryToSetProjector();
    }

    void TryToSetProjector()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(CatcherRayLaunchPoint.transform.position, CatcherRayLaunchPoint.transform.TransformDirection(Vector3.forward), out hit, 200f, GlowPointProjectionMask))

        {
            Portal hitPortal = hit.collider.gameObject.GetComponent<Portal>();
            if (hitPortal)  //We need to raycast through a portal
            {
                //RaycastHit recursiveHit;
                //Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, int recursiveDepth = 0) {
                if (PortalPhysics.Raycast(CatcherRayLaunchPoint.transform.position, CatcherRayLaunchPoint.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, GlowPointProjectionMask, QueryTriggerInteraction.UseGlobal, 0))
                {
                    //We've hit something THROUGH the portals
                    PositionGlowSpot(hit.point, hit.normal, true);
                    //Check and see if we're hitting something that's an "object"
                    PelletLauncherBehavior hitPelletLauncher = hit.collider.gameObject.GetComponent<PelletLauncherBehavior>();
                    if (hitPelletLauncher)
                    {
                        PositionGlowSpot(hit.point, hit.normal, false); //Turn off our blip
                    }

                    Teleportable objectTeleportable = hit.collider.gameObject.GetComponent<Teleportable>();
                    if (objectTeleportable)
                    {
                        PositionGlowSpot(hit.point, hit.normal, false);
                    }
                }
            }
            else
            {
                PositionGlowSpot(hit.point, hit.normal, true);
            }
        }
        else
        {
            PositionGlowSpot(Vector3.zero, Vector3.zero, false);    //Turn our point off
        }
    }

    void PositionGlowSpot(Vector3 hitPoint, Vector3 hitNormal, bool bWasHit)
    {
        if (!bWasHit)
        {
            CatcherGlowPoint.SetActive(false); //turn this off
        } else {
            CatcherGlowPoint.SetActive(true);
            CatcherGlowPoint.transform.position = hitPoint + hitNormal * 0.1f; //So that we're just ahead of the point
            //CatcherGlowPoint.transform.LookAt(hitPoint + hitNormal * 3f);   //Get the transform to rotate to be against the surface
            //Alternatively:
            CatcherGlowPoint.transform.rotation =  Quaternion.FromToRotation(Vector3.up, hitNormal);
        }
    }

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

            pelletScript.ourPelletLauncher.PelletDocked();
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
