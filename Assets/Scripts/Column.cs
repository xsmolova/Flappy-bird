using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Brain bird;

        if ((bird = other.GetComponent<Brain>())!= null)
        {
            bird.score++;
            //tag column
            gameObject.tag = "scored";
        }
    }
}
