using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionSound : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider MasterSlider;
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider EffectSlider;

    private void Start()
    {
        // PlayerPrefs 초기화 및 모든 볼륨 로드
        if (PlayerPrefs.HasKey("MasterSound"))
        {
            LoadAllVolume();
        }
        else
        {
            InitializeDefaultVolume();
        }

        // 슬라이더 변경 이벤트 연결
        MasterSlider.onValueChanged.AddListener(delegate { SetMasterVolume(); });
        BGMSlider.onValueChanged.AddListener(delegate { SetBGMVolume(); });
        EffectSlider.onValueChanged.AddListener(delegate { SetEffectVolume(); });
    }

    // 마스터 볼륨 설정
    public void SetMasterVolume()
    {
        float MasterVolume = MasterSlider.value;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(MasterVolume) * 20);
        PlayerPrefs.SetFloat("MasterSound", MasterVolume);
        Debug.Log($"[SetMasterVolume] MasterVolume: {MasterVolume}");
    }

    // BGM 볼륨 설정
    public void SetBGMVolume()
    {
        float BGMVolume = BGMSlider.value;
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(BGMVolume) * 20);
        PlayerPrefs.SetFloat("BGMSound", BGMVolume);
        Debug.Log($"[SetBGMVolume] BGMVolume: {BGMVolume}");
    }

    // 이펙트 볼륨 설정
    public void SetEffectVolume()
    {
        float EffectVolume = EffectSlider.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(EffectVolume) * 20);
        PlayerPrefs.SetFloat("EffectSound", EffectVolume);
        Debug.Log($"[SetEffectVolume] EffectVolume: {EffectVolume}");
    }

    // 모든 볼륨 로드
    private void LoadAllVolume()
    {
        MasterSlider.value = PlayerPrefs.GetFloat("MasterSound", 0.5f);
        BGMSlider.value = PlayerPrefs.GetFloat("BGMSound", 0.5f);
        EffectSlider.value = PlayerPrefs.GetFloat("EffectSound", 0.5f);
        Debug.Log("[LoadAllVolume] Volumes loaded from PlayerPrefs");

        SetMasterVolume();
        SetBGMVolume();
        SetEffectVolume();
    }

    // 초기 볼륨 기본값 설정
    private void InitializeDefaultVolume()
    {
        // 기본값
        MasterSlider.value = 0.5f; 
        BGMSlider.value = 0.5f;
        EffectSlider.value = 0.5f;

        //설정값
        SetMasterVolume();
        SetBGMVolume();
        SetEffectVolume();
    }
}
