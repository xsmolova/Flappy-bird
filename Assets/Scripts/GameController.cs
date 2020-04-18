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

    // Update is called once per frame
    void Update()
    {
        if (gameOver && Input.GetMouseButtonDown(0))
        {
            //Start over
            foreach (ScrollingObject scrollingObject in scrollingObjects)
            {
                scrollingObject.StartOver();
            }
            // refresh score
            columnPool.DeleteColumns();
            gameOver = false;
        }
        
    }

    public void BirdScored() {

        if (gameOver)
        {
            return;
        }

        //score++;
        //scoreText.text = "Score: " + score.ToString();
    }


    public void BirdDied() {
        //gameOverText.SetActive(true);
        gameOver = true;
    }
}
