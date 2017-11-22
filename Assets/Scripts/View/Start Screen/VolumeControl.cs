using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour {
    public const float DEFAULT_SFX_VOLUME = 0.50f;
    public const float DEFAULT_MUSIC_VOLUME = 0.30f;

    public enum VolumeType {

        [Description(MUSIC_VOLUME_KEY)]
        MUSIC,

        [Description(SFX_VOLUME_KEY)]
        SFX,
    }

    public const string MUSIC_VOLUME_KEY = "music";
    public const string SFX_VOLUME_KEY = "sfx";

    [SerializeField]
    private Text volumeName;

    [SerializeField]
    private Text volumeAmount;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private VolumeType volumeControlled;

    private float defaultVolume {
        get {
            if (volumeControlled == VolumeType.MUSIC) {
                return DEFAULT_MUSIC_VOLUME;
            } else {
                return DEFAULT_SFX_VOLUME;
            }
        }
    }

    private float currentVolume {
        get {
            return PlayerPrefs.GetFloat(volumeControlled.GetDescription(), defaultVolume);
        }
    }

    private void Start() {
        volumeName.text = volumeControlled.GetDescription();
        slider.value = currentVolume;
        UpdateVolumeAmountText();
    }

    public void OnValueChanged(Slider slider) {
        PlayerPrefs.SetFloat(volumeControlled.GetDescription(), slider.value);
        UpdateVolumeAmountText();
    }

    private void UpdateVolumeAmountText() {
        volumeAmount.text = Mathf.RoundToInt((currentVolume * 100)).ToString();
    }
}