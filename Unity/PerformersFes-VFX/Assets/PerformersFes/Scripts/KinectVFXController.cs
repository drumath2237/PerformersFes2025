using UnityEngine;
using UnityEngine.VFX;

namespace PerformersFes
{
    public class KinectVFXController : MonoBehaviour
    {
        [SerializeField]
        private KinectTest kinectBehaviour;

        [SerializeField]
        private VisualEffect kinectVFXGraph;

        private readonly int _resolutionShaderProperty = Shader.PropertyToID("Resolution");
        private readonly int _depthTextureProperty = Shader.PropertyToID("DepthTexture");
        private readonly int _colorTextureProperty = Shader.PropertyToID("ColorTexture");
        private readonly int _focalPointProperty = Shader.PropertyToID("FocalPoint");
        private readonly int _opticalPointProperty = Shader.PropertyToID("OpticalPoint");

        private void Start()
        {
            if (kinectBehaviour == null || kinectVFXGraph == null)
            {
                Debug.LogError("asset not attached correctly");
                return;
            }

            kinectBehaviour.OnStartCamera += OnCameraStarted_SetVFXAttributes;
        }

        private void OnDestroy()
        {
            kinectBehaviour.OnStartCamera -= OnCameraStarted_SetVFXAttributes;
        }

        private void OnCameraStarted_SetVFXAttributes(StartCameraEventParams @params)
        {
            kinectVFXGraph.SetVector2(_resolutionShaderProperty, @params.Resolution);
            kinectVFXGraph.SetVector2(_focalPointProperty, @params.FocalPoint);
            kinectVFXGraph.SetVector2(_opticalPointProperty, @params.OpticalPoint);
            kinectVFXGraph.SetTexture(_depthTextureProperty, kinectBehaviour.DepthTexture);
            kinectVFXGraph.SetTexture(_colorTextureProperty, kinectBehaviour.ColorTexture);
        }
    }
}