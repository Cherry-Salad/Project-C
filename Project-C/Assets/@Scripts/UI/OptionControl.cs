using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum KeyInput
{
    NONE = -1,
    UP,         // 0
    DOWN,       // 1
    LEFT,       // 2
    RIGHT,      // 3
    JUMP,       // 4
    DASH,       // 5
    ATTACK,     // 6
    HEAL,       // 7
    NEXT,       // 8
    SKILL1,     // 9, 아이스 볼
    SKILL2,     // 10, 아이스브레이크
    SKILL3,     // 11, 미정
    MENU,       // 12
    MAP,        // 13
    INVENTORY,  // 14
    SKILL,      // 15, 스킬 창
    KEYCOUNT    // 16 (키 개수 카운트)
}

public static class KeySetting
{
    public static Dictionary<KeyInput, KeyCode> keys = new Dictionary<KeyInput, KeyCode>();

    //아래 3가지 코드를 사용하면 바인딩이 가능합니다~
    public static bool GetKeyDown(KeyInput key) => keys.ContainsKey(key) && Input.GetKeyDown(keys[key]); // 키 누를 때
    public static bool GetKey(KeyInput key) => keys.ContainsKey(key) && Input.GetKey(keys[key]);         // 키 눌려있는 상태
    public static bool GetKeyUp(KeyInput key) => keys.ContainsKey(key) && Input.GetKeyUp(keys[key]);     // 키 땔 때
}

public class OptionControl : MonoBehaviour
{
    public Player player; // Player 객체 참조

    public static event Action OnKeyChanged; // 키 변경 이벤트
    [SerializeField] private GameObject alertPanel;
    [SerializeField] private TMP_Text alertText;

