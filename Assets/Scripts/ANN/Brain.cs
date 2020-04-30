using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


// States:
// distance to top collider
// distance to bottom collider
// vertical distance to columns (gap)
// horizontal distance to (gap)

public class Replay
{
    public List<double> states;
    public double reward;

    public Replay(double distVertGap, double distHorGap, double r)
    {
        states = new List<double>();

        states.Add(distVertGap);
        states.Add(distHorGap);
        reward = r;
    }
}

public class Brain : MonoBehaviour
{
    // Bird
    public float upForce = 250f;
    public int score = 0;
    public bool isDead = false;
    public bool isExploring = false;
    public float timeAlive = 0;

    private Rigidbody2D rb;
    private Animator anim;
    public bool deadInPopulation = false;

    private float halfScreen = 3.4f;
    private float maxDistanceToColumn = 10f;

    // ANN Brain
    public ANN ann = null;

    float reward = 0.0f;
    List<Replay> replayMemory = new List<Replay>();
    int maxMemoryCapacity = 1000;

    float discount = 0.99f;
    public float exploreRate = 100.0f;
    float maxExploreRate = 100.0f;
    float minExploreRate = 0.01f;
    float exploreDecay = 0.0001f;

    // Stats
    public static int maxScore = 0;
    public static float maxFlightTime = 0;

    public void Init()
    {
        score = 0;
        timeAlive = 0;
        isDead = false;
        deadInPopulation = false;
        reward = 0.0f;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.velocity = Vector2.zero;

        ann = new ANN(2, 2, 1, 6, 0.9f);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) Flap();
    }
    private void FixedUpdate()
    {
        // If brain wasnt inicialized
        if (ann == null) return;

        if (deadInPopulation) return;
        
        timeAlive += Time.deltaTime;

        List<double> states = new List<double>();
        List<double> qs;

        GameObject currColumn = GameController.instance.GetCurrentColumn();
        if (!currColumn) return;

        // Get vertical and horizontal distance to the current column
        float yDist = currColumn.transform.position.y - transform.position.y;
        float xDist = currColumn.transform.position.x - transform.position.x;

        // Normalize and round
        float vertDist = 1 - (float)System.Math.Round((Map(-1.0f, 1.0f, -halfScreen, halfScreen, yDist)), 2);
        float horDist = 1 - (float)System.Math.Round((Map(0.0f, 1.0f, 0.0f, maxDistanceToColumn, xDist)), 2);

        // Add values to the states
        states.Add(vertDist);
        states.Add(horDist);

        // Calc output for states
        qs = SoftMax(ann.CalcOutput(states));
        double maxQ = qs.Max();
        int maxQIndex = qs.ToList().IndexOf(maxQ);

        // Explore
        if (isExploring)
        {
            exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);
            if (Random.Range(0, 100) < exploreRate)
                maxQIndex = Random.Range(0, 2);
        }

        // Action
        if (maxQIndex == 0) Flap();

        if (isDead) reward = -1f;
        else reward = 0.1f;

        // Add a new memory
        Replay lastMemory = new Replay(vertDist, horDist, reward);

        if (replayMemory.Count > maxMemoryCapacity)
            replayMemory.RemoveAt(0);

        replayMemory.Add(lastMemory);

        if (isDead)
        {
            //TrainFromMemories();

            if (timeAlive > maxFlightTime)
                maxFlightTime = timeAlive;

            if (score > maxScore)
                maxScore = score;

            replayMemory.Clear();

            // After training with memories
            if (!deadInPopulation)
            {
                deadInPopulation = true;
                PopulationManager.instance.BirdDied();
            }
        }


    }

    private void TrainFromMemories()
    {
        for (int i = replayMemory.Count - 1; i >= 0; i--)
        {
            List<double> outputsOld;
            List<double> outputsNew;

            outputsOld = SoftMax(ann.CalcOutput(replayMemory[i].states));

            double maxQOld = outputsOld.Max();
            int action = outputsOld.ToList().IndexOf(maxQOld);

            double feedback;
            if (i == replayMemory.Count - 1 || replayMemory[i].reward == -1)
                feedback = replayMemory[i].reward;
            else
            {
                outputsNew = SoftMax(ann.CalcOutput(replayMemory[i + 1].states));
                double maxQ = outputsNew.Max();
                //Bellmans equation
                feedback = replayMemory[i].reward + discount * maxQ;
            }

            outputsOld[action] = feedback;
            ann.Train(replayMemory[i].states, outputsOld);
        }
    }

    private void Flap()
    {
        anim.SetTrigger("Flap");
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(0, upForce));
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        isDead = true;
        anim.SetBool("Die", isDead);
        rb.velocity = Vector2.zero;
    }


    //normalize 0-1, all of the values add up to 1 => percentage system
    List<double> SoftMax(List<double> oSums)
    {
        double max = oSums.Max();

        float scale = 0.0f;
        for (int i = 0; i < oSums.Count; ++i)
            scale += Mathf.Exp((float)(oSums[i] - max));

        List<double> result = new List<double>();
        for (int i = 0; i < oSums.Count; ++i)
            result.Add(Mathf.Exp((float)(oSums[i] - max)) / scale);

        return result;
    }

    // Map values between newfrom and newto
    float Map(float newfrom, float newto, float origfrom, float origto, float value)
    {
        if (value <= origfrom)
            return newfrom;
        else if (value >= origto)
            return newto;
        return (newto - newfrom) * ((value - origfrom) / (origto - origfrom)) + newfrom;
    }

}
