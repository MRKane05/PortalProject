using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class ReticuleHandler : MonoBehaviour {
    public PortalIndicator leftIndicator, rightIndicator;   //Our different portal indicators
    public CanvasGroup IndicatorsCanvasGroup;
    void Start()
    {
        //Set our initial indicator states
        switch (LevelController.Instance.playerControlType)
        {
            case LevelController.enPlayerControlType.NULL:
                leftIndicator.gameObject.SetActive(false);
                rightIndicator.gameObject.SetActive(false);
                IndicatorsCanvasGroup.alpha = 0;
                break;
            case LevelController.enPlayerControlType.NONE:
                leftIndicator.gameObject.SetActive(false);
                rightIndicator.gameObject.SetActive(false);
                IndicatorsCanvasGroup.alpha = 0;
                break;
            case LevelController.enPlayerControlType.NOTYETLEFT:
                leftIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                rightIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                leftIndicator.gameObject.SetActive(false);
                rightIndicator.gameObject.SetActive(false);
                IndicatorsCanvasGroup.alpha = 0;
                break;
            case LevelController.enPlayerControlType.LEFTONLY:
                //The game makes both of these blue
                leftIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                rightIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                IndicatorsCanvasGroup.alpha = 1f;
                break;
            case LevelController.enPlayerControlType.NOTYETRIGHT:
                leftIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                rightIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                IndicatorsCanvasGroup.alpha = 1f;
                break;
            case LevelController.enPlayerControlType.FULL:
                goto default;
                break;
            default:
                leftIndicator.SetIconsColor(PortalSpawnerBase.LeftPortalColor);
                rightIndicator.SetIconsColor(PortalSpawnerBase.RightPortalColor);
                IndicatorsCanvasGroup.alpha = 1f;
                break;
        }
    }

    public void ChangeStartState(bool bRevealLeft)
    {
        if (bRevealLeft)
        {
            leftIndicator.gameObject.SetActive(true);
            rightIndicator.gameObject.SetActive(true);
            //Lets do a really nice reveal here
            Sequence mySequence = DOTween.Sequence();
            mySequence.AppendInterval(1f);  //Small pause to do the reveal animation before we fade in our indicator
            mySequence.Append(IndicatorsCanvasGroup.DOFade(1f, 0.5f));
        } else
        {
            rightIndicator.SetIconsColor(PortalSpawnerBase.RightPortalColor);
        }
    }

    public void SetValidSurface(bool bSurfaceValid)
    {
        if (LevelController.Instance.playerControlType != LevelController.enPlayerControlType.NONE)
        {
            IndicatorsCanvasGroup.alpha = bSurfaceValid ? 1f : 0.5f;
        }
    }

    public void PortalGunFired(bool bWasLeft)
    {
        if (bWasLeft)
        {
            leftIndicator.SetFilledState(true);
        } else
        {
            rightIndicator.SetFilledState(true);
        }

        leftIndicator.SetLastFired(bWasLeft);
        rightIndicator.SetLastFired(bWasLeft);
    }

    public void PortalsRemoved()
    {
        leftIndicator.SetFilledState(false);
        rightIndicator.SetFilledState(false);
    }
}
