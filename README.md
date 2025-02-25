# Unity ML-Agents Parking Project

This project is a simulation of an automated parking system built in Unity using ML-Agents. The agent is trained using reinforcement learning (PPO algorithm) to learn parking behaviors, including approaching the parking spot, avoiding obstacles, and ensuring all wheels and the car's center are correctly positioned within designated areas.
  
## Setup

1. **Unity Project:**  
   - Ensure that the ML-Agents package is imported (via Package Manager).

2. **Python Environment:**  
   - Install the ML‑Agents Python package:
     ```bash
     pip install mlagents
     ```
   - Ensure your `config.yaml` file is correctly set up with your desired training parameters.


## Training

To train the agent using ML-Agents, follow these steps:

1. **Prepare the Configuration:**  
   - Ensure your `config.yaml` file is set up with your desired training parameters (e.g., trainer type, learning rate, batch size, max_steps, etc.).

2. **Run the Training Command:**  
   - Open a terminal or command prompt.
   - Navigate to the root folder of your Unity project.
   - Run the following command (replace `your_run_id` with a unique identifier for this training run):
     ```
     mlagents-learn config.yaml --run-id=your_run_id --force
     ```
   - After the command starts, switch to the Unity Editor and press the **Play** button. The agent will begin training, and training logs will be displayed in the terminal.

3. **Monitoring Training:**  
   - Use TensorBoard to monitor training metrics such as Mean Reward, Standard Deviation of Reward, and other training statistics.  
   - To launch TensorBoard, run:
     ```
     tensorboard --logdir=results
     ```

## Testing the Trained Model

After training is complete:

1. **Assign the Model:**  
   - Locate the generated `.nn` file in the `results/your_run_id/` folder.
   - In Unity, select the car GameObject and open the **Behavior Parameters** component.
   - Change the **Behavior Type** to **Inference Only** and assign the `.nn` file to the **Model** field.

2. **Run the Scene:**  
   - Press **Play** in Unity to test the trained agent. The car should now use the trained policy to perform parking maneuvers autonomously.

## Technologies Used

- **Unity** for 3D simulation and environment creation.
- **ML-Agents** for reinforcement learning.
- **Python** for managing the training process and logging.

https://www.youtube.com/watch?v=SemdTwKCZUI
