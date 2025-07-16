using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPortalGovenor : PortalSpawnerBase {
    public enum enTriggerType { NULL, ONCE, EACHTIME }
    public enTriggerType TriggerType = enTriggerType.ONCE;

	public AutoportalBehavior BaseAutoportal; //The portal that won't change, this'll be our right-hand portal
	public List<AutoportalBehavior> CyclingPortals; //The portals that'll cycle
    int currentPortalIndex = 0;
	public float cycleTime = 4f; //How many seconds before we spawn the portal and move it somewhere else?

    bool bHasBeenTriggered = false;

    void Start()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        _leftPortal = SpawnPortal(Vector3.zero, Quaternion.identity, LeftPortalColor);
        _rightPortal = SpawnPortal(Vector3.zero, Quaternion.identity, RightPortalColor);

        _leftPortal.ExitPortal = _rightPortal;
        _rightPortal.ExitPortal = _leftPortal;

        _leftPortal.name = "Left AutoPortal";
        _rightPortal.name = "Right AutoPortal";

        _leftPortal.gameObject.SetActive(false);
        _rightPortal.gameObject.SetActive(false);
    }
  
    float cycleStartTime = 0;
    bool bCycleActive = false;

    public void TriggerActive(bool bIsActive)
    {
        if (bHasBeenTriggered && TriggerType == enTriggerType.ONCE && bIsActive)
        {
            return;
        }
        bHasBeenTriggered = true;

        bCycleActive = bIsActive;
        if (bIsActive)
        {
            cycleStartTime = Time.time;
            PlacePortal(false, BaseAutoportal.AutoPortalSpawnPoint.transform.position, BaseAutoportal.AutoPortalSpawnPoint.transform.forward);
            PlacePortal(true, CyclingPortals[0].AutoPortalSpawnPoint.transform.position, CyclingPortals[0].AutoPortalSpawnPoint.transform.forward);
        } else
        {
            //We need to close down our portals
            _leftPortal.gameObject.SetActive(false);
            _rightPortal.gameObject.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update () {
		if (Time.time > cycleStartTime + cycleTime && CyclingPortals.Count > 1 && bCycleActive)
        {
            cycleStartTime = Time.time;
            currentPortalIndex = (int)Mathf.Repeat(cycleTime++, CyclingPortals.Count);
            PlacePortal(true, CyclingPortals[currentPortalIndex].AutoPortalSpawnPoint.transform.position, CyclingPortals[currentPortalIndex].AutoPortalSpawnPoint.transform.forward);
        }
	}
}
