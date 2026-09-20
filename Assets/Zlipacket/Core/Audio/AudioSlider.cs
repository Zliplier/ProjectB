using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zlipacket.Core.Audio
{
    public class AudioSlider : MonoBehaviour, IPointerUpHandler
    {
        [field: SerializeField] public Slider slider { get; private set; }
             
        public MixerType mixerType = MixerType.Master;
        
        public AudioClip sliderUpSound;
        
        private AudioManager manager => AudioManager.Instance;
        
        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            UpdateSliderValue();
        }

        private void Start()
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            UpdateSliderValue();
        }

        public void UpdateSliderValue()
        {
            if (slider != null)
                slider.value = manager.GetVolume(mixerType);
        }

        private void OnSliderValueChanged(float value)
        {
            manager.SetVolume(mixerType, value);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (sliderUpSound != null)
                SfxManager.Instance.PlaySfx(sliderUpSound);
        }
    }
}