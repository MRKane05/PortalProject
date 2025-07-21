using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalIndicator : MonoBehaviour {
    public Image Empty, Full;
    public Image FiredIcon;
    public bool bIsLeft = true;

    void Start()
    {
        //Disable our core icons for the start
        SetLastFired(!bIsLeft);
        SetFilledState(false);
    }

    public void SetIconsColor(Color newColor)
    {
        Empty.color = newColor;
        Full.color = newColor;
        FiredIcon.color = newColor;
    }

    public void SetLastFired(bool bWasLeft)
    {
        FiredIcon.gameObject.SetActive(bIsLeft == bWasLeft);
    }

    public void SetFilledState(bool bIsFilled)
    {
        Empty.gameObject.SetActive(!bIsFilled);
        Full.gameObject.SetActive(bIsFilled);
    }
}
