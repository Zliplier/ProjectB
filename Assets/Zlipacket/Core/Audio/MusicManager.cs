using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Audio
{
    public class MusicManager : Singleton<MusicManager>
    {
        public AudioMixerGroup MixerGroup => AudioManager.Instance.GetMixerGroup(MixerType.Music);
        
        public int initialChannelNumber = 3;
        
        private List<AudioChannel> channels = new List<AudioChannel>();

        public void AudioInitialize()
        {
            for (int i = 0; i < initialChannelNumber; i++)
            {
                channels.Add(AudioChannel.CreateChannel(gameObject, MixerGroup, channels.Count));
            }
        }
        
        public AudioChannel GetChannel(int channelIndex) => channels[channelIndex];

        public AudioChannel PlayMusic(AudioClip clip, int channelIndex = 0, float fadeInDuration = 0f, float fadeOutDuration = 0f, bool loop = true, bool createIfChannelNotExist = false, bool resetSameClip = true)
        {
            if (channelIndex > channels.Count - 1)
            {
                if (createIfChannelNotExist)
                    channels.Add(AudioChannel.CreateChannel(gameObject, MixerGroup, channels.Count));
                else
                {
                    Debug.LogWarning($"Music channel index {channelIndex} is out of range");
                    return null;
                }
            }

            if (channels[channelIndex].audioSource.clip == clip && !resetSameClip)
                return null;
            
            channels[channelIndex].PlayAudio(clip, fadeInDuration, fadeOutDuration, loop);
            
            return channels[channelIndex];
        }
    }
}