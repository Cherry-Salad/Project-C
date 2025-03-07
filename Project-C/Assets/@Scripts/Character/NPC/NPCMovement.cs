using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState
{
    Idle,
    Move
}

public class NPCMovement : MonoBehaviour
{
    public float moveDistance = 1.5f;  // 이동 거리
    public float moveSpeed = 2f;       // 이동 속도
    private Vector3 startPos;
    private bool movingRight = true;   // 원래 이동 방향 유지

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;

    public NPCState curState = NPCState.Move;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        StartCoroutine(LoadPlayer());
    }

    IEnumerator LoadPlayer()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);

        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player != null)
        {
            Debug.Log($"Player 할당 완료: {player.name}");
        }
        else
        {
            Debug.LogError("Player를 찾지 못했습니다!");
        }
    }

    void Update()
    {
        switch (curState)
        {
            case NPCState.Idle:
                animator.SetBool("isMoving", false);
                animator.Play("idle");
                FacePlayer();  // 플레이어를 바라봄
                break;

            case NPCState.Move:
                animator.SetBool("isMoving", true);
                MoveNPC();
                break;
        }
    }

    void MoveNPC()
    {
        if (curState == NPCState.Idle) return;  // Idle 상태일 때 이동 중지

        float moveDirection = movingRight ? 1 : -1;
        transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);

        // 이동 범위를 초과하면 방향 전환
        if (Vector3.Distance(startPos, transform.position) >= moveDistance)
        {
            movingRight = !movingRight;
            spriteRenderer.flipX = !movingRight; // 방향 전환
        }
    }

    void FacePlayer()
    {
        if (player == null) return;
        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    public void SetState(NPCState state)
    {
        if (curState == state) return; // 이미 같은 상태면 실행하지 않음
        curState = state;
    }

    public void ExitDialogue()
    {
        // 대화 종료 후 NPC가 원래 이동 방향을 바라보도록 설정
        spriteRenderer.flipX = !movingRight;
        SetState(NPCState.Move);
    }
}
