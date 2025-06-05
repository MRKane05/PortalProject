using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]

[RequireComponent(typeof(Renderer))]
public class LightProbeSampler : MonoBehaviour
{
    public List<Renderer> m_Renderers = new List<Renderer>();
    private MaterialPropertyBlock m_PropertyBlock;

    // Cache the property IDs for better performance
    private static readonly int SHAr_ID = Shader.PropertyToID("_SHAr");
    private static readonly int SHAg_ID = Shader.PropertyToID("_SHAg");
    private static readonly int SHAb_ID = Shader.PropertyToID("_SHAb");
    private static readonly int SHBr_ID = Shader.PropertyToID("_SHBr");
    private static readonly int SHBg_ID = Shader.PropertyToID("_SHBg");
    private static readonly int SHBb_ID = Shader.PropertyToID("_SHBb");
    private static readonly int SHC_ID = Shader.PropertyToID("_SHC");

    [Header("Settings")]
    [Tooltip("How often to update the light probe sampling (in seconds)")]
    public float updateInterval = 0.1f;

    [Tooltip("Use object center or a custom sample position")]
    public bool useCustomSamplePosition = false;

    [Tooltip("Custom position to sample light probes from")]
    public Transform customSamplePosition;

    private float m_LastUpdateTime;

    public float updateDistance = 0.125f;    //Update every eigths of a meter we move
    float squareUpdateDistance = 0.0625f;
    Vector3 lastUpdatePosition = Vector3.zero;


    void Start()
    {
        squareUpdateDistance = updateDistance * updateDistance;
        if (m_Renderers.Count == 0)
        {
            m_Renderers.Add(gameObject.GetComponent<Renderer>());
        }
        m_PropertyBlock = new MaterialPropertyBlock();

        // Initial sampling
        SampleLightProbes();
    }

    void Update()
    {
        // Update at specified interval for performance
        //if (Time.time - m_LastUpdateTime >= updateInterval)
        //Alternatively only update if we've moved
        if (Vector3.SqrMagnitude(lastUpdatePosition - transform.position) > squareUpdateDistance)
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

        // Sample light probes at the object's position
        SphericalHarmonicsL2 sh;
        Vector3[] positions = { samplePosition };
        SphericalHarmonicsL2[] lightProbes = new SphericalHarmonicsL2[1];

        LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, lightProbes, null);
        sh = lightProbes[0];

        // Convert SphericalHarmonicsL2 to the format Unity shaders expect
        // This matches Unity's internal SH representation

        // SH coefficients for red, green, blue channels
        Vector4 SHAr = new Vector4(sh[0, 3], sh[0, 1], sh[0, 2], sh[0, 0] - sh[0, 6]);
        Vector4 SHAg = new Vector4(sh[1, 3], sh[1, 1], sh[1, 2], sh[1, 0] - sh[1, 6]);
        Vector4 SHAb = new Vector4(sh[2, 3], sh[2, 1], sh[2, 2], sh[2, 0] - sh[2, 6]);

        Vector4 SHBr = new Vector4(sh[0, 4], sh[0, 6], sh[0, 5] * 3, sh[0, 7]);
        Vector4 SHBg = new Vector4(sh[1, 4], sh[1, 6], sh[1, 5] * 3, sh[1, 7]);
        Vector4 SHBb = new Vector4(sh[2, 4], sh[2, 6], sh[2, 5] * 3, sh[2, 7]);

        Vector4 SHC = new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1.0f);

        if (m_PropertyBlock == null)
        {
            m_PropertyBlock = new MaterialPropertyBlock();
        }
        // Pass all SH coefficients to shader
        m_PropertyBlock.SetVector(SHAr_ID, SHAr);
        m_PropertyBlock.SetVector(SHAg_ID, SHAg);
        m_PropertyBlock.SetVector(SHAb_ID, SHAb);
        m_PropertyBlock.SetVector(SHBr_ID, SHBr);
        m_PropertyBlock.SetVector(SHBg_ID, SHBg);
        m_PropertyBlock.SetVector(SHBb_ID, SHBb);
        m_PropertyBlock.SetVector(SHC_ID, SHC);

        for (int i = 0; i < m_Renderers.Count; i++)
        {
            m_Renderers[i].SetPropertyBlock(m_PropertyBlock);
        }
    }


    // Optional: Update immediately when object moves (for moving objects)
    void OnTransformParentChanged()
    {
        SampleLightProbes();
    }
}