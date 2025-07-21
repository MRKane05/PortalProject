using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//This script does things like handling the loading of the levels and feeding through the necessary information
public class GameLevelHandler : MonoBehaviour {
	private static GameLevelHandler instance = null;
	public static GameLevelHandler Instance { get { return instance; } }

	bool bSceneLoading = false;

	void Awake()
	{
		if (instance)
		{
			//Debug.Log("Duplicate attempt to create SaveUtility");
			//Debug.Log(gameObject.name);
			Destroy(this);  //This might get mopped up by the game manager, but that doesn't matter
			return; //cancel this
		}

		instance = this;
	}

	public void StartNewGame()
    {
		if (bSceneLoading) { return; }
		//Set the player prefs checkpoints with data that's blank
		//Load the first level and set everything in order
		LoadLevel("test_chamber_00-01", "");
    }

	public void ContinueGame()
    {
		if (bSceneLoading) { return; }
		//Pull the data from our player prefs
		//Load the necessary level
		//Set the player to the correct checkpoint after the level has loaded
		//Fade in
		//Do a "system release" of sorts to allow play to resume
		string continueLevel = PlayerPrefs.GetString("CheckpointLevel");
		string checkpointObject = PlayerPrefs.GetString("CheckpointObject");

		Debug.Log("TargetLevel: " + continueLevel);
		Debug.Log("TargetCheckpoint: " + checkpointObject);

		//Quick check that there's nothing faulty here
		if (continueLevel.Length > 3)
        {
			StartCoroutine(LoadLevel(continueLevel, checkpointObject));
        } else
        {
			StartNewGame();
        }
    }

	public void LoadTargetChamber(string targetLevel, string targetCheckpoint)
    {
		if (bSceneLoading) { return; }
		StartCoroutine(LoadLevel(targetLevel, targetCheckpoint));
    }

	IEnumerator LoadLevel(string targetLevel, string targetCheckpoint)
	{
		bSceneLoading = true;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetLevel);

		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
		{
			yield return null;
		}

		yield return null;  //Give everything a beat
		if (targetCheckpoint.Length > 3)
		{
			while (!LevelController.Instance)
			{
				yield return null;
			}

			LevelController.Instance.PositionPlayerOnCheckpoint(targetCheckpoint);
			/*
			if (LevelController.Instance.entryElevatorSystem != null)
			{
				LevelController.Instance.entryElevatorSystem.SetPlayerElevatorStart();

			} else
            {
				//So now our levelcontroller should be up and running
				LevelController.Instance.PositionPlayerOnCheckpoint(targetCheckpoint);
				
			}*/
		}
		bSceneLoading = false;
	}
}
