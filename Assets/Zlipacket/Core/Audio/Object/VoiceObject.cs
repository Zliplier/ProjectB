using System.Collections.Generic;
using UnityEngine;

namespace Zlipacket.Core.Audio.Object
{
    public class VoiceObject : MonoBehaviour
    {
        public List<AudioClip> voiceLines;
        
        public float minVolume = 1f;
        public float maxVolume = 1f;
        public float minPitch = 1f;
        public float maxPitch = 1f;

        public bool loop = false;
        public bool useLocation = false;
        
        public void Play()
        {
        }
    }
}