using System;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ProceduralMapGenerator : MonoBehaviour
{
    public GameObject Player;
    
    public int MapWidth = 50;
    public int MapHeight = 50;
    
    public int RoomsizeLowerBound = 3;
    public int RoomsizeUpperBound = 7;

    [Range(0, 100)]
    public int MinimumMapFillPercentage = 45;
    [Range(0, 100)]
    public int MaxMapFillPercentage = 60;

    public GameObject WallPrefab;
    public GameObject FloorPrefab;

    public Color WallPlaceholderColor = Color.black;
    public Color FloorPlaceholderColor = Color.white;

    private GameObject[,] mapGrid;
    private Random rnd;
    private string mapName = "Map";
    private int _maxGenerationSteps = 1000;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Reset and initialize
        GameObject _existingMap = GameObject.FindWithTag(mapName);
        if (_existingMap != null) DestroyImmediate(_existingMap);

        rnd = new Random((uint)DateTime.Now.Ticks);
        mapGrid = new GameObject[MapWidth, MapHeight];
        GameObject mapParent = new GameObject(mapName) { tag = mapName };

        // Initialize map with walls
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                mapGrid[x, y] = GenerateVoid(x, y, WallPlaceholderColor, mapParent);
            }
        }

        // Begin carving
        Vector2Int cursor = new Vector2Int(rnd.NextInt(MapWidth / 2 - 10, MapWidth / 2 + 10),
            rnd.NextInt(MapHeight / 2 - 10, MapHeight / 2 + 10));
        int directionCache = -1;
        
        Player.transform.position = new Vector3(cursor.x, cursor.y, 0);

        CreateRoom(rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound), rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound), cursor);

        for (int i = 0; i < _maxGenerationSteps; i++)
        {
            int direction;
            do
            {
                direction = rnd.NextInt(0, 4);
            } while (IsOppositeDirection(directionCache, direction) || DirectionTakingUsOutOfBounds(direction, cursor));

            directionCache = direction;

            Vector2Int prevCursor = cursor; // Save the previous position
            cursor = MoveCursor(cursor, direction, rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound));
            cursor = ApplyCenterBias(cursor); // Pull cursor toward the center

            CreateRoom(rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound), rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound), cursor);
            ConnectRooms(prevCursor, cursor); // Ensure connectivity

            if (i % 5 == 0 && CheckMapFill()) break; // Periodically check fill percentage
        }
    }

    private Vector2Int MoveCursor(Vector2Int cursor, int direction, int distance)
    {
        switch (direction)
        {
            case 0: cursor.y += distance; break; // Up
            case 1: cursor.x += distance; break; // Right
            case 2: cursor.y -= distance; break; // Down
            case 3: cursor.x -= distance; break; // Left
        }
        return cursor;
    }

    private Vector2Int ApplyCenterBias(Vector2Int cursor)
    {
        int centerX = MapWidth / 2;
        int centerY = MapHeight / 2;

        cursor.x += Mathf.Clamp(centerX - cursor.x, -1, 1); // Small nudge toward center
        cursor.y += Mathf.Clamp(centerY - cursor.y, -1, 1);
        return cursor;
    }

    private void ConnectRooms(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;

        // Define hallway width with some random variance
        int hallwayWidth = rnd.NextInt(2, 5); // Adjust the range to control the width

        // Move horizontally towards the target
        while (current.x != to.x)
        {
            current.x += current.x < to.x ? 1 : -1;
            CarveHallway(current, hallwayWidth, true); // Horizontal hallway
        }

        // Move vertically towards the target
        while (current.y != to.y)
        {
            current.y += current.y < to.y ? 1 : -1;
            CarveHallway(current, hallwayWidth, false); // Vertical hallway
        }
    }

// Helper function to carve hallways
    private void CarveHallway(Vector2Int position, int width, bool isHorizontal)
    {
        if (isHorizontal)
        {
            // Carve a horizontal hallway with width
            for (int yOffset = -width / 2; yOffset <= width / 2; yOffset++)
            {
                SwapVoidToFloor(position.x, position.y + yOffset, FloorPlaceholderColor);
            }
        }
        else
        {
            // Carve a vertical hallway with width
            for (int xOffset = -width / 2; xOffset <= width / 2; xOffset++)
            {
                SwapVoidToFloor(position.x + xOffset, position.y, FloorPlaceholderColor);
            }
        }
    }

    private bool CheckMapFill()
    {
        int mapFillPercentage = GetMapFillPercentage();
        if (mapFillPercentage >= MinimumMapFillPercentage)
        {
            //Debug.Log($"Map filled to minimum of{mapFillPercentage}%!");
            if( mapFillPercentage >= MaxMapFillPercentage)
            {
                Debug.Log($"Map filled to maximum of{mapFillPercentage}%! Breaking loop.");
                return true;
            }
        }
        return false;
    }

    private bool IsOppositeDirection(int prevDirection, int newDirection)
    {
        if (prevDirection == -1) return false;
        return (prevDirection == 0 && newDirection == 2) || (prevDirection == 2 && newDirection == 0) ||
               (prevDirection == 1 && newDirection == 3) || (prevDirection == 3 && newDirection == 1);
    }

    private bool DirectionTakingUsOutOfBounds(int direction, Vector2Int cursor)
    {
        switch (direction)
        {
            case 0: return cursor.y + 10 >= MapHeight;
            case 1: return cursor.x + 10 >= MapWidth;
            case 2: return cursor.y - 10 < 0;
            case 3: return cursor.x - 10 < 0;
        }
        return false;
    }

    private GameObject GenerateVoid(int x, int y, Color color, GameObject parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.transform.position = new Vector3(x, y, 0);
        obj.name = $"Void ({x},{y})";
        obj.tag = "Void";

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.material.color = color;
        obj.transform.parent = parent.transform;

        return obj;
    }

    public void CreateRoom(int roomWidth, int roomHeight, Vector2Int centerPoint)
    {
        for (int x = centerPoint.x - roomWidth / 2; x < centerPoint.x + roomWidth / 2; x++)
        {
            for (int y = centerPoint.y - roomHeight / 2; y < centerPoint.y + roomHeight / 2; y++)
            {
                SwapVoidToFloor(x, y, FloorPlaceholderColor);
            }
        }
    }

    public void SwapVoidToFloor(int x, int y, Color color)
    {
        if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return;

        GameObject obj = mapGrid[x, y];
        if (obj != null && obj.CompareTag("Void"))
        {
            obj.name = $"Floor ({x},{y})";
            obj.tag = "Floor";
            obj.GetComponent<Renderer>().material.color = color;
            //Remove mesh collider
            DestroyImmediate(obj.GetComponent<MeshCollider>());
        }
    }

    public int GetMapFillPercentage()
    {
        int filledTiles = 0;
        foreach (var obj in mapGrid)
        {
            if (obj.CompareTag("Floor")) filledTiles++;
        }
        return (int)(filledTiles / (float)(MapWidth * MapHeight) * 100);
    }
}