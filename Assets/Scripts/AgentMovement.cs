using UnityEngine;
using System.Collections.Generic;

public class AgentMovement : MonoBehaviour
{
    public DiffusionGrid diffusionGrid; // Reference to the DiffusionGrid script
    public Vector2Int currentPosition;  // Manually set in Inspector
    public float moveSpeed = 2f;        // Speed at which the agent moves
    public Vector2Int goalPosition;     // The goal position

    private Vector3 targetWorldPosition;
    private bool hasReachedGoal = false;

    private void Start()
    {
        // Set the initial world position based on the current grid position
        transform.position = new Vector3(currentPosition.x, currentPosition.y, 0);
        targetWorldPosition = transform.position;

        // Access the goal position from the DiffusionGrid instance
        goalPosition = diffusionGrid.goalPosition;
    }

    private void Update()
    {
        goalPosition = diffusionGrid.goalPosition;
        if (hasReachedGoal)
        {
            return;
        }

        // Smoothly move to the target world position
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);

        // Only choose a new position if we've reached the current target
        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
        {
            MoveAgent();
        }
    }

    private void MoveAgent()
    {
        // Debug: log current position and goal position
        Debug.Log($"Current Position: {currentPosition}, Goal Position: {goalPosition}");

        // Check if the agent is exactly at the goal position
        if (currentPosition == goalPosition)
        {
            hasReachedGoal = true;
            targetWorldPosition = new Vector3(goalPosition.x, goalPosition.y, 0); 
            return;
        }

        Vector2Int nextPosition = GetNextPosition();

        // If we wanted it to stop
        // // If next position is goal, directly set it as the current position
        // if (nextPosition == goalPosition)
        // {
        //     currentPosition = goalPosition;
        //     targetWorldPosition = new Vector3(goalPosition.x, goalPosition.y, 0);
        //     hasReachedGoal = true;
        //     return;
        // }

        // If the next position is different from the current one, update position
        if (nextPosition != currentPosition)
        {
            currentPosition = nextPosition;
            targetWorldPosition = new Vector3(currentPosition.x, currentPosition.y, 0);
        }
    }

    private Vector2Int GetNextPosition()
    {
        List<Vector2Int> neighbors = diffusionGrid.GetDirections();
        Vector2Int bestPosition = currentPosition;
        float highestDiffusion = -1f;

        foreach (Vector2Int direction in neighbors)
        {
            int nx = currentPosition.x + direction.x;
            int ny = currentPosition.y + direction.y;

            if (diffusionGrid.InBounds(nx, ny) && !diffusionGrid.obstacles[nx, ny])
            {
                float diffusionValue = diffusionGrid.grid[nx, ny];

                if (diffusionValue > highestDiffusion)
                {
                    highestDiffusion = diffusionValue;
                    bestPosition = new Vector2Int(nx, ny);
                }
            }
        }

        return bestPosition;
    }
}