using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This could be assigned as a reference to a cube/object that could be destroyed
public class DestroyTrigger : MonoBehaviour {

	public void ObjectDestroyed()
    {
        OnObjectDestroyed();
    }

    public virtual void OnObjectDestroyed()
    {

    }
}
