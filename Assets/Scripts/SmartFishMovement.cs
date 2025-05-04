using UnityEngine;
using System.Collections.Generic;

public class SmartFishMovement : MonoBehaviour
{
    public enum SwimStyle { Random, GoalSeeking, Straight }
    public SwimStyle swimStyle = SwimStyle.Straight; // Starting behavior

    public float moveSpeed = 5f;
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

    //Local wandering goal to simulate per-fish random behavior
    private Vector2Int localWanderGoal;
    private float localWanderTimer = 0f;
    private float localWanderInterval = 3f; // change direction every few seconds

    //cooldown to reduce jitter in random movement
    private float moveDecisionCooldown = 0.5f; // how often to decide next tile
    private float moveDecisionTimer = 0f;

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
        if (diffusionGrid == null)
        {
            GameObject bg = GameObject.Find("Background");
            if (bg != null)
                diffusionGrid = bg.GetComponent<DiffusionGrid>();
    
            if (diffusionGrid == null)
            {
                Debug.LogWarning("[SmartFishMovement] diffusionGrid is not set. Delaying Start().");
                return; // early out
            }
        }


        currFishX = transform.position.x;
        currFishY = transform.position.y;
        directionChangeTimer = directionChangeTime;

        // Update initial grid position based on new grid size after resize
        currentGridPos = diffusionGrid.WorldToGrid(transform.position);
        targetWorldPosition = transform.position;

        Debug.Log($"Target World Position: {targetWorldPosition}");

        fish = GetComponent<Fish>();
        originalMoveSpeed = moveSpeed;
        
        // if fish is out of bounds start with straight style
         if (!diffusionGrid.InBounds(currentGridPos.x, currentGridPos.y))
        {
            swimStyle = SwimStyle.Straight;
            Debug.Log("Fish spawned off-grid, starting in Straight mode.");
        }
        else if (swimStyle == SwimStyle.Random)
        {
            NewLocalWanderGoal(); 
        }

        Debug.Log($"[SmartFish] Initial GridPos: {currentGridPos}, Grid Size: {diffusionGrid.width}x{diffusionGrid.height}");
    }

    private void Update()
    {   if (diffusionGrid == null) return;
        if (!GameManager.Instance.minigameStart){
            isSwimming = true;
        }
        // currentGridPos = diffusionGrid.WorldToGrid(transform.position);

        // hasReachedGoal functionality may no longer be needed
        if (!isSwimming || hasReachedGoal) return;

        // Bounds Check (Maybe come back to this if having weird behavior on edges)
        // if (!diffusionGrid.InBounds(currentGridPos.x, currentGridPos.y)) return;

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
                    globalTargetPrey = null;
                    swimStyle = SwimStyle.Random;
                    moveSpeed = originalMoveSpeed;
                    // Reset Wander
                    NewLocalWanderGoal(); 
                    Debug.Log("Prey left grid. Predator stops pursuit.");
                }
            }
        }

        // Logic for switching between straight and random after reaching the grid
       if (swimStyle == SwimStyle.Straight && diffusionGrid.InBounds(currentGridPos.x, currentGridPos.y))
        {
            onGrid = true;

            swimStyle = SwimStyle.Random;
            NewLocalWanderGoal();
            Vector2Int next = GetNextPositionToward(localWanderGoal); // NEW
            targetWorldPosition = diffusionGrid.GridToWorld(next.x, next.y); // NEW
            Debug.Log("Fish entered grid and switched from Straight to Random.");
 
        }
        
        //update cooldown timer
        moveDecisionTimer -= Time.deltaTime;

        // Perform the active behavior
        switch (swimStyle)
        {
            case SwimStyle.Random:
                DiffusionBasedRandomSwim(); 
                break;
            case SwimStyle.GoalSeeking:
                GoalSeek();
                break;
            case SwimStyle.Straight:
                MoveStraightToEndPoint(endPoint.position);
                break;
        }
    }

    // Swim using diffusion grid in Random mode
    void DiffusionBasedRandomSwim()
    {
        // Update local goal every few seconds
        localWanderTimer -= Time.deltaTime;
        if (localWanderTimer <= 0f)
        {
            NewLocalWanderGoal();
        }

        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f && moveDecisionTimer <= 0f)
        {
            Vector2Int next = GetNextPositionToward(localWanderGoal);
            if (next != currentGridPos)
            {
                currentGridPos = next;
                targetWorldPosition = diffusionGrid.GridToWorld(next.x, next.y);
                // reset decision timer
                moveDecisionTimer = moveDecisionCooldown;  
            }
        }

        TurnToWaypoint(targetWorldPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
    }

    // Choose a local wander goal in grid space
   void NewLocalWanderGoal()
    {
        int buffer = 2; // margin from the edge to avoid selecting border tiles
        int range = 10;

        int minX = Mathf.Max(buffer, currentGridPos.x - range);
        int maxX = Mathf.Min(diffusionGrid.width - 1 - buffer, currentGridPos.x + range);

        int minY = Mathf.Max(buffer, currentGridPos.y - range);
        int maxY = Mathf.Min(diffusionGrid.height - 1 - buffer, currentGridPos.y + range);

        int x = Random.Range(minX, maxX + 1);
        int y = Random.Range(minY, maxY + 1);

        localWanderGoal = new Vector2Int(x, y);
        localWanderTimer = localWanderInterval;
    }       


    // Get next position toward a specific goal (does not affect grid)
    Vector2Int GetNextPositionToward(Vector2Int goal)
    {
        List<Vector2Int> neighbors = diffusionGrid.GetDirections();
        Vector2Int bestPosition = currentGridPos;
        float bestDist = float.MaxValue;

        foreach (Vector2Int dir in neighbors)
        {
            int nx = currentGridPos.x + dir.x;
            int ny = currentGridPos.y + dir.y;
            if (!diffusionGrid.InBounds(nx, ny) || diffusionGrid.obstacles[nx, ny]) continue;

            float dist = Vector2Int.Distance(new Vector2Int(nx, ny), goal);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestPosition = new Vector2Int(nx, ny);
            }
        }

        return bestPosition;
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
                    NewLocalWanderGoal();
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

    // Visualize predator detection radius (LLM generated)
        // Visualize predator detection radius and local wander direction
void OnDrawGizmosSelected()
{   
    // Predator detection radius
    Fish f = GetComponent<Fish>();
    if (f != null && f.fishInfo != null && !f.fishInfo.isPrey)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // draw line toward local wander goal (only in Random mode)
    if (swimStyle == SwimStyle.Random && diffusionGrid != null)
    {
        Vector3 goalWorldPos = diffusionGrid.GridToWorld(localWanderGoal.x, localWanderGoal.y);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, goalWorldPos);
        Gizmos.DrawSphere(goalWorldPos, 0.1f);
    }
}

    // One-time switch to Random after grid entry (only if not in prey pursuit)
    void SwitchToRandomAfterGridEntry()
    {
        // Only switch if not currently pursuing prey
        if (swimStyle != SwimStyle.GoalSeeking)
        {
            swimStyle = SwimStyle.Random;
            NewLocalWanderGoal();
            Debug.Log("Switched to Random after entering grid.");
        }
    }
}






