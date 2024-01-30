using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class SightStyle
{
    public Color Color = Color.white;
    [Range(0, 1)] public float Alpha = 1;

    public Color FinalColor
    {
        get
        {
            Color c = Color;
            c.a = Alpha;
            return c;
        }
    }
}

public class GetSightDepth : MonoBehaviour
{
    public int RendererIndex;
    public int TexSize = 1024;
    public float HeightOffset = 0.5f;
    [Range(0, 160)] public float SightAngle = 90f;
    public float SightDistance = 20f;

    [Space] public SightStyle SightStyle;

    private GameObject m_SightCamObj;
    private Camera m_SightCamera;
    private GameObject m_SightPlane;
    private RenderTexture m_SightDepthTex;
    private Material m_Material;
    private Shader m_SightShader;

    private void Awake()
    {
        m_SightShader = Shader.Find("Unlit/ConeOfSight");
    }

    private void OnEnable()
    {
        m_SightDepthTex = RenderTexture.GetTemporary(TexSize, 1, 16, RenderTextureFormat.RHalf);
        m_SightDepthTex.filterMode = FilterMode.Point;
        m_SightDepthTex.name = "SightDistanceRT";

        m_SightCamObj = new GameObject("sight camera");
        m_SightCamObj.hideFlags = HideFlags.HideAndDontSave;
        m_SightCamObj.transform.parent = transform;
        m_SightCamObj.transform.localPosition = new Vector3(0, HeightOffset, 0);
        m_SightCamObj.transform.localRotation = Quaternion.identity;
        m_SightCamObj.transform.localScale = Vector3.one;

        m_SightCamera = m_SightCamObj.AddComponent<Camera>();
        m_SightCamera.depth = Camera.main ? Camera.main.depth - 1 : -10;
        m_SightCamera.clearFlags = CameraClearFlags.Color;
        m_SightCamera.backgroundColor = Color.white;
        m_SightCamera.targetTexture = m_SightDepthTex;
        m_SightCamera.enabled = true;
        
        var cameraData = m_SightCamera.AddComponent<UniversalAdditionalCameraData>();
        cameraData.SetRenderer(RendererIndex);
        
        m_Material = new Material(m_SightShader);
        m_Material.SetTexture("_SightDepthTexture", m_SightDepthTex);

        m_SightPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        m_SightPlane.layer = LayerMask.NameToLayer("Sight");
        m_SightPlane.transform.parent = transform;
        m_SightPlane.transform.localPosition = new Vector3(0, HeightOffset, 0);
        m_SightPlane.transform.localEulerAngles = new Vector3(90, 0, 0);
        m_SightPlane.transform.localScale = new Vector3(SightDistance * 2, SightDistance * 2, 1);
        m_SightPlane.GetComponent<MeshRenderer>().sharedMaterial = m_Material;
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(m_SightDepthTex);
        
        DestroyImmediate(m_SightCamObj);
        DestroyImmediate(m_SightPlane);
        DestroyImmediate(m_Material);
    }

    private void Update()
    {
        m_SightCamera.nearClipPlane = 0.01f;
        m_SightCamera.farClipPlane = SightDistance;
        m_SightCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(SightAngle, m_SightCamera.aspect);

        float scale = SightDistance * 2f / transform.lossyScale.x; // 父对象必须缩放统一
        m_SightPlane.transform.localScale = new Vector3(scale, scale, 1);

        float radians = Mathf.Deg2Rad * SightAngle * 0.5f;
        float tanTheta = Mathf.Tan(radians);
        float cosTheta = Mathf.Cos(radians);
        m_Material.SetFloat("_SightAngleTan", tanTheta);
        m_Material.SetFloat("_SightAngleCos", cosTheta);
        m_Material.SetFloat("_PixelSize", 1f / TexSize);
        m_Material.SetColor("_BaseColor", SightStyle.FinalColor);
    }
}
