using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public float upForce = 250f;

    private bool isDead = false;
    private Rigidbody2D rb;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!isDead)
        {
            if (Input.GetMouseButtonDown(0))
            {
                anim.SetTrigger("Flap");
                rb.velocity = Vector2.zero;
                rb.AddForce(new Vector2(0, upForce)) ;
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector2.zero;
        isDead = true;
        anim.SetBool("Die", isDead);
        GameController.instance.BirdDied();
    }
}
