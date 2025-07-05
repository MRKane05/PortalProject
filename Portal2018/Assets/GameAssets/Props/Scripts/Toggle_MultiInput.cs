using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ExpectedToggleObjects
{
	public GameObject targetObject;
	public bool bState = false;
}

//A toggle that's designed to take in multiple inputs and output a final state once the bools have been combined
public class Toggle_MultiInput : MonoBehaviour {

	public BoolUnityEvent OnButtonTriggered;  //This can't be public...
	public List<ExpectedToggleObjects> expectedToggleObjects;

	public void bObjectChangeState(GameObject thisObject, bool bNewState) //, bool bNewState)
    {

		bool bOpen = true;

		foreach (ExpectedToggleObjects thisToggle in expectedToggleObjects)
        {
			if (thisToggle.targetObject == thisObject)	//Set our new object state
            {
				thisToggle.bState = bNewState;
            }

			if (!thisToggle.bState)	//This is our global list check to see if we've got two things down
            {
				bOpen = false;
            }
        }

		OnButtonTriggered.Invoke(bOpen);
    }
}
