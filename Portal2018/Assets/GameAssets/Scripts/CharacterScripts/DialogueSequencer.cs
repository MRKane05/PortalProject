using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using DG.Tweening;

[System.Serializable]
public class SequenceEvent
{
	//public enum enNodeType {NULL, DIALOGUE, WAIT, EVENT}	//We really don't need this
	//public enNodeType NodeType = enNodeType.DIALOGUE;

	public UnityEvent TriggerEvents;  //This can't be public...
	public AudioClip AudioLine;
    public string Subtitle = "";
	public float PauseDuration = 0f;
}

public class DialogueSequencer : MonoBehaviour {
	public enum enSequenceType { NULL, DIALOGUE, EVENTS }
	public enSequenceType SequenceType = enSequenceType.DIALOGUE;

	public AudioMixer AudioMixerGroup;
	public float EventIntermission = 0.5f;//How long between each event
	public List<SequenceEvent> SequenceEvents;

	float voiceLineDropVolume = -10;
	bool bDoingVolumeDown = false;

	bool bSequenceActive = false;
	AudioSource ourAudio;
	int currentNode = 0;

	bool bKillSequence = false;

	void Awake()
    {
		ourAudio = gameObject.GetComponent<AudioSource>();
    }

	public void TriggerSequence()
    {
		if (!bSequenceActive)
		{
			bSequenceActive = true;
			if (SequenceEvents.Count > 0)
			{
				if (!LevelController.Instance.bDialoguePlaying && SequenceType == enSequenceType.DIALOGUE)
				{
					PlayNode(SequenceEvents[0]);
				} else
                {
					StartCoroutine(WaitForDialogueFree());
                }
			}
		}
    }

	IEnumerator WaitForDialogueFree()
    {
		while (LevelController.Instance.bDialoguePlaying)
        {
			yield return null;
        }
		PlayNode(SequenceEvents[0]);
	}

	void PlayNextNode()
    {
		if (currentNode < SequenceEvents.Count && !bKillSequence)
		{
			PlayNode(SequenceEvents[currentNode]);
		}
		else
        {
			if (SequenceType == enSequenceType.DIALOGUE)
			{
				AudioMixerGroup.DOSetFloat("GameVolume", 0f, 1f);   //Restore the backing game volume
				LevelController.Instance.bDialoguePlaying = false;
            }
            else
            {
				//Reset our sequence because Dialogue plays only once, but sequences can play upon triggering
				currentNode = 0;
				bSequenceActive = false;

			}
		}
    }

	void PlayNode(SequenceEvent thisEvent)
    {
		if (SequenceType == enSequenceType.DIALOGUE)
		{
			LevelController.Instance.bDialoguePlaying = true;
		}
		StartCoroutine(doPlayNode(thisEvent));
		currentNode++;
    }

	public void CancelWhenAudioComplete()
    {
		bKillSequence = true;
    }

	IEnumerator doPlayNode(SequenceEvent thisEvent)
    {		
		thisEvent.TriggerEvents.Invoke();	//Invoke events

		//Do audio
		if (thisEvent.AudioLine)
        {
			if (SequenceType == enSequenceType.DIALOGUE)
			{
				AudioMixerGroup.DOSetFloat("GameVolume", voiceLineDropVolume, 0.5f); //Turn down the game volume for the GlaDOS lines
			}
			ourAudio.PlayOneShot(thisEvent.AudioLine);
		//Debug.Log("Playing Audio: " + thisEvent.AudioLine.name);
		yield return new WaitForSeconds(thisEvent.AudioLine.length);	//Don't kill a sequence while dialogue is playing
		}

		//Do Pause
		float waitStart = Time.time;
		while (Time.time < waitStart + thisEvent.PauseDuration)
		{
			yield return null;
			if (bKillSequence)
			{
				break;
			}
		}
		yield return new WaitForSeconds(EventIntermission);  //A small pause at the end of this
		PlayNextNode();
    }
}
