using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    private int _previousSpawn = 0;
    private bool[,] _tileArray;
    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _endingTile, _wall;
    private Stack<Tile> _tileQueue = new Stack<Tile>();

    int spawnCount = 30, _currentX, _currentY;
    [Header("Grid Visualization")]
    [SerializeField] private GameObject[] _row0, _row1, _row2;
    private void Start()
    {
        InitArray();
        SpawnCenter();
        SpawnNext();
    }
    private void InitArray()
    {
        _tileArray = new bool[_dungeonSize, _dungeonSize];
    }
    private void SpawnCenter()
    {
        _currentX = _dungeonSize / 2;
        _currentY = _currentX;
        Tile centerTile = Instantiate(_tile[0]).GetComponent<Tile>();
        PlaceTile(centerTile, Quaternion.identity);
        _tileQueue.Push(centerTile);
    }
    private void SpawnNext()
    {
        if(spawnCount > 0)
        {
            if (_tileQueue.Count > 0)
            {
                Tile previousTile = _tileQueue.Peek();
                int spawn = previousTile.GetNextSpawn();
                // calculate grid positon
                float f = Mathf.PI * spawn / 2 - Mathf.Deg2Rad * previousTile.transform.eulerAngles.y;
                _currentX += (int)Mathf.Cos(f);
                _currentY += (int)Mathf.Sin(f);
                bool inRange = _currentX > -1 && _currentX < _dungeonSize && _currentY > -1 && _currentY < _dungeonSize;
                if (spawn >= 0 && inRange)
                {
                    if (_tileArray[_currentX, _currentY])
                    {
                        SpawnNext();
                        return;
                    }
                    // calculate rotation of a new tile
                    Vector3 spawnEuler = Vector3.up * (spawn - 1) * -90 + previousTile.transform.localEulerAngles;
                    // randomly choose a new tile
                    int index = Random.Range(0, _tile.Length);
                    Tile newTile = Instantiate(_tile[index]).GetComponent<Tile>();
                    PlaceTile(newTile, Quaternion.Euler(spawnEuler));
                    _tileQueue.Push(newTile);
                    _previousSpawn = spawn;
                }
                else
                {
                    // Place a wall

                    // Remove tile from queue
                    _tileQueue.Pop();
                }
                SpawnNext();
                spawnCount--;
            }
        }

    }
    private void PlaceTile(Tile tile, Quaternion rotation)
    {
        tile.transform.position = (Vector3.right * _currentX + Vector3.forward * _currentY) * _tileScale;
        tile.transform.rotation = rotation;
        _tileArray[_currentX, _currentY] = true;
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        float verticalOffset = 5f;
        float radius = .3f;
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
