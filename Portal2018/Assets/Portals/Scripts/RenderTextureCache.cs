using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderTextureCache : MonoBehaviour
{
    [System.Serializable]
    public struct RTSpec
    {
        public int width, height, depth;
        public RenderTextureFormat format;

        public RTSpec(int w, int h, int d, RenderTextureFormat f)
        {
            width = w; height = h; depth = d; format = f;
        }

        public override int GetHashCode()
        {
            return width ^ (height << 8) ^ (depth << 16) ^ ((int)format << 24);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RTSpec)) return false;
            RTSpec other = (RTSpec)obj;
            return width == other.width && height == other.height &&
                   depth == other.depth && format == other.format;
        }
    }

    private static RenderTextureCache _instance;
    public static RenderTextureCache Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("RenderTextureCache");
                _instance = go.AddComponent<RenderTextureCache>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Pool of available RTs by specification
    private Dictionary<RTSpec, Queue<RenderTexture>> _availableRTs = new Dictionary<RTSpec, Queue<RenderTexture>>();

    // Currently rented RTs (for tracking)
    private HashSet<RenderTexture> _rentedRTs = new HashSet<RenderTexture>();

    // Vita-specific limits
    private const int MAX_RT_WIDTH = 512;
    private const int MAX_RT_HEIGHT = 512;
    private const int MAX_CACHED_RTS_PER_SPEC = 12;  // Vita has limited VRAM
    private const int TOTAL_RT_LIMIT = 12;  // Total RT limit for Vita

    // Cached values to avoid repeated calculations
    private static int _cachedScreenWidth = -1;
    private static int _cachedScreenHeight = -1;
    private static int _cachedDepth = -1;
    private static RenderTextureFormat _cachedFormat = RenderTextureFormat.Default;

    public RenderTexture GetRT(int width, int height, int depthBufferQuality, bool allowHDR = false)
    {
        // Refresh cache if needed
        if (_cachedScreenWidth == -1)
        {
            RefreshCachedValues(depthBufferQuality, allowHDR);
        }

        // Calculate dimensions with Vita optimizations
        int w = width;
        int h = height;

        /*
        // Force power-of-2 for Vita GPU efficiency
        w = Mathf.NextPowerOfTwo(w);
        h = Mathf.NextPowerOfTwo(h);
        */
        RTSpec spec = new RTSpec(w, h, _cachedDepth, _cachedFormat);

        // Try to get from cache first
        if (_availableRTs.ContainsKey(spec) && _availableRTs[spec].Count > 0)
        {
            RenderTexture rt = _availableRTs[spec].Dequeue();
            _rentedRTs.Add(rt);
            return rt;
        }

        // Create new RT if cache miss
        RenderTexture newRT = CreateNewRT(spec);
        _rentedRTs.Add(newRT);
        return newRT;
    }

    public RenderTexture GetRT(int downscaling, int depthBufferQuality, bool allowHDR = false)
    {
        // Refresh cache if needed
        if (_cachedScreenWidth == -1)
        {
            RefreshCachedValues(depthBufferQuality, allowHDR);
        }

        // Calculate dimensions with Vita optimizations
        int w = Mathf.Min(_cachedScreenWidth / downscaling, MAX_RT_WIDTH);
        int h = Mathf.Min(_cachedScreenHeight / downscaling, MAX_RT_HEIGHT);

        //Actually what we could do for the Vita is enact some more aggressive case-handling for this...
        if (downscaling == 2)
        {
            w = 256;
            h = 256;
        }

        // Force power-of-2 for Vita GPU efficiency
        w = Mathf.NextPowerOfTwo(w);
        h = Mathf.NextPowerOfTwo(h);

        RTSpec spec = new RTSpec(w, h, _cachedDepth, _cachedFormat);

        // Try to get from cache first
        if (_availableRTs.ContainsKey(spec) && _availableRTs[spec].Count > 0)
        {
            RenderTexture rt = _availableRTs[spec].Dequeue();
            _rentedRTs.Add(rt);
            return rt;
        }

        // Create new RT if cache miss
        RenderTexture newRT = CreateNewRT(spec);
        _rentedRTs.Add(newRT);
        return newRT;
    }

    public RenderTexture GetUniqueRT(int downscaling, int depthBufferQuality, bool allowHDR = false)
    {
        // Refresh cache if needed
        if (_cachedScreenWidth == -1)
        {
            RefreshCachedValues(depthBufferQuality, allowHDR);
        }

        // Calculate dimensions with Vita optimizations
        int w = Mathf.Min(_cachedScreenWidth / downscaling, MAX_RT_WIDTH);
        int h = Mathf.Min(_cachedScreenHeight / downscaling, MAX_RT_HEIGHT);

        //Actually what we could do for the Vita is enact some more aggressive case-handling for this...
        if (downscaling == 2)
        {
            w = 256;
            h = 256;
        }

        // Force power-of-2 for Vita GPU efficiency
        w = Mathf.NextPowerOfTwo(w);
        h = Mathf.NextPowerOfTwo(h);

        RTSpec spec = new RTSpec(w, h, _cachedDepth, _cachedFormat);

        // Try to get from cache first
        /*
        if (_availableRTs.ContainsKey(spec) && _availableRTs[spec].Count > 0)
        {
            RenderTexture rt = _availableRTs[spec].Dequeue();
            _rentedRTs.Add(rt);
            return rt;
        }*/

        // Create new RT if cache miss
        RenderTexture newRT = CreateNewRT(spec);
        _rentedRTs.Add(newRT);
        return newRT;
    }

    public void ReturnRT(RenderTexture rt)
    {
        if (rt == null || !_rentedRTs.Contains(rt)) return;

        _rentedRTs.Remove(rt);

        RTSpec spec = new RTSpec(rt.width, rt.height, rt.depth, rt.format);

        // Add to cache if we have room
        if (!_availableRTs.ContainsKey(spec))
        {
            _availableRTs[spec] = new Queue<RenderTexture>();
        }

        if (_availableRTs[spec].Count < MAX_CACHED_RTS_PER_SPEC &&
            GetTotalCachedRTCount() < TOTAL_RT_LIMIT)
        {

            // DON'T clear the RT - this might be breaking the blit chain
            // The RT should retain its content for potential reuse

            _availableRTs[spec].Enqueue(rt);
        }
        else
        {
            // Cache is full, release it properly using GetTemporary's release method
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    private RenderTexture CreateNewRT(RTSpec spec)
    {
        // CRITICAL: Use the same method as GetTemporary to ensure compatibility
        RenderTexture rt = RenderTexture.GetTemporary(
            spec.width, spec.height, spec.depth, spec.format,
            RenderTextureReadWrite.Default,
            1,  // antiAliasing
            RenderTextureMemoryless.None,
            VRTextureUsage.None,
            false  // useDynamicScale
        );

        // Apply the same settings that GetTemporary would use
        rt.filterMode = FilterMode.Point;  // GetTemporary default
        rt.wrapMode = TextureWrapMode.Clamp;

        // DO NOT call rt.Create() - GetTemporary already creates it
        return rt;
    }

    private void RefreshCachedValues(int depthBufferQuality, bool allowHDR)
    {
        var currentCam = Camera.current;
        if (currentCam != null)
        {
            _cachedScreenWidth = currentCam.pixelWidth;
            _cachedScreenHeight = currentCam.pixelHeight;
        }
        else
        {
            _cachedScreenWidth = Screen.width;
            _cachedScreenHeight = Screen.height;
        }

        _cachedDepth = depthBufferQuality;

#if UNITY_PSP2
        _cachedFormat = RenderTextureFormat.RGB565;  // 16-bit saves VRAM on Vita
#else
        _cachedFormat = allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
#endif
    }

    private int GetTotalCachedRTCount()
    {
        int count = 0;
        foreach (var queue in _availableRTs.Values)
        {
            count += queue.Count;
        }
        return count;
    }

    // Call this when screen resolution changes
    public void InvalidateCache()
    {
        _cachedScreenWidth = -1;
    }

    // Clean up cache (call this during scene transitions)
    public void ClearCache()
    {
        foreach (var queue in _availableRTs.Values)
        {
            while (queue.Count > 0)
            {
                RenderTexture.ReleaseTemporary(queue.Dequeue());
            }
        }
        _availableRTs.Clear();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ClearCache();  // Free VRAM when app is backgrounded
        }
    }

    void OnDestroy()
    {
        ClearCache();
    }
}

/*
// Modified portal method to use the cache
public class PortalRenderer : MonoBehaviour
{
    private RenderTexture GetTemporaryRT()
    {
        return RenderTextureCache.Instance.GetRT(
            _portal.Downscaling,
            (int)_portal.DepthBufferQuality,
            _camera.allowHDR
        );
    }

    private void ReturnTemporaryRT(RenderTexture rt)
    {
        RenderTextureCache.Instance.ReturnRT(rt);
    }

    // Make sure to call ReturnTemporaryRT when done with the RT!
    // This would go in your portal rendering cleanup code
}
*/