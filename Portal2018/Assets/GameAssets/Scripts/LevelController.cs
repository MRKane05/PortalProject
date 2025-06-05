using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {
	private static LevelController instance = null;
	public static LevelController Instance { get { return instance; } }

	public GameObject playerObject;	//Will no doubt be referenced by lots of things

	void Awake()
    {
		if (instance)
		{
			Debug.Log("Runtime Error: More than one LevelController instance present");
		}

		instance = this;	//This should be the correct one. We won't be doing additive or seamless loads
	}

	// Update is called once per frame
	void Update () {
		
	}
}
