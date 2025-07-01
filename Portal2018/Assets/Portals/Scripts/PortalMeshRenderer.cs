using System.Collections.Generic;
using UnityEngine;
using Nothke.Utils;
using System.Linq;
using UnityEngine.Rendering;

namespace PortalSystem
{
    public static class PortalMeshRenderer
    {
        //Our command buffers (which will be sorted by camera)
        private static Dictionary<string, CommandBuffer> _commandBuffers = new Dictionary<string, CommandBuffer>();
        // Cached lists to avoid garbage collection
        private static List<MeshRenderer> cachedRenderers = new List<MeshRenderer>();
        private static List<MeshFilter> cachedFilters = new List<MeshFilter>();
        private static List<RenderData> renderQueue = new List<RenderData>();

        private static List<RenderData> cachedRenderList = new List<RenderData>();

        // Frustum planes for culling
        private static Plane[] frustumPlanes = new Plane[6];

        private struct RenderData
        {
            public Renderer renderer;
            public Mesh mesh;
            public Material material;
            public Matrix4x4 worldMatrix;
            public int submeshIndex;
            public int materialIndex;
        }

        private static CommandBuffer floatingBuffer;

        /// <summary>
        /// Renders all meshes within the specified view frustum to the target RenderTexture
        /// </summary>
        /// <param name="targetTexture">The RenderTexture to render to</param>
        /// <param name="viewMatrix">View matrix (inverse of camera world transform)</param>
        /// <param name="projectionMatrix">Projection matrix for frustum culling</param>
        /// <param name="occlusionMatrix">Optional occlusion matrix from portal system</param>
        /// <param name="clearColor">Color to clear the render texture with</param>
        /// <param name="cullingMask">Layer mask for filtering objects</param>
        /// <param name="maxDistance">Maximum render distance</param>
        /// <param name="sortByDistance">Whether to sort objects by distance (back to front)</param>
        public static void RenderMeshesInFrustum(
            RenderTexture targetTexture,
            Matrix4x4 viewMatrix,
            Matrix4x4 projectionMatrix,
            Color clearColor = default(Color),
            int cullingMask = -1,
            float maxDistance = 1000f,
            bool sortByDistance = false)
        {
            if (targetTexture == null)
            {
                Debug.LogWarning("PortalMeshRenderer: Target texture is null");
                return;
            }

            // Use occlusion matrix if provided, otherwise use regular projection
            Matrix4x4 finalProjectionMatrix = projectionMatrix;
            Matrix4x4 viewProjectionMatrix = finalProjectionMatrix * viewMatrix;

            // Extract frustum planes for culling
            ExtractFrustumPlanes(viewProjectionMatrix, frustumPlanes);

            // Clear the render queue
            renderQueue.Clear();

            // Collect all visible meshes
            CollectVisibleMeshes(viewMatrix, frustumPlanes, cullingMask, maxDistance);

            // Sort by distance if requested (for transparency)
            if (sortByDistance)
            {
                Vector3 cameraPos = ExtractCameraPosition(viewMatrix);
                SortRenderQueueByDistance(cameraPos);
            }
            /*
            float aspect = (float)rt.width / rt.height;
            Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
            Matrix4x4 viewMatrix = Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));

            Matrix4x4 cameraMatrix = (projectionMatrix * viewMatrix.inverse);

            */
            float aspect = (float)targetTexture.width / targetTexture.height;

            //Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
            //Matrix4x4 viewMatrix = Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));

            //Matrix4x4 cameraMatrix = (projectionMatrix * viewMatrix.inverse);

            // Begin rendering to the target texture
            targetTexture.BeginRendering(projectionMatrix * viewMatrix.inverse);

            // Clear the render texture
            GL.Clear(true, true, clearColor);

            // Render all collected meshes
            RenderCollectedMeshes(targetTexture);

            // End rendering
            targetTexture.EndRendering();
        }

        public static CommandBuffer getCommandBuffer(RenderTexture targetTexture)
        {
            CommandBuffer cmd = new CommandBuffer();
            cmd.name = "Custom Render";

            cmd.SetRenderTarget(targetTexture, 16);

            return cmd;
        }

