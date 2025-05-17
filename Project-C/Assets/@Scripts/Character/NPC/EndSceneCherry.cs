using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneCherry : MonoBehaviour
{
    private enum EEndSceneCherryAction
    {
        Start,
        Move,
        In
    }

    public GameObject StartPoint;
    public GameObject EndPoint;
    public float moveSpeed = 3f; // 이동 속도

    private EEndSceneCherryAction action = EEndSceneCherryAction.Start;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        transform.position = StartPoint.transform.position; // 시작 위치 초기화

    }

    void Update()
    {
        switch (action)
        {
            case EEndSceneCherryAction.Start:
                break;

            case EEndSceneCherryAction.Move:
                MoveToEndPoint();
                break;

            case EEndSceneCherryAction.In:
                break;
        }
    }

    public void MoveEndPoint()
    {
        action = EEndSceneCherryAction.Move;
        animator.Play("W_Run_Right");
    }

    private void MoveToEndPoint()
    {
        // 현재 위치에서 EndPoint 방향으로 일정 속도로 이동
        transform.position = Vector3.MoveTowards(transform.position, EndPoint.transform.position, moveSpeed * Time.deltaTime);

        // 도착 시 상태 전환
        if (Vector3.Distance(transform.position, EndPoint.transform.position) < 0.01f)
        {
            action = EEndSceneCherryAction.In;
            animator.Play("W_Other_Sniff");
            StartCoroutine(IntoTheDungeon());
        }
    }

    private IEnumerator IntoTheDungeon()
    {
        yield return new WaitForSeconds(1.5f);

        animator.Play("W_Walk_Top");

        // 천천히 줄어들기
        float duration = 1.0f; // 줄어드는 데 걸리는 시간
        float elapsed = 0f;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero; // 완전히 사라지게

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            yield return null;
        }

        // 최종적으로 정확히 0으로 고정
        transform.localScale = targetScale;

        // 비활성화하거나 파괴 (선택)
        gameObject.SetActive(false);
        // 또는 Destroy(gameObject);
    }
}
