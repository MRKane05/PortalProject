using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorParenting : MonoBehaviour
{

	void OnTriggerEnter(Collider collision)
	{
		if (collision.gameObject.name == "Player")
		{
			collision.gameObject.transform.SetParent(transform);
		}
	}

	void OnTriggerExit(Collider collision)
	{
		if (collision.gameObject.name == "Player")
		{
			collision.gameObject.transform.SetParent(null);
		}
	}
}
