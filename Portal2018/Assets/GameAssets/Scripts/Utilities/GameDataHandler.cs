using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region SaveData
[System.Serializable]
public class ChamberState
{
	public string name = "";
	public bool bUnlocked = false;
}

[System.Serializable]
public class GameSave
{
	public List<ChamberState> Chambers;	//So this is a list of all the saves we've got pertaining to what's complete, and what isn't
}

#endregion

#region Level Data
[System.Serializable]
public class LevelItem
{
	public string name = "Chamber 00";
	//public Object SceneAsset;
	public string LoadedSceneName = "";
	public bool bUnlocked = false;
}
#endregion

public class GameDataHandler : MonoBehaviour
{
	//On top of containing the game data this will also have to contain a list of the levels and the start points for each chamber

	public List<LevelItem> LevelList;
	public GameSave GameSaveData;

	private static GameDataHandler instance = null;
	public static GameDataHandler Instance { get { return instance; } }
	void Awake()
	{
		if (instance)
		{
			Destroy(this);  //This might get mopped up by the game manager, but that doesn't matter
			return; //cancel this
		}

		instance = this;
	}

	void Start()
    {
		LoadGameSaveData();
		SyncroniseLevelListToSave();
    }

	public void LoadGameSaveData()
    {
		string saveFile = SaveUtility.Instance.LoadTXTFile("GameSave.json");
		if (saveFile.Length > 3)
        {
			GameSaveData = JsonUtility.FromJson<GameSave>(saveFile);
        }
    }

	public void SyncroniseLevelListToSave()
    {
		//So in theory this should mostly be aligned, unless I really screw up...
		for (int i=0; i<GameSaveData.Chambers.Count; i++)
        {
			for (int y=0; y<LevelList.Count; y++)
            {
				if (LevelList[y].name == GameSaveData.Chambers[i].name)
                {
					//PROBLEM: Need to expand the details contained within the level list stuff
					LevelList[y].bUnlocked = GameSaveData.Chambers[i].bUnlocked;
                }
            }
        }
    }

	public void UnlockedChamber(string ChamberName)
    {
		for (int i=0; i<LevelList.Count; i++)
        {
			if (LevelList[i].name == ChamberName)
			{
				if (!LevelList[i].bUnlocked)
				{
					LevelList[i].bUnlocked = true;
					//Need to modify our game save data and save
					SyncroniseSaveToLevelList();
				}
			}
        }
    }

	public void SyncroniseSaveToLevelList()
    {
		bool bNeedsToSave = false;
		for (int i = 0; i< LevelList.Count; i++)
        {
			//This should logically be sequential, but just in case lets make it so that it can handle any order
			bool bFoundEntry = false;
			for (int y= 0; y<GameSaveData.Chambers.Count; y++)
            {
				if (GameSaveData.Chambers[y].name == LevelList[i].name)
                {
					//PROBLEM: This will need refactored

					if (GameSaveData.Chambers[y].bUnlocked != LevelList[i].bUnlocked)
					{
						GameSaveData.Chambers[y].bUnlocked = LevelList[i].bUnlocked;
						bNeedsToSave = true;
					}
					bFoundEntry = true;
                }
            }
			if (!bFoundEntry)
            {
				ChamberState newChamber = new ChamberState();
				newChamber.name = LevelList[i].name;
				newChamber.bUnlocked = LevelList[i].bUnlocked;

				GameSaveData.Chambers.Add(newChamber);
				bNeedsToSave = true;
            }
        }

		if (bNeedsToSave)
        {
			SaveUtility.Instance.SaveTXTFile(JsonUtility.ToJson(GameSaveData), "GameSave.json");
        }
    }
}
