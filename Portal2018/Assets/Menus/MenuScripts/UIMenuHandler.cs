﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using static Mission_MapSection;

[System.Serializable]
public class loadedScene
{
    public Scene scene;
    public PanelHandler callingPanel;
    public GameObject callingButton;
    public bool IsValid()
    {
        if (scene != null)
        {
            return scene.IsValid();
        }
        return false;
    }
    public loadedScene(Scene newScene, PanelHandler newCallingPanel, GameObject newCallingButton)
    {
        scene = newScene;
        callingPanel = newCallingPanel;
        callingButton = newCallingButton;
    }
}


//This script handles menu elements, loading menus as scenes and the chatter/functionality between them
public class UIMenuHandler : MonoBehaviour
{
    public List<loadedScene> loadedScenes = new List<loadedScene>();
    private Scene lastLoadedScene;
    public PanelHandler baseMenu;    //This will be our fallback reference menu
    private static UIMenuHandler instance = null;
    public static UIMenuHandler Instance { get { return instance; } }

    public string cachedReturnPanel_Scene = "";
    public string cachedReturnPanel_Button = "";

    void Awake()
    {
        if (instance)
        {
            //Debug.Log("Somehow there's a duplicate UIMenuHandler in the scene");
            //Debug.Log(gameObject.name);
            DestroyImmediate(gameObject);    //Remove ourselves from the scene
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    //This is called by a menu that's marked as "Base" to set itself as the menu that the entire system can return back to
    public void AssertMenuAsBase(PanelHandler thisMenu)
    {
        //Debug.Log("Menu asserted as base: " + thisMenu.name);
        if (baseMenu) //we should do something about this...
        {

        }
        baseMenu = thisMenu;
    }

    public void OpenBaseScene(string targetSceneName, string targetButtonName)
    {
        //Kick off with the easy way first
        if (baseMenu)
        {
            baseMenu.gameObject.SetActive(true); //Just turn this back on again
            baseMenu.DoEnable(targetButtonName);
        }
    }

    //Function to unload menus that have been loaded into the scene. This'll be used with the menu close function
    public void UnloadMenu(GameObject targetMenu)
    {
        Scene menuScene = targetMenu.scene;
        List<int> removeEntries = new List<int>();
        if (menuScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(menuScene.name);
        }
        else
        {
            Debug.LogError("Menu scene is not loaded, cannot unload.");
        }
    }

    public void TransitionLoadScene(string sceneName)
    {
        //there'll be no menu stuff here, it's simply doing a load
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // Function to load a scene additively and store its reference
    public void LoadMenuSceneAdditively(string sceneName, PanelHandler caller, GameObject callingButton)
    {
        StartCoroutine(LoadMenuSceneAsync(sceneName, caller, callingButton));
    }

    private IEnumerator LoadMenuSceneAsync(string sceneName, PanelHandler caller, GameObject callingButton)
    {
        // Begin loading the scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the scene loading is complete
        while (!asyncLoad.isDone)
        {
            yield return null;
        }


        // Once loaded, store the scene reference
        lastLoadedScene = SceneManager.GetSceneByName(sceneName);
        loadedScene newScene = new loadedScene(lastLoadedScene, caller, callingButton);

        //Get our panel information and pass on bits and pieces as necessary
        GameObject[] sceneGameObjects = lastLoadedScene.GetRootGameObjects();   //Our PanelHandler will be on one of these
        foreach (GameObject thisObject in sceneGameObjects)
        {
            PanelHandler attachedPanelHandler = thisObject.GetComponent<PanelHandler>();
            if (attachedPanelHandler)
            {
                if (attachedPanelHandler.returnPanelType == PanelHandler.enInteractionType.CACHED)  //We want to callback to the panel that loaded us
                {
                    //This only works once...
                    if (caller)
                    {
                        cachedReturnPanel_Scene = caller.gameObject.scene.name;
                    }

                    if (callingButton)
                    {
                        cachedReturnPanel_Scene = callingButton.scene.name;
                        cachedReturnPanel_Button = callingButton.name;
                    }
                }
            }
        }

        //PROBLEM: We're getting a flash here while switching menus. I'm not sure how to resolve this within the limitations of Unity and what it's giving me

        if (caller != null)
        {
            caller.LoadMenuSceneCallback(newScene, true);
        }
    }
}