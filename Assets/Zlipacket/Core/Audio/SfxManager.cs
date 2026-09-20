using UnityEngine;
using UnityEngine.Audio;
using Zlipacket.Core.Tools.Extension;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Audio
{
    public class SfxManager : Singleton<SfxManager>
    {
        public AudioMixerGroup MixerGroup => AudioManager.Instance.GetMixerGroup(MixerType.Sfx);

        public void AudioInitialize()
        {
            
        }
        
        public AudioSource PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            return PlaySfxAtLocation(clip, transform.position, volume, pitch, loop);
        }
        
        public AudioSource PlaySfxAtLocation(AudioClip clip, Vector3 location, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            GameObject newObject = new GameObject($"Sfx - {clip.name}");
            newObject.transform.SetParent(transform);
            newObject.transform.position = location;
            AudioSource newSfx = newObject.AddComponent<AudioSource>();
            newSfx.outputAudioMixerGroup = MixerGroup;
            newSfx.playOnAwake = false;
            newSfx.clip = clip;
            newSfx.volume = volume;
            newSfx.pitch = pitch;
            newSfx.loop = loop;
            newSfx.Play();
            
            if (!loop)
                Destroy(newSfx.gameObject, clip.length);
            
            return newSfx;
        }

        public void StopAllSfx()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}