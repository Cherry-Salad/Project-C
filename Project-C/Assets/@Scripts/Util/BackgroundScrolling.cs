using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScrolling : MonoBehaviour
{
    public Transform[] backgrounds;
    public float speed = 2f;

    float backgroundWidth;

    void Start()
    {
        backgroundWidth = backgrounds[0].GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        foreach (Transform bg in backgrounds)
            bg.position += Vector3.left * speed * Time.deltaTime;
        

        if (backgrounds[0].position.x < -backgroundWidth)
        {
            backgrounds[0].position += Vector3.right * backgroundWidth * 1.8f;
            Swap();
        }
    }

    void Swap()
    {
        Transform temp = backgrounds[0];
        backgrounds[0] = backgrounds[1];
        backgrounds[1] = temp;
    }
}