using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class LightProbeDualSampler : MonoBehaviour
{
    private Renderer m_Renderer;
    private MaterialPropertyBlock m_PropertyBlock;

    // Cache the property IDs for better performance
    private static readonly int SHAr_ID = Shader.PropertyToID("_SHAr");
    private static readonly int SHAg_ID = Shader.PropertyToID("_SHAg");
    private static readonly int SHAb_ID = Shader.PropertyToID("_SHAb");
    private static readonly int SHBr_ID = Shader.PropertyToID("_SHBr");
    private static readonly int SHBg_ID = Shader.PropertyToID("_SHBg");
    private static readonly int SHBb_ID = Shader.PropertyToID("_SHBb");
    private static readonly int SHC_ID = Shader.PropertyToID("_SHC");

    // Second set of SH coefficients for interpolation
    private static readonly int SHAr2_ID = Shader.PropertyToID("_SHAr2");
    private static readonly int SHAg2_ID = Shader.PropertyToID("_SHAg2");
    private static readonly int SHAb2_ID = Shader.PropertyToID("_SHAb2");
    private static readonly int SHBr2_ID = Shader.PropertyToID("_SHBr2");
    private static readonly int SHBg2_ID = Shader.PropertyToID("_SHBg2");
    private static readonly int SHBb2_ID = Shader.PropertyToID("_SHBb2");
    private static readonly int SHC2_ID = Shader.PropertyToID("_SHC2");

    // Interpolation parameter
    private static readonly int LightT_ID = Shader.PropertyToID("_LightT");

    [Header("Settings")]
    [Tooltip("How often to update the light probe sampling (in seconds)")]
    public float updateInterval = 0.1f;

    [Tooltip("Use object center or a custom sample position")]
    public bool useCustomSamplePosition = false;

    [Tooltip("Custom position to sample light probes from")]
    public Transform customSamplePosition;

    [Header("Dual Probe Interpolation")]
    [Tooltip("Enable sampling from a second light probe position")]
    public bool useDualProbes = false;

    [Tooltip("Second position to sample light probes from")]
    public Transform secondSamplePosition;

    [Tooltip("Interpolation value between first (0) and second (1) probe")]
    [Range(0f, 1f)]
    public float lightT = 0f;

    [Tooltip("Allow external scripts to control lightT")]
    public bool allowExternalControl = true;

    private float m_LastUpdateTime;

    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        m_PropertyBlock = new MaterialPropertyBlock();

        // Initial sampling
        SampleLightProbes();
    }

    void Update()
    {
        // Update at specified interval for performance
        if (Time.time - m_LastUpdateTime >= updateInterval)
        {
            SampleLightProbes();
            m_LastUpdateTime = Time.time;
        }
    }

    void SampleLightProbes()
    {
        // Determine sample position
        Vector3 samplePosition = useCustomSamplePosition && customSamplePosition != null
            ? customSamplePosition.position
            : transform.position;

        // Sample first light probe
        SphericalHarmonicsL2 sh1 = SampleProbeAtPosition(samplePosition);

        // Convert first SH to shader format
        Vector4 SHAr, SHAg, SHAb, SHBr, SHBg, SHBb, SHC;
        ConvertSHToShaderFormat(sh1, out SHAr, out SHAg, out SHAb, out SHBr, out SHBg, out SHBb, out SHC);

        // Pass first set of SH coefficients to shader
        m_PropertyBlock.SetVector(SHAr_ID, SHAr);
        m_PropertyBlock.SetVector(SHAg_ID, SHAg);
        m_PropertyBlock.SetVector(SHAb_ID, SHAb);
        m_PropertyBlock.SetVector(SHBr_ID, SHBr);
        m_PropertyBlock.SetVector(SHBg_ID, SHBg);
        m_PropertyBlock.SetVector(SHBb_ID, SHBb);
        m_PropertyBlock.SetVector(SHC_ID, SHC);

        // Handle dual probe sampling
        if (useDualProbes && secondSamplePosition != null)
        {
            // Sample second light probe
            SphericalHarmonicsL2 sh2 = SampleProbeAtPosition(secondSamplePosition.position);

            // Convert second SH to shader format
            Vector4 SHAr2, SHAg2, SHAb2, SHBr2, SHBg2, SHBb2, SHC2;
            ConvertSHToShaderFormat(sh2, out SHAr2, out SHAg2, out SHAb2, out SHBr2, out SHBg2, out SHBb2, out SHC2);

            // Pass second set of SH coefficients to shader
            m_PropertyBlock.SetVector(SHAr2_ID, SHAr2);
            m_PropertyBlock.SetVector(SHAg2_ID, SHAg2);
            m_PropertyBlock.SetVector(SHAb2_ID, SHAb2);
            m_PropertyBlock.SetVector(SHBr2_ID, SHBr2);
            m_PropertyBlock.SetVector(SHBg2_ID, SHBg2);
            m_PropertyBlock.SetVector(SHBb2_ID, SHBb2);
            m_PropertyBlock.SetVector(SHC2_ID, SHC2);

            // Pass interpolation parameter
            m_PropertyBlock.SetFloat(LightT_ID, lightT);
        }
        else
        {
            // When not using dual probes, set lightT to 0 (use only first probe)
            m_PropertyBlock.SetFloat(LightT_ID, 0f);
        }

        m_Renderer.SetPropertyBlock(m_PropertyBlock);
    }

    private SphericalHarmonicsL2 SampleProbeAtPosition(Vector3 position)
    {
        Vector3[] positions = { position };
        SphericalHarmonicsL2[] lightProbes = new SphericalHarmonicsL2[1];

        LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, lightProbes, null);
        return lightProbes[0];
    }

    private void ConvertSHToShaderFormat(SphericalHarmonicsL2 sh, out Vector4 SHAr, out Vector4 SHAg, out Vector4 SHAb,
                                       out Vector4 SHBr, out Vector4 SHBg, out Vector4 SHBb, out Vector4 SHC)
    {
        // Convert SphericalHarmonicsL2 to the format Unity shaders expect
        SHAr = new Vector4(sh[0, 3], sh[0, 1], sh[0, 2], sh[0, 0] - sh[0, 6]);
        SHAg = new Vector4(sh[1, 3], sh[1, 1], sh[1, 2], sh[1, 0] - sh[1, 6]);
        SHAb = new Vector4(sh[2, 3], sh[2, 1], sh[2, 2], sh[2, 0] - sh[2, 6]);

        SHBr = new Vector4(sh[0, 4], sh[0, 6], sh[0, 5] * 3, sh[0, 7]);
        SHBg = new Vector4(sh[1, 4], sh[1, 6], sh[1, 5] * 3, sh[1, 7]);
        SHBb = new Vector4(sh[2, 4], sh[2, 6], sh[2, 5] * 3, sh[2, 7]);

        SHC = new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1.0f);
    }

    // Public method for external scripts to control lightT
    public void SetLightT(float value)
    {
        if (allowExternalControl)
        {
            lightT = Mathf.Clamp01(value);
        }
    }

    // Public method to get current lightT value
    public float GetLightT()
    {
        return lightT;
    }


    // Optional: Update immediately when object moves (for moving objects)
    void OnTransformParentChanged()
    {
        SampleLightProbes();
    }
}