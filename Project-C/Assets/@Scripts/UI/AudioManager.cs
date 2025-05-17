using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 인스턴스를 저장할 정적 변수
    private static AudioManager _instance;

    // 싱글톤 인스턴스를 반환하는 프로퍼티
    public static AudioManager Instance
    {
        get
        {
            // 인스턴스가 없다면 씬에서 찾아서 반환
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go); // 씬 전환 시에도 파괴되지 않도록 설정
                }
            }
            return _instance;
        }
    }

    // 이제 인스턴스를 private으로 선언하여 외부에서 생성할 수 없도록
    private AudioManager() { }

    [SerializeField] AudioSource BGMSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---Audio clip System---")]
    public AudioClip background;
    public AudioClip BossBackground;
    public AudioClip button;
    public AudioClip AroundItem;
    public AudioClip AcquireItem;

    [Header("---Audio clip Player---")]
    public AudioClip PlayerWalk;
    public AudioClip PlayerJump;
    public AudioClip PlayerDash;
    public AudioClip PlayerFalldown;
    public AudioClip PlayerWallSlide;
    public AudioClip PlayerDead;
    public AudioClip PlayerAttack;
    public AudioClip PlayerHeal_Charge;
    public AudioClip PlayerHeal;
    public AudioClip PlayerIceBall_Charge;
    public AudioClip PlayerIceBall_Shot;
    public AudioClip PlayerIceBreak_Charge;
    public AudioClip PlayerIceBreak_Shot;
    public AudioClip PlayerDamaged1;

    [Header("---Audio clip Monster---")]
    public AudioClip MonsterWarrior;
    public AudioClip MonsterBomerang_Shot;
    public AudioClip MonsterBomerang_Flying;
    public AudioClip MonsterFly_BodySlam_Ready;
    public AudioClip MonsterFly_BodySlam_Shot;
    public AudioClip MonsterElite_Axe_BackStep;
    public AudioClip MonsterElite_Axe_Throw;
    public AudioClip MonsterElite_Laser_MaskUP;
    public AudioClip MonsterElite_Laser_Shot;
    public AudioClip MonsterElite_ExplosionPunch_Charge;
    public AudioClip MonsterElite_ExplosionPunch_Punch;
    public AudioClip MonsterElite_ExplosionPunch_Explosion;
    public AudioClip MonsterElite_Tornado_Start;
    public AudioClip MonsterElite_Tornadoing;
    public AudioClip MonsterElite_Tornado_End;
    public AudioClip MonsterElite_Phase2;
    public AudioClip MonsterElite_Groggy;
    public AudioClip MonsterDamaged;

    // Start에서 배경 음악을 재생합니다.
    private void Start()
    {
        StartBGM(background);
    }

    //배경 음악을 재생
    public void StartBGM(AudioClip clip)
    {
        BGMSource.clip = clip;
        BGMSource.loop = true;
        BGMSource.Play();
    }

    // 배경 음악을 중지
    public void StopBGM()
    {
        BGMSource.Stop();
    }

    // SFX를 재생
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
    
    // SFX를 정지
    public void StopSFX(AudioClip clip)
    {
        SFXSource.Stop();
        SFXSource.clip = null;
        SFXSource.loop = false;
    }

    public void PlaySFXAfterDelay(AudioClip clip, float delay)
    {
        StartCoroutine(DelayedPlaySFX(clip, delay));
    }

    //딜레이 후 재생
    private IEnumerator DelayedPlaySFX(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        SFXSource.PlayOneShot(clip);
    }

    //사용 시
    //AudioManager.Instance.PlaySFX(AudioManager.Instance.원하는 AudioClip);
}
