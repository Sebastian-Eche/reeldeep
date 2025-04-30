using UnityEngine;
using System.Collections.Generic;

public class SmartFishMovement : MonoBehaviour
{
    public enum SwimStyle { Random, GoalSeeking, Straight }
    public SwimStyle swimStyle = SwimStyle.Straight; // Starting behavior

    public float moveSpeed = 3f;
    public Transform head;
    public Transform endPoint;

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

    // Timer for switching between straight and random after reaching grid
    public float swimToggleInterval = 2f;
    private float swimToggleTimer;
    private bool onGrid = false;

    // Singleton (Experimenting with only allowing 1 predator-prey interaction to be going on at same time)
    public static Fish globalTargetPrey = null;

    public float detectionRadius = 5f; // Radius for predator to detect prey
    private Fish fish;

    private float originalMoveSpeed;
    private Fish currentTargetPrey = null; // Track current prey target

    private void OnEnable()
    {
        Fish.OnFishHooked += PauseMovement;
    }

    void OnDisable()
    {
        Fish.OnFishHooked -= PauseMovement;
    }

    private void Start()
    {
        currFishX = transform.position.x;
        currFishY = transform.position.y;
        directionChangeTimer = directionChangeTime;

        // Update initial grid position based on new grid size after resize
        currentGridPos = diffusionGrid.WorldToGrid(transform.position);
        targetWorldPosition = transform.position;

        Debug.Log($"Target World Position: {targetWorldPosition}");

        fish = GetComponent<Fish>();
        originalMoveSpeed = moveSpeed;

        if (swimStyle == SwimStyle.Random)
            NewWayPoint(); // Set initial random destination

        Debug.Log($"[SmartFish] Initial GridPos: {currentGridPos}, Grid Size: {diffusionGrid.width}x{diffusionGrid.height}");
    }

    private void Update()
    {
        if (!GameManager.Instance.minigameStart){
            isSwimming = true;
        }
        currentGridPos = diffusionGrid.WorldToGrid(transform.position);

        // hasReachedGoal functionality may no longer be needed
        if (!isSwimming || hasReachedGoal) return;

        // Handle predator-prey detection or tracking
        if (!diffusionGrid.InBounds(currentGridPos.x, currentGridPos.y)) return;

        // Handle predator-prey detection or tracking
        if (fish != null && fish.fishInfo != null && !fish.fishInfo.isPrey)
        {
            if (currentTargetPrey == null)
            {
                DetectAndPursuePrey();
            }
            else if (currentTargetPrey != null)
            {
                Vector2Int preyGridPos = diffusionGrid.WorldToGrid(currentTargetPrey.transform.position);
                if (diffusionGrid.InBounds(preyGridPos.x, preyGridPos.y))
                {
                    diffusionGrid.SetDynamicGoal(currentTargetPrey.transform.position);
                }
                else
                {
                    // Prey is off-grid: stop pursuing
                    currentTargetPrey = null;
                    swimStyle = SwimStyle.Straight;
                    moveSpeed = originalMoveSpeed;
                    Debug.Log("Prey left grid. Predator stops pursuit.");
                }
            }
        }

        // Logic for switching between straight and random after reaching the grid
        if (!onGrid && diffusionGrid.InBounds(currentGridPos.x, currentGridPos.y))
        {
            onGrid = true;
            swimToggleTimer = swimToggleInterval;
        }

        if (onGrid && swimStyle != SwimStyle.GoalSeeking)
        {
            swimToggleTimer -= Time.deltaTime;
            if (swimToggleTimer <= 0f)
            {
                swimStyle = (swimStyle == SwimStyle.Straight) ? SwimStyle.Random : SwimStyle.Straight;
                swimToggleTimer = swimToggleInterval;
            }
        }

        // Perform the active behavior
        switch (swimStyle)
        {
            case SwimStyle.Random:
                RandomSwim();
                break;
            case SwimStyle.GoalSeeking:
                GoalSeek();
                break;
            case SwimStyle.Straight:
                MoveStraightToEndPoint(endPoint.position);
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
                // hasReachedGoal = true;

                if (currentTargetPrey != null)
                {
                    // Despawn the prey and switch back to normal movement patterns
                    Destroy(currentTargetPrey.gameObject); 
                    currentTargetPrey = null;
                    globalTargetPrey = null;
                    moveSpeed = originalMoveSpeed;
                    swimStyle = SwimStyle.Random;
                    Debug.Log("Prey caught and destroyed.");

                    // Start moving again
                    NewWayPoint();
                    TurnToWaypoint(targetWorldPosition);
                    isSwimming = true;
                }

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

    // Move in a straight line to a fixed endpoint
    void MoveStraightToEndPoint(Vector3 waypoint)
    {
        if ((waypoint - transform.position).magnitude < 0.01f)
        {
            isSwimming = false;
        }
        transform.position = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
        Debug.DrawLine(transform.position, waypoint, Color.red);
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


    // Public methods to pause/resume swimming
    void PauseMovement(Fish hookedFish){
        Debug.Log("PAUSEDDDD");
        isSwimming = false;
    }

    // predator detection and pursuit
    void DetectAndPursuePrey()
    {   
        // Another predator already has a prey (trying to limit predator and prey interactions)
        if (globalTargetPrey != null) return; 

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            Fish targetFish = hit.GetComponent<Fish>();
            if (targetFish != null && targetFish.fishInfo.isPrey)
            {
                currentTargetPrey = targetFish;
                globalTargetPrey = targetFish;
                diffusionGrid.SetDynamicGoal(currentTargetPrey.transform.position);
                swimStyle = SwimStyle.GoalSeeking;
                hasReachedGoal = false;
                moveSpeed = originalMoveSpeed + 4f; // Increase speed when pursuing
                Debug.Log($"Predator pursuing prey: {targetFish.fishInfo.fishSpecies}");
                break;
            }
        }
    }

    // Visualize predator detection radius
    void OnDrawGizmosSelected()
    {
        Fish f = GetComponent<Fish>();
        if (f != null && f.fishInfo != null && !f.fishInfo.isPrey)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}






