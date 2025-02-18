using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinsBoomerangs : MonsterProjectileBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;


        return true;
    }

    protected override IEnumerator ChangingVector()
    {
        float speedConversionValue = 0f;
        float changeValue = Mathf.Sqrt(Data.BaseSpeed) * (Data.LifeTime / 2f) * 0.1f;
        
        float sec = 0;
        
        while(sec <= Data.LifeTime)
        {
            Speed = Data.BaseSpeed - (speedConversionValue * speedConversionValue);
            this.Rigidbody.velocity = new Vector2(Dir.x * Speed, Dir.y * Speed);

            yield return new WaitForSeconds(0.1f);

            speedConversionValue += changeValue;
            sec += 0.1f;
        }
    }

}
