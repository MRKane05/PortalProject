using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSpawnerBase : MonoBehaviour {
    protected static Color RightPortalColor = new Color(1f, 0.627f, 0);
    protected static Color LeftPortalColor = new Color(0, 0.627f, 1f);

    [SerializeField] protected GameObject _portalPrefab;
    [SerializeField] protected LayerMask _canHit = -1;
    [SerializeField] protected float _normalOffset = 0.05f;
    [SerializeField] protected float _portalSpawnTime = 0.25f;
    [SerializeField] protected AnimationCurve _portalSpawnCurve = AnimationCurves.Overshoot;
    [SerializeField] protected int _meshColliderIterationCount = 10;

    protected Portal _leftPortal;
    protected Portal _rightPortal;



    //PROBLEM: Really need to refactor the entire portal spawner base to make things more manageable
    public Portal SpawnPortal(Vector3 location, Quaternion rotation, Color color)
    {
        GameObject obj = Instantiate(_portalPrefab, location, rotation);
        Portal portal = obj.GetComponent<Portal>();
        portal.setPortalColor(color);
        portal.PortalColor = color;
        //While this is cool I'm 100% sure we're not going to have the FPS to pipe it
        ParticleSystem particles = portal.GetComponentInChildren<ParticleSystem>();
        if (particles)
        {
            ParticleSystem.MainModule main = particles.main;
            main.startColor = color;
        }
        return portal;
    }

    public void PlacePortal(bool bIsLeftPortal, Vector3 startPosition, Vector3 startDirection)
    {
        Color color = bIsLeftPortal ? LeftPortalColor : RightPortalColor;
        Ray ray = new Ray(startPosition, startDirection);
        RaycastHit hit;
        bool hitWall;

        /*
        if (_shootThroughPortals)
        {
            hitWall = PortalPhysics.Raycast(ray, out hit, Mathf.Infinity, _canHit, QueryTriggerInteraction.Collide);
        }
        else*/
        {
            int mask = _canHit | PortalPhysics.PortalLayerMask;
            hitWall = Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Collide);
        }

        if (hitWall)
        {
            Portal portal = hit.collider.GetComponent<Portal>();
            if (portal)
            {
                //For this implementation we don't much care about fancy effects
                //WavePortalOverTime(portal, hit.point, _portalWaveAmplitude, _portalWaveDuration);
            }
            else
            {
                bool spawnedPortal = TrySpawnPortal(bIsLeftPortal, hit, startDirection);
                /*
                if (!spawnedPortal)
                {
                    SpawnSplashParticles(hit.point, hit.normal, color);
                }*/
            }
        }

        // Spawn a bullet that will auto-destroy itself after it travels a certain distance
        /*
        if (_bulletPrefab)
            SpawnBullet(_bulletPrefab, _camera.transform.position + _camera.transform.forward * _bulletSpawnOffset, _camera.transform.forward, hit.distance, color);
        */
    }

    Quaternion CalculateRotation(Vector3 forward, Vector3 normal)
    {
        Vector3 forwardOnPlane = Vector3.Cross(-normal, Vector3.right);
        Vector3 projectedForward = forward - Vector3.Dot(forward, normal) * normal;
        Quaternion faceCamera = Quaternion.FromToRotation(forwardOnPlane, projectedForward);
        if (Mathf.Abs(normal.y) < 0.999f)
        {
            faceCamera = Quaternion.identity;
        }
        Quaternion alongNormal = Quaternion.LookRotation(-normal);
        Quaternion rotation = faceCamera * alongNormal;
        return rotation;
    }

    public float PortalScale = 0.1f;
    public bool TrySpawnPortal(bool bIsLeft, RaycastHit hit, Vector3 forwardDir)
    {
        Portal portal = bIsLeft ? _leftPortal : _rightPortal;

        // Calculate the portal's rotation based off the hit object's normal.
        // Portals on walls should be upright, portals on the ground can be rotated in any way.
        Quaternion rotation = CalculateRotation(forwardDir, hit.normal);

        // Set portal position and rotation. Need to do this before calling FindFit so we can get
        // the portal's corners in world space
        portal.transform.position = hit.point;
        portal.transform.rotation = rotation;

        // Make sure the portal can fit flushly on the object we've hit.
        // If it can fit, but it's hanging off the edge, push it in.
        // Otherwise, disable the portal.
        Vector3 newPosition;
        portal.gameObject.SetActive(false);
        if (FindFit(portal, hit.collider, out newPosition))
        {
            portal.transform.position = newPosition + hit.normal * _normalOffset;
            portal.IgnoredColliders = new Collider[] { hit.collider };
            portal.gameObject.SetActive(true);
            portal.PortalSetPosition(newPosition + hit.normal * _normalOffset, hit.normal);
            // Scale the portal's renderer up from 0 to 1 for a nice visual pop-in
            Renderer portalRenderer = portal.GetComponentInChildren<MeshRenderer>();
            //PROBLEM: Hack here to keep things scaled for a plane as oppoed to a generated surface
            SetScaleOverTime(portal, portalRenderer.transform, Vector3.zero, Vector3.one * PortalScale, _portalSpawnCurve, _portalSpawnTime);

            return true;
        }

        return false;
    }

    void SetScaleOverTime(Portal parentPortal, Transform t, Vector3 startSize, Vector3 endSize, AnimationCurve curve, float duratio)
    {
        StartCoroutine(ScaleOverTimeRoutine(parentPortal, t, startSize, endSize, curve, duratio));
    }

    IEnumerator ScaleOverTimeRoutine(Portal parentPortal, Transform t, Vector3 startSize, Vector3 endSize, AnimationCurve curve, float duration)
    {
        float elapsed = 0;
        LazyPortalCamera lazyPortal = parentPortal.GetComponent<LazyPortalCamera>();

        while (elapsed < duration)
        {
            if (lazyPortal)
            {
                lazyPortal.portalScale = Mathf.Clamp01(curve.Evaluate(elapsed / duration));
                lazyPortal.portalBase.localScale = Vector3.LerpUnclamped(startSize, endSize, curve.Evaluate(elapsed / duration));
            }
            else
            {
                float scaleT = curve.Evaluate(elapsed / duration);
                t.localScale = Vector3.LerpUnclamped(startSize, endSize, scaleT);
                yield return null;
                elapsed += Time.deltaTime;
                parentPortal.PortalRenderer.setPortalAlpha(scaleT);
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
        t.localScale = endSize;
    }

    bool FindFit(Portal portal, Collider collider, out Vector3 newPosition)
    {
        if (collider is BoxCollider)
        {
            return FindFitBoxCollider(portal, collider, out newPosition);
        }
        else if (collider is MeshCollider)
        {
            return FindFitMeshCollider(portal, collider, out newPosition);
        }
        else
        {
            newPosition = portal.transform.position;
            return false;
        }
    }

    bool FindFitMeshCollider(Portal portal, Collider collider, out Vector3 newPosition)
    {
        newPosition = portal.transform.position;

        MeshCollider meshCollider = collider as MeshCollider;
        if (!meshCollider)
        {
            return false;
        }
        Vector3 center = portal.transform.position;
        Vector3[] corners = portal.WorldSpaceCorners;
        Vector3 forward = portal.transform.forward;

        int numIterations = _meshColliderIterationCount;
        Vector3 offset;
        bool viablePositionExists = FindViableCoplanarRectOnCollider(collider, center, corners, forward, numIterations, out offset);
        if (!viablePositionExists)
        {
            return false;
        }

        newPosition = portal.transform.position + offset;
        return true;
    }

    bool FindViableCoplanarRectOnCollider(Collider collider, Vector3 center, Vector3[] corners, Vector3 forward, int iterations, out Vector3 offset)
    {
        bool[] hits = new bool[4];
        int numHits = 0;
        offset = Vector3.zero;
        int currentIteration = 0;
        for (currentIteration = 0; currentIteration < iterations; currentIteration++)
        {
            numHits = RaycastCorners(collider, corners, offset, forward, hits);

            // Success
            if (numHits == 4)
            {
                break;
            }

            // If none of the corner raycasts hit the collider, can't guess which direction to go
            if (numHits == 0)
            {
                break;
            }

            Vector3 stepOffset = Vector3.zero;
            for (int i = 0; i < corners.Length; i++)
            {
                if (hits[i])
                {
                    Vector3 toCorner = corners[i] - center;
                    stepOffset += toCorner;
                }
            }

            // If two of our corners are coplanar and share an edge, our offset will be facing the correct direction,
            // but it will have too much magnitude because the vectors face partially in the same direction.
            // If there are three hits, this isn't an issue because two of them will have to be opposites, so the two
            // will cancel eachother out.
            if (numHits == 2)
            {
                stepOffset /= 2;
            }

            // Reduce our offset distance by a power of 2 each iteration.
            stepOffset *= Mathf.Pow(0.5f, currentIteration);

            offset += stepOffset;
        }

        // Test again with the latest offset
        numHits = RaycastCorners(collider, corners, offset, forward, hits);

        // If viable solution is found, try to improve it with remaining iterations by creeping backwards
        if (numHits == 4)
        {
            for (int i = currentIteration; i < iterations; i++)
            {

                // Creep backwards by a smaller distance each iteration
                Vector3 stepOffset = offset * Mathf.Pow(0.5f, i);
                Vector3 newOffset = offset - stepOffset;

                // Check new offset
                int foo = RaycastCorners(collider, corners, newOffset, forward, hits);
                if (foo == 4)
                {
                    offset = newOffset;
                }
            }
        }

        return numHits == 4;
    }


    bool FindFitBoxCollider(Portal portal, Collider collider, out Vector3 offset)
    {
        // Loop through each corner of the portal rect.
        // For each point, calculate the min and max distance to the collider surface
        // and choose the most extreme of each.
        Vector3 minOffset = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 maxOffset = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);
        foreach (Vector3 corner in portal.WorldSpaceCorners)
        {
            Vector3 closestPoint = collider.ClosestPoint(corner);
            Vector3 offset_ = closestPoint - corner;

            minOffset = Vector3.Min(minOffset, offset_);
            maxOffset = Vector3.Max(maxOffset, offset_);
        }

        float epsilon = 0.00001f;
        if ((Mathf.Abs(minOffset.x) > epsilon && Mathf.Abs(maxOffset.x) > epsilon) ||
            (Mathf.Abs(minOffset.y) > epsilon && Mathf.Abs(maxOffset.y) > epsilon) ||
            (Mathf.Abs(minOffset.z) > epsilon && Mathf.Abs(maxOffset.z) > epsilon))
        {
            offset = portal.transform.position;
            return false;
        }
        else
        {
            offset = portal.transform.position + minOffset + maxOffset;
            return true;
        }
    }

    int RaycastCorners(Collider collider, Vector3[] corners, Vector3 offset, Vector3 direction, bool[] outHits)
    {
        int numHits = 0;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 corner = corners[i] + offset;

            // Perform raycast from a tiny bit back from the contact point to a little bit through the contact point
            float normalOffset = 0.1f;
            Ray ray = new Ray(corner - direction * normalOffset, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, normalOffset * 2, _canHit, QueryTriggerInteraction.Collide))
            {
                if (hit.collider == collider)
                {
                    outHits[i] = true;
                    numHits++;
                }
                else
                {
                    outHits[i] = false;
                }
            }
        }
        return numHits;
    }
}
