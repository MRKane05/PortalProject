using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BoolUnityEvent : UnityEvent<bool>
{
}

[System.Serializable]
public class BoolObjectUnityEvent : UnityEvent<GameObject, bool>
{
}


//Basic pressureplate button
public class WeightButton : MonoBehaviour
{
    public float triggerWeight = 7f;
    public List<GameObject> objectsOn = new List<GameObject>();

    bool bSufficientWeight = false;
    Vector3 buttonDepressed = new Vector3(0, -0.08f, 0);

    float lerpTime = 0f;
    float buttonLerpSpeed = 3f;

    public BoolUnityEvent OnButtonTriggered;  //This can't be public...
    public BoolObjectUnityEvent OnButtonTriggeredMulti;
    public BoolUnityEvent OnButtonTriggeredByObject;  //This can't be public...

    void OnCollisionEnter(Collision collision)
    {
        if (!objectsOn.Contains(collision.gameObject))
        {
            objectsOn.Add(collision.gameObject);
        }
        SumAllWeight();
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        if (objectsOn.Contains(collisionInfo.gameObject))
        {
            objectsOn.Remove(collisionInfo.gameObject);
        }

        SumAllWeight();

        //Do a little check to see if the player is stepping off the button and has left a box
        if (collisionInfo.gameObject.name.ToLower().Contains("player") && objectsOn.Count > 0 && bSufficientWeight)
        {
            CheckBoxOn();
        }
    }

    void SumAllWeight()
    {
        float totalWeight = 0;
        for (int i=0; i<objectsOn.Count; i++)
        {
            totalWeight += objectsOn[i].GetComponent<Rigidbody>().mass;
        }
        bSufficientWeight = totalWeight > triggerWeight;
    }

    bool bButtonTriggered = false;

    void Update()
    {
        if (bSufficientWeight && !Mathf.Approximately(lerpTime, 1f))
        {
            lerpTime = Mathf.Lerp(lerpTime, 1f, Time.deltaTime * buttonLerpSpeed);
            transform.localPosition = buttonDepressed * lerpTime;
            if (lerpTime > 0.5f && !bButtonTriggered)
            {
                bButtonTriggered = true;
                OnButtonTriggered.Invoke(bButtonTriggered);
                OnButtonTriggeredMulti.Invoke(gameObject, bButtonTriggered);

                CheckBoxOn();
            }
        }
        if (!bSufficientWeight && !Mathf.Approximately(lerpTime, 0f))
        {
            lerpTime = Mathf.Lerp(lerpTime, 0f, Time.deltaTime * buttonLerpSpeed);
            transform.localPosition = buttonDepressed * lerpTime;
            if (lerpTime < 0.5f && bButtonTriggered)
            {
                bButtonTriggered = false;
                OnButtonTriggered.Invoke(bButtonTriggered);
                OnButtonTriggeredMulti.Invoke(gameObject, bButtonTriggered);
            }
        }
    }

    void CheckBoxOn()
    {
        //Check and see if our player is standing on this button, or if it has a cube for weight (as should be expected)
        bool bPlayerOn = false;

        for (int i = 0; i < objectsOn.Count; i++)
        {
            //Check and see if the player is simply holding this object on the switch
            if (objectsOn[i].name.ToLower().Contains("player"))
            {
                bPlayerOn = true;
            }
        }
        //Don't care if it's a cube making contact
        for (int i = 0; i < objectsOn.Count; i++)
        {
            //Check and see if the player is simply holding this object on the switch
            if (objectsOn[i].name.ToLower().Contains("cube"))
            {
                bPlayerOn = false;
            }
        }
        if (!bPlayerOn)
        {
            //PROBLEM: Kind of need to check and see if the cube might fall off before we call this
            OnButtonTriggeredByObject.Invoke(true);
        }
    }
}
