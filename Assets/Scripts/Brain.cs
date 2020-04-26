using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public Replay(double distToTop, double distToBottom, double distVertGap, double distHorGap, double r)
    {
        states = new List<double>();
        // states.Add(distToTop);
        // states.Add(distToBottom);
        states.Add(distVertGap);
        states.Add(distHorGap);
        reward = r;
    }
}

public class Brain : MonoBehaviour
{
    // Colliders 
    public GameObject top;
    public GameObject bottom;

    public float timeScale = 1.0f;

    // Bird
    public float upForce = 250f;
    public int score = 0;
    public bool isDead = false;
    public bool isExploring = true;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 startPosition;

    // ANN Brain
    private ANN ann;

    float reward = 0.0f;
    List<Replay> replayMemory = new List<Replay>();
    int maxMemoryCapacity = 10000;

    float discount = 0.99f;
   public float exploreRate = 100.0f;
    float maxExploreRate = 100.0f;
    float minExploreRate = 0.01f;
    float exploreDecay = 0.0001f;

    // Stats
    float timer = 0;
    float maxFlightTime = 0;
    int failCount = 0;
    int maxScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        failCount = 0;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        score = 0;

        ann = new ANN(2, 2, 1, 6, 0.5f);
        startPosition = transform.position;
        Time.timeScale = timeScale;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) Flap();
        if(Input.GetKeyDown(KeyCode.T)) Time.timeScale = timeScale; 
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        List<double> states = new List<double>();
        List<double> qs = new List<double>();

        GameObject currColumn = GameController.instance.GetCurrentColumn();
        if (!currColumn) return;
        Debug.Log("current column " + currColumn );

        // states.Add(top.transform.position.y - transform.position.y);
        // states.Add(transform.position.y - bottom.transform.position.y);

        //Normalize
        float yDist = currColumn.transform.position.y - transform.position.y;
        float xDist = currColumn.transform.position.x - transform.position.x;

        yDist = 1 - (float)System.Math.Round((Map(-1, 1, -3.4f, 3.4f, yDist)),2);
        xDist = 1 - (float)System.Math.Round((Map(0, 1,0,10,xDist)),2);
        
        states.Add(yDist);
        states.Add(xDist);

        //Debug.Log("Y distance: " + yDist);
        //Debug.Log("X distance: " + xDist);

        qs = SoftMax(ann.CalcOutput(states));
        double maxQ = qs.Max();
        int maxQIndex = qs.ToList().IndexOf(maxQ);
        
        //Debug.Log("output 0: " + qs[0]);
        //Debug.Log("output 1: " + qs[1]);

        // Explore
        if (isExploring)
        {
           // exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);
            if (Random.Range(0, 100) < exploreRate)
                maxQIndex = Random.Range(0, 2);
        }

        if (maxQIndex == 0) Flap();

        if (isDead) reward = -1.0f;
        else reward = 0.1f;

        if (currColumn.tag == "scored") reward = 1.0f;


        Replay lastMemory = new Replay(0,0, yDist, xDist, reward);

        if (replayMemory.Count > maxMemoryCapacity)
            replayMemory.RemoveAt(0);

        replayMemory.Add(lastMemory);

        if (isDead)
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
                    maxQ = outputsNew.Max();
                    //Bellmans equation
                    feedback = replayMemory[i].reward + discount * maxQ;
                }

                outputsOld[action] = feedback;
                ann.Train(replayMemory[i].states, outputsOld);
            }

            if (timer > maxFlightTime)
                maxFlightTime = timer;

            if (score > maxScore)
                maxScore = score;

            timer = 0;

            ResetBird();
            replayMemory.Clear();
            failCount++;

        }


    }
 

    //if not dead flap
    private void Flap()
    {
        anim.SetTrigger("Flap");
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(0, upForce));
    }
    void ResetBird()
    {
        isDead = false;
        score = 0;
        rb.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
        transform.position = startPosition;

        // anim.SetBool("Die", isDead);
        GameController.instance.ResetGame();
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

    float Map(float newfrom, float newto, float origfrom, float origto, float value)
    {
        if (value <= origfrom)
            return newfrom;
        else if (value >= origto)
            return newto;
        return (newto - newfrom) * ((value - origfrom) / (origto - origfrom)) + newfrom;
    }

    float Round(float x)
    {
        return (float)System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 2.0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector2.zero;
        isDead = true;
        //  anim.SetBool("Die", isDead);
        GameController.instance.BirdDied();
    }

    GUIStyle gUIStyle = new GUIStyle();
    private void OnGUI()
    {
        gUIStyle.fontSize = 25;
        gUIStyle.normal.textColor = Color.white;
        GUI.BeginGroup(new Rect(10, 10, 600, 150));
        GUI.Box(new Rect(0, 0, 140, 140), "Stats", gUIStyle);
        GUI.Label(new Rect(10, 25, 500, 30), "Fails: " + failCount, gUIStyle);
        GUI.Label(new Rect(10, 50, 500, 30), "Decay Rate: " + exploreRate, gUIStyle);
        GUI.Label(new Rect(10, 75, 500, 30), "Last Best Time: " + maxFlightTime, gUIStyle);
        GUI.Label(new Rect(10, 100, 500, 30), "Last Best Score: " + maxScore, gUIStyle);
        GUI.Label(new Rect(10, 125, 500, 30), "This Flyght: " + timer, gUIStyle);
        GUI.EndGroup();
    }

}
