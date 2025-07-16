using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SequenceEvent
{
	public enum enNodeType {NULL, DIALOGUE, WAIT, EVENT}
	public enNodeType NodeType = enNodeType.DIALOGUE;

	public UnityEvent TriggerEvents;  //This can't be public...
	public AudioClip AudioLine;
    public string Subtitle = "";
	public float PauseDuration = 0f;
}

public class DialogueSequencer : MonoBehaviour {
	public List<SequenceEvent> SequenceEvents;

	bool bSequenceActive = false;
	AudioSource ourAudio;
	int currentNode = 0;

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
				PlayNode(SequenceEvents[0]);
			}
		}
    }

	void PlayNextNode()
    {
		if (currentNode < SequenceEvents.Count)
        {
			PlayNode(SequenceEvents[currentNode]);
        }
    }

	void PlayNode(SequenceEvent thisEvent)
    {
		StartCoroutine(doPlayNode(thisEvent));
		currentNode++;
    }

	IEnumerator doPlayNode(SequenceEvent thisEvent)
    {
		switch (thisEvent.NodeType)
        {
			
			case SequenceEvent.enNodeType.DIALOGUE:
				goto default;
				//Remember to put up our dialogue line
				break;
			case SequenceEvent.enNodeType.WAIT:
				Debug.Log("Waiting: " + thisEvent.PauseDuration);
				yield return new WaitForSeconds(thisEvent.PauseDuration);
				break;
			case SequenceEvent.enNodeType.EVENT:
				goto default;
				break;
			default:
				thisEvent.TriggerEvents.Invoke();

				if (thisEvent.AudioLine)
                {
					ourAudio.PlayOneShot(thisEvent.AudioLine);
					Debug.Log("Playing Audio: " + thisEvent.AudioLine.name);
					yield return new WaitForSeconds(thisEvent.AudioLine.length);
				}
				break;
        }

		yield return new WaitForSeconds(0.5f);  //A small pause at the end of this
		PlayNextNode();
    }
}
