using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour {
	private static HUDManager instance = null;
	public static HUDManager Instance { get { return instance; } }

	[Header("Game Messages")]
	public CanvasGroup MessageGroup;
	public TextMeshProUGUI Message;
	bool bMessageDisplaying = false;

	void Awake()
	{
		if (instance)
		{
			Debug.LogError("Duplicate HUDManager found. There has been a serious oversight that urgently needs fixed");	//Of course our clone will make this...
			Destroy(this);  //This might get mopped up by the game manager, but that doesn't matter
			return; //cancel this
		}

		instance = this;
	}

	public void DisplayMessage(string message)
    {
		if (bMessageDisplaying) { return; }	//Hopefully this won't be likely to happen

		Message.text = message;

		MessageGroup.alpha = 1f;
		Sequence mySequence = DOTween.Sequence();
		mySequence.AppendInterval(2f);
		mySequence.Append(MessageGroup.DOFade(0f, 1f).OnComplete(() => bMessageDisplaying = false));
	}
}
