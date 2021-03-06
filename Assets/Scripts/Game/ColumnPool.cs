﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnPool : MonoBehaviour
{
    public int columnPoolSize = 5;
    public GameObject columnPrefab;
    public float spawnRate = 3f;
    public float columnMin = -1.64f;
    public float columnMax = 1.64f;
    public int currentColumnIndex = 0;

    private List<GameObject> columns;
    private Vector2 objectPoolPosition = new Vector2(-15f, -25f);
    private float timeSinceLastSpawn;
    private float spawnXPosition = 10f;
    private int posColumnIndex = 0;
    private bool isFirst = true;

   
    void Start()
    {
        SpawnColumns();
    }

    
    void FixedUpdate()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (!GameController.instance.gameOver && timeSinceLastSpawn >= spawnRate)
        {
            timeSinceLastSpawn = 0;
            float spawnYPosition = Random.Range(columnMin, columnMax);
            columns[posColumnIndex].transform.position = new Vector2(spawnXPosition, spawnYPosition);
            columns[posColumnIndex].tag = "unscored";
            posColumnIndex++;

            if (isFirst) { 
                isFirst = false;
            }

            if (posColumnIndex >= columnPoolSize) posColumnIndex = 0;
        }

    }

    private void SpawnColumns()
    {
        isFirst = true;
        posColumnIndex = 0;
        currentColumnIndex = 0;
        timeSinceLastSpawn = spawnRate;

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
        if (columns[currentColumnIndex].transform.position.y == objectPoolPosition.y) return null;
        return columns[currentColumnIndex];
    }

}
