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
    private Rigidbody2D rigid;
    public Animator animator;
    public Transform player;
    public float interval = 0.3f;
    public float followDelay = 5f;
    public float speed = 4f;
    public float teleportThreshold = 5f;

    private Vector2? _nextPoint = null;
    private Queue<Vector2> positionQueue = new Queue<Vector2>();
    public CherryState curState = CherryState.Idle;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(LoadPlayer());
        StartCoroutine(RecordPlayerPosition());

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Npc"), true);
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
            float distance = Vector2.Distance(transform.position, targetPos);

            if (distance > teleportThreshold)
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
        if (curState == newState) return;
        curState = newState;

        switch (curState)
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

    void TeleportToPlayer()
    {
        transform.position = new Vector2(player.position.x - 1f, player.position.y);
        positionQueue.Clear();
        StartCoroutine(RecordPlayerPosition());
    }
}
