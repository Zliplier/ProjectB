using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        public const float DEFAULT_VOLUME = 0.7f;
        
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private MusicManager musicManager;
        [SerializeField] private SfxManager sfxManager;
        [SerializeField] private VoiceManager voiceManager;

        public override void Awake()
        {
            base.Awake();
            musicManager?.AudioInitialize();
            sfxManager?.AudioInitialize();
            voiceManager?.AudioInitialize();
        }

        private void Start()
        {
            foreach (MixerType type in Enum.GetValues(typeof(MixerType)))
                SetVolume(type, GetVolume(type));
        }

        public float GetVolume(MixerType type)
        {
            float volume = DEFAULT_VOLUME;  
            
            if (PlayerPrefs.HasKey(type.ToString()))
                volume = PlayerPrefs.GetFloat(type.ToString());
            else
            {
                //No Saved Volume Data
                SetVolume(type, volume);
            }
            
            return volume;
        }

        public void SetVolume(MixerType type, float volume)
        {
            PlayerPrefs.SetFloat(type.ToString(), volume);

            float dB = volume > 0.0001f 
                ? Mathf.Log10(volume) * 20f 
                : -80f;

            audioMixer.SetFloat(type.ToString(), dB);
        }

        public AudioMixerGroup GetMixerGroup(MixerType type)
            => audioMixer.FindMatchingGroups(type.ToString()).FirstOrDefault();
    }
    
    public enum MixerType
    {
        Master,
        Music,
        Sfx,
        Voice
    }
}
