using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnPool : MonoBehaviour
{
    public int columnPoolSize = 5;
    public GameObject columnPrefab;
    public float spawnRate = 4f;
    public float columnMin = -1f;
    public float columnMax = 3.5f;

    private GameObject[] columns;
    private Vector2 objectPoolPosition = new Vector2(-15f, -25f);
    private float timeSinceLastSpawn = 4f;
    private float spawnXPosition = 10f;
    private int currColumn = 0;

    // Start is called before the first frame update
    void Start()
    {
        columns = new GameObject[columnPoolSize];
       
        for (int i = 0; i < columnPoolSize; i++)
        {
            columns[i] = (GameObject)Instantiate(columnPrefab, objectPoolPosition, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (!GameController.instance.gameOver && timeSinceLastSpawn >= spawnRate) {
            timeSinceLastSpawn = 0;
            float spawnYPosition = Random.Range(columnMin,columnMax);
            columns[currColumn].transform.position = new Vector2(spawnXPosition, spawnYPosition);
           
            currColumn++;
            if (currColumn >= columnPoolSize) currColumn = 0;
        }

    }
}
