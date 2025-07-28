using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightrailPlatform : MonoBehaviour {
	//So we could have the system assign the length of the laser beam
	public List<GameObject> railPoints;

	public enum enLightrailType { NULL, PINGPONG, RAIL }
	public enLightrailType LightrailType = enLightrailType.PINGPONG;
	public GameObject LiftObject;
	public bool bPlatformActive = false;
	int platformTarget = 0; //Which point are we currently moving towards?
	public float platformSpeed = 1.5f;
	public float endPauseTime = 3f;
	float pauseTime = 0f;
	[Space]
	[Header("Effect Settings")]
	public GameObject beamObject;
	public ParticleSystem[] glowParticleSystems;

	public void SetLightrailActive(bool bState)
    {
		bPlatformActive = bState;
		if (beamObject)
        {
			beamObject.SetActive(bState);
        }

		if (glowParticleSystems.Length > 0)
        {
			foreach (ParticleSystem pSys in glowParticleSystems)
			{
				if (bState)
				{
					pSys.Play();
				}
				else
				{
					pSys.Stop();
				}
			}
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
		if (!bPlatformActive) { return; }

		if (Time.time > pauseTime + endPauseTime)
		{
			LiftObject.transform.position = Vector3.MoveTowards(LiftObject.transform.position, railPoints[platformTarget].transform.position, platformSpeed * Time.fixedDeltaTime);
		}
		if ((Vector3.SqrMagnitude(railPoints[platformTarget].transform.position - LiftObject.transform.position) < 0.01f))
        {
			platformTarget++;
			platformTarget = (int)Mathf.Repeat(platformTarget, railPoints.Count);
			pauseTime = Time.time;
        }
	}
}
