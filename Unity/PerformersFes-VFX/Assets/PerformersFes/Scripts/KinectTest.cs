using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.Sensor;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace PerformersFes
{
    public struct KinectImageData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int ColorImageByteSize;
        public readonly int DepthImageSize;

        public KinectImageData(int width, int height, int colorImageByteSize, int depthImageSize)
        {
            Width = width;
            Height = height;
            ColorImageByteSize = colorImageByteSize;
            DepthImageSize = depthImageSize;
        }
    }

    public class KinectTest : MonoBehaviour
    {
        private Device _kinect;

        private bool _isRunning;

        private byte[] _colorImageBuffer;
        private ushort[] _depthImageBuffer;

        // [SerializeField]
        private Texture2D colorTexture;

        // [SerializeField]
        private Texture2D depthTexture;

        public event Action<KinectImageData> OnCapture;

        private void Start()
        {
            _colorImageBuffer = new byte[1280 * 720 * 4];
            _depthImageBuffer = new ushort[1280 * 720];

            colorTexture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            depthTexture = new Texture2D(1280, 720, TextureFormat.R16, false);

            _kinect = Device.Open();

            _kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = true,
                ColorResolution = ColorResolution.R720p,
                CameraFPS = FPS.FPS30,
            });

            var intrinsics = _kinect.GetCalibration().ColorCameraCalibration.Intrinsics.Parameters;
            var (cx, cy, fx, fy) = (intrinsics[0], intrinsics[1], intrinsics[2], intrinsics[3]);
            Debug.Log($"{fx}, {fy}, {cx}, {cy}");

            _isRunning = true;
            OnCapture += OnCapture_Callback;

            _ = RunCaptureLoopAsync(destroyCancellationToken);
        }


        private void OnCapture_Callback(KinectImageData imageData)
        {
            // Debug.Log(imageData.Width + " x " + imageData.Height);
            // Debug.Log($"color:{imageData.ColorImageByteSize} depth:{imageData.DepthImageSize}");
            //
            //
            colorTexture.LoadRawTextureData(_colorImageBuffer);
            colorTexture.Apply();

            depthTexture.SetPixelData(_depthImageBuffer, 0);
            depthTexture.Apply();
        }

        private async Task RunCaptureLoopAsync(
            CancellationToken token
        )
        {
            while (_isRunning && !token.IsCancellationRequested)
            {
                var sleepTask = Task.Delay(1000 / 15, token);

                var image = await Task.Run(() =>
                {
                    using var capture = _kinect.GetCapture();

                    var calibration = _kinect.GetCalibration();
                    using var transformation = calibration.CreateTransformation();
                    using var depthImage = transformation.DepthImageToColorCamera(capture);
                    using var colorImage = capture.Color;

                    depthImage.GetPixels<ushort>().CopyTo(_depthImageBuffer);
                    var colorSpan = colorImage.GetPixels<BGRA>().Span;
                    for (var i = 0; i < colorSpan.Length; i++)
                    {
                        _colorImageBuffer[i * 4 + 0] = colorSpan[i].R;
                        _colorImageBuffer[i * 4 + 1] = colorSpan[i].G;
                        _colorImageBuffer[i * 4 + 2] = colorSpan[i].B;
                        _colorImageBuffer[i * 4 + 3] = colorSpan[i].A;
                    }

                    var kinectImage = new KinectImageData(
                        calibration.ColorCameraCalibration.ResolutionWidth,
                        calibration.ColorCameraCalibration.ResolutionHeight,
                        colorImage.Memory.Length,
                        1280 * 720
                    );

                    return kinectImage;
                }, token);

                // await Awaitable.MainThreadAsync();
                OnCapture?.Invoke(image);

                await sleepTask;
            }
        }

        private void OnDestroy()
        {
            _isRunning = false;
            _kinect?.StopCameras();
            _kinect?.Dispose();
            _kinect = null;

            _colorImageBuffer = null;
            _depthImageBuffer = null;

            colorTexture = null;
            depthTexture = null;

            OnCapture -= OnCapture_Callback;
        }
    }
}