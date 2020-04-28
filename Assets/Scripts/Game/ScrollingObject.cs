using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingObject : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 startingPosition;

    // Start is called before the first frame update
    void Start()
    {
        startingPosition = new Vector2(transform.position.x, transform.position.y);
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(GameController.instance.scrollSpeed,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.instance.gameOver)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void StartOver() {
        transform.position = startingPosition;
        rb.velocity = new Vector2(GameController.instance.scrollSpeed, 0);
    }
}
