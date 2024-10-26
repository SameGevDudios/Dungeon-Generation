using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    [SerializeField] private int _seed;
    [SerializeField] private bool _useSeed;
    private bool[,] _tileGrid;
    private bool _largeTileSpawned;
    private float _f;
    // This vector represents not a world position of a new tile,
    // but indexes of a _tileArray's element, that new tile occupy
    private Grid _currentGridPosition;
    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _emptyTile;
    private GameObject _currentTile;
    private Stack<GameObject> _tileQueue = new();
    private List<GameObject> _tileSpawned = new();

    [Header("Testing")]
    [SerializeField] private bool _stepByStepSpawn;
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
        GetNewTile();
        SpawnRandomTile(Vector3.zero);
        ReserveSouthTile();
        if (!_stepByStepSpawn)
        {
            GenerateDungeon();
            FillEmptySlots();
        }
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(_stepByStepSpawn) 
                SpawnNextTile();
            else
                GenerateNewDungeon();
        }
    }
#endif
    private void GenerateNewDungeon()
    {
        ClearTiles();
        StartSpawn();
    }
    private void ClearTiles()
    {
        foreach (GameObject tile in _tileSpawned)
            Destroy(tile);
        _tileSpawned.Clear();
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
    private void GenerateDungeon()
    {
        while (_tileQueue.Count > 0) 
            SpawnNextTile();
    }
    private void SpawnNextTile()
    {
        GameObject previousTileObject = _tileQueue.Peek();
        Tile previousTile = previousTileObject.GetComponent<Tile>();
        int spawn = previousTile.GetNextSpawn();
        _largeTileSpawned = previousTile.IsLarge();
        _currentGridPosition = previousTile.GetGridPosition();
        if (spawn != -1)
        {
            // Calculate grid positon
            _f = Mathf.PI * spawn / 2 - Mathf.Deg2Rad * previousTileObject.transform.eulerAngles.y;
            AdjustGridPosition();
            if (_largeTileSpawned)
                AdjustGridPosition();
            TrySpawnTile(previousTileObject, spawn);
        }
        else
        {
            // Remove tile from queue
            if (previousTile.SpawnsAvalable == 0)
            {
                _tileQueue.Pop();
            }
        }
    }

    private void TrySpawnTile(GameObject previousTileObject, int spawn)
    {
        Tile currentTile = GetNewTile();
        // Check if a new tile would be inside of the grid
        if (InRange(_currentGridPosition.x, _currentGridPosition.z,
            currentTile.IsLarge() ? 1 : 0))
        {
            // Check if space is sufficient for spawning current tile
            if (!GridSlotReserved(currentTile.IsLarge() ? 1 : 0))
            {
                // Calculate rotation of a new tile
                Vector3 spawnEuler = Vector3.up * (spawn - 1) * -90 + previousTileObject.transform.localEulerAngles;
                // Randomly choose a new tile
                SpawnRandomTile(spawnEuler);
            }
            else
            {
                // Check if space is sufficient for spawning at least 1x1 tile
                if (!GridSlotReserved(0))
                    TrySpawnTile(previousTileObject, spawn);
            }
        }
        else
        {
            SpawnEmptyTile(_currentGridPosition.x, _currentGridPosition.z);
        }
    }

    private void AdjustGridPosition()
    {
        _currentGridPosition.x += (int)Mathf.Cos(_f);
        _currentGridPosition.z += (int)Mathf.Sin(_f);
    }
    private void SpawnRandomTile(Vector3 spawnEuler)
    {
        GameObject newTile = Instantiate(_currentTile);
        Tile tile = newTile.GetComponent<Tile>();
        // Reserve a grid slot
        // A tile also can be large (3x3) in size
        if (tile.IsLarge())
            AdjustGridPosition();
        if((int)spawnEuler.y % 180 == 0)
            ReserveGridTile(tile.GetWidth() - 1, tile.GetLength() - 1);
        else
            ReserveGridTile(tile.GetLength() - 1, tile.GetWidth() - 1);
        // Set tile's world position according to it's grid position
        PlaceTile(newTile, Quaternion.Euler(spawnEuler));
        // Add a new tile to queue
        _tileQueue.Push(newTile);
        _tileSpawned.Add(newTile);
    }
    private void PlaceTile(GameObject tile, Quaternion rotation)
    {
        tile.transform.position = _currentGridPosition.ToVector3() * _tileScale +
            Vector3.up * transform.position.y;
        tile.transform.rotation = rotation;
        tile.GetComponent<Tile>().SetGridPosition(_currentGridPosition);
    }
    private void ReserveGridTile(int radius)
    {
        for (int i = _currentGridPosition.x - radius; i <= _currentGridPosition.x + radius; i++)
            for (int j = _currentGridPosition.z - radius; j <= _currentGridPosition.z + radius; j++)
                if (InRange(i, j))
                    _tileGrid[i, j] = true;
    }
    private void ReserveGridTile(int width, int length)
    {
        for (int i = _currentGridPosition.x - width; i <= _currentGridPosition.x + width; i++)
            for (int j = _currentGridPosition.z - length; j <= _currentGridPosition.z + length; j++)
                if (InRange(i, j))
                    _tileGrid[i, j] = true;
    }
    private void FillEmptySlots()
    {
        for (int i = -1; i < _dungeonSize - 1; i++)
        {
            for (int j = -1; j < _dungeonSize - 1; j++)
            {
                if (!InRange(i, j) || !_tileGrid[i, j])
                {
                    SpawnEmptyTile(i, j);
                }
            }
        }
    }
    private void SpawnEmptyTile(int x, int z)
    {
        Vector3 position = (Vector3.right * x + Vector3.forward * z) * _tileScale + 
            Vector3.up * transform.position.y;
        _tileSpawned.Add(Instantiate(_emptyTile, position, Quaternion.identity));
    }
    private Tile GetNewTile()
    {
        int index = Random.Range(0, _tile.Length);
        _currentTile = _tile[index];
        return _currentTile.GetComponent<Tile>();
    }
    private bool GridSlotReserved(int radius)
    {
        for (int i = _currentGridPosition.x; i <= _currentGridPosition.x + radius; i++)
        {
            for (int j = _currentGridPosition.z; j <= _currentGridPosition.z + radius; j++)
            {
                if (InRange(i,j) && _tileGrid[i, j])
                        return true;
            }
        } 
        return false;
    }
    private bool GridSlotReserved(int width, int length)
    {
        for (int i = _currentGridPosition.x-width; i <= _currentGridPosition.x + width; i++)
        {
            for (int j = _currentGridPosition.z - length; j <= _currentGridPosition.z + length; j++)
            {
                if (InRange(i, j) && _tileGrid[i, j])
                        return true;
            }
        } 
        return false;
    }
    private bool InRange(int x, int z, int radius)
    {
        for(int i = x - radius; i <= x + radius; i++)
        {
            for (int j = z - radius; j <= z + radius; j++)
            {
                if (!InRange(i, j))
                    return false;
            }
        }
        return true;
    }
    private bool InRange(int x, int z)
    {
        return x > -1 && x < _dungeonSize
                && z > -1 && z < _dungeonSize;
    }
    private void OnDrawGizmosSelected()
    {
        // Bigger the dungeon, laggier DrawGizmos gets,
        // so it's required to shut down visualization in case of a large dungeon
        if (Application.isPlaying && _dungeonSize < 100)
        {
            float verticalOffset = transform.position.y + 3f;
            float radius = 0.3f;
            for (int i = 0; i < _dungeonSize; i++)
            {
                for (int j = 0; j < _dungeonSize; j++)
                {
                    Gizmos.color = _tileGrid[i, j] ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(new Vector3(i * _tileScale, verticalOffset, j * _tileScale), radius);
                }
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(_currentGridPosition.x * _tileScale, 
                verticalOffset, _currentGridPosition.z * _tileScale), radius);
        }
    }
}
