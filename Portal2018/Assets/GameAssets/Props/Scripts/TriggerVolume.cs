using UnityEngine;
using UnityEngine.Events;

//This calls an event when the player walks into the volume
public class TriggerVolume : MonoBehaviour {
    public UnityEvent TriggerEvent;  //This can't be public...
	public bool bTriggerOnlyOnce = false;
	int numTriggers = 0;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			DoTrigger();
		}
	}

	public virtual void DoTrigger()
    {
		numTriggers++;
		if (bTriggerOnlyOnce)
		{
			if (numTriggers <= 1)
				TriggerEvent.Invoke();
		}
		else
		{
			TriggerEvent.Invoke();
		}
	}
}
