using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LevelController : MonoBehaviour {
	private static LevelController instance = null;
	public static LevelController Instance { get { return instance; } }

	public enum enPlayerControlType { NULL, NONE, LEFTONLY, FULL, NOTYETLEFT, NOTYETRIGHT }
	public enPlayerControlType playerControlType = enPlayerControlType.FULL;

	public GameObject playerObject; //Will no doubt be referenced by lots of things
	public ReticuleHandler reticuleHandler;	//Also referenced by a lot of activity
	[HideInInspector]
	public SpawnPortalOnClick playersPortalGun;

	[Space]
	[Header("Settings for Portal Cameras")]
	public float PortalCameraDistance = 25f;
	public LayerMask PortalCameraLayerMask;// = LayerMask.NameToLayer("Default");

	public ElevatorHandler entryElevatorSystem;

	void Awake()
    {
		if (instance)
		{
			Debug.Log("Runtime Error: More than one LevelController instance present");
		}

		instance = this;    //This should be the correct one. We won't be doing additive or seamless loads
		playersPortalGun = playerObject.GetComponentInChildren<SpawnPortalOnClick>();

		//Remove our TempMusicHandler if there is one
		if (TempMusicHandler.Instance)
        {
			RemoveTempMusicHandler();
        }
	}

	public void RemoveTempMusicHandler()
    {
		//Fade out and destroy our music player
		Sequence mySequence = DOTween.Sequence();
		mySequence.Append(TempMusicHandler.Instance.GetComponent<AudioSource>().DOFade(0f, 0.5f).OnComplete(() => { Destroy(TempMusicHandler.Instance.gameObject); } ));
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

	public void PositionPlayerOnTransform(Transform thisTransform)
    {
		playerObject.transform.position = thisTransform.position;
		RigidbodyCharacterController playerCharacter = playerObject.GetComponent<RigidbodyCharacterController>();
		playerCharacter.SetHeadRotation(thisTransform.eulerAngles.y);
	}

	public void PositionPlayerOnCheckpoint(string checkpointName)
    {
		Checkpoint[] Checkpoints = Object.FindObjectsOfType<Checkpoint>();
		for (int i=0; i<Checkpoints.Length; i++)
        {
			if (Checkpoints[i].name == checkpointName)
            {
				if (Checkpoints[i].entryElevatorSystem != null)
				{
					Checkpoints[i].entryElevatorSystem.SetPlayerElevatorStart();
				}
				else
				{
					Transform targetTrans = Checkpoints[i].transform;
					//Set our player position to this point
					playerObject.transform.position = targetTrans.position;
					RigidbodyCharacterController playerCharacter = playerObject.GetComponent<RigidbodyCharacterController>();
					playerCharacter.SetHeadRotation(targetTrans.eulerAngles.y);
					//playerObject.transform.eulerAngles = targetTrans.eulerAngles;
				}
				Checkpoints[i].SetupPortalGun();
				break;
            }
        }
	}

	void Update()
    {
		//Handle our pause menu
		if ((Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("Start")))
		{
			if (UIMenuHandler.Instance)
			{
				Debug.Log("Loading Pause Menu");
				Time.timeScale = 0.00001f;  //Set our pause timescale. I'm not sure if this is effective elsewhere
				UIMenuHandler.Instance.LoadMenuSceneAdditively("Menu_Pause", null, null);
			}
		}
	}
}
