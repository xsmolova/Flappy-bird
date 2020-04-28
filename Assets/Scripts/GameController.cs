using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public float scrollSpeed = -1.5f;
    public bool gameOver = false;

    private ColumnPool columnPool;
    private ScrollingObject[] scrollingObjects = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        columnPool = GetComponent<ColumnPool>();
        scrollingObjects = FindObjectsOfType<ScrollingObject>();
    }


    public void ResetGame()
    {
        gameOver = false;
        columnPool.RespawnColumns();
        
        //Start over
        foreach (ScrollingObject scrollingObject in scrollingObjects)
        {
            scrollingObject.StartOver();
        }
    }

    public void BirdDied()
    {
        gameOver = true;
    }

    public GameObject GetCurrentColumn()
    {
        return columnPool.GetCurrentColumn();
    }

   // public GameObject GetNextColumn()
   // {
   //     return columnPool.GetNextColumn();
   // }


}
