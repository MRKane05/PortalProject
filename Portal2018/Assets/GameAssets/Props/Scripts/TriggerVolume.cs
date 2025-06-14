using UnityEngine;
using UnityEngine.Events;

//This calls an event when the player walks into the volume
public class TriggerVolume : MonoBehaviour {
    public UnityEvent TriggerEvent;  //This can't be public...

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			TriggerEvent.Invoke();
		}
	}
}
