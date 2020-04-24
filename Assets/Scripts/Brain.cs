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

    // Bird
    public float upForce = 250f;
    public int score = 0;
    public bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 startPosition;

    // ANN Brain
    private ANN ann;

    float reward = 0.0f;
    List<Replay> replayMemory = new List<Replay>();
    int maxMemoryCapacity = 10000;

    float discount = 0.99f;
    float exploreRate = 10.0f;
    float maxExploreRate = 10.0f;
    float minExploreRate = 0.01f;
    float exploreDecay = 0.0001f;

    // Timer
    float timer = 0;
    float maxFlightTime = 0;
    int failCount;

    // Start is called before the first frame update
    void Start()
    {
        failCount = 0;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        score = 0;

        ann = new ANN(2, 2, 1, 6, 0.2f);
        startPosition = transform.position;
        Time.timeScale = 5.0f;
    }
    private void Update()
    {
        if (Input.GetKeyDown("space")) Flap();
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        List<double> states = new List<double>();
        List<double> qs = new List<double>();

        GameObject currColumn = GameController.instance.GetCurrentColumn();

       // states.Add(top.transform.position.y - transform.position.y);
       // states.Add(transform.position.y - bottom.transform.position.y);
        states.Add(currColumn.transform.position.y - transform.position.y);
        states.Add(currColumn.transform.position.x - transform.position.x);

        qs = SoftMax(ann.CalcOutput(states));
        double maxQ = qs.Max();
        int maxQIndex = qs.ToList().IndexOf(maxQ);

        // Explore
         exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);
         if(Random.Range(0,100) < exploreRate)
        	maxQIndex = Random.Range(0,2);

        if (maxQIndex == 0) Flap();

        if (isDead) reward = -10.0f;
        else if (currColumn.tag == "scored") reward = 0.5f;
        else reward = 0.1f;

        Replay lastMemory = new Replay(top.transform.position.y - transform.position.y,
                                       transform.position.y - bottom.transform.position.y,
                                       currColumn.transform.position.y - transform.position.y,
                                       currColumn.transform.position.x - transform.position.x,
                                       reward);
        if (replayMemory.Count > maxMemoryCapacity)
            replayMemory.RemoveAt(0);

        replayMemory.Add(lastMemory);

        if (isDead)
        {
            for (int i = replayMemory.Count - 1; i >= 0; i--)
            {
                List<double> outputsOld = new List<double>();
                List<double> outputsNew = new List<double>();
                outputsOld = SoftMax(ann.CalcOutput(replayMemory[i].states));

                double maxQOld = outputsOld.Max();
                int action = outputsOld.ToList().IndexOf(maxQOld);

                double feedback;
                if (i == replayMemory.Count - 1)
                    feedback = replayMemory[i].reward;
                else
                {
                    outputsNew = SoftMax(ann.CalcOutput(replayMemory[i+1].states));
                    maxQ = outputsNew.Max();
                    //Bellmans equation
                    feedback = replayMemory[i].reward + discount * maxQ;
                }

                outputsOld[action] = feedback;
                ann.Train(replayMemory[i].states, outputsOld);
            }

            if (timer > maxFlightTime)
            {
                maxFlightTime = timer;
            }

            timer = 0;

            isDead = false;
            ResetBird();
            replayMemory.Clear();
            failCount++;
            Debug.Log("fail count " + failCount);
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
        rb.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
        transform.position = startPosition;
       
        isDead = false;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector2.zero;
        isDead = true;
      //  anim.SetBool("Die", isDead);
        GameController.instance.BirdDied();
    }

}
