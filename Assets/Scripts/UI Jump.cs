using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJump : MonoBehaviour
{
    public float jumpForce = 10f;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 50f); // 0.1f는 검사 범위입니다.

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Ground"))
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                break;
            }
        }
    }
}
