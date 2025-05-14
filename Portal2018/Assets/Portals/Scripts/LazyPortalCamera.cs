using Portals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//So this is simply an attempt to have our system place a camera to correctly render the view this portal should be seeing
public class LazyPortalCamera : MonoBehaviour {
	public RenderTexture ourRenderTexture;
	public Camera ourCamera;
	public MeshRenderer ourPortalPlane;
	public Material planeMat;
	public Portal ourPortal;
    public Texture2D RenderTextureDump;
    public bool bDoRecursiveDump = true;

    IEnumerator Start () {
        yield return null; //Wait for the portal class to intilize etc.
		//Get a fresh set of everything for us
		ourRenderTexture = new RenderTexture(ourRenderTexture); //Clone our rendertexture

        if (bDoRecursiveDump)
        {
            RenderTextureDump = new Texture2D(ourRenderTexture.width, ourRenderTexture.height, TextureFormat.RGBA32, false);
        }

        ourRenderTexture.name = "~RenderTexture";
		ourCamera.targetTexture = ourRenderTexture;
		planeMat = ourPortalPlane.materials[0];    //Lazy grab
                                                   //planeMat.SetTexture("_LeftEyeTexture", ourRenderTexture);
        planeMat.SetFloat("portal_rec", portalRecs);
        if (bDoRecursiveDump)
        {
            planeMat.SetTexture("_LeftEyeTexture", RenderTextureDump);
        } else
        {
            planeMat.SetTexture("_LeftEyeTexture", ourRenderTexture);
        }

        planeMat.SetColor("_Color", ourPortal.PortalColor);
		ourPortalPlane.materials[0] = planeMat;

		//ourPortal = gameObject.GetComponent<Portal>();
	}

	//Position our camera!
	void LateUpdate()
	{
		if (ourPortal && ourPortal.ExitPortal)
        {
            SetCameraPositions();

            SetMaterialTexOffsetScale();
		}
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

            // Optionally skip corners behind the camera
            if (vp.z < 0)
                continue;

            minX = Mathf.Min(minX, vp.x);
            minY = Mathf.Min(minY, vp.y);
            maxX = Mathf.Max(maxX, vp.x);
            maxY = Mathf.Max(maxY, vp.y);
        }

        // If all corners were behind the camera
        if (minX > maxX || minY > maxY)
            return Rect.zero;

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    bool PortalVisible(Vector3 targetPortalScreen, Rect basePortal, float baseScale)
    {
        Rect ChildPortal = new Rect(new Vector2(targetPortalScreen.x, targetPortalScreen.y), basePortal.size / baseScale);
        return basePortal.Overlaps(ChildPortal);
    }


    void SetMaterialTexOffsetScale()
    {

        //bool exitPortalVisible = PortalVisible(ourPortal.ExitPortal, ourCamera);    //Except it's ourselves we should be looking at...
        //Debug.Log("ExitPortalVisible: " + exitPortalVisible);
        //Debug.Log("Portal through Visible: " + IsPortalOccluded(ourPortal.ExitPortal, ourCamera));

        //portalRecs can be calculated from a "maximum allowed zoom" which will be defined by how far apart our portals are (if it's too large we end up wasting ticks)
        //portalRecs is also calculated based off of how much each portal can see the other
        //if portalRecs is zero then we need to disable everything and forget it

        //For this we need to know what the orientations are for our cameras/portals, and the distances between them, and of course the player
        //The first portal is one beyond the rendered portal
        Vector3 portalDir = transform.position - ourPortal.ExitPortal.transform.position;
        
        Vector3 closeDistancePos = transform.position + portalDir * 2f;
        Vector3 farDistancePos = transform.position + portalDir * (2f + portalRecs);

        float closeDistanceScale = 1f + Vector3.Distance(transform.position, closeDistancePos) / distanceDividor;
        float farDistanceScale = 1f + Vector3.Distance(transform.position, farDistancePos) / distanceDividor;

        Vector3 portalScreenCenter = Camera.main.WorldToViewportPoint(closeDistancePos);// gameObject.transform.position);   //Need to do this on a point through the portal, and maybe one further still
        Vector3 portalScreenFar = Camera.main.WorldToViewportPoint(farDistancePos);

        Rect MainPortalRect = GetViewportRectFromBounds(Camera.main, ourPortal.gameObject.GetComponent<Collider>().bounds);
        
        //We can use our positions to get the centers for our recursive portals
        bool bPortalRecurseVisible = PortalVisible(portalScreenCenter, MainPortalRect, closeDistanceScale); //This works
        Debug.Log(bPortalRecurseVisible);

        if (planeMat) {
            //Debug.Log(new Vector4(portalScreenCenter.x, portalScreenCenter.y, distance, 1));
            planeMat.SetVector("offset_close", new Vector4(portalScreenCenter.x, portalScreenCenter.y, closeDistanceScale, 1f));
            planeMat.SetVector("offset_far", new Vector4(portalScreenFar.x, portalScreenFar.y, farDistanceScale, 1f));
            //Debug.Log("DT:" + new Vector4(portalScreenCenter.x, portalScreenCenter.y, closeDistanceScale, 1f) + ", " + new Vector4(portalScreenFar.x, portalScreenFar.y, farDistanceScale, 1f));
        }
        
    }

    void GraphicsShift()
    {
        Graphics.CopyTexture(ourRenderTexture, RenderTextureDump);
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
}
