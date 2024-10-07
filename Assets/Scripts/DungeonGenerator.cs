using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    [SerializeField] private int _seed;
    [SerializeField] private bool _useSeed;
    private bool[,] _tileArray;
    private List<GameObject> _tileSpawned = new List<GameObject>();

    // This vector represents not a world position of a new tile,
    // but indexes of a _tileArray's element, that new tile occupy
    private Vector3 _tileGridPosition; 

    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _endingTile, _wall;
    private Stack<GameObject> _tileQueue = new Stack<GameObject>();
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
        _tileArray = new bool[_dungeonSize, _dungeonSize];
    }
    private void SetStartPosition()
    {
        // Set position of a first tile in the middle of a grid
        _tileGridPosition = new Vector3(_dungeonSize / 2, transform.position.y, _dungeonSize / 2);
    }
    private void ReserveSouthTile()
    {
        // Reserve a tile south from center for an entrance or elevator
        _tileArray[(int)_tileGridPosition.x, (int)_tileGridPosition.z - 1] = true;
    }
    private void SpawnNextTile()
    {
        GameObject previousTile = _tileQueue.Peek();
        _tileGridPosition = previousTile.GetComponent<Tile>().GetGridPosition();
        int spawn = previousTile.GetComponent<Tile>().GetNextSpawn();
        if (spawn != -1)
        {
            // Calculate grid positon
            float f = Mathf.PI * spawn / 2 - Mathf.Deg2Rad * previousTile.transform.eulerAngles.y;
            _tileGridPosition.x += (int)Mathf.Cos(f);
            _tileGridPosition.z += (int)Mathf.Sin(f);
            // Check if a new tile would be inside of the grid
            bool inRange = _tileGridPosition.x > -1 && _tileGridPosition.x < _dungeonSize
                && _tileGridPosition.z > -1 && _tileGridPosition.z < _dungeonSize;
            if (inRange)
            {
                // Randomly choose other grid position if one is already used
                if (!_tileArray[(int)_tileGridPosition.x, (int)_tileGridPosition.z])
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
    private void SpawnRandomTile(Vector3 spawnEuler)
    {
        int index = Random.Range(0, _tile.Length);
        GameObject newTile = Instantiate(_tile[index]);
        PlaceTile(newTile, Quaternion.Euler(spawnEuler));
        _tileQueue.Push(newTile);
        _tileSpawned.Add(newTile);
    }
    private void PlaceTile(GameObject tile, Quaternion rotation)
    {
        tile.transform.position = _tileGridPosition * _tileScale;
        tile.transform.rotation = rotation;
        _tileArray[(int)_tileGridPosition.x, (int)_tileGridPosition.z] = true;
        tile.GetComponent<Tile>().SetGridPosition(_tileGridPosition);
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
                    Gizmos.color = _tileArray[i, j] ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(new Vector3(_tileScale * i, verticalOffset, _tileScale * j), radius);
                }
            }
        }
    }
}
