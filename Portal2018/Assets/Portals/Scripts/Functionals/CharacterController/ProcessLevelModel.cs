using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProcessLevelModel : MonoBehaviour {
	public bool bDoModelProcess = false;
	// Use this for initialization

	void Update()
    {
        if (bDoModelProcess)
        {
            bDoModelProcess = false;
            ProcessLevelByName();
        }
    }

	void ProcessLevelByName()
    {
        //our base shouldn't have any model on it
        ProcessTransform(transform);
    }

    void ProcessTransform(Transform thisTrans)
    {
        Debug.Log(thisTrans.gameObject.name);
        
        //Handle our layers
        if (thisTrans.gameObject.name.ToLower().Contains("noportal"))
        {
            thisTrans.gameObject.layer = LayerMask.NameToLayer("BackfacePortal");
        } else
        {
            thisTrans.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        //Check if something should have collisions off
        if (thisTrans.gameObject.name.ToLower().Contains("info_overlay"))
        {
            Collider transCollider = thisTrans.gameObject.GetComponent<Collider>();
            if (transCollider)
            {
                transCollider.enabled = false;
            }
        }

        if (thisTrans.gameObject.name.ToLower().Contains("glass"))
        {
            MeshRenderer thisRenderer = thisTrans.gameObject.GetComponent<MeshRenderer>();
            if (thisRenderer)
            {
                thisRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        //Should do a check for materials that are still the standard shader also
        
        if (thisTrans.childCount >0)
        {
            foreach (Transform child in thisTrans)
            {
                ProcessTransform(child);
            }
        }
    }
}
