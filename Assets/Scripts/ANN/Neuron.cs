using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron
{

    public int numInputs;
    public double bias;
    public double output;
    public double errorGradient;
    public List<double> weights = new List<double>();
    public List<double> inputs = new List<double>();

    public Neuron(int nInputs)
    {
        numInputs = nInputs;

        SetRandomBias();
        SetRandomWeights();
    }

    public void SetRandomBias()
    {
        float weightRange = (float)2.4 / (float)numInputs;
        bias = UnityEngine.Random.Range(-weightRange, weightRange);
    }

    public void SetRandomWeights()
    {
        float weightRange = (float)2.4 / (float)numInputs;
        for (int i = 0; i < numInputs; i++)
            weights.Add(UnityEngine.Random.Range(-weightRange, weightRange));
    }
}
