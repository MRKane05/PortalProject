using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSpawnerBase : MonoBehaviour {
    //These could do with becoming a global reference...
    public static Color RightPortalColor = new Color(1f, 0.627f, 0);
    public static Color LeftPortalColor = new Color(0, 0.627f, 1f);

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

        int mask = _canHit | PortalPhysics.PortalLayerMask;
        hitWall = Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Collide);


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

            }
        }
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
            portal.PlayPortalOpenSound();
            // Scale the portal's renderer up from 0 to 1 for a nice visual pop-in
            Renderer portalRenderer = portal.GetComponentInChildren<MeshRenderer>();
            //PROBLEM: Hack here to keep things scaled for a plane as oppoed to a generated surface
            SetScaleOverTime(portal, portalRenderer.transform, Vector3.zero, Vector3.one * PortalScale, _portalSpawnCurve, _portalSpawnTime);

            return true;
        }

        return false;
    }

    public void HideOnlyLeft()
    {
        _leftPortal.gameObject.SetActive(false);
        _leftPortal.PortalRenderer.setPortalAlpha(0);
    }

    public void HidePortals()   //Called from date to dissapear portals
    {
        _leftPortal.gameObject.SetActive(false);
        _leftPortal.PortalRenderer.setPortalAlpha(0);
        _rightPortal.gameObject.SetActive(false);
        _rightPortal.PortalRenderer.setPortalAlpha(0);
    }

    void SetScaleOverTime(Portal parentPortal, Transform t, Vector3 startSize, Vector3 endSize, AnimationCurve curve, float duratio)
    {
        StartCoroutine(ScaleOverTimeRoutine(parentPortal, t, startSize, endSize, curve, duratio));
    }

    IEnumerator ScaleOverTimeRoutine(Portal parentPortal, Transform t, Vector3 startSize, Vector3 endSize, AnimationCurve curve, float duration)
    {
        float elapsed = 0;
        LazyPortalCamera lazyPortal = parentPortal.GetComponent<LazyPortalCamera>();
        bool bScaleUp = endSize.x > 0f;
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
                if (bScaleUp)
                {
                    parentPortal.PortalRenderer.setPortalAlpha(scaleT);
                } else
                {
                    parentPortal.PortalRenderer.setPortalAlpha(1.0f-scaleT);
                }

            }
            if (bScaleUp)
            {
                parentPortal.PortalRenderer.setPortalAlpha(1.0f);
            } else
            {
                parentPortal.PortalRenderer.setPortalAlpha(0.0f);
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
        t.localScale = endSize;
        if (!bScaleUp)
        {
            parentPortal.gameObject.SetActive(false);
        }
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

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    bool fitFromExtents(Portal portal, Vector3 forward, Vector3 centerPoint, Vector3[] corners, out Vector3 fitPosition)
    {
        //Raycast from the center to all of the corners, and if we hit something we need to offset our center point according to the hit (say the user aims at a wall close to the floor)
        int repeats = 4; //Don't do this more than this many times to try and find a fit
        corners = portal.WorldSpaceCorners;

        Vector3[] offsetCorners = new Vector3[4];
        Debug.DrawLine(centerPoint, centerPoint + forward, Color.green, 10f);
        for (int i = 0; i < 4; i++)
        {
            offsetCorners[i] = centerPoint - corners[i];
        }

        float spherecastRadius = 0.01f;
        bool bFitFound = false;
        while (!bFitFound && repeats > 0)
        {
            Ray ray;
            RaycastHit hit;
            bool bHitPass = true;
            corners = portal.WorldSpaceCorners;
            for (int i = 0; i < 3; i++)
            {
                //Lets try going around the edges of our portal
                /*
                 * _cornerBuffer[0] = topLeft;
                    _cornerBuffer[1] = topRight;
                    _cornerBuffer[2] = bottomRight;
                    _cornerBuffer[3] = bottomLeft;*/
                ray = new Ray(corners[i] - forward *spherecastRadius, (corners[i + 1] - corners[i]).normalized);
                if (Physics.SphereCast(ray, spherecastRadius * 0.95f, out hit, Vector3.Distance(corners[i], corners[i + 1]), _canHit, QueryTriggerInteraction.UseGlobal))
                //if (Physics.Raycast(ray, out hit, (corners[i] - corners[i + 1]).magnitude, _canHit, QueryTriggerInteraction.UseGlobal))
                {
                    Debug.DrawLine(hit.point, hit.point - forward, Color.red, 10f);
                    //Shift this into local space to see what we should do
                    Vector3 LocalHit = portal.transform.InverseTransformPoint(hit.point);
                    Debug.Log("Local Hit: " + LocalHit);

                    //Ok, so we've got a reliable hit on one of our sides. We need to move our portal now to suit that hit
                    //Vector3 HitDirection = hit.point - centerPoint;  //Not that this tell us all that much actually
                    
                    //Debug.Log("Hit on trace: " + i);
                    //So if we've got a hit we need to offset our position to suit
                    //float t = InverseLerp(corners[i], corners[i + 1], hit.point);
                    //Debug.Log("Hit on trace: " + i + ", t: " + t);
                    //Debug.DrawRay(hit.point, forward, Color.green, 5f);
                    //So to calculate our move
                    /*
                    t = Mathf.Clamp01(Mathf.Abs(t));
                    if (t > 0.5f) //move towards end
                    {
                        Debug.Log("ShiftU: " + (corners[i] - corners[i + 1]) * (1f - t));
                        portal.transform.position += (corners[i] - corners[i+1]) * (1f - t);
                    } else
                    {
                        Debug.Log("ShiftD: " + (corners[i + 1] - corners[i]) * t);
                        portal.transform.position += (corners[i+1] - corners[i]) * t;
                    }
                    centerPoint = portal.transform.position;
                    */
                    
                }

                /*
                ray = new Ray(centerPoint - forward.normalized * spherecastRadius, (offsetCorners[i]).normalized);
                
                if (Physics.SphereCast(ray, spherecastRadius * 0.95f, out hit, offsetCorners[i].magnitude, _canHit, QueryTriggerInteraction.UseGlobal))
                {
                    //Need to reposition the portal based off of this hit
                    Debug.DrawLine(centerPoint - forward.normalized * spherecastRadius, hit.point, Color.red, 5f);
                    Vector3 centerOffset = (centerPoint - forward.normalized * spherecastRadius + offsetCorners[i]);

                    Debug.Log(centerOffset + ", H: " + hit.point);
                    centerOffset -= hit.point;
                    centerPoint += centerOffset;                   

                    bHitPass = false;
                }
                else
                {
                    Debug.DrawLine(centerPoint - forward.normalized * spherecastRadius, centerPoint + offsetCorners[i] - forward.normalized * spherecastRadius, Color.yellow, 5f);
                }
                
                //Original raycast
                /
                if (Physics.Raycast(ray, out hit, Vector3.Distance(centerPoint, centerPoint + offsetCorners[i]), _canHit, QueryTriggerInteraction.UseGlobal))
                {
                    //Need to reposition the portal based off of this hit
                    centerPoint += centerPoint - hit.point;
                    Debug.DrawLine(centerPoint, centerPoint + offsetCorners[i], Color.red, 5f);

                    bHitPass = false;
                } else
                {
                    Debug.DrawLine(centerPoint, centerPoint + offsetCorners[i], Color.yellow, 5f);
                }*/
            }

            if (bHitPass)
            {
                bFitFound = true;
            }
            repeats--;
        }
        fitPosition = centerPoint;

        if (!bFitFound)
        {
            return false;
        }        
        return true;
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

        //This needs a position according to the extents of the portal and hypothetical offset to make the portal fit

        int numIterations = _meshColliderIterationCount;
        Vector3 offset;
        bool viablePositionExists = FindViableCoplanarRectOnCollider(collider, center, corners, forward, numIterations, out offset);

        if (!viablePositionExists)
        {
            return false;
        }

        newPosition = portal.transform.position + offset;
        /*
        portal.transform.position = newPosition;

        corners = portal.WorldSpaceCorners;
        //Try and do the world-space fitting considering edges
        Vector3 newOffset = newPosition;
        bool fittedPositionExists = fitFromExtents(portal, forward, newPosition, corners, out newOffset);

        if (!fittedPositionExists)
        {
            return false;
        }

        newPosition = newOffset; //Because this will have bene hard set in the above function
        */
        return true;
    }

    public bool CheckMaterialValidForPortal(RaycastHit hit)
    {
        if (hit.transform == null) return false;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null) return false;

        Mesh mesh = meshCollider.sharedMesh;
        int triangleIndex = hit.triangleIndex;

        int subMeshIndex = GetSubMeshIndex(mesh, triangleIndex);

        if (subMeshIndex == -1) return false;

        MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
        if (meshRenderer == null) return false;

        Material[] materials = meshRenderer.materials;
        if (subMeshIndex >= materials.Length) return false;

        Material hitMaterial = materials[subMeshIndex];
        string matName = hitMaterial.name.ToLower();

        return true;
        // You can now work with the 'hitMaterial'
    }

    private int GetSubMeshIndex(Mesh mesh, int triangleIndex)
    {
        int subMeshCount = mesh.subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 1)
            {
                if (triangleIndex >= j && triangleIndex < j + 1)
                {
                    return i;
                }
            }
        }
        return -1; // Should not happen if triangleIndex is valid
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
                if (hit.collider == collider && collider.gameObject.layer != LayerMask.NameToLayer("BackfacePortal"))
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
