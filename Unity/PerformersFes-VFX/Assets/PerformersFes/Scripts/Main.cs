using UnityEngine;

namespace PerformersFes
{
    public class Main : MonoBehaviour
    {
        [SerializeField]
        private KinectBehaviour kinect;

        [SerializeField]
        private KinectVFXController kinectVFXController;

        [SerializeField]
        private AudioDataProvider audioDataProvider;


        private async void Start()
        {
            if (kinect == null || kinectVFXController == null)
            {
                Debug.LogError("kinect assets are not attached correctly");
                return;
            }

            if (audioDataProvider == null)
            {
                Debug.LogError("audio data provider is not attached!");
                return;
            }

            kinect.OnStartCamera += OnStartKinectCamera_InitializeVFXProperties;

            await audioDataProvider.StartAudioSource();
        }

        private void OnDestroy()
        {
            kinect.OnStartCamera -= OnStartKinectCamera_InitializeVFXProperties;
        }

        private void OnStartKinectCamera_InitializeVFXProperties(StartCameraEventParams @params)
        {
            kinectVFXController.SetKinectProperties(
                @params,
                kinect.ColorTexture,
                kinect.DepthTexture
            );
        }
    }
}