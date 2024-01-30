using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DefaultNamespace
{
    public class GetSightDepthRounded : MonoBehaviour
    {
        public int RendererIndex;
        public int TexSize = 1024;
        public float HeightOffset = 0.5f;
        [Range(0, 160)] public float SightAngle = 90f;
        public float SightDistance = 20f;

        [Space] public SightStyle SightStyle;

        private List<GameObject> m_SightCameraObjList = new List<GameObject>();
        private List<Camera> m_SightCameraList = new List<Camera>();
        // private GameObject m_SightCamObj;
        // private Camera m_SightCamera;
        private RenderTexture m_SightDepthTex;
        private GameObject m_SightPlane;
        private Material m_Material;
        private Shader m_SightShader;
        
        private void Awake()
        {
            m_SightShader = Shader.Find("Unlit/ConeOfSight");
        }
        
        private void OnEnable()
        {
            m_Material = new Material(m_SightShader);
            
            m_SightDepthTex = RenderTexture.GetTemporary(TexSize, 4, 16, RenderTextureFormat.RHalf);
            m_SightDepthTex.filterMode = FilterMode.Point;
            m_SightDepthTex.name = $"_SightDepthTexture";
            
            for (int i = 0; i < 4; i++)
            {
                GameObject sightCamObj = new GameObject("sight camera");
                // sightCamObj.hideFlags = HideFlags.HideAndDontSave;
                sightCamObj.transform.parent = transform;
                sightCamObj.transform.localPosition = new Vector3(0, HeightOffset, 0);
                sightCamObj.transform.localScale = Vector3.one;

                Camera sightCamera = sightCamObj.AddComponent<Camera>();
                sightCamera.enabled = true;
                sightCamera.depth = Camera.main ? Camera.main.depth - 1 : -10;
                sightCamera.clearFlags = CameraClearFlags.Color;
                sightCamera.backgroundColor = Color.white;
                sightCamera.targetTexture = m_SightDepthTex;
                sightCamera.nearClipPlane = 0.01f;
                sightCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(90, sightCamera.aspect);
                sightCamera.rect = new Rect(0, 0.25f * i, 1, 0.25f);
        
                var cameraData = sightCamera.AddComponent<UniversalAdditionalCameraData>();
                cameraData.SetRenderer(RendererIndex);

                m_SightCameraObjList.Add(sightCamObj);
                m_SightCameraList.Add(sightCamera);
            }
            
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
            DestroyImmediate(m_SightPlane);
            DestroyImmediate(m_Material);

            for (int i = 0; i < 4; i++)
            {
                DestroyImmediate(m_SightCameraObjList[i]);
            }
            m_SightCameraObjList.Clear();
            m_SightCameraList.Clear();
        }

        private void Update()
        {
            for (int i = 0; i < 4; i++)
            {
                m_SightCameraList[i].farClipPlane = SightDistance;
                m_SightCameraObjList[i].transform.eulerAngles = new Vector3(0, 90 * i, 0);
            }

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
}