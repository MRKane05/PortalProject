﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {
	private static LevelController instance = null;
	public static LevelController Instance { get { return instance; } }

	public enum enPlayerControlType { NULL, NONE, LEFTONLY, FULL, NOTYETLEFT, NOTYETRIGHT }
	public enPlayerControlType playerControlType = enPlayerControlType.FULL;

	public GameObject playerObject; //Will no doubt be referenced by lots of things
	[HideInInspector]
	public SpawnPortalOnClick playersPortalGun;
	public List<string> validPortalMaterialTags = new List<string>();   //Not sure if this should simply be on the gun itself

	[Space]
	[Header("Settings for Portal Cameras")]
	public float PortalCameraDistance = 25f;
	public LayerMask PortalCameraLayerMask;// = LayerMask.NameToLayer("Default");

	void Awake()
    {
		if (instance)
		{
			Debug.Log("Runtime Error: More than one LevelController instance present");
		}

		instance = this;    //This should be the correct one. We won't be doing additive or seamless loads
		playersPortalGun = playerObject.GetComponentInChildren<SpawnPortalOnClick>();
	}

	public void playerCollectPortalGun()	//An internal switch to allow player firing states
    {
		if (playerControlType == enPlayerControlType.NOTYETLEFT)
        {
			playerControlType = enPlayerControlType.LEFTONLY;
        } else if (playerControlType == enPlayerControlType.NOTYETRIGHT)
        {
			playerControlType = enPlayerControlType.FULL;
        }
    }
}
