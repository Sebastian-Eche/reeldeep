using UnityEngine;
using System.Collections.Generic;

public class SmartFishMovement : MonoBehaviour
{
    public enum SwimStyle { Random, GoalSeeking }
    public SwimStyle swimStyle = SwimStyle.Random; // Starting behavior

    public float moveSpeed = 3f;
    public Transform head;

    public DiffusionGrid diffusionGrid;
    public Vector2Int currentGridPos;
    private Vector3 targetWorldPosition;
    private Quaternion keepRotation;

    private float currFishX;
    private float currFishY;
    private float directionChangeTimer;
    public float directionChangeTime = 3f;

    private bool isSwimming = true;
    private bool hasReachedGoal = false;

    // Timer for switching between behaviors
    public float behaviorSwitchInterval = 10f; // Time in seconds to switch behavior
    private float behaviorSwitchTimer;

    private void Start()
    {
        currFishX = transform.position.x;
        currFishY = transform.position.y;
        directionChangeTimer = directionChangeTime;

        // Update initial grid position based on new grid size after resize
        currentGridPos = diffusionGrid.WorldToGrid(transform.position);
        targetWorldPosition = transform.position;

        Debug.Log($"Target World Position: {targetWorldPosition}");

        // Debug log for tracking updated positions
        //Debug.Log($"Initial Grid Position: {currentGridPos} (World Position: {transform.position})");
        //Debug.Log($"Target World Position: {targetWorldPosition}");

        // Initialize behavior switch timer
        //behaviorSwitchTimer = behaviorSwitchInterval;

        if (swimStyle == SwimStyle.Random)
            NewWayPoint(); // Set initial random destination

        Debug.Log($"[SmartFish] Initial GridPos: {currentGridPos}, Grid Size: {diffusionGrid.width}x{diffusionGrid.height}");

    }

    

    private void Update()
    {   
        currentGridPos = diffusionGrid.WorldToGrid(transform.position);

        if (!isSwimming || hasReachedGoal) return;

        // Handle behavior switching on a timer
        // behaviorSwitchTimer -= Time.deltaTime;
        // if (behaviorSwitchTimer <= 0f)
        // {
        //     ToggleSwimStyle();
        //     behaviorSwitchTimer = behaviorSwitchInterval;
        // }

        // Perform the active behavior
        switch (swimStyle)
        {
            case SwimStyle.Random:
                RandomSwim();
                break;
            case SwimStyle.GoalSeeking:
                GoalSeek();
                break;
        }
    }

    // Swim randomly to different waypoints
    void RandomSwim()
    {
        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
        {
            head.rotation = keepRotation;
            if (directionChangeTimer <= 0f)
            {
                directionChangeTimer = directionChangeTime;
                NewWayPoint();
            }
            else
            {
                directionChangeTimer -= Time.deltaTime;
            }
        }
        else
        {
            keepRotation = head.rotation;
            TurnToWaypoint(targetWorldPosition);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
    }

    // Move toward the goal using the diffusion grid
    void GoalSeek()
    {
        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
        {
            // Stop if goal is reached
            if (currentGridPos == diffusionGrid.goalPosition)
            {
                hasReachedGoal = true;
                return;
            }

            // Get next best move based on diffusion values
            Vector2Int next = GetNextPosition();
            if (next != currentGridPos)
            {
                currentGridPos = next;
                targetWorldPosition = diffusionGrid.GridToWorld(next.x, next.y);
            }
        }

        TurnToWaypoint(targetWorldPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
    }

    // Choose a new random point to swim toward
    void NewWayPoint()
    {
        float randomX = Random.Range(currFishX - 5f, currFishX + 15f);
        float randomY = Random.Range(currFishY - 4f, currFishY + 4f);
        targetWorldPosition = new Vector3(randomX, randomY, 0);
        directionChangeTime = Mathf.Round(Random.Range(1.5f, directionChangeTime + 1));
    }

    // Smoothly rotate toward the next waypoint
    void TurnToWaypoint(Vector3 newWaypoint)
    {
        Vector3 dir = newWaypoint - head.position;
        Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, dir);
        head.rotation = Quaternion.Slerp(head.rotation, lookRotation, Time.deltaTime * moveSpeed);
    }

    // Get the next best position to move toward based on diffusion values
    Vector2Int GetNextPosition()
    {
        List<Vector2Int> neighbors = diffusionGrid.GetDirections();
        Vector2Int bestPosition = currentGridPos;
        float highest = -1f;

        foreach (Vector2Int dir in neighbors)
        {
            int nx = currentGridPos.x + dir.x;
            int ny = currentGridPos.y + dir.y;

            if (diffusionGrid.InBounds(nx, ny) && !diffusionGrid.obstacles[nx, ny])
            {
                float value = diffusionGrid.grid[nx, ny];
                if (value > highest)
                {
                    highest = value;
                    bestPosition = new Vector2Int(nx, ny);
                }
            }
        }

        return bestPosition;
    }

    // Switch between Random and GoalSeeking behaviors
    void ToggleSwimStyle()
    {
        swimStyle = (swimStyle == SwimStyle.Random) ? SwimStyle.GoalSeeking : SwimStyle.Random;
        Debug.Log("Switched swim style to: " + swimStyle);

        if (swimStyle == SwimStyle.Random)
        {
            directionChangeTimer = directionChangeTime;
            NewWayPoint(); // Get a fresh random target
        }
        else if (swimStyle == SwimStyle.GoalSeeking)
        {
            hasReachedGoal = false; // Allow seeking to start again
        }
    }

    // Public methods to pause/resume swimming
    public void PauseMovement() => isSwimming = false;
    public void ResumeMovement() => isSwimming = true;
}

