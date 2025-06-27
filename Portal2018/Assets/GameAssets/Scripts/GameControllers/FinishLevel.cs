using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevel : MonoBehaviour {
	public string nextLevel;
	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.name == "Player")
		{
			SceneManager.LoadScene(nextLevel);
		}
	}
}
