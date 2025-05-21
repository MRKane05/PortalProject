using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//So this is simply an attempt to have our system place a camera to correctly render the view this portal should be seeing
public class LazyPortalCamera : MonoBehaviour {
	public RenderTexture ourRenderTexture;
	public Camera ourCamera;
    public Transform portalBase;
	public MeshRenderer ourPortalPlane;
    public MeshRenderer ourPortalBackface;
	public Material planeMat;
    public Material backfaceMat;
	public Portal ourPortal;

    public LazyPortalCamera exitPortalCamera;
    public float portalScale = 0f;

    // Buffer for calculating near plane corners
    private static Vector3[] _nearPlaneCorners;

    IEnumerator Start () {
        yield return null; //Wait for the portal class to intilize etc.
                           //Get a fresh set of everything for us
        exitPortalCamera = ourPortal.ExitPortal.gameObject.GetComponent<LazyPortalCamera>();
		ourRenderTexture = new RenderTexture(ourRenderTexture); //Clone our rendertexture

        ourRenderTexture.name = "~RenderTexture";
		ourCamera.targetTexture = ourRenderTexture;
		planeMat = ourPortalPlane.materials[0];    //Lazy grab
        backfaceMat = ourPortalBackface.materials[0];

        planeMat.SetTexture("_LeftEyeTexture", ourRenderTexture);
        backfaceMat.SetTexture("_LeftEyeTexture", ourRenderTexture);

        planeMat.SetFloat("portal_rec", portalRecs);
        backfaceMat.SetFloat("portal_rec", portalRecs);

        planeMat.SetColor("_Color", ourPortal.PortalColor);
        backfaceMat.SetColor("_Color", ourPortal.PortalColor);

        ourPortalPlane.materials[0] = planeMat;
        ourPortalBackface.materials[0] = backfaceMat;

		//ourPortal = gameObject.GetComponent<Portal>();
	}

    private bool CameraIsInFrontOfPortal(Camera camera, Portal portal)
    {
        Vector3 cameraPosWS = Camera.main.transform.position;
        if (portal.Plane.GetSide(cameraPosWS))
        {
            return false;
        }

        // Check if camera is inside the rectangular bounds of portal
        float extents = 0.5f;
        Vector3 cameraPosOS = transform.InverseTransformPoint(cameraPosWS);
        if (cameraPosOS.x < -extents) return false;
        if (cameraPosOS.x > extents) return false;
        if (cameraPosOS.y > extents) return false;
        if (cameraPosOS.y < -extents) return false;
        return true;
    }


    private bool NearClipPlaneIsBehindPortal(Camera camera, Portal portal)
    {
        if (_nearPlaneCorners == null)
        {
            _nearPlaneCorners = new Vector3[4];
        }

        Vector4 portalPlaneWorldSpace = ourPortal.VectorPlane;
        Vector4 portalPlaneViewSpace = camera.worldToCameraMatrix.inverse.transpose * portalPlaneWorldSpace;
        portalPlaneViewSpace.z *= -1; // TODO: Why is this necessary?
        Plane p = new Plane((Vector3)portalPlaneViewSpace, portalPlaneViewSpace.w);

        camera.CalculateFrustumCorners(camera.rect, camera.nearClipPlane, camera.stereoActiveEye, _nearPlaneCorners);
        for (int i = 0; i < _nearPlaneCorners.Length; i++)
        {
            Vector3 corner = _nearPlaneCorners[i];

            // Return as soon as a single corner is found behind the portal
            bool behindPortalPlane = p.GetSide(corner);
            if (behindPortalPlane)
            {
                return true;
            }
        }
        return false;
    }

    private bool ShouldRenderBackface(Camera camera)
    {
        // Don't render back face for recursive calls
        if (!CameraIsInFrontOfPortal(camera, ourPortal)) { return false; }
        if (!NearClipPlaneIsBehindPortal(camera, ourPortal)) { return false; }
        return true;
    }


