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

                //Check and see if our player is standing on this button, or if it has a cube for weight (as should be expected)
                bool bPlayerOn = false;
                
                for (int i=0; i<objectsOn.Count; i++)
                {
                    if (objectsOn[i].name.ToLower().Contains("player")) {
                        bPlayerOn = true;
                    }
                }
                if (!bPlayerOn)
                {
                    OnButtonTriggeredByObject.Invoke(true);
                }
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

}
