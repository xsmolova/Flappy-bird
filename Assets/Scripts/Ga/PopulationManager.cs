using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager instance;

    public Text scoreText;
    public GameObject botPrefab;
    public Vector2 startingPos = new Vector2(0, 0);
    public int populationSize = 10;
    public bool mutate = false;
    public int mutateRate = 1;
    public int currentPopulationScore = 0;

    public static float elapsed = 0;
    public float timeScale = 1f;

    public bool saveStatistics = true;
    public bool saveWeightsToFile = false;

    List<GameObject> population = new List<GameObject>();
    int generation = 1;
    int deadBirds = 0;
    List<string> collectedStatistics = new List<string>();
    StreamWriter tdf;

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

        string path = Application.dataPath + "/statistics.csv";
        tdf = File.CreateText(path);

        Time.timeScale = timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        if (saveWeightsToFile) { saveWeightsToFile = false; SaveWeightsToFile(); }
    }


    private GameObject Breed(GameObject parent1, GameObject parent2)
    {
        GameObject offspring = Instantiate(botPrefab, startingPos, Quaternion.identity);
        Brain brain = offspring.GetComponent<Brain>();

        brain.Init();
        brain.ann.Combine(parent1.GetComponent<Brain>().ann, parent2.GetComponent<Brain>().ann);

        if (mutate && Random.Range(0, 100) < mutateRate) //mutate in 1%
            brain.ann.Mutate();

        return offspring;
    }

    public void BreedNewPopulation()
    {
        if (saveStatistics) WriteStatistics();
        if (generation == 100) UnityEditor.EditorApplication.isPlaying = false;

        List<GameObject> sortedList = population.OrderBy(o => (o.GetComponent<Brain>().score * 10 + o.GetComponent<Brain>().timeAlive)).ToList();

        population.Clear();

        // Best 20% of population
        for (int i = sortedList.Count - 1; i > (int)(4 * sortedList.Count / 5.0f) - 1; i--)
        {
            // Make a exact copy of best 20% of birds
            population.Add(Breed(sortedList[i], sortedList[i]));

            // Breed bird with next best one to produce 4 offsprings
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


    private Brain GetBestBird()
    {
        List<GameObject> sortedList = population.OrderBy(o => (o.GetComponent<Brain>().score)).ToList();
        return sortedList[sortedList.Count - 1].GetComponent<Brain>();
    }


    // Save statistics
    private void WriteStatistics()
    {
        string statistics = generation + "," + currentPopulationScore;
        collectedStatistics.Add(statistics);
    }

    void OnApplicationQuit()
    {
        foreach (string sd in collectedStatistics)
        {
            tdf.WriteLine(sd);
        }
        tdf.Close();
    }

    // Save weights of best bird
    private void SaveWeightsToFile()
    {
        string path = Application.dataPath + "/weights.txt";
        StreamWriter tdf = File.CreateText(path);

        Brain bird = GetBestBird();
        tdf.WriteLine(bird.ann.PrintWeights());
        tdf.Close();
    }

    void LoadWeightsFromFile(ANN ann)
    {
        string path = Application.dataPath + "/weights.txt";
        StreamReader wf = File.OpenText(path);

        if (File.Exists(path))
        {
            string line = wf.ReadLine();
            ann.LoadWeights(line);
        }
    }
}