    Plane[] planes;
    Collider portalCollider;
    bool isPortalVisible()
    {
        planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        if (!portalCollider)
        {
            portalCollider = ourPortal.GetComponent<Collider>();
        }

        if (GeometryUtility.TestPlanesAABB(planes, portalCollider.bounds))
        {
            return true;
        }
        return false;
    }

    //Position our camera!
    public bool portalVisible = false;
    void LateUpdate()
	{
        portalVisible = isPortalVisible();
        ourCamera.gameObject.SetActive(portalVisible);

		if (ourPortal && ourPortal.ExitPortal && portalVisible)
        {
            SetCameraPositions();

            HandlePortalRendering();

            bool renderBackface = ShouldRenderBackface(Camera.main);
            //backfaceMat.SetFloat("_BackfaceAlpha", renderBackface ? 1.0f : 0.0f); //This doesn't need to be a float, we can just turn this off
            ourPortalBackface.gameObject.SetActive(renderBackface);
            //ourPortalBackface.gameObject.SetActive(false); // turn the backface portal off
        }
	}

    void HandlePortalRendering()
    {
        //Couple of things to do here:
        //-Check and see if our portal (or our exit portal) is transitioning and assign blend values
        //-Check to see if the portals can "see" each other and assign the optimised material if they cannot (or just set the recs to 0)
        if (exitPortalCamera)
        {
            planeMat.SetFloat("portalViewAlpha", portalScale * exitPortalCamera.portalScale);
            backfaceMat.SetFloat("portalViewAlpha", portalScale * exitPortalCamera.portalScale);
        }
       // SetMaterialTexOffsetScale();
    }

    public float distanceDividor = 10f;

    public int portalRecs = 4;


