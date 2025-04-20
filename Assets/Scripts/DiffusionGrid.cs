using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiffusionGrid : MonoBehaviour
{
    public int width = 40; // Number of grid cells in the x-direction
    public int height = 40; // Number of grid cells in the y-direction
    public Vector2Int goalPosition;  // This will be randomized every 420 frames
    public float diffusionRate = 0.9f; // Decay rate per step (Modify to change diffusion strength)
    public float[,] grid;
    public bool[,] obstacles;

    private int frameCounter = 0;  // Frame counter to track when to randomize the goal
    private Vector3 backgroundPosition;
    private Vector3 backgroundScale;

    public Vector2 gridOrigin = Vector2.zero; // Grid offset in worldspace

    void Start()
    {
        grid = new float[width, height];
        obstacles = new bool[width, height];

        Debug.Log($"Background Scale: {backgroundScale}");

        // Get background information (Needs to be attached to background)
        backgroundPosition = transform.position;  
        backgroundScale = transform.localScale;

        Debug.Log($"Background Scale: {backgroundScale}");

        CalculateGridOriginFromBackground();
        ResizeGridToMatchSprite();

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

        // First clear the old goal in the grid
        if (goalPosition.x >= 0 && goalPosition.y >= 0 && goalPosition.x < width && goalPosition.y < height)
        {
            grid[goalPosition.x, goalPosition.y] = 0f;
        }

        // Randomize new goal position
        int x = rand.Next(0, width);
        int y = rand.Next(0, height);

        // Ensure the goal is not placed on an obstacle
        while (obstacles[x, y])
        {
            x = rand.Next(0, width);
            y = rand.Next(0, height);
        }

        goalPosition = new Vector2Int(x, y);
        grid[goalPosition.x, goalPosition.y] = 100f;  // Set new goal
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
    public List<Vector2Int> GetDirections()
    {
        return new List<Vector2Int> {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,

            // Diagonal directions
            new Vector2Int(1, 1),    // NE
            new Vector2Int(-1, 1),   // NW
            new Vector2Int(1, -1),   // SE
            new Vector2Int(-1, -1)   // SW
        };
    }

    // Converts a world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x - gridOrigin.x);
        int y = Mathf.FloorToInt(worldPos.y - gridOrigin.y);
        return new Vector2Int(x, y);
    }

    // Converts grid coordinates to world position
    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x + gridOrigin.x, y + gridOrigin.y, 0);
    }

    // Align grid origin to background bottom-left corner
    void CalculateGridOriginFromBackground()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            gridOrigin = sr.bounds.min;
        }
    }

    void ResizeGridToMatchSprite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("No SpriteRenderer found on this GameObject.");
            return;
        }

        // Get the size of the sprite in world units
        Vector2 worldSize = sr.bounds.size;

        float cellSize = 1f; // Each grid cell is 1x1 unit

        // Calculate how many cells fit
        width = Mathf.FloorToInt(worldSize.x / cellSize);
        height = Mathf.FloorToInt(worldSize.y / cellSize);

        // Set grid origin to bottom-left of the sprite
        gridOrigin = sr.bounds.min;

        // Reinitialize the grid and obstacles
        grid = new float[width, height];
        obstacles = new bool[width, height];

        Debug.Log($"Grid resized: {width} x {height}, Origin: {gridOrigin}");
    }

    // For visualization/debug (This was all generated by LLM for testing purposes)
    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = grid[x, y] / 100f;
                Gizmos.color = (x == goalPosition.x && y == goalPosition.y)
                    ? new Color(1f, 0f, 0f, 0.5f)
                    : new Color(val, val, val, 0.1f);

                Vector3 cellPos = new Vector3(x + gridOrigin.x, y + gridOrigin.y, 0);
                Gizmos.DrawCube(cellPos, Vector3.one * 0.95f);
            }
        }

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle
        {
            fontSize = 12,
            normal = { textColor = Color.black }
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string label = grid[x, y].ToString("F1");
                Vector3 labelPosition = new Vector3(x + gridOrigin.x, y + gridOrigin.y, 0);
                Handles.Label(labelPosition, label, style);
            }
        }
#endif
    }
}
