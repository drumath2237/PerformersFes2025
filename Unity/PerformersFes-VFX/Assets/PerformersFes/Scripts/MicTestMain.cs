using UnityEngine;

namespace PerformersFes
{
    public class MicTestMain : MonoBehaviour
    {
        [SerializeField]
        private AudioDataProvider audioDataProvider;

        private void Start()
        {
            audioDataProvider?.StartAudioSource();
        }
    }
}