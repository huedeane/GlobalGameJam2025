using System;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ProceduralMapGenerator : MonoBehaviour
{
    public GameObject Player;

    [Header("Map Dimensions")]
    public int MapWidth = 50;
    public int MapHeight = 50;

    [Header("Room Size")]
    public int RoomsizeLowerBound = 3;
    public int RoomsizeUpperBound = 7;

    [Header("Map Fill Constraints")]
    [Range(0, 100)]
    public int MinimumMapFillPercentage = 45;
    [Range(0, 100)]
    public int MaxMapFillPercentage = 60;

    [Header("Prefabs and Colors")]
    public GameObject WallPrefab;
    public GameObject FloorPrefab;
    public Color WallPlaceholderColor = Color.black;
    public Material FloorTexture;

    [Header("Scaling")]
    public float scaleFactor = 1000f;

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
        // Destroy old map
        GameObject _existingMap = GameObject.FindWithTag(mapName);
        if (_existingMap != null) DestroyImmediate(_existingMap);

        rnd = new Random((uint)DateTime.Now.Ticks);
        mapGrid = new GameObject[MapWidth, MapHeight];
        GameObject mapParent = new GameObject(mapName) { tag = mapName };

        // Initialize map as all walls
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                mapGrid[x, y] = GenerateVoid(x, y, WallPlaceholderColor, mapParent);
            }
        }

        // Pick a random start point near the center
        Vector2Int cursor = new Vector2Int(
            rnd.NextInt(MapWidth / 2 - 10, MapWidth / 2 + 10),
            rnd.NextInt(MapHeight / 2 - 10, MapHeight / 2 + 10)
        );

        Player.transform.position = new Vector3(cursor.x * scaleFactor, cursor.y * scaleFactor, 0); // <---- SCALED

        CreateRoom(rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound),
                   rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound),
                   cursor);

        int directionCache = -1;
        for (int i = 0; i < _maxGenerationSteps; i++)
        {
            int direction;
            do
            {
                direction = rnd.NextInt(0, 4);
            } while (IsOppositeDirection(directionCache, direction) || DirectionTakingUsOutOfBounds(direction, cursor));

            directionCache = direction;

            Vector2Int prevCursor = cursor;
            cursor = MoveCursor(cursor, direction, rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound));
            cursor = ApplyCenterBias(cursor);

            CreateRoom(rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound),
                       rnd.NextInt(RoomsizeLowerBound, RoomsizeUpperBound),
                       cursor);

            ConnectRooms(prevCursor, cursor);

            // Periodically check fill
            if (i % 5 == 0 && CheckMapFill())
                break;
        }
    }

    private GameObject GenerateVoid(int x, int y, Color color, GameObject parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);

        // Scale the position up
        obj.transform.position = new Vector3(x * scaleFactor, y * scaleFactor, 0);
        obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f); // <---- SCALED

        obj.name = $"Void ({x},{y})";
        obj.tag = "Void";

        var renderer = obj.GetComponent<Renderer>();
        renderer.material.color = color;
        obj.transform.parent = parent.transform;

        // Swap MeshCollider for BoxCollider2D
        DestroyImmediate(obj.GetComponent<MeshCollider>());
        var boxCollider = obj.AddComponent<BoxCollider2D>();
        // Normally this will auto-scale with the transform in 2D. 
        // If needed, you can manually set boxCollider.size.

        return obj;
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
        cursor.x += Mathf.Clamp(centerX - cursor.x, -1, 1);
        cursor.y += Mathf.Clamp(centerY - cursor.y, -1, 1);
        return cursor;
    }

    private void ConnectRooms(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        int hallwayWidth = rnd.NextInt(2, 5); // random hallway width

        // Move horizontally
        while (current.x != to.x)
        {
            current.x += (current.x < to.x) ? 1 : -1;
            CarveHallway(current, hallwayWidth, true);
        }

        // Move vertically
        while (current.y != to.y)
        {
            current.y += (current.y < to.y) ? 1 : -1;
            CarveHallway(current, hallwayWidth, false);
        }
    }

    private void CarveHallway(Vector2Int position, int width, bool isHorizontal)
    {
        if (isHorizontal)
        {
            for (int yOffset = -width / 2; yOffset <= width / 2; yOffset++)
            {
                SwapVoidToFloor(position.x, position.y + yOffset, FloorTexture);
            }
        }
        else
        {
            for (int xOffset = -width / 2; xOffset <= width / 2; xOffset++)
            {
                SwapVoidToFloor(position.x + xOffset, position.y, FloorTexture);
            }
        }
    }

    private bool CheckMapFill()
    {
        int mapFillPercentage = GetMapFillPercentage();
        if (mapFillPercentage >= MinimumMapFillPercentage)
        {
            if (mapFillPercentage >= MaxMapFillPercentage)
            {
                Debug.Log($"Map filled to maximum of {mapFillPercentage}%. Breaking loop.");
                return true;
            }
        }
        return false;
    }

    private bool IsOppositeDirection(int prevDirection, int newDirection)
    {
        if (prevDirection == -1) return false;
        return (prevDirection == 0 && newDirection == 2) ||
               (prevDirection == 2 && newDirection == 0) ||
               (prevDirection == 1 && newDirection == 3) ||
               (prevDirection == 3 && newDirection == 1);
    }

    private bool DirectionTakingUsOutOfBounds(int direction, Vector2Int cursor)
    {
        switch (direction)
        {
            case 0: return cursor.y + 10 >= MapHeight; // Up
            case 1: return cursor.x + 10 >= MapWidth;  // Right
            case 2: return cursor.y - 10 < 0;          // Down
            case 3: return cursor.x - 10 < 0;          // Left
        }
        return false;
    }

    public void CreateRoom(int roomWidth, int roomHeight, Vector2Int centerPoint)
    {
        for (int x = centerPoint.x - roomWidth / 2; x < centerPoint.x + roomWidth / 2; x++)
        {
            for (int y = centerPoint.y - roomHeight / 2; y < centerPoint.y + roomHeight / 2; y++)
            {
                SwapVoidToFloor(x, y, FloorTexture);
            }
        }
    }

    public void SwapVoidToFloor(int x, int y, Material texture)
    {
        if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return;

        GameObject obj = mapGrid[x, y];
        if (obj != null && obj.CompareTag("Void"))
        {
            obj.name = $"Floor ({x},{y})";
            obj.tag = "Floor";
            obj.GetComponent<Renderer>().material = texture;
            // remove collider if you don’t need collisions on the floor
            DestroyImmediate(obj.GetComponent<BoxCollider2D>());
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
