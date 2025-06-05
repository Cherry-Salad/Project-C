using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionResloution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown; // 드롭다운 UI
    private List<Resolution> customResolutions; // 사용자 지정 해상도 리스트

    private void Start()
    {
        // 사용자 지정 해상도 리스트
        customResolutions = new List<Resolution>
        {
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 960, height = 540 }
        };

        // 드롭다운 초기화
        InitializeDropdown();
    }

    private void InitializeDropdown()
    {
        // 드롭다운 옵션 초기화
        resolutionDropdown.ClearOptions();

        // 사용자 지정 해상도를 드롭다운 옵션으로 추가
        List<string> options = new List<string>();
        foreach (var res in customResolutions)
        {
            options.Add($"{res.width} x {res.height}");
        }

        resolutionDropdown.AddOptions(options);

        // 현재 해상도와 일치하는 항목을 선택
        Resolution currentResolution = Screen.currentResolution;
        int currentIndex = customResolutions.FindIndex(r => r.width == currentResolution.width && r.height == currentResolution.height);
        resolutionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
        resolutionDropdown.RefreshShownValue();

        // 드롭다운 변경 이벤트 연결
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void OnResolutionChanged(int index)
    {
        // 선택한 해상도로 변경
        Resolution selectedResolution = customResolutions[index];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);

        Debug.Log($"해상도 변경: {selectedResolution.width} x {selectedResolution.height}"); //확인용
    }
}
