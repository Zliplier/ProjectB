using System;
using UnityEngine;

namespace Zlipacket.Core.Audio.Object
{
    public class MusicObject : MonoBehaviour
    {
        public AudioClip music;
        public int channelIndex = 0;
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 0.5f;
        public bool loop = true;
        public bool createIfChannelNotExist = false;
        public bool resetSameClip = true;
        public bool playOnStart = true;

        private void Start()
        {
            if (playOnStart && music != null)
                PlayMusic();
        }

        public void PlayMusic()
            => MusicManager.Instance.PlayMusic(music, channelIndex, fadeInDuration, fadeOutDuration, loop, createIfChannelNotExist, resetSameClip);

        public void StopMusic(float fadeOutDuration = 0f)
            => MusicManager.Instance.GetChannel(channelIndex).StopAudio(fadeOutDuration);
    }
}