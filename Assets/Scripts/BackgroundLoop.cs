using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    float width;

    private void Awake()
    {
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        width = bc.size.x;
    }
    // Start is called before the first frame update
    void Update()
    {
        if (transform.position.x <= -(width / 2f))
            Reposition();
    }

    void Reposition()
    {
        Vector2 offset = new Vector2(width, 0);
        transform.position = (Vector2)transform.position + offset;
    }

    // Update is called once per frame
    
}
