using UnityEngine;
using UnityEngine.Audio;
using Zlipacket.Core.Tools.Extension;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Audio
{
    public class VoiceManager : Singleton<VoiceManager>
    {
        public AudioMixerGroup MixerGroup => AudioManager.Instance.GetMixerGroup(MixerType.Voice);
        
        [SerializeField] private AudioSource audioSource;
        
        public bool IsPlaying => audioSource.isPlaying;

        public void AudioInitialize()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = MixerGroup;
            }
        }
        
        public AudioSource PlayVoiceLine(AudioClip clip, bool voiceCut = true)
        {
            if (IsPlaying && voiceCut)
                audioSource.Stop();

            audioSource.pitch = 1f;
            audioSource.PlayOneShot(clip);
            return audioSource;
        }

        public AudioSource PlayVoiceChar(char c, AudioClip[] clipPool, float minPitch, float maxPitch, bool useHash = true, bool voiceCut = true)
        {
            if (IsPlaying && voiceCut)
                audioSource.Stop();

            float pitch = 1f;
            int clipIndex = 0;
            
            if (useHash)
            {
                int hashCode = c.ComputeFNV1aHash();
                
                clipIndex = hashCode % clipPool.Length;

                int minPitchInt = (int)(minPitch * 100);
                int maxPitchInt = (int)(maxPitch * 100);
                int pitchInt = maxPitchInt - minPitchInt;
                
                pitch = pitchInt != 0 ? ((hashCode % pitchInt) + minPitchInt) / 100f : minPitch;
            }
            else
            {
                pitch = Random.Range(minPitch, maxPitch);
                clipIndex = Random.Range(0, clipPool.Length);
            }
            
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clipPool[clipIndex]);
            return audioSource;
        }
    }
}