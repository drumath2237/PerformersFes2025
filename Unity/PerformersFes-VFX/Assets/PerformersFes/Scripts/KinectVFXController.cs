using UnityEngine;
using UnityEngine.VFX;

namespace PerformersFes
{
    public class KinectVFXController : MonoBehaviour
    {
        [SerializeField]
        private VisualEffect kinectVFXGraph;

        private readonly int _resolutionShaderProperty = Shader.PropertyToID("Resolution");
        private readonly int _depthTextureProperty = Shader.PropertyToID("DepthTexture");
        private readonly int _colorTextureProperty = Shader.PropertyToID("ColorTexture");
        private readonly int _focalPointProperty = Shader.PropertyToID("FocalPoint");
        private readonly int _opticalPointProperty = Shader.PropertyToID("OpticalPoint");


        public void SetKinectProperties(StartCameraEventParams @params, Texture2D colorTexture, Texture2D depthTexture)
        {
            if (kinectVFXGraph == null)
            {
                Debug.LogError("kinect vfx asset not attached!");
                return;
            }

            kinectVFXGraph.SetVector2(_resolutionShaderProperty, @params.Resolution);
            kinectVFXGraph.SetVector2(_focalPointProperty, @params.FocalPoint);
            kinectVFXGraph.SetVector2(_opticalPointProperty, @params.OpticalPoint);
            kinectVFXGraph.SetTexture(_depthTextureProperty, depthTexture);
            kinectVFXGraph.SetTexture(_colorTextureProperty, colorTexture);
        }
    }
}