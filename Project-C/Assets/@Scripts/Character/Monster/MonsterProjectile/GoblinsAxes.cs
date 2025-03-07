using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinsAxes : MonsterProjectileBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void ShootingProjectile(Vector2 pos, Vector2 dir)
    {
        _isActive = true;

        Dir = dir;

        Speed = Data.BaseSpeed;
        DefaultGravityScale = Data.DefaultGravity;

        this.transform.position = pos;
        this.Rigidbody.velocity = Vector2.zero;

        StartCoroutine(ChangingVector());
        StartCoroutine(StartLifeTime());
    }

    protected override IEnumerator ChangingVector()
    {
        Vector2 startPos = transform.position; // 시작 위치 저장

        float x = 0f;
        float y = 0f;

        float w = Dir.x; // 포물선의 넓이
        float h = Mathf.Max(0.1f, Dir.y); // 포물선의 높이
        float T = Speed; // 바닥에 닿을 시간
        float v = (2 * Mathf.Abs(w * 3)) / T; // T초 후를 기준으로 속도 조정 

        float dir = Dir.x > 0 ? 1 : -1;

        while (Mathf.Abs(x) < 2 * Mathf.Abs(w * 3))
        {
            x += Time.deltaTime * v * dir;
            y = -Mathf.Pow((x / w - h), 2) + (h * h);

            transform.position = startPos + new Vector2(x, y);
            yield return null;
        }
    }
}
