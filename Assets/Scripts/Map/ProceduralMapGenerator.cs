using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    public Material FloorTexture;
    public Material WallTexture;
    
    public Color EnemyNodeColor = Color.red;
    public Color SpawnNodeColor = Color.magenta;
    public Color ItemNodeColor = Color.blue;
    
    public Color SeaweedNodeColor = Color.green;
    public GameObject SeaweedPrefab;

    [Header("Scaling")]
    public float scaleFactor = 1000f;

    private GameObject[,] mapGrid;
    private Random rnd;
    private string mapName = "Map";
    private int _maxGenerationSteps = 1000;

    public NavMeshSurface NavMeshController;

    public List<Node> Nodes = new List<Node>();

    public bool IsGenerationDone = false;

    public GameObject[] MonsterPrefabs;
    
    
    public int MaxMonsters = 5;
    
    public struct Node {
        public GameObject obj;
        public NodeType type;
        public int x;
        public int y;
    }
    
    public enum NodeType
    {
        Enemy,
        Spawn,
        Item,
        Seaweed
    }
    
    [SerializeField]
    private SubBiomeData[] SubBiomes;

    [Serializable]
    private enum SubBiome
    {
        SeaweedForest,
        OpenPlains,
        SpawnArea,
        EnemyArea,
        TreasureTrove
    }
    
    [Serializable]
    private struct SubBiomeData
    {
        public SubBiome biome;
        
        [Range(0, 25)]
        public int GeneralDensity;
        
        [Range(0, 100)]
        public int ItemDensity;
        
        [Range(0, 100)]
        public int EnemyDensity;
        
        [Range(0, 100)]
        public int SeaweedDensity;

        public bool PlayerSpawnBiome;

        public bool ClusterSeaweed;

        public bool ClusterItems;
        
        public SubBiomeData(SubBiome biome, int generalDensity, int itemDensity, int enemyDensity, int seaweedDensity, bool playerSpawnBiome)
        {
            this.biome = biome;
            GeneralDensity = generalDensity;
            ItemDensity = itemDensity;
            EnemyDensity = enemyDensity;
            SeaweedDensity = seaweedDensity;
            PlayerSpawnBiome = playerSpawnBiome;
            ClusterSeaweed = false;
            ClusterItems = false;

            
        }
    }
    
    [Header("Debug")]
    //DEBUG: Skip NavMesh generation for performance
    public bool DEBUG_SKIP_NAVMESH_GENERATION = false;
    public bool DEBUG_USE_PLACEHOLDER_NODES = false;

    private void Start()
    {
        GenerateMap();
        
    }

    public void GenerateMap()
    {
        IsGenerationDone = false;
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
                mapGrid[x, y] = GenerateVoid(x, y, mapParent);
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
        
        RunPostGenerationIteration();
        
        // Generate the NavMesh
        if (!DEBUG_SKIP_NAVMESH_GENERATION && NavMeshController != null)
        {
            NavMeshController.SendMessage("BuildNavMesh");
            NavMeshController.BuildNavMesh();
            Debug.Log("NavMesh generated.");
        }
        IsGenerationDone = true;
    }

    private GameObject GenerateVoid(int x, int y, GameObject parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);

        // Scale the position up
        obj.transform.position = new Vector3(x * scaleFactor, y * scaleFactor, 0);
        obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f); // <---- SCALED

        obj.name = $"Void ({x},{y})";
        obj.tag = "Void";

        var renderer = obj.GetComponent<Renderer>();
        obj.transform.parent = parent.transform;

        // Swap MeshCollider for BoxCollider2D
        DestroyImmediate(obj.GetComponent<MeshCollider>());
        BoxCollider2D boxCollider = obj.AddComponent<BoxCollider2D>();
        // Normally this will auto-scale with the transform in 2D. 
        // 
        
        return obj;
    }

    private void CreateNode(Vector2Int position, NodeType type, GameObject manualGameObject = null)
    {
        
        GameObject obj = GenerateVoid(position.x, position.y, GameObject.FindWithTag(mapName));
        obj.name = $"{type} Node ({position.x},{position.y})";
        //obj.tag = "Node";

        switch (type)
        {
            case NodeType.Enemy:
                obj.GetComponent<Renderer>().material.color = EnemyNodeColor;
                GameObject enemy;
                if (manualGameObject)
                    enemy = manualGameObject;
                
                else
                {
                    Debug.Log("No GameObject Passed - Choosing a random monster");
                    enemy = MonsterPrefabs[rnd.NextInt(0, MonsterPrefabs.Length)];
                }
                DestroyImmediate(obj);
                
                //Instiate the enemy prefab at the proper position
                obj = Instantiate(enemy, new Vector3(position.x * scaleFactor, position.y * scaleFactor, 0), Quaternion.identity);
                obj.transform.parent = GameObject.FindWithTag(mapName).transform;
                
      
                break;
            case NodeType.Spawn:
                obj.GetComponent<Renderer>().material.color = SpawnNodeColor;
                break;
            case NodeType.Item:
                obj.GetComponent<Renderer>().material.color = ItemNodeColor;
                break;
            case NodeType.Seaweed:
                obj.GetComponent<Renderer>().material.color = SeaweedNodeColor;
                //Create Seaweed Prefab at proper position
                GameObject seaweed = Instantiate(SeaweedPrefab, obj.transform.position, Quaternion.identity);
                
                //Destroy the quad
                DestroyImmediate(obj);
                
                obj = seaweed;
                
                //Set parent to the map
                obj.transform.parent = GameObject.FindWithTag(mapName).transform;
                
                break;
        }
        
        //Set Z Axis to render above the floor
        obj.transform.position = new Vector3(position.x * scaleFactor, position.y * scaleFactor, -1); 
        
        //Set Sorting Layer to Layer 2 - 'Above Floor'
        obj.GetComponent<Renderer>().sortingLayerName = "Above Floor";
        
        //Remove Box Collider
        DestroyImmediate(obj.GetComponent<BoxCollider2D>());
        
        
        Node node = new Node
        {
            obj = obj,
            type = type,
            x = position.x,
            y = position.y
        };

        Nodes.Add(node);
        
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
            // remove collider if you don�t need collisions on the floor
            DestroyImmediate(obj.GetComponent<BoxCollider2D>());
            
            // Add NavMeshModifier
            NavMeshModifier mod = obj.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.area = 0;

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
    
    // Call this after generating the map
    private void RunPostGenerationIteration()
    {
        //Populate a dictionary of subbiome data for more efficient access. Handle duplicates
        Dictionary<SubBiome, SubBiomeData> subBiomeDict = new Dictionary<SubBiome, SubBiomeData>();
        
        //Generate an array of Monsters equal to the max number of monsters, populating sequentially so that all monsters are generated at least once
        Queue<GameObject> monstersQueue = new Queue<GameObject>();

        List<int> numbersUsed = new List<int>();
        
        for (int i = 0; i < MaxMonsters; i++)
        {
            if(numbersUsed.Count == MonsterPrefabs.Length)
            {
                numbersUsed.Clear();
            }

            int x, y = 0;
            do
            {
                x = rnd.NextInt(0, MonsterPrefabs.Length);
            } while (numbersUsed.Contains(x) && y++ < 1000);
            
            numbersUsed.Add(x);
            monstersQueue.Enqueue(MonsterPrefabs[x]);
        }
        
        Debug.Log("Monsters Queue: " + monstersQueue.Count);
        
        
        
        foreach (SubBiomeData subBiome in SubBiomes)
        {
            if (!subBiomeDict.ContainsKey(subBiome.biome))
            {
                subBiomeDict.Add(subBiome.biome, subBiome);
            }
            else
            {
                Debug.LogWarning("Duplicate SubBiome found in SubBiomes array. Ignoring duplicate.");
            }
        }


        
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                if (mapGrid[x, y] != null)
                {
                    if (mapGrid[x, y].CompareTag("Void"))
                    {
                        AddStoneToBorderWalls(x, y);
                    }
                    else if (mapGrid[x, y].CompareTag("Floor"))
                    {
                        (List<(Node node, int distance)> nearbyNodes, SubBiome regionBiome)  = GetNearbyContext(x, y);
                        Debug.Log("At position " + x + ", " + y + " there are " + nearbyNodes.Count + " nearby nodes in the region " + regionBiome);
                        
                        
                        //Get the subbiome data for the region
                        SubBiomeData regionData = subBiomeDict[regionBiome];
                        
                        //Make sure enemies and items are more than 5 units away from each other
                        bool disableEnemySpawn = false;
                        bool disableItemSpawn = false;

                        int seaweedCount = 0;
                        //If there is a non-seaweed node adjacent to this node, break to avoid important nodes right next to each other
                        foreach ((Node node, int distance) nearbyNode in nearbyNodes)
                        {
                            if (nearbyNode.distance == 1 && nearbyNode.node.type != NodeType.Seaweed)
                            {
                                break;
                            }
                            else if (regionData.ClusterSeaweed && nearbyNode.distance <= 1 && nearbyNode.node.type == NodeType.Seaweed)
                            {
                                seaweedCount++;
                            }
                            else if (nearbyNode.node.type == NodeType.Enemy)
                            {
                                disableEnemySpawn = true;
                            }
                            else if (nearbyNode.node.type == NodeType.Item)
                            {
                                disableItemSpawn = true;
                            }
                            
                        }
                        
                        //For each node, if there are 2 or more seaweed nodes within 2 units, break to avoid excessive seaweed
                        int seaweedChance = 20 - seaweedCount * 5;
                        
                        //10% chance to place a seaweed node and pass
                        if (seaweedCount > 0 && seaweedChance > 0 && rnd.NextInt(0, 100) < seaweedChance)
                        {
                            CreateNode(new Vector2Int(x, y), NodeType.Seaweed);
                            continue;
                        }




                        
                        //Determine if a node should be placed at this position
                        if (rnd.NextInt(0, 100) < regionData.GeneralDensity)
                        {
                            GameObject manualGameObject = null;
                            
                            //Determine the type of node to place
                            NodeType nodeType;
                            
                            //Add All Percentages together and generate a random number between 0 and the total
                            int total = regionData.ItemDensity + regionData.EnemyDensity + regionData.SeaweedDensity;
                            int random = rnd.NextInt(0, total);
                            
                            //Determine the type of node to place based on the random number
                            if (random < regionData.ItemDensity)
                            {
                                if (disableItemSpawn) continue;
                 
                                nodeType = NodeType.Item;
                            }
                            else if (random < regionData.ItemDensity + regionData.EnemyDensity)
                            {
                                Debug.Log("Attempting to place an enemy node.");
                                if (disableEnemySpawn || monstersQueue.Count == 0) continue;

                                Debug.Log("Enemy Node Placed.");
                                //Pop the first monster from the queue
                                manualGameObject = monstersQueue.Dequeue();
                                
                                nodeType = NodeType.Enemy; 
                            }
                            else
                            {
                                nodeType = NodeType.Seaweed;
                            }
                            
                            CreateNode(new Vector2Int(x, y), nodeType, manualGameObject);
                        }
                    }
                }
            }
        }
    }
    
    private (List<(Node node, int distance)> nearbyNodes, SubBiome regionBiome) GetNearbyContext(int x, int y)
    {
        int radius = 5;
        
        List<(Node node, int distance)> nearbyNodes = new List<(Node node, int distance)>();
        SubBiome regionBiome = SubBiome.OpenPlains;
        
        foreach(Node node in Nodes)
        {
            int distance = Mathf.Abs(node.x - x) + Mathf.Abs(node.y - y);
            if (distance <= radius)
            {
                nearbyNodes.Add((node, distance));
            }
        }
        
        //Sort by distance
        nearbyNodes.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        return (nearbyNodes, regionBiome);
    }

    public GameObject GetRandomFloorTileObject()
    {
        List<GameObject> floorTiles = new List<GameObject>();
        foreach (GameObject obj in mapGrid)
        {
            if(obj == null) continue;
            if (obj.CompareTag("Floor"))
            {
                floorTiles.Add(obj);
            }
        }

        if (floorTiles.Count == 0)
        {
            Debug.LogWarning("No floor tiles found in map.");
            return null;
        }

        return floorTiles[rnd.NextInt(0, floorTiles.Count)];
    }
    
    private void AddStoneToBorderWalls(int x, int y)
    {
        if (IsAdjacentToFloor(x, y))
        {
            // Convert the void block to a wall
            var obj = mapGrid[x, y];
            obj.name = $"Wall ({x},{y})";
            obj.tag = "Wall";
            obj.GetComponent<Renderer>().material = WallTexture;
            NavMeshModifier modifier = obj.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = 1;
        }
        else
        {
            // If the void block is unnecessary, destroy it
            DestroyImmediate(mapGrid[x, y]);
            mapGrid[x, y] = null; // Ensure the reference is cleared
        }
    }
    
    private bool IsAdjacentToFloor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                // Skip out-of-bounds tiles
                if (nx < 0 || nx >= MapWidth || ny < 0 || ny >= MapHeight)
                    continue;

                // Check if adjacent tile is a floor
                if (mapGrid[nx, ny] != null && mapGrid[nx, ny].CompareTag("Floor"))
                    return true;
            }
        }
        return false;
    }

}