        public static void releaseCommandBuffer(CommandBuffer cmd)
        {
            cmd.Release();
        }

        public static void ClearCommandRenderTarget(CommandBuffer cmd)
        {
            cmd.ClearRenderTarget(true, true, Color.black);
        }

        public static CommandBuffer GetCommandBuffer(string bufferName)
        {
            if (_commandBuffers.ContainsKey(bufferName))
            {
                return _commandBuffers[bufferName];
            }
            else
            {
                CommandBuffer cmd = new CommandBuffer();
                cmd.name = bufferName;

                _commandBuffers.Add(bufferName, cmd);

                return cmd;
            }
        }


        public static void SetupFloatingBuffer()
        {
            floatingBuffer = new CommandBuffer();
            floatingBuffer.name = "Custom Render";
            //Draw scene
            //CommandBufferRenderCollectedMeshes(targetTexture, cmd);
        }

        public static void CommandBufferRenderAll(RenderTexture targetTexture,
           Camera _camera,
           bool sortByDistance = true,
           Color clearColor = default(Color))
        {


            CommandBuffer cmd = new CommandBuffer();
            cmd.name = "Custom Render";

            cmd.SetRenderTarget(targetTexture);

            cmd.ClearRenderTarget(true, true, Color.black);

            // Set camera matrices
            cmd.SetViewProjectionMatrices(_camera.worldToCameraMatrix,
                                         _camera.projectionMatrix);

            //Draw scene
            //CommandBufferRenderCollectedMeshes(targetTexture, cmd);
            CommandBufferRenderAll(targetTexture, cmd);
            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }


        public static void CommandBufferRenderMeshesInFustrum(RenderTexture targetTexture,
           Camera _camera,
           bool sortByDistance = true,
           Color clearColor = default(Color))
            {
            // Extract frustum planes for culling
            //ExtractFrustumPlanes(_camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left), frustumPlanes);

            // Clear the render queue
            //renderQueue.Clear();

            // Collect all visible meshes
           // CollectVisibleMeshes(_camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left), frustumPlanes, _camera.cullingMask, _camera.farClipPlane);

            // Sort by distance if requested (for transparency)
            /*
            if (sortByDistance)
            {
                Vector3 cameraPos = _camera.transform.position; // ExtractCameraPosition(viewMatrix);
                SortRenderQueueByDistance(cameraPos);
            }*/

            CommandBuffer cmd = GetCommandBuffer(_camera.name);
            cmd.SetRenderTarget(targetTexture, 16);
            
            /*
            if (floatingBuffer == null)
            {
                SetupFloatingBuffer();
            }*/



            cmd.SetRenderTarget(targetTexture);
            
            cmd.ClearRenderTarget(true, true, Color.black);

            // Set camera matrices
            cmd.SetViewProjectionMatrices(_camera.worldToCameraMatrix,
                                         _camera.projectionMatrix);

            //Draw scene
            //CommandBufferRenderCollectedMeshes(targetTexture, cmd);
            CommandBufferRenderAll(targetTexture, cmd);
            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }

            public static void RenderMeshesInFrustum(
           RenderTexture targetTexture,
           Camera _camera,
           bool sortByDistance = true,
           Color clearColor = default(Color))
        {
            if (targetTexture == null)
            {
                Debug.LogWarning("PortalMeshRenderer: Target texture is null");
                return;
            }

            // Extract frustum planes for culling
            ExtractFrustumPlanes(_camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left), frustumPlanes);

            // Clear the render queue
            renderQueue.Clear();

            // Collect all visible meshes
            CollectVisibleMeshes(_camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left), frustumPlanes, _camera.cullingMask, _camera.farClipPlane);

            // Sort by distance if requested (for transparency)
            if (sortByDistance)
            {
                Vector3 cameraPos = _camera.transform.position; // ExtractCameraPosition(viewMatrix);
                SortRenderQueueByDistance(cameraPos);
            }
            /*
            float aspect = (float)rt.width / rt.height;
            Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
            Matrix4x4 viewMatrix = Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));

            Matrix4x4 cameraMatrix = (projectionMatrix * viewMatrix.inverse);

            */
            float aspect = (float)targetTexture.width / targetTexture.height;

