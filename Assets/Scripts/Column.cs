using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    private ColumnPool cp;

    private void Start()
    {
        cp = FindObjectOfType<ColumnPool>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Brain bird;

        if ((bird = other.GetComponent<Brain>()) != null)
        {
            bird.score++;
            //tag column
            gameObject.tag = "scored";

            if (cp.currentColumnIndex + 1 >= cp.columnPoolSize) cp.currentColumnIndex = 0;
            else cp.currentColumnIndex++;
        }
    }
}
