using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.Sensor;
using UnityEngine;

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

    public struct StartCameraEventParams
    {
        public Vector2 Resolution;
        public Vector2 FocalPoint;
        public Vector2 OpticalPoint;
    }

    public class KinectTest : MonoBehaviour
    {
        private Device _kinect;

        private bool _isRunning;

        private byte[] _colorImageBuffer;
        private ushort[] _depthImageBuffer;

        private Transformation _cameraTransformation;

        // [SerializeField]
        public Texture2D ColorTexture { get; private set; }

        // [SerializeField]
        public Texture2D DepthTexture { get; private set; }

        public event Action<KinectImageData> OnCapture;

        public event Action<StartCameraEventParams> OnStartCamera;

        private void Start()
        {
            _colorImageBuffer = new byte[1280 * 720 * 4];
            _depthImageBuffer = new ushort[1280 * 720];

            ColorTexture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            DepthTexture = new Texture2D(1280, 720, TextureFormat.R16, false)
            {
                filterMode = FilterMode.Point
            };

            _kinect = Device.Open();

            _kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                ColorResolution = ColorResolution.R720p,
                CameraFPS = FPS.FPS30,
            });

            var intrinsics = _kinect.GetCalibration().ColorCameraCalibration.Intrinsics.Parameters;
            var (cx, cy, fx, fy) = (intrinsics[0], intrinsics[1], intrinsics[2], intrinsics[3]);
            // Debug.Log($"{fx}, {fy}, {cx}, {cy}");
            _cameraTransformation = _kinect.GetCalibration().CreateTransformation();

            _isRunning = true;
            OnCapture += OnCapture_Callback;

            OnStartCamera?.Invoke(new StartCameraEventParams
            {
                Resolution = new Vector2(ColorTexture.width, ColorTexture.height),
                FocalPoint = new Vector2(fx, fy),
                OpticalPoint = new Vector2(cx, cy),
            });

            _ = RunCaptureLoopAsync(destroyCancellationToken);
        }


        private void OnCapture_Callback(KinectImageData imageData)
        {
            // Debug.Log(imageData.Width + " x " + imageData.Height);
            // Debug.Log($"color:{imageData.ColorImageByteSize} depth:{imageData.DepthImageSize}");
            //
            //
            ColorTexture.LoadRawTextureData(_colorImageBuffer);
            ColorTexture.Apply();

            DepthTexture.SetPixelData(_depthImageBuffer, 0);
            DepthTexture.Apply();
        }

        private async Task RunCaptureLoopAsync(
            CancellationToken token
        )
        {
            while (_isRunning && !token.IsCancellationRequested)
            {
                // var sleepTask = Task.Delay(1000 / 15, token);

                var image = await Task.Run(() =>
                {
                    using var capture = _kinect.GetCapture();

                    // var calibration = _kinect.GetCalibration();
                    using var depthImage = _cameraTransformation.DepthImageToColorCamera(capture);
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
                        1280,
                        720,
                        colorImage.Memory.Length,
                        1280 * 720
                    );

                    return kinectImage;
                }, token);

                // await Awaitable.MainThreadAsync();
                OnCapture?.Invoke(image);

                // await sleepTask;
            }
        }

        private void OnDestroy()
        {
            _isRunning = false;
            _cameraTransformation?.Dispose();
            _kinect?.StopCameras();
            _kinect?.Dispose();
            _kinect = null;

            _colorImageBuffer = null;
            _depthImageBuffer = null;

            ColorTexture = null;
            DepthTexture = null;

            OnCapture -= OnCapture_Callback;
        }
    }
}