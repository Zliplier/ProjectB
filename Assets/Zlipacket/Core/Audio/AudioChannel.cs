using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

namespace Zlipacket.Core.Audio
{
    public class AudioChannel : MonoBehaviour
    {
        public AudioSource audioSource;
        public bool IsPlaying => audioSource.isPlaying;

        private Coroutine co_Fade = null;
        public bool IsFading => co_Fade != null;
        
        public static AudioChannel CreateChannel(GameObject owner, AudioMixerGroup mixerGroup, int channelIndex)
        {
            GameObject newObject = new GameObject($"AudioChannel - {channelIndex}");
            newObject.transform.SetParent(owner.transform);
            newObject.transform.localPosition = Vector3.zero;
            
            AudioChannel newChannel = newObject.AddComponent<AudioChannel>();
            newChannel.audioSource = newObject.AddComponent<AudioSource>();
            newChannel.audioSource.playOnAwake = false;
            
            if (mixerGroup != null)
                newChannel.audioSource.outputAudioMixerGroup = mixerGroup;
            
            return newChannel;
        }

        public AudioSource PlayAudio(AudioClip clip, float fadeInDuration = 0f, float fadeOutDuration = 0f, bool loop = true)
        {
            if (IsFading)
                StopCoroutine(co_Fade);
            
            co_Fade = StartCoroutine(FadeInOut(clip, fadeInDuration, fadeOutDuration));
            audioSource.loop = loop;
            
            return audioSource;
        }
        
        public AudioSource StopAudio(float fadeOutDuration = 0f)
        {
            if (fadeOutDuration == 0f)
            {
                audioSource.Stop();
                return audioSource;
            }
            
            if (IsFading)
                StopCoroutine(co_Fade);
            
            co_Fade = StartCoroutine(FadeInOut(null, 0f, fadeOutDuration));
            
            return audioSource;
        }

        private IEnumerator FadeInOut(AudioClip clip, float fadeInDuration, float fadeOutDuration)
        {
            float elapsedTime = 0f;
            float startVolume = 0f;
            float endVolume = 0f;
            
            if (fadeOutDuration > 0f && IsPlaying)
            {
                elapsedTime = 0f;
                startVolume = audioSource.volume;
                endVolume = 0f;
                
                while (elapsedTime < fadeOutDuration)
                {
                    audioSource.volume = Mathf.Lerp(startVolume, endVolume, Mathf.Clamp(elapsedTime / fadeOutDuration, 0f, 1f));
                    
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            if (clip == null)
            {
                co_Fade = null;
                yield break;
            }
                
            audioSource.clip = clip;
            audioSource.Play();

            if (fadeInDuration > 0f)
            {
                elapsedTime = 0f;
                startVolume = 0f;
                endVolume = 1f;
                
                while (elapsedTime < fadeInDuration)
                {
                    audioSource.volume = Mathf.Lerp(startVolume, endVolume, Mathf.Clamp(elapsedTime / fadeInDuration, 0f, 1f));
                    
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            
            co_Fade = null;
        }
    }
}