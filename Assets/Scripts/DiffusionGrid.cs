using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiffusionGrid : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public Vector2Int goalPosition;  // This will be randomized every 420 frames
    public float diffusionRate = 0.9f; // Decay rate per step (Modify to change diffusion strength)
    public float[,] grid;
    public bool[,] obstacles;

    private int frameCounter = 0;  // Frame counter to track when to randomize the goal

    void Start()
    {
        grid = new float[width, height];
        obstacles = new bool[width, height];

        // Initial randomization of the goal position
        RandomizeGoalPosition();

        // Place the goal value
        grid[goalPosition.x, goalPosition.y] = 100f;

        // Generate random obstacles
        GenerateObstacles();
    }

    void Update()
    {
        // Increment frame counter
        frameCounter++;

        // Randomize the goal position every 420 Frames (Blaze it?)
        if (frameCounter >= 420)
        {
            RandomizeGoalPosition();

            // Reset the frame counter
            frameCounter = 0;  
        }

        // Diffusion update logic
        if (grid == null || obstacles == null)
        {
            Debug.LogError("Grid or Obstacles not initialized!");
            return;
        }

        float[,] newGrid = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (obstacles[x, y])
                {
                    // Set new grid to 0 if position is an obstacle
                    newGrid[x, y] = 0f;
                    continue;
                }

                // Set goal position on new grid
                if (x == goalPosition.x && y == goalPosition.y)
                {
                    newGrid[x, y] = 100f; 
                    continue;
                }

                float maxNeighbor = 0f;

                // Apply diffusion by checking all directions (including diagonals)
                foreach (Vector2Int dir in GetDirections())
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    if (InBounds(nx, ny) && !obstacles[nx, ny])
                    {
                        float neighborValue = grid[nx, ny];

                        // Basically without this factor the diagonal diffuses too strongly
                        // A diagonal move is actually sqrt(2) or 1.414. 
                        // By multiplying by 1/sqrt2 this normalizes the move.
                        // Aka (This makes the diffusion pattern similar to the 4 direction one while still allowing diagonal movement)
                        // Apply diagonal decay for diagonal directions
                        if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
                        {   
                            neighborValue *= 0.7071f; 
                        }

                        maxNeighbor = Mathf.Max(maxNeighbor, neighborValue);
                    }
                }

                newGrid[x, y] = maxNeighbor * diffusionRate;
            }
        }

        grid = newGrid;
    }

    // Randomize the goal position to a valid grid cell
    void RandomizeGoalPosition()
    {
        System.Random rand = new System.Random();
        int x = rand.Next(0, width);
        int y = rand.Next(0, height);

        // Ensure the goal is not placed on an obstacle
        while (obstacles[x, y])
        {
            x = rand.Next(0, width);
            y = rand.Next(0, height);
        }

        goalPosition = new Vector2Int(x, y);
        grid[goalPosition.x, goalPosition.y] = 100f; 
    }

    // Generate random obstacles, avoiding the goal position
    void GenerateObstacles()
    {
        int obstacleCount = 0;
        int maxObstacles = 20;
        System.Random rand = new System.Random();

        while (obstacleCount < maxObstacles)
        {
            int x = rand.Next(0, width);
            int y = rand.Next(0, height);

            // Avoid placing on the goal or on an already placed obstacle
            if ((x == goalPosition.x && y == goalPosition.y) || obstacles[x, y])
                continue;

            obstacles[x, y] = true;
            obstacleCount++;
        }
    }

    // Make sure that everything stays in grid bounds
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Get possible directions (4 cardinal + 4 diagonal)
    // Note: Because of Agent logic the order of these in list will determine ties in diffusion values
    public List<Vector2Int> GetDirections()
    {
        return new List<Vector2Int> {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,

            // This allows for Diagonal Calculations
            new Vector2Int(1, 1),    // NE
            new Vector2Int(-1, 1),   // NW
            new Vector2Int(1, -1),   // SE
            new Vector2Int(-1, -1)   // SW
        };
    }

    // For visualization/debug (This was all generated by LLM for testing purposes)
    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = grid[x, y] / 100f; // Normalize to 0-1
                if (x == goalPosition.x && y == goalPosition.y)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
                }
                else
                {
                    Gizmos.color = new Color(val, val, val, 0.1f); // Non-opaque squares with transparency
                }
                Gizmos.DrawCube(new Vector3(x, y, 0), Vector3.one * 0.95f); // Draw grid cells
            }
        }

#if UNITY_EDITOR
        // Display diffusion values inside the squares
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.black; // Black for better visibility on light colors

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string label = grid[x, y].ToString("F1"); // Format the number to one decimal point
                Vector3 labelPosition = new Vector3(x, y, 0);

                // Adjust label position and size
                Handles.Label(labelPosition, label, style);
            }
        }
#endif
    }
}
