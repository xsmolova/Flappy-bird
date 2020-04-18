﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANN
{
    public int numInputs;
    public int numOutputs;
    public int numHidden;
    public int numNPerHidden;
    public double alpha;  //integrate certain percentage of training set to influence weights
    List<Layer> layers = new List<Layer>();

    public ANN(int nI, int nO, int nH, int nPH, double a)
    {
        numInputs = nI;
        numOutputs = nO;
        numHidden = nH;
        numNPerHidden = nPH;
        alpha = a;

        if (numHidden > 0)
        {
            layers.Add(new Layer(numNPerHidden, numInputs));

            for (int i = 0; i < numHidden - 1; i++)
            {
                layers.Add(new Layer(numNPerHidden, numNPerHidden));
            }
            layers.Add(new Layer(numOutputs, numNPerHidden));
        }
        else
        {
            layers.Add(new Layer(numOutputs, numInputs));
        }
    }

    public List<double> Go(List<double> inputValues, List<double> desiredOutput)
    {

        List<double> inputs;
        List<double> outputs = new List<double>();

        if (inputValues.Count != numInputs)
        {
            Debug.Log("Error: Number of Inputs must be " + numInputs);
            return outputs;
        }

        inputs = new List<double>(inputValues);
        for (int i = 0; i < numHidden + 1; i++)
        {
            if (i > 0)
            {
                inputs = new List<double>(outputs);
            }
            outputs.Clear();

            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                double actFunctionValue = 0;
                layers[i].neurons[j].inputs.Clear();

                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    layers[i].neurons[j].inputs.Add(inputs[k]);
                    actFunctionValue += layers[i].neurons[j].weights[k] * inputs[k]; // sum(weights * inputs)
                }

                actFunctionValue -= layers[i].neurons[j].bias;

                if (i == numHidden)
                    layers[i].neurons[j].output = ActivationFunctionOutputLayer(actFunctionValue);
                else
                    layers[i].neurons[j].output = ActivationFunction(actFunctionValue);

                outputs.Add(layers[i].neurons[j].output);
            }
        }
        UpdateWeights(outputs, desiredOutput);

        return outputs;
    }

    void UpdateWeights(List<double> outputs, List<double> desiredOutput)
    {
        double error;

        //Back propagation 
        for (int i = numHidden; i >= 0; i--)
        {
            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                if (i == numHidden)
                {
                    error = desiredOutput[j] - outputs[j];
                    layers[i].neurons[j].errorGradient = outputs[j] * (1 - outputs[j]) * error; //Delta rule
                }
                else
                {
                    layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1 - layers[i].neurons[j].output);
                    double errorGradSum = 0;
                    for (int p = 0; p < layers[i + 1].numNeurons; p++)
                    {
                        errorGradSum += layers[i + 1].neurons[p].errorGradient * layers[i + 1].neurons[p].weights[j];
                    }
                    layers[i].neurons[j].errorGradient *= errorGradSum;
                }
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    if (i == numHidden)
                    {
                        error = desiredOutput[j] - outputs[j];
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * error;
                    }
                    else
                    {
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
                    }
                }
                layers[i].neurons[j].bias += alpha * -1 * layers[i].neurons[j].errorGradient;
            }
        }

    }

    double ActivationFunction(double value)
    {
        return ReLU(value);
    }

    double ActivationFunctionOutputLayer(double value)
    {
        return Sigmoid(value);
    }

    //binary step function
    double Step(double value)
    {
        if (value < 0) return 0;
        return 1;
    }

    double Sigmoid(double value)
    {
        double k = System.Math.Exp(value);
        return k / (1.0f + k);
    }

    double TanH(double value)
    {
        double eToX2 = System.Math.Exp(2 * value);
        return (2.0f * eToX2) / (eToX2 + 1.0f);
    }

    double ReLU(double value)
    {
        if (value >= 0) return value;
        else return 0;
    }

    double LeakyReLU(double value)
    {
        if (value >= 0) return value;
        else return 0.01f * value;
    }

    double Sinusoid(double value)
    {
        return Mathf.Sin((float)value);
    }

    double ArcTan(double value)
    {
        return Mathf.Atan((float)value);
    }

    double SoftSign(double value)
    {
        return value / (1 + Mathf.Abs((float)value));
    }
}
