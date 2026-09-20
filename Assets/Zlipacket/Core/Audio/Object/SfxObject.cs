using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Zlipacket.Core.Audio.Object
{
    public class SfxObject : MonoBehaviour
    {
        public List<AudioClip> clips;
        
        public float minVolume = 1f;
        public float maxVolume = 1f;
        public float minPitch = 1f;
        public float maxPitch = 1f;

        public bool loop = false;
        public bool useLocation = false;
        
        public bool playOnEnable = false;

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        public void Play()
        {
            if (clips.Count <= 0)
                return;
            
            AudioClip clip = clips[Random.Range(0, clips.Count)];
            float volume = Random.Range(minVolume, maxVolume);
            float pitch = Random.Range(minPitch, maxPitch);
            
            if (!useLocation)
                SfxManager.Instance.PlaySfx(clip, volume, pitch);
            else
                SfxManager.Instance.PlaySfxAtLocation(clip, transform.position, volume, pitch, loop);
        }
    }
}