    private static readonly KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.UpArrow,    // 0  (UP)
        KeyCode.DownArrow,  // 1  (DOWN)
        KeyCode.LeftArrow,  // 2  (LEFT)
        KeyCode.RightArrow, // 3  (RIGHT)
        KeyCode.LeftControl,// 4  (JUMP)
        KeyCode.LeftShift,  // 5  (DASH)
        KeyCode.Z,          // 6  (ATTACK)
        KeyCode.X,          // 7  (HEAL)
        KeyCode.Space,      // 8  (NEXT)
        KeyCode.A,          // 9  (SKILL1)
        KeyCode.S,          // 10 (SKILL2)
        KeyCode.Alpha1,     // 11 (SKILL3)
        KeyCode.Escape,     // 12 (MENU)
        KeyCode.M,          // 13 (MAP)
        KeyCode.I,          // 14 (INVENTORY)
        KeyCode.K,          // 15 (SKILL)
    };

    /// <summary>
    /// 현재 키 변경 대기 상태
    /// </summary>
    private int key = -1;

    void Awake()
    {
        StartCoroutine(WaitForPlayerDataLoad());
        if (FindObjectsOfType<OptionControl>().Length > 1) // 씬에 OptionControl이 여러 개 존재하는 경우
        {
            Destroy(gameObject); // 다른 객체는 파괴
            return;
        }

        DontDestroyOnLoad(gameObject); // 씬이 변경되어도 설정값 유지

        KeySetting.keys.Clear(); // 기존 키 딕셔너리 초기화
        AddDefaultKeys(); // 기본 키 추가
        LoadKeyMappings(); // 저장된 키 불러오기, 주석처리시 키 초기화
        SaveKeyMappings(); // 기본 키를 강제로 저장
        OnKeyChanged?.Invoke(); // UI 업데이트
        alertPanel.SetActive(false); // 경고 패널 비활성화
    }

    private void Update()
    {
        if (key >= 0) DetectKeyChange(); // 키 변경 대기 상태일 때 키 입력 감지
    }

    // 기본 키를 KeySetting.keys에 추가
    private void AddDefaultKeys()
    {
        KeyInput[] keyInputs = (KeyInput[])Enum.GetValues(typeof(KeyInput));
        for (int i = 0; i < (int)KeyInput.KEYCOUNT; i++) // KeyInput.KEYCOUNT까지 처리
        {
            if (keyInputs[i] == KeyInput.NONE) // NONE은 제외
                continue;

            if (!KeySetting.keys.ContainsKey(keyInputs[i]))
            {
                if (i < defaultKeys.Length) // 배열 범위를 초과하지 않도록 확인
                {
                    KeySetting.keys[keyInputs[i]] = defaultKeys[i]; // KeySetting.keys에 기본키 추가
                }
                else
                {
                    // 배열 크기 초과 시 기본값 처리 (추가적인 기본 키 값이 필요할 경우)
                    Debug.LogWarning($"No default key found for {keyInputs[i]}");
                }
            }
        }
    }

    // 키 변경 감지 및 처리
    private void DetectKeyChange()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                if (keyCode == KeyCode.Escape) // ESC 변경 방지
                {
                    ShowAlert("ESC can't change");
                    key = -1;
                    return;
                }

                if (KeySetting.keys.ContainsValue(keyCode)) // 중복 키 확인
                {
                    ShowAlert($"'{keyCode}' is already used.");
                    key = -1;
                    return;
                }

                // TODO: @최혁도, 테스크용 코드로 플레이어 스킬 키 변경(게임 매니저 생성 이후 변경할 필요가 있음
                // 하긴했는데 진짜 임시임 - 현재 스킬키 변경에 대해서는 저장을 못함
                foreach (PlayerSkillBase skill in player.Skills)
                {
                    if (key == (int)KeyInput.SKILL1 && skill is IceBall) // SKILL1에 해당하는 경우
                    {
                        skill.Key = keyCode; // 스킬의 Key를 새로 입력한 keyCode로 변경
                        Debug.Log($"Changed Skill Name: {skill.GetType().Name}, New Key: {skill.Key}");
                    }
                    else if (key == (int)KeyInput.SKILL2 && skill is IceBreak) // SKILL2에 해당하는 경우
                    {
                        skill.Key = keyCode; // 스킬의 Key를 새로 입력한 keyCode로 변경
                        Debug.Log($"Changed Skill Name: {skill.GetType().Name}, New Key: {skill.Key}");
                    }
                    else if (key == (int)KeyInput.HEAL && skill is SelfHealing) // HEAL에 해당하는 경우
                    {
                        skill.Key = keyCode; // 스킬의 Key를 새로 입력한 keyCode로 변경
                        Debug.Log($"Changed Skill Name: {skill.GetType().Name}, New Key: {skill.Key}");
                    }
                }

                // 모든 키가 업데이트 되었다면 저장 및 UI 업데이트
                KeySetting.keys[(KeyInput)key] = keyCode;
                key = -1; // 키 변경 대기 상태 종료
                SaveKeyMappings(); // 변경 즉시 저장
                OnKeyChanged?.Invoke(); // UI 업데이트
                return;
            }
        }
    }

    // 사용자가 버튼을 눌렀을 때 키 변경 대기 상태로 전환 
    public void ChangeKey(int num)
    {
        if ((KeyInput)num == KeyInput.MENU) // ESC 변경 방지
        {
            ShowAlert("ESC can't change");
            return;
        }

        key = num; // 키 변경 대기 상태 활성화
        OnKeyChanged?.Invoke(); // UI 업데이트
    }

    // 저장된 키 로드 (없을 경우 기본 키 적용)
    private void LoadKeyMappings()
    {
        KeyInput[] keyInputs = (KeyInput[])Enum.GetValues(typeof(KeyInput));

        // KeyInput.KEYCOUNT는 실제 키 설정에 사용되지 않으므로 마지막 항목은 제외하고 반복
        for (int i = 0; i < (int)KeyInput.KEYCOUNT; i++) // KeyInput.KEYCOUNT까지 처리
        {
            if (keyInputs[i] == KeyInput.NONE) // NONE은 제외
                continue;

            if (PlayerPrefs.HasKey(keyInputs[i].ToString())) // 저장된 키가 있을 경우
            {
                string savedKey = PlayerPrefs.GetString(keyInputs[i].ToString());
                if (Enum.TryParse(savedKey, out KeyCode loadedKey))
                {
                    KeySetting.keys[keyInputs[i]] = loadedKey; // 저장된 키로 바인딩
                }
                else
                {
                    KeySetting.keys[keyInputs[i]] = defaultKeys[i]; // 유효하지 않으면 기본값 사용
                }
            }
            else
            {
                KeySetting.keys[keyInputs[i]] = defaultKeys[i]; // 저장된 값이 없으면 기본값 사용
            }
            //Debug.Log($"{KeySetting.keys[keyInputs[i]]}");
        }
    }

    // 현재 키 설정을 PlayerPrefs에 저장
    private void SaveKeyMappings()
    {
        foreach (var key in KeySetting.keys)
        {
            PlayerPrefs.SetString(key.Key.ToString(), key.Value.ToString()); // 키와 키 코드를 저장
        }
        PlayerPrefs.Save(); // PlayerPrefs 저장
    }

    // 경고 메시지를 화면에 표시
    private void ShowAlert(string message)
    {
        alertText.text = message;
        alertPanel.SetActive(true);
        StartCoroutine(ShowAlertDelay(2f)); // 2초 후 경고 메시지 숨기기
    }

    private IEnumerator ShowAlertDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        alertPanel.SetActive(false);
    }

    private IEnumerator WaitForPlayerDataLoad()
    {
        //Player 찾기
        while (player == null)
        {
            player = GameObject.FindWithTag("Player")?.GetComponent<Player>(); //이거 왜 Tag로 못찾지..??
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            yield return null;
        }
    }
}