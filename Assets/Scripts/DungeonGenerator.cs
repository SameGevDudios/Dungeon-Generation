using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    [SerializeField] private int _seed;
    [SerializeField] private bool _useSeed;
    private float _f;
    private bool[,] _tileGrid;
    private bool _largeTileSpawned;
    // This vector represents not a world position of a new tile,
    // but indexes of a _tileArray's element, that new tile occupy
    private Grid _currentGridPosition; 

    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _wall;
    private Stack<GameObject> _tileQueue = new Stack<GameObject>();
    private List<GameObject> _tileSpawned = new List<GameObject>();
    private void Start()
    {
        StartSpawn();
    }
    private void StartSpawn()
    {
        if (_useSeed)
            Random.InitState(_seed);
        InitializeArray();
        SetStartPosition();
        SpawnRandomTile(Vector3.zero);
        ReserveSouthTile();
        GenerateDungeon();
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GenerateNewDungeon();
        }
    }
#endif
    private void GenerateDungeon()
    {
        while (_tileQueue.Count > 0) SpawnNextTile();
    }
    private void InitializeArray()
    {
        _tileGrid = new bool[_dungeonSize, _dungeonSize];
    }
    private void SetStartPosition()
    {
        // Set position of a first tile in the middle of a grid
        _currentGridPosition = new Grid(_dungeonSize / 2, _dungeonSize / 2);
    }
    private void ReserveSouthTile()
    {
        // Reserve a tile south from center for an entrance or elevator
        _tileGrid[_currentGridPosition.x, _currentGridPosition.z - 1] = true;
    }
    private void SpawnNextTile()
    {
        GameObject previousTile = _tileQueue.Peek();
        _currentGridPosition = previousTile.GetComponent<Tile>().GetGridPosition();
        _largeTileSpawned = previousTile.GetComponent<Tile>().IsLarge;
        int spawn = previousTile.GetComponent<Tile>().GetNextSpawn();
        if (spawn != -1)
        {
            // Calculate grid positon
            _f = Mathf.PI * spawn / 2 - Mathf.Deg2Rad * previousTile.transform.eulerAngles.y;
            AdjustGridPosition();
            if (_largeTileSpawned)
            {
                AdjustGridPosition();
            }
            // Check if a new tile would be inside of the grid
            if (InRange(_currentGridPosition.x, _currentGridPosition.z))
            {
                // Randomly choose other grid position if one is already used
                if (!_tileGrid[_currentGridPosition.x, _currentGridPosition.z])
                {
                    // Calculate rotation of a new tile
                    Vector3 spawnEuler = Vector3.up * (spawn - 1) * -90 + previousTile.transform.localEulerAngles;
                    // Randomly choose a new tile
                    SpawnRandomTile(spawnEuler);
                }
            }
        }
        else
        {
            // Place a wall

            // Remove tile from queue
            if (previousTile.GetComponent<Tile>().SpawnsAvalable == 0)
            {
                _tileQueue.Pop();
            }
        }
    }

    private void AdjustGridPosition()
    {
        _currentGridPosition.x += (int)Mathf.Cos(_f);
        _currentGridPosition.z += (int)Mathf.Sin(_f);
    }

    private void SpawnRandomTile(Vector3 spawnEuler)
    {
        // Instantiate a random tile
        int index = Random.Range(0, _tile.Length);
        GameObject newTile = Instantiate(_tile[index]);
        // Reserve a grid slot
        // A tile also can be large (3x3) in size
        bool isLarge = newTile.GetComponent<Tile>().IsLarge;
        if (isLarge)
        {
            AdjustGridPosition();
        }
        ReserveGridTile(isLarge ? 1 : 0);
        // Set tile's world position according to it's grid position
        PlaceTile(newTile, Quaternion.Euler(spawnEuler));
        // Add a new tile to queue
        _tileQueue.Push(newTile);
        _tileSpawned.Add(newTile);
    }
    private void PlaceTile(GameObject tile, Quaternion rotation)
    {
        tile.transform.position = _currentGridPosition.ToVector3() * _tileScale 
            + Vector3.up * transform.position.y;
        tile.transform.rotation = rotation;
        tile.GetComponent<Tile>().SetGridPosition(_currentGridPosition);
    }
    private void ReserveGridTile(int radius)
    {
        for (int i = _currentGridPosition.x-radius; i < _currentGridPosition.x + radius + 1; i++)
        {
            for (int j = _currentGridPosition.z - radius; j < _currentGridPosition.z + radius + 1; j++)
            {
                print($"Current x: {i}. Current y: {j}");
                if(InRange(i,j)) _tileGrid[i, j] = true;
            }
        }
    }
    private void GenerateNewDungeon()
    {
        ClearTiles();
        StartSpawn();
    }
    private void ClearTiles()
    {
        foreach (GameObject tile in _tileSpawned)
        {
            Destroy(tile);
        }
        _tileSpawned.Clear();
    }
    private void OnDrawGizmosSelected()
    {
        // Bigger the dungeon, laggier DrawGizmos gets,
        // so it's required to shut down visualization in case of a large dungeon
        if (Application.isPlaying && _dungeonSize < 100)
        {
            float verticalOffset = 5f;
            float radius = 0.3f;
            for (int i = 0; i < _dungeonSize; i++)
            {
                for (int j = 0; j < _dungeonSize; j++)
                {
                    Gizmos.color = _tileGrid[i, j] ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(new Vector3(_tileScale * i, verticalOffset, _tileScale * j), radius);
                }
            }
        }
    }
    private bool InRange(int x, int y)
    {
        return x > -1 && x < _dungeonSize
                && y > -1 && y < _dungeonSize;
    }
}
