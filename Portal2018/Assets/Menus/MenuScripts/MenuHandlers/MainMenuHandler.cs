using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Just a simple handler to pass through information, but also control button text to be reflective of things as necessary
public class MainMenuHandler : MonoBehaviour {

    #region Button Controls
    public void doPlayContinue()
    {
        GameLevelHandler.Instance.ContinueGame();
    }

    public void doNewGame()
    {
        GameLevelHandler.Instance.StartNewGame();
    }
    #endregion
}
