using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Load a list of chambers and set them according to what's unlocked etc.
public class ChamberSelectMenu : UIButtonFunction {
	public GameObject ChamberButton_Prefab;
	VerticalLayoutGroup baseLayoutGroup;
	List<GameObject> spawnedButtons = new List<GameObject>();

	// Use this for initialization
	IEnumerator Start () {
		//We need to wait for our other systems to start up
		for (int i = 0; i<2; i++)
        {
			yield return null;
        }
		PopulateChamberList();
	}

	void PopulateChamberList()
    {
		Debug.Log("Chamber Entries: " + GameDataHandler.Instance.LevelList.Count);
		for (int i=0; i<GameDataHandler.Instance.LevelList.Count; i++)
        {
			Debug.Log("Generating Button");
			GameObject newButton = Instantiate(ChamberButton_Prefab, transform);
			Debug.Log("Button: " + newButton);
			//TextMeshProUGUI buttonLabel = newButton.GetComponentInChildren<TextMeshProUGUI>();
			//buttonLabel.text = GameDataHandler.Instance.LevelList[i].name;
			ChamberSelectButton selectButton = newButton.GetComponent<ChamberSelectButton>();
			Debug.Log("Button Select: " + selectButton);
			selectButton.SetLevelLoadDetails(GameDataHandler.Instance.LevelList[i].name, GameDataHandler.Instance.LevelList[i].LoadedSceneName);// .SceneAsset.name);
		}
    }

	public void SelectChamber(string targetChamber)
	{
	}
}
