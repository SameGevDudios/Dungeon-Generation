using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    private bool[,] _tileArray;

    // This vector represents not a world position of a new tile,
    // but indexes of a _tileArray's element, that new tile occupy
    private Vector3 _tileGridPosition; 

    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _endingTile, _wall;
    private Stack<GameObject> _tileQueue = new Stack<GameObject>();
    private void Start()
    {
        InitializeArray();
        SpawnCenterTile();
        SpawnNextTile();
    }
    private void InitializeArray()
    {
        _tileArray = new bool[_dungeonSize, _dungeonSize];
    }
    private void SpawnCenterTile()
    {
        _tileGridPosition = new Vector3(_dungeonSize / 2, transform.position.y, _dungeonSize / 2);
        GameObject centerTile = Instantiate(_tile[0]);
        PlaceTile(centerTile, Quaternion.identity);
        _tileQueue.Push(centerTile);
    }
    private void SpawnNextTile()
    {
        if (_tileQueue.Count > 0)
        {
            GameObject previousTile = _tileQueue.Peek(); 
            _tileGridPosition = previousTile.GetComponent<Tile>().GetGridPosition();
            int spawn = previousTile.GetComponent<Tile>().GetNextSpawn();
            if(spawn != -1)
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
                    if (_tileArray[(int)_tileGridPosition.x, (int)_tileGridPosition.z])
                    {
                        SpawnNextTile();
                        return;
                    }
                    // Calculate rotation of a new tile
                    Vector3 spawnEuler = Vector3.up * (spawn - 1) * -90 + previousTile.transform.localEulerAngles;
                    // Randomly choose a new tile
                    int index = Random.Range(0, _tile.Length);
                    GameObject newTile = Instantiate(_tile[index]);
                    PlaceTile(newTile, Quaternion.Euler(spawnEuler));
                    _tileQueue.Push(newTile);
                }
            }
            else
            {
                // Place a wall

                // Remove tile from queue
                if (previousTile.GetComponent<Tile>().SpawnsAvalable == 0)
                {
                    _tileQueue.Pop();
                    print($"Queue has {_tileQueue.Count} elements");
                    
                }
            }
            SpawnNextTile();
        }
    }
    private void PlaceTile(GameObject tile, Quaternion rotation)
    {
        tile.transform.position = _tileGridPosition * _tileScale;
        tile.transform.rotation = rotation;
        _tileArray[(int)_tileGridPosition.x, (int)_tileGridPosition.z] = true;
        tile.GetComponent<Tile>().SetGridPosition(_tileGridPosition);
    }
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
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