            //Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
            //Matrix4x4 viewMatrix = Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));

            //Matrix4x4 cameraMatrix = (_camera.projectionMatrix * _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse);

            // Begin rendering to the target texture
            //targetTexture.BeginRendering(_camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
            //Matrix4x4 GLCameraMatrix = GL.GetGPUProjectionMatrix(_camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left), true);
            Camera cam = _camera;
            Matrix4x4 viewMatrix = cam.worldToCameraMatrix;
            Matrix4x4 projectionMatrix = cam.projectionMatrix;
            Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(1, -1, 1));
            projectionMatrix = scaleMatrix * projectionMatrix;

            Matrix4x4 viewProjectionMatrix = projectionMatrix * viewMatrix;

            //targetTexture.BeginPerspectiveRendering(_camera.fieldOfView, _camera.transform.position, _camera.transform.rotation, _camera.nearClipPlane, _camera.farClipPlane);
            targetTexture.BeginRendering(viewProjectionMatrix);

            // Clear the render texture
            GL.Clear(true, true, clearColor);

            // Render all collected meshes
            RenderCollectedMeshes(targetTexture);

            // End rendering
            targetTexture.EndRendering();
        }

        /// <summary>
        /// Simplified version that takes camera parameters directly
        /// </summary>
        public static void RenderMeshesInFrustum(
            RenderTexture targetTexture,
            Vector3 cameraPosition,
            Quaternion cameraRotation,
            float fieldOfView,
            float nearPlane,
            float farPlane,
            Color clearColor = default(Color),
            int cullingMask = -1)
        {
            float aspect = (float)targetTexture.width / targetTexture.height;

            Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fieldOfView, aspect, nearPlane, farPlane);
            Matrix4x4 viewMatrix = Matrix4x4.TRS(cameraPosition, cameraRotation, Vector3.one).inverse;

            RenderMeshesInFrustum(targetTexture, viewMatrix, projectionMatrix,
                clearColor, cullingMask, farPlane, false);
        }

        private static void CacheAllMeshes()
        {
            cachedRenderers.Clear();
            cachedRenderers = Object.FindObjectsOfType<MeshRenderer>().ToList();

            foreach (MeshRenderer renderer in cachedRenderers)
            {
                //Quick hack to add everything
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                AddMeshToRenderList(meshFilter, renderer);
            }
        }

        private static void CollectVisibleMeshes(Matrix4x4 viewMatrix, Plane[] frustumPlanes,
            int cullingMask, float maxDistance)
        {
            Vector3 cameraPos = ExtractCameraPosition(viewMatrix);

            // Get all renderers in the scene (cached to avoid GC)
            cachedRenderers.Clear();
            cachedRenderers = Object.FindObjectsOfType<MeshRenderer>().ToList();

            foreach (MeshRenderer renderer in cachedRenderers)
            {
                //Quick hack to add everything
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                // Add to render queue
                AddMeshToRenderQueue(meshFilter, renderer);
                AddMeshToRenderList(meshFilter, renderer);
                continue;
                
                /*
                // Skip if renderer is null or inactive
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;

                // Check layer mask
                if ((cullingMask & (1 << renderer.gameObject.layer)) == 0)
                    continue;

                // Distance culling
                float distance = Vector3.Distance(cameraPos, renderer.transform.position);
                if (distance > maxDistance)
                    continue;

                // Frustum culling
                Bounds bounds = renderer.bounds;
                if (!IsInFrustum(bounds, frustumPlanes))
                    continue;

                // Get mesh filter
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                // Add to render queue
                AddMeshToRenderQueue(meshFilter, renderer);
                */
            }
        }

        private static void AddMeshToRenderQueue(MeshFilter meshFilter, MeshRenderer mtrenderer)
        {
            Mesh mtmesh = meshFilter.sharedMesh;
            Material[] materials = mtrenderer.sharedMaterials;

            Matrix4x4 worldMatrix = mtrenderer.localToWorldMatrix;    //This outputs correct rotational information, but incorrect positions?

            // Handle multi-material meshes
            int submeshCount = mtmesh.subMeshCount;
            for (int i = 0; i < submeshCount && i < materials.Length; i++)
            {
                Material mtmaterial = materials[i];
                if (mtmaterial == null)
                    continue;

                /*
                // PS Vita optimization - skip transparent materials in first pass
#if UNITY_PSP2
                if (mtmaterial.renderQueue > 3000) // Transparent queue
                    continue;
#endif*/

                RenderData renderData = new RenderData
                {
                    renderer = mtrenderer,
                    mesh = mtmesh,
                    material = mtmaterial,
                    worldMatrix = worldMatrix,
                    submeshIndex = i,
                    materialIndex = i
                };

                renderQueue.Add(renderData);
            }
        }

        private static void AddMeshToRenderList(MeshFilter meshFilter, MeshRenderer mtrenderer)
        {
            Mesh mtmesh = meshFilter.sharedMesh;
            Material[] materials = mtrenderer.sharedMaterials;

            Matrix4x4 worldMatrix = mtrenderer.localToWorldMatrix;    //This outputs correct rotational information, but incorrect positions?

            // Handle multi-material meshes
            int submeshCount = mtmesh.subMeshCount;
            for (int i = 0; i < submeshCount && i < materials.Length; i++)
            {
                Material mtmaterial = materials[i];
                if (mtmaterial == null)
                    continue;


                RenderData renderData = new RenderData
                {
                    renderer = mtrenderer,
                    mesh = mtmesh,
                    material = mtmaterial,
                    worldMatrix = worldMatrix,
                    submeshIndex = i,
                    materialIndex = i
                };

                //renderQueue.Add(renderData);
                cachedRenderList.Add(renderData);
            }
        }

        private static void RenderCollectedMeshes(RenderTexture targetTexture)
        {
            foreach (RenderData data in renderQueue)
            {
                if (data.material.SetPass(0))
                {
                    if (data.submeshIndex < data.mesh.subMeshCount)
                    {
                        //Graphics.DrawMeshNow(data.mesh, data.worldMatrix, data.submeshIndex);
                        //targetTexture.DrawMesh(data.mesh, data.material, data.worldMatrix);
                        Matrix4x4 objectMatrix = data.worldMatrix;
                        GL.MultMatrix(objectMatrix);

                        Graphics.DrawMeshNow(data.mesh, data.worldMatrix);
                        
                    }
                }
            }
        }

        private static void CommandBufferRenderAll(RenderTexture targetTexture, CommandBuffer cmd)
        {
            if (cachedRenderList.Count() ==0)
            {
                CacheAllMeshes();
            }


            foreach (RenderData data in cachedRenderList)
            {
                if (data.material.SetPass(0))
                {
                    if (data.submeshIndex < data.mesh.subMeshCount)
                    {
                        cmd.DrawRenderer(data.renderer, data.material, data.submeshIndex);
                    }
                }
            }
        }

        private static void CommandBufferRenderCollectedMeshes(RenderTexture targetTexture, CommandBuffer cmd)
        {
            foreach (RenderData data in renderQueue)
            {
                if (data.material.SetPass(0))
                {
                    if (data.submeshIndex < data.mesh.subMeshCount)
                    {
                        cmd.DrawRenderer(data.renderer, data.material, data.submeshIndex);
                    }
                }
            }
        }

        private static void SortRenderQueueByDistance(Vector3 cameraPosition)
        {
            renderQueue.Sort((a, b) =>
            {
                Vector3 posA = new Vector3(a.worldMatrix.m03, a.worldMatrix.m13, a.worldMatrix.m23);
                Vector3 posB = new Vector3(b.worldMatrix.m03, b.worldMatrix.m13, b.worldMatrix.m23);

                float distA = Vector3.SqrMagnitude(cameraPosition - posA);
                float distB = Vector3.SqrMagnitude(cameraPosition - posB);

                return distB.CompareTo(distA); // Back to front
            });
        }

        private static Vector3 ExtractCameraPosition(Matrix4x4 viewMatrix)
        {
            Matrix4x4 worldMatrix = viewMatrix.inverse;
            return new Vector3(worldMatrix.m03, worldMatrix.m13, worldMatrix.m23);
        }

        private static void ExtractFrustumPlanes(Matrix4x4 viewProjectionMatrix, Plane[] planes)
        {
            // Left plane
            Vector3 leftNormal = new Vector3(
                viewProjectionMatrix.m30 + viewProjectionMatrix.m00,
                viewProjectionMatrix.m31 + viewProjectionMatrix.m01,
                viewProjectionMatrix.m32 + viewProjectionMatrix.m02);
            float leftDistance = viewProjectionMatrix.m33 + viewProjectionMatrix.m03;
            planes[0] = new Plane(leftNormal.normalized, leftDistance / leftNormal.magnitude);

            // Right plane
            Vector3 rightNormal = new Vector3(
                viewProjectionMatrix.m30 - viewProjectionMatrix.m00,
                viewProjectionMatrix.m31 - viewProjectionMatrix.m01,
                viewProjectionMatrix.m32 - viewProjectionMatrix.m02);
            float rightDistance = viewProjectionMatrix.m33 - viewProjectionMatrix.m03;
            planes[1] = new Plane(rightNormal.normalized, rightDistance / rightNormal.magnitude);

            // Bottom plane
            Vector3 bottomNormal = new Vector3(
                viewProjectionMatrix.m30 + viewProjectionMatrix.m10,
                viewProjectionMatrix.m31 + viewProjectionMatrix.m11,
                viewProjectionMatrix.m32 + viewProjectionMatrix.m12);
            float bottomDistance = viewProjectionMatrix.m33 + viewProjectionMatrix.m13;
            planes[2] = new Plane(bottomNormal.normalized, bottomDistance / bottomNormal.magnitude);

            // Top plane
            Vector3 topNormal = new Vector3(
                viewProjectionMatrix.m30 - viewProjectionMatrix.m10,
                viewProjectionMatrix.m31 - viewProjectionMatrix.m11,
                viewProjectionMatrix.m32 - viewProjectionMatrix.m12);
            float topDistance = viewProjectionMatrix.m33 - viewProjectionMatrix.m13;
            planes[3] = new Plane(topNormal.normalized, topDistance / topNormal.magnitude);

            // Near plane
            Vector3 nearNormal = new Vector3(
                viewProjectionMatrix.m20,
                viewProjectionMatrix.m21,
                viewProjectionMatrix.m22);
            float nearDistance = viewProjectionMatrix.m23;
            planes[4] = new Plane(nearNormal.normalized, nearDistance / nearNormal.magnitude);

            // Far plane
            Vector3 farNormal = new Vector3(
                viewProjectionMatrix.m30 - viewProjectionMatrix.m20,
                viewProjectionMatrix.m31 - viewProjectionMatrix.m21,
                viewProjectionMatrix.m32 - viewProjectionMatrix.m22);
            float farDistance = viewProjectionMatrix.m33 - viewProjectionMatrix.m23;
            planes[5] = new Plane(farNormal.normalized, farDistance / farNormal.magnitude);
        }

        private static bool IsInFrustum(Bounds bounds, Plane[] frustumPlanes)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = frustumPlanes[i];
                Vector3 normal = plane.normal;

                // Get the positive vertex (farthest point in direction of plane normal)
                Vector3 positiveVertex = center + new Vector3(
                    normal.x > 0 ? extents.x : -extents.x,
                    normal.y > 0 ? extents.y : -extents.y,
                    normal.z > 0 ? extents.z : -extents.z);

                // If positive vertex is behind the plane, the bounds is completely outside
                if (plane.GetDistanceToPoint(positiveVertex) < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Performance monitoring - call this to get stats about the last render
        /// </summary>
        public static void GetLastRenderStats(out int meshCount, out int triangleCount)
        {
            meshCount = renderQueue.Count;
            triangleCount = 0;

            foreach (RenderData data in renderQueue)
            {
                if (data.mesh != null && data.submeshIndex < data.mesh.subMeshCount)
                {
                    triangleCount += data.mesh.GetTriangles(data.submeshIndex).Length / 3;
                }
            }
        }

        /// <summary>
        /// Clear all cached data to free memory
        /// </summary>
        public static void ClearCache()
        {
            cachedRenderers.Clear();
            cachedFilters.Clear();
            renderQueue.Clear();
        }
    }
}