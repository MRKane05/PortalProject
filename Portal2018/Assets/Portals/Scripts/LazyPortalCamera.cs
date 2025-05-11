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
    // Use this for initialization
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
            if (bDoRecursiveDump)
            {
                GraphicsShift();
            }
		}
	}

    void GraphicsShift()
    {
        Graphics.CopyTexture(ourRenderTexture, RenderTextureDump);
    }

	void SetCameraPositions()
    {
        /*public RenderTexture RenderToTexture(Camera.MonoOrStereoscopicEye eye, Rect viewportRect, bool renderBackface)
        {
            _framesSinceLastUse = 0;

            // Copy parent camera's settings
            CopyCameraSettings(Camera.main, ourCamera);*/
            //ourCamera.farClipPlane = Camera.main.farClipPlane * ourPortal.PortalScaleAverage();

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
    /*
    private void ClearRenderTexture(RenderTexture rt)
    {
        // TODO: This is probably a fairly expensive operation. We can make it cheaper by using
        // CommandBuffers to avoid swapping the active texture, but we have to clear the whole screen
        // instead of just the portion that is rendering
        var oldRT = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = oldRT;
    }*/

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
