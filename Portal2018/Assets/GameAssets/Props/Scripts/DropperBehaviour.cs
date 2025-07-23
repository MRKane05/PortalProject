using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A basic behavior method for our cube dropper. This'll also have to include spawning/respawning a cube after it's been destroyed. For the moment lets start simple
public class DropperBehaviour : MonoBehaviour {
    public List<Animation> ArmAnimations = new List<Animation>();
    public GameObject cubeSpawnPoint;
    public GameObject cubePrefab;
    [Space]
    public bool bPrepareSecondCube = false;
    public GameObject preparedCube;
    public GameObject currentCube;

    bool bTriggerAvaliable = true;
    
    public void DoCubeAction()  //This'll need an action to pre-prepare extra cubes...
    {
        if (!bTriggerAvaliable)
        {
            return; //We can't do another trigger.
        } else
        {
            bTriggerAvaliable = false;
        }
        triggerCubeSpawn();
        currentCube = preparedCube; //Move our prepared cube into current position
        preparedCube = null; //Clear this off
        StartCoroutine(WaitOpenDoors(2f));
    }

    IEnumerator WaitOpenDoors(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        triggerAnimation(true);
        yield return new WaitForSeconds(10f);
        if (bPrepareSecondCube)
        {
            triggerCubeSpawn(); //Add another cube to our cache
        }
    }

    public void triggerCubeSpawn()  //Spawns into the prepared cube
    {
        if (preparedCube == null)
        {
            preparedCube = Instantiate(cubePrefab) as GameObject;
            preparedCube.name = "Dropper_Cube";
            //Debug.LogError("Prepared Cube Name: " + preparedCube.name);
            preparedCube.transform.position = cubeSpawnPoint.transform.position;
            Rigidbody cubeRB = preparedCube.GetComponent<Rigidbody>();
            float maxTorque = 3f;
            cubeRB.AddTorque(new Vector3(Random.Range(-maxTorque, maxTorque), Random.Range(-maxTorque, maxTorque), Random.Range(-maxTorque, maxTorque)));   //To make it more interesting
        }
    }

    public void triggerAnimation(bool bState)
    {
        if (currentCube)
        {
            currentCube.GetComponent<Rigidbody>().AddForce(Vector3.up);
        }
        if (bState)
        {
            for (int i=0; i<ArmAnimations.Count; i++)
            {
                ArmAnimations[i].Play();
            }
        }
    }
}
