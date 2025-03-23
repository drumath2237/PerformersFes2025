using System;
using System.Threading.Tasks;
using UnityEngine;

namespace PerformersFes
{
    internal enum AudioProvideType
    {
        Microphone,
        SampleAudioClip
    }

    public class AudioDataProvider : MonoBehaviour
    {
        [Header("Provider Settings")]
        [SerializeField]
        private AudioProvideType audioProvideType = AudioProvideType.Microphone;

        [SerializeField]
        private AudioSource audioSource;

        [Space]
        [Header("Microphone Settings")]
        [SerializeField]
        private string[] micList;


        [SerializeField]
        private int micIndex;

        [Space, Header("Test Audio Data Settings")]
        [SerializeField]
        private AudioClip sampleAudioDataClip;

        private void OnEnable()
        {
            micList = Microphone.devices;
            // audioSource.GetSpectrumData(new[] { 0.0f }, 0, FFTWindow.BlackmanHarris);
            _ = StartAudioSource();
        }

        public async Task StartAudioSource()
        {
            switch (audioProvideType)
            {
                case AudioProvideType.Microphone:
                    if (micList == null || micList.Length == 0)
                    {
                        Debug.LogError("microphones are not found or initialized!");
                        return;
                    }

                    var micName = micList[micIndex];
                    audioSource.clip = Microphone.Start(micName, true, 1, 48_000);
                    await Task.Run(async () =>
                    {
                        while (!(Microphone.GetPosition("") > 0))
                        {
                            await Task.Yield();
                        }
                    });
                    break;
                case AudioProvideType.SampleAudioClip:
                    if (sampleAudioDataClip == null)
                    {
                        Debug.LogError("sample audio data is null!");
                        return;
                    }

                    audioSource.clip = sampleAudioDataClip;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            audioSource.Play();
        }
    }
}