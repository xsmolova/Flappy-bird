using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public float scrollSpeed = -1.5f;
    public GameObject gameOverText;
    public Text scoreText;
    public bool gameOver = false;

    private int score = 0;
    private ColumnPool columnPool;
    private ScrollingObject[] scrollingObjects = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        columnPool = GetComponent<ColumnPool>();
        scrollingObjects = FindObjectsOfType<ScrollingObject>();
    }


    public void ResetGame() {
        //Start over
        foreach (ScrollingObject scrollingObject in scrollingObjects)
        {
            scrollingObject.StartOver();
        }
        // refresh score
        columnPool.RespawnColumns();
        gameOver = false;
    }

    public void BirdDied() {
        //gameOverText.SetActive(true);
        gameOver = true;
    }

    public GameObject GetCurrentColumn() {
        return columnPool.GetCurrentColumn();
    }

    //if all birds died -> restart
}
