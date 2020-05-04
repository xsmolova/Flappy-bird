# Flappy-bird

This project focuses on the process of neuro-evolution of a neural network with a simple genetic algorithm. In individual experiments, we tried to evolve the neural network and a neural network with Q-learning to learn how to play a very popular mobile game - Flappy Bird.

For game logic as well as game assets we used [Unity Learn tutorial](https://learn.unity.com/tutorial/live-session-making-a-flappy-bird-style-game#).

## Manual
### Environment
This project requires download of Unity 2019.2.6f1 editor.

After opening the project in the Unity, click on the Population Manager game object in the Hierarchy window. The PopulationManager.cs script attached to the game object can be found in the Inspector window and has many variables that controll the experiments.

![PopManager](https://github.com/xsmolova/Flappy-bird/blob/master/insp.png)

### Variables
- Starting Pos - Vector2, starting position of a population.
- Population Size - number of birds in a population.
- End At Generation - stop the experiment at certain generation.
- Mutate - bool value, true if we want to mutate offsprings by Mutate Rate.
- Mutate Rate - probability of mutation.
- Current Population Score - shows the current score of the best bird in the population.
- Q Learning - bool value, true if we want to use Q-learning and train ANN from the memories of states, actions and rewards.
- Is Exploring - bool value, true if we want the ANN to choose a random action and explore state space by Explore Rate.
- Stop Exploring At - stop exploring at certain generation.
- Explore Rate - probability of choosing a random action.
- Max Explore Rate - maximum value of Explore Rate.
- Min Explore Rate - minimum value of Explore Rate.
- Explore Decay - value by which the Explore Rate is periodically reduced.
- Time Scale - speeds up the experiment execution, 1 - normal speed, 10 - super speed.
- Save Statistics - bool, if true saves score of each generation to a file.
- Save Weights To File - bool, when clicked weights of the best bird are save to a file (it doesn't stay checked).
- Load Weights From File - bool, if true first generation weights are loaded from a file.