    Rect GetViewportRectFromBounds(Camera cam, Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            Vector3 vp = cam.WorldToViewportPoint(corner);

            minX = Mathf.Min(minX, vp.x);
            minY = Mathf.Min(minY, vp.y);
            maxX = Mathf.Max(maxX, vp.x);
            maxY = Mathf.Max(maxY, vp.y);
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    bool PortalVisible(Vector3 targetPortalScreen, Rect basePortal, float baseScale)
    {
        Rect ChildPortal = new Rect(new Vector2(targetPortalScreen.x, targetPortalScreen.y), basePortal.size / baseScale);
        return basePortal.Overlaps(ChildPortal);
    }

    float maxRepeats = 5;
    float maxRepeatScale = 7;
    int currentRepeats = 7;
    void setRepeats(int newRepeat)
    {
        //We really should be handling things with distinct shaders here
        /*
        if (currentRepeats != newRepeat && planeMat)
        {
            //Debug.Log("Setting portalRecs: " + newRepeat + ", " + currentRepeats);
            currentRepeats = newRepeat;
            planeMat.SetFloat("portal_rec", (float)currentRepeats);
            backfaceMat.SetFloat("portal_rec", (float)currentRepeats);
        }*/
    }

    void SetMaterialTexOffsetScale()
    {

        //OK, just do a march on this to either maximum scale, or maximum recursion

        Vector3 portalDir = transform.position - ourPortal.ExitPortal.transform.position;
        
        //We need to calculate the max recs...

        Vector3 closeDistancePos = transform.position + portalDir * 2f;
        float closeDistanceScale = 1f + Vector3.Distance(transform.position, closeDistancePos) / distanceDividor;

        //An initial grab portal
        Vector3 farDistancePos = transform.position + portalDir * (2f + portalRecs);
        float farDistanceScale = 1f + Vector3.Distance(transform.position, farDistancePos) / distanceDividor;

        Vector3 portalScreenCenter = Camera.main.WorldToViewportPoint(closeDistancePos);// gameObject.transform.position);   //Need to do this on a point through the portal, and maybe one further still
        Vector3 portalScreenFar = Camera.main.WorldToViewportPoint(farDistancePos);
        int localReps = 0;
        //I can't handle all use-cases with this because sometimes the main portal rec will be invalid
        Rect MainPortalRect = GetViewportRectFromBounds(Camera.main, ourPortal.gameObject.GetComponent<Collider>().bounds);
        
        for (int i = 0; i < maxRepeats; i++)
        {
            farDistancePos = transform.position + portalDir * (2f + i);
            farDistanceScale = 1f + Vector3.Distance(transform.position, farDistancePos) / distanceDividor;
            Vector3 portalScreenPos = Camera.main.WorldToViewportPoint(farDistancePos);
            //We can use our positions to get the centers for our recursive portals
            bool bPortalRecurseVisible = PortalVisible(portalScreenPos, MainPortalRect, farDistanceScale); //This works
            if (!bPortalRecurseVisible || farDistanceScale > maxRepeatScale)
            {
                localReps = i;
                break;
            }
            //And a check to see that we've made it to the end of our pass
            if (i >= maxRepeats-1)
            {
                localReps = Mathf.RoundToInt(maxRepeats);
            }
        }
        setRepeats(localReps == 0 ? 0 : (int)maxRepeats);

        if (planeMat) {
            planeMat.SetVector("offset_close", new Vector4(portalScreenCenter.x, portalScreenCenter.y, closeDistanceScale, 1f));
            planeMat.SetVector("offset_far", new Vector4(portalScreenFar.x, portalScreenFar.y, farDistanceScale, 1f));
        }
        if (backfaceMat)
        {
            backfaceMat.SetVector("offset_close", new Vector4(portalScreenCenter.x, portalScreenCenter.y, closeDistanceScale, 1f));
            backfaceMat.SetVector("offset_far", new Vector4(portalScreenFar.x, portalScreenFar.y, farDistanceScale, 1f));
        }
    }

	void SetCameraPositions()
    {
        Matrix4x4 projectionMatrix;
            Matrix4x4 worldToCameraMatrix;
            projectionMatrix = Camera.main.projectionMatrix;
            worldToCameraMatrix = Camera.main.worldToCameraMatrix;

            ourCamera.transform.position = ourPortal.TeleportPoint(Camera.main.transform.position);
            ourCamera.transform.rotation = ourPortal.TeleportRotation(Camera.main.transform.rotation);
            ourCamera.projectionMatrix = projectionMatrix;
            //ourCamera.worldToCameraMatrix = worldToCameraMatrix * ourPortal.PortalMatrix().inverse;
            ourCamera.ResetWorldToCameraMatrix();

            Matrix4x4 defaultProjection = ourCamera.projectionMatrix;
            
            if (ourPortal.UseObliqueProjectionMatrix)
            {
                ourCamera.ResetProjectionMatrix();
                ourCamera.projectionMatrix = CalculateObliqueProjectionMatrix(projectionMatrix);
            }
            else
            {
                ourCamera.ResetProjectionMatrix();
            }

        /*
        //PROBLEM: Need to get this working again and perhaps break down the ScissorsMatrix function to put some checks in place?
        if (ourPortal.UseScissorRect)
        {
            ourCamera.rect = viewportRect;
            ourCamera.projectionMatrix = MathUtil.ScissorsMatrix(ourCamera.projectionMatrix, viewportRect);
        }
        else
        {
            ourCamera.rect = new Rect(0, 0, 1, 1);
        }*/
        ourCamera.rect = new Rect(0, 0, 1, 1);

        if (ourPortal.UseOcclusionMatrix)
            {
                ourCamera.cullingMatrix = CalculateCullingMatrix();
            }
            else
            {
                ourCamera.cullingMatrix = ourCamera.projectionMatrix * ourCamera.worldToCameraMatrix;
            }

            if (ourPortal.DebuggingEnabled)
            {
                //Util.DrawDebugFrustum3(ourCamera.projectionMatrix * ourCamera.worldToCameraMatrix, Color.white);

                if (ourPortal.UseOcclusionMatrix)
                {
                    Util.DrawDebugFrustum3(ourCamera.cullingMatrix, Color.blue);
                }
            }

        
            if (ourPortal.UseRaycastOcclusion)
            {
                ourCamera.useOcclusionCulling = false;
            }

            //RenderTexture texture = GetTemporaryRT();

            if (ourPortal.FakeInfiniteRecursion)
            {
                // RenderTexture must be cleared when using fake infinite recursion because
                // we might sometimes sample uninitialized garbage pixels otherwise, which can
                // cause significant visual artifacts.
                
                //ClearRenderTexture(texture);
            }
            /*
            ourCamera.targetTexture = texture;
            ourCamera.Render();

            SaveFrameData(eye);

            return texture;
            
            
        }
            */

        
    }
    bool IsPointVisible(Camera cam, Vector3 worldPos)
    {
        Matrix4x4 viewProj = cam.projectionMatrix * cam.worldToCameraMatrix;
        Vector4 clipPos = viewProj * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1.0f);

        // Perspective divide
        if (clipPos.w == 0) return false;
        Vector3 ndc = new Vector3(clipPos.x, clipPos.y, clipPos.z) / clipPos.w;

        // Inside clip space: -1 to +1 on all axes
        return Mathf.Abs(ndc.x) <= 1 && Mathf.Abs(ndc.y) <= 1 && ndc.z >= -1 && ndc.z <= 1;
    }

