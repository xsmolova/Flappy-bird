using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnPool : MonoBehaviour
{
    public int columnPoolSize = 5;
    public GameObject columnPrefab;
    public float spawnRate = 4f;
    public float columnMin = 0.5f;
    public float columnMax = 3f;

    private List<GameObject> columns;
    private Vector2 objectPoolPosition = new Vector2(-15f, -25f);
    private float timeSinceLastSpawn;
    private float spawnXPosition = 10f;
    private int currColumn = 0;

    // Start is called before the first frame update
    void Start()
    {
        SpawnColumns();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (!GameController.instance.gameOver && timeSinceLastSpawn >= spawnRate)
        {
            timeSinceLastSpawn = 0;
            float spawnYPosition = Random.Range(columnMin, columnMax);
            columns[currColumn].transform.position = new Vector2(spawnXPosition, spawnYPosition);

            currColumn++;
            if (currColumn >= columnPoolSize) currColumn = 0;
        }

    }

    private void SpawnColumns()
    {
        timeSinceLastSpawn = 4f;
        columns = new List<GameObject>(columnPoolSize);

        for (int i = 0; i < columnPoolSize; i++)
        {
            columns.Add((GameObject)Instantiate(columnPrefab, objectPoolPosition, Quaternion.identity));
        }
    }

    public void RespawnColumns()
    {
        foreach (GameObject column in columns)
        {
            Destroy(column);
        }
        columns.Clear();
        SpawnColumns();
    }

    public GameObject GetCurrentColumn()
    {
        foreach (GameObject column in columns)
        {
            if (column.tag == "unscored") return column;
        }
        return null;
    }
}
