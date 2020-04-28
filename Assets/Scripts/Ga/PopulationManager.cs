using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager instance;

    public Text scoreText;
    public GameObject botPrefab;
    public Vector2 startingPos = new Vector2(0, 0);
    public int populationSize = 10;
    public bool mutate = false;
    public int currentPopulationScore = 0;

    public static float elapsed = 0;
    public float timeScale = 1f;

    List<GameObject> population = new List<GameObject>();
    int generation = 1;
    int deadBirds = 0;

    GUIStyle guiStyle = new GUIStyle();
    void OnGUI()
    {
        guiStyle.fontSize = 25;
        guiStyle.normal.textColor = Color.white;
        GUI.BeginGroup(new Rect(10, 10, 250, 150));
        GUI.Box(new Rect(0, 0, 140, 140), "Stats", guiStyle);
        GUI.Label(new Rect(10, 25, 200, 30), "Gen: " + generation, guiStyle);
        GUI.Label(new Rect(10, 50, 200, 30), string.Format("Time: {0:0.00}", elapsed), guiStyle);
        GUI.Label(new Rect(10, 75, 200, 30), "Population: " + population.Count, guiStyle);
        GUI.Label(new Rect(10, 100, 200, 30), "Dead: " + deadBirds, guiStyle);
        GUI.Label(new Rect(10, 125, 200, 30), "Max Score: " + Brain.maxScore, guiStyle);
        GUI.EndGroup();
    }

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

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < populationSize; i++)
        {
            GameObject b = Instantiate(botPrefab, startingPos, Quaternion.identity);
            b.GetComponent<Brain>().Init();
            population.Add(b);
        }
        deadBirds = 0;

        currentPopulationScore = 0;
        ShowPopulationScore();

        Time.timeScale = timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
    }


    private GameObject Breed(GameObject parent1, GameObject parent2)
    {
        GameObject offspring = Instantiate(botPrefab, startingPos, Quaternion.identity);
        Brain b = offspring.GetComponent<Brain>();

        if (mutate && Random.Range(0, 100) == 1) //mutate 1 in 100
        {
            b.Init();
            b.ann.Mutate();
        }
        else
        {
            b.Init();
            b.ann.Combine(parent1.GetComponent<Brain>().ann, parent2.GetComponent<Brain>().ann);
        }
        return offspring;
    }

    public void BreedNewPopulation()
    {
        List<GameObject> sortedList = population.OrderBy(o => (o.GetComponent<Brain>().score * 10 + o.GetComponent<Brain>().timeAlive)).ToList();

        population.Clear();

        // Best 25% of population
        for (int i = sortedList.Count - 1; i > (int)(3 * sortedList.Count / 4.0f) - 1; i--)
        {
            // Best two -> in the new population + breed
            if (i == sortedList.Count - 1)
            {
                population.Add(Breed(sortedList[i], sortedList[i]));
                population.Add(Breed(sortedList[i - 1], sortedList[i - 1]));
                population.Add(Breed(sortedList[i], sortedList[i - 1]));
                population.Add(Breed(sortedList[i - 1], sortedList[i]));

                continue;
            }

            population.Add(Breed(sortedList[i], sortedList[i - 1]));
            population.Add(Breed(sortedList[i - 1], sortedList[i]));
            population.Add(Breed(sortedList[i], sortedList[i - 1]));
            population.Add(Breed(sortedList[i - 1], sortedList[i]));
        }

        //destroy all parents and previous population
        for (int i = 0; i < sortedList.Count; i++)
        {
            Destroy(sortedList[i]);
        }
        Debug.Log("new generation with population = " + population.Count);
        generation++;

        currentPopulationScore = 0;
        ShowPopulationScore();

        deadBirds = 0;
    }

    public void BirdDied()
    {
        deadBirds++;

        if (deadBirds == population.Count)
        {
            Debug.Log("all birds died");
            GameController.instance.AllBirdsDied();
            BreedNewPopulation();
            GameController.instance.ResetGame();
        }
    }

    public void ShowPopulationScore()
    {
        scoreText.text = "Score: " + currentPopulationScore;
    }
}
