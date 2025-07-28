using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterWaterVolume : MonoBehaviour {
    public Color waterFogColor;
    public float waterFogDensity = 0.2f;

    public void PlayerEnteredWaterVolume()
    {
        /*
        //Turn fog on
        RenderSettings.fog = true;
        //Set fog colour and details
        RenderSettings.fogColor = waterFogColor;
        RenderSettings.fogDensity = waterFogDensity;
        */
        //Turning fog on creats a ton of nightmares for my currently optimised render pipeline
        HUDManager.Instance.SetPlayerDrowned();
        //Lets just kill the player
        PlayerHealth playerHealth = LevelController.Instance.playerObject.GetComponent<PlayerHealth>();
        if (playerHealth)
        {
            playerHealth.TakeDamage(110f, "Drown");	//Kill our player
            //Play the sound of the portal gun going off with an energy burst, and have an effect
        }
    }

    //Of course we're going to need a handler for anything else that the player throws in
}
