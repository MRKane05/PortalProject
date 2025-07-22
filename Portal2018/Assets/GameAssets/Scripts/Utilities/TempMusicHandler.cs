using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TempMusicHandler : MonoBehaviour {
	private static TempMusicHandler instance = null;
	public static TempMusicHandler Instance { get { return instance; } }

	void Awake()
	{
		if (instance)
		{
			DestroyImmediate(gameObject);   //If we're playing music we don't want to keep playing music
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
	}

}
