using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

//In theory we'll only be saving on the map screen so won't need to know about this anywhere else. We're going to make our luck with this one!
//This'll become one of those PSVita Boilerplate classes I'd say, so it pays to keep it tidy!
public class SaveUtility : MonoBehaviour {
	private static SaveUtility instance = null;
	public static SaveUtility Instance { get { return instance; } }

	string path = "ux0:data/AptrRecon";

	void Awake()
	{
		if (instance)
		{
			//Debug.Log("Duplicate attempt to create SaveUtility");
			//Debug.Log(gameObject.name);
			Destroy(this);	//This might get mopped up by the game manager, but that doesn't matter
			return; //cancel this
		}

		instance = this;

		//Put this here so it can be ahead of the load call
#if UNITY_EDITOR
		path = Application.persistentDataPath;
		Debug.Log("Persistent Path: " + path);
#endif
		Debug.Log("Path: " + path);
	}

	public void Start()
	{
		if (!Directory.Exists(path))	//Ensure we are able to save
		{
			Directory.CreateDirectory(path);
			Debug.Log("Creating path for save: " + path);
		} else
        {
			Debug.Log("Found path for save: " + path);
		}
	}

	public void SaveTXTFile(string SaveData, string FileName)
	{
		StreamWriter writer = new StreamWriter(path + "/" + FileName);
		writer.AutoFlush = true;
		writer.Write(SaveData);
		writer.Close();
		Debug.Log("File Saved: " + path + "/" + FileName);
	}

	public bool CheckSaveFile(string FileName)
	{
		//Check and see if we've got a saved map state
		if (!System.IO.File.Exists(path + "/" + FileName))
		{
			Debug.Log("Save File Not Found: " + path + "/" + FileName);
			return false;
		}
		return true;
	}

	public string LoadTXTFile(string Filename)
	{
		if (CheckSaveFile(Filename))
		{
			//Debug.Log("DataPath: " + Application.persistentDataPath + "/VHScores.json");
			Debug.Log("Save File Read Path: " + path + "/" + Filename);
			string fileData = File.ReadAllText(path + "/" + Filename);
			return fileData;
		}
		return "";
	}

	public string CalculateFileChecksum(string FileName, string algorithm = "MD5")
	{
		if (!File.Exists(path + "/" + FileName))
		{
			return null; // Or throw an exception
		}

		using (FileStream stream = File.OpenRead(path + "/" + FileName))
		{
			HashAlgorithm hashAlgorithm;
			switch (algorithm.ToUpper())
			{
				case "MD5":
					hashAlgorithm = MD5.Create();
					break;
				case "SHA256":
					hashAlgorithm = SHA256.Create();
					break;
				// Add more algorithms as needed
				default:
					throw new ArgumentException("Unsupported hashing algorithm.", algorithm);
			}

			byte[] hashBytes = hashAlgorithm.ComputeHash(stream);
			return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
		}
	}
}