    bool IsBoundsVisible(Camera cam, Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        // Get all 8 corners of the bounds
        int i = 0;
        for (int x = 0; x <= 1; x++)
            for (int y = 0; y <= 1; y++)
                for (int z = 0; z <= 1; z++)
                    corners[i++] = new Vector3(
                        x == 0 ? min.x : max.x,
                        y == 0 ? min.y : max.y,
                        z == 0 ? min.z : max.z);

        foreach (var corner in corners)
        {
            if (IsPointVisible(cam, corner))
            {
                Debug.Log("Exit portal seen through portal");
                return true;
            }
                
        }
        Debug.Log("Exit portal not seen through portal");
        return false;
    }

    private bool IsPortalOccluded(Portal portal, Camera camera)
    {
        if (!portal.UseRaycastOcclusion)
        {
            return false;
        }

        Vector3 origin = camera.transform.position;
        //PortalCamera pc = PortalCamera.current;

        var corners = portal.WorldSpaceCorners;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 corner = corners[i];
            Vector3 direction = corner - origin;
            float distance = direction.magnitude;
            int layerMask = portal.RaycastOccluders;

            // If this portal is being rendered from another portal, the raycast check
            // must begin from the portal plane instead of the camera origin in order
            // to avoid intersection non-rendered geometry.
            /*
            if (pc)
            {
                origin = IntersectPlane(origin, direction, pc.portal.ExitPortal.Plane);
                // Offset along ray slightly to avoid early intersection
                origin += direction * 0.001f;
                distance = (corner - origin).magnitude;
            }*/

            if (portal.DebuggingEnabled) { Debug.DrawRay(origin, direction.normalized * distance, Color.white); }

            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                // Hit something, check how far
                if (portal.DebuggingEnabled) { Debug.DrawRay(origin, direction.normalized * hit.distance, Color.red); }
                float epsilon = 1f;
                if (hit.distance + epsilon >= distance)
                {
                    return false;
                }
            }
            else
            {
                // Didn't hit anything, no occluders
                return false;
            }
        }
        return true;
    }

    private Vector3 IntersectPlane(Vector3 point, Vector3 direction, Plane plane)
    {
        Vector3 nDir = direction.normalized;
        float enter = 0;
        if (plane.Raycast(new Ray(point, nDir), out enter))
        {
            return point + nDir * enter;
        }
        return point;
    }

    private const float ObliqueClippingOffset = 0.001f;

    Matrix4x4 CalculateObliqueProjectionMatrix(Matrix4x4 projectionMatrix)
    {
        // Calculate plane made from the exit portal's transform
        Plane exitPortalPlane = ourPortal.ExitPortal.Plane;

        Vector3 position = ourCamera.cameraToWorldMatrix.MultiplyPoint3x4(Vector3.zero);
        float distanceToPlane = exitPortalPlane.GetDistanceToPoint(position);
        if (distanceToPlane > ourPortal.ClippingOffset)
        {
            Vector4 exitPlaneWorldSpace = ourPortal.ExitPortal.VectorPlane;
            Vector4 exitPlaneCameraSpace = ourCamera.worldToCameraMatrix.inverse.transpose * exitPlaneWorldSpace;
            // Offset the clipping plane itself so that a character walking through a portal has no seams
            exitPlaneCameraSpace.w -= ObliqueClippingOffset;
            exitPlaneCameraSpace *= -1;
            MathUtil.MakeProjectionMatrixOblique(ref projectionMatrix, exitPlaneCameraSpace);
            //projectionMatrix = ourCamera.CalculateObliqueMatrix(exitPlaneCameraSpace);
        }
        return projectionMatrix;
    }

    private Matrix4x4 CalculateCullingMatrix()
    {
        ourCamera.ResetCullingMatrix();
        Vector3[] corners = ourPortal.ExitPortal.WorldSpaceCorners;

        Vector3 pa = corners[3]; // Lower left
        Vector3 pb = corners[2]; // Lower right
        Vector3 pc = corners[0]; // Upper left
        Vector3 pe = ourCamera.transform.position;

        // Calculate what our horizontal field of view would be with off-axis projection.
        // If this fov is greater than our camera's fov, we should just use the camera's default projection
        // matrix instead. Otherwise, the frustum's fov will approach 180 degrees (way too large).
        Vector3 camToLowerLeft = pa - ourCamera.transform.position;
        camToLowerLeft.y = 0;
        Vector3 camToLowerRight = pb - ourCamera.transform.position;
        camToLowerRight.y = 0;
        float fieldOfView = Vector3.Angle(camToLowerLeft, camToLowerRight);
        if (fieldOfView > ourCamera.fieldOfView)
        {
            return ourCamera.cullingMatrix;
        }
        else
        {
            float near = ourCamera.nearClipPlane;
            float far = ourCamera.farClipPlane;
            return MathUtil.OffAxisProjectionMatrix(near, far, pa, pb, pc, pe);
        }
    }

    private bool ShouldRenderPortal(Camera camera)
    {
        // Don't render if renderer disabled. Not sure if this is possible anyway, but better to be safe.
        /*
        bool isRendererEnabled = enabled && _renderer && _renderer.enabled;
        if (!isRendererEnabled) { return false; }
        */
        // Don't render non-supported camera types (preview cameras can cause issues)
        bool isCameraSupported = (ourPortal.SupportedCameraTypes & camera.cameraType) == camera.cameraType;
        if (!isCameraSupported) { return false; }

        // Only render if an exit portal is set
        bool isExitPortalSet = ourPortal.ExitPortal != null;
        if (!isExitPortalSet) { return false; }

        // Don't ever render an exit portal
        // TODO: Disable portal until end of frame
        /*
        bool isRenderingExitPortal = _currentRenderDepth > 0 && _currentlyRenderingPortal == _portal.ExitPortal;
        if (isRenderingExitPortal) { return false; }
        
        // Don't render too deep
        bool isAtMaxDepth = _currentRenderDepth >= _portal.MaxRecursion;
        if (isAtMaxDepth) { return false; }
        */
        // Don't render if hidden behind objects
        bool isOccluded = IsPortalOccluded(ourPortal, camera);
        if (isOccluded) { return false; }

        return true;
    }
}
