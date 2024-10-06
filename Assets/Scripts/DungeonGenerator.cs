using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    private bool[,] _tileArray;
    private Vector3 _tilePosition;
    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _endingTile, _wall;
    private Stack<GameObject> _tileQueue = new Stack<GameObject>();
    private void Start()
    {
        InitializeArray();
        SpawnCenter();
        SpawnNext();
    }
    private void InitializeArray()
    {
        _tileArray = new bool[_dungeonSize, _dungeonSize];
    }
    private void SpawnCenter()
    {
        _tilePosition = new Vector3(_dungeonSize / 2, transform.position.y, _dungeonSize / 2);
        GameObject centerTile = Instantiate(_tile[0]);
        PlaceTile(centerTile, Quaternion.identity);
        _tileQueue.Push(centerTile);
    }
    private void SpawnNext()
    {
        if (_tileQueue.Count > 0)
        {
            GameObject previousTile = _tileQueue.Peek();
            int spawn = previousTile.GetComponent<Tile>().GetNextSpawn();
            // Calculate grid positon
            float f = Mathf.PI * spawn / 2 - Mathf.Deg2Rad * previousTile.transform.eulerAngles.y;
            _tilePosition.x += (int)Mathf.Cos(f);
            _tilePosition.z += (int)Mathf.Sin(f);
            // Check if a new tile would be inside of the grid
            bool inRange = _tilePosition.x > -1 && _tilePosition.x < _dungeonSize
                && _tilePosition.z > -1 && _tilePosition.z < _dungeonSize;
            if (spawn != -1 && inRange)
            {
                // Randomly choose other tile if one is already used
                if (_tileArray[(int)_tilePosition.x, (int)_tilePosition.z])
                {
                    SpawnNext();
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
            else
            {
                // Place a wall

                // Remove tile from queue
                _tileQueue.Pop();
            }
            SpawnNext();
        }
    }
    private void PlaceTile(GameObject tile, Quaternion rotation)
    {
        tile.transform.position = _tilePosition * _tileScale;
        tile.transform.rotation = rotation;
        _tileArray[(int)_tilePosition.x, (int)_tilePosition.z] = true;
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
