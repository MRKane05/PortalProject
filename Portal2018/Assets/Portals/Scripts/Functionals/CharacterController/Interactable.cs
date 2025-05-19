using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public Portal visiblePortal; //If we're visible through a portal it's this one. It could be both.

    public LayerMask layer_world = 1 << 0;
    public LayerMask layer_portal = 1 << 9;


    public bool bVisibleToMainCamera()
    {
        //See if we've got a clear ray to boot
        RaycastHit hit;
        float rayDist = Vector3.SqrMagnitude(this.transform.position - Camera.main.transform.position);
        if (Physics.Raycast(Camera.main.transform.position, (this.transform.position - Camera.main.transform.position).normalized, out hit, rayDist, layer_world))
        {
            return true; //We can see the player directly
        }

        return false;
    }

    


}
