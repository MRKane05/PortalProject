using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorParenting : MonoBehaviour
{
	public GameObject optionalBase;

	void OnTriggerEnter(Collider collision)
	{
		if (collision.gameObject.name == "Player")
		{
			if (!optionalBase)
			{
				collision.gameObject.transform.SetParent(transform);
			} else
            {
				collision.gameObject.transform.SetParent(optionalBase.transform);
            }
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
