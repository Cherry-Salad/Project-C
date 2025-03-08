using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CherryState
{
    Idle,
    Walk
}

public class CherryFollow : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform player;
    public float interval = 0.3f; // 큐를 받는 간격(속도)
    public float followDelay = 5f; // 시작 후 언제부터 따라갈지
    public float speed = 4f; // 고양이 속도
    public float tpDistance = 5f; // TP 최소 거리

    private Vector2? _nextPoint = null;
    private Queue<Vector2> positionQueue = new Queue<Vector2>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        StartCoroutine(LoadPlayer());
        StartCoroutine(RecordPlayerPosition());

        // 플레이어와 고양이 간 충돌 & NPC와 고양이 충돌을 무시
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Cherry"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Cherry"), true);
    }

    IEnumerator LoadPlayer()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    IEnumerator RecordPlayerPosition()
    {
        yield return new WaitUntil(() => player != null);
        yield return new WaitForSeconds(followDelay);

        while (true)
        {
            positionQueue.Enqueue(player.position);

            while (positionQueue.Count > 30)
            {
                positionQueue.Dequeue();
            }
            yield return new WaitForSeconds(interval);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (positionQueue.Count > 0)
        {
            _nextPoint = positionQueue.Dequeue();
        }

        if (_nextPoint != null)
        {
            Vector2 targetPos = _nextPoint.Value;

            // 플레이어보다 0.1만큼 더 낮은 Y 위치로 설정
            targetPos.y = player.position.y - 0.1f;

            float distance = Vector2.Distance(transform.position, targetPos);

            if (distance > tpDistance)
            {
                TeleportToPlayer();
                return;
            }

            FlipSprite(targetPos);

            if (distance > 0.5f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                SetState(CherryState.Walk);
            }
            else
            {
                SetState(CherryState.Idle);
            }

            if (distance < 0.1f)
            {
                _nextPoint = null;
            }
        }
        else
        {
            SetState(CherryState.Idle);
        }
    }

    void SetState(CherryState newState)
    {
        switch (newState)
        {
            case CherryState.Idle:
                animator.SetBool("isWalking", false);
                break;
            case CherryState.Walk:
                animator.SetBool("isWalking", true);
                break;
        }
    }

    void FlipSprite(Vector2 targetPos)
    {
        spriteRenderer.flipX = targetPos.x < transform.position.x;
    }

    void TeleportToPlayer() // 일정 거리 이상 멀어지면 텔레포트 및 큐 초기화
    {
        transform.position = new Vector2(player.position.x - 1f, player.position.y);
        positionQueue.Clear();
        StartCoroutine(RecordPlayerPosition());
    }
}
