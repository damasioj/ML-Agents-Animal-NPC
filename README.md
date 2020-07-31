# Master's Thesis: Animal Simulation

This project is being developed for a Master's thesis that studies the implementation of behaviours in subjects using Machine Learning. The AnimalSimulation project simulates the an animal in search for food while also avoiding dangers. 

## Summary

In this scenario, we assume that the animal is herbivorous and that no action must be taken to consume the food aside from standing at its location. For added complexity, the environment has a basic enemy to represent a predator that slowly follows the agent. If the predator reaches the agent, the agent “dies”, receives a penalty and the trial is reset. The predator has no actual AI, and is only used for additional complexity and creating a more realistic environment. The presented problem demonstrates the effectiveness of using RL in simplistic subjects with minimal spatial awareness. 

# Unity ML-Agents SDK

The project uses Unity ML-Agents to train agents. It includes the example environments but these are only used for reference purposes.

# Goals

The following goals of this project is to simulate an animal:

- [x] reaching a point in space
- [x] reaching and consuming an object
- [x] reaching and consuming an object while avoiding danger
- [x] reaching and consuming multiple objects while avoiding danger

# Requirements

To run this environment, you will need:
- Python 3.6.1 or greater
	- mlagents version 0.16.1
- Unity 2019.2.0f1 or greater

# Limitations and Observations

## Bias (Bad input variation)

One of the challenges faced when integrating the agent in the Ecosystem scenario was bias on certain input values, in this case the amount of resources a current target has. While training, this value always began with the same value (50) and decreased when the agent consumed the target. Due to this the agent created a bias and always expected the initial value of resources of the target to be the same. Changing this value in the Ecosystem caused the agent to fail at the given task.