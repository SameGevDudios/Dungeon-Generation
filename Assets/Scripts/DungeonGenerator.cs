using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonSize, _tileScale;
    private int previousSpawn = 1;
    private bool[,] _tileArray;
    [SerializeField] private GameObject[] _tile;
    [SerializeField] private GameObject _endingTile, _wall;
    private Stack<Tile> _tileQueue = new Stack<Tile>();

    int spawnCount = 30, _currentX = 1, _currentY = 1;
    [Header("Grid Visualization")]
    [SerializeField] private GameObject[] _row0, _row1, _row2;
    private void Start()
    {
        InitArray();
        SpawnCenter();
        ActivateRowVisualizaiton();
    }
    private void InitArray()
    {
        _tileArray = new bool[_dungeonSize, _dungeonSize];
    }
    private void SpawnCenter()
    {
        Tile centerTile = Instantiate(_tile[0]).GetComponent<Tile>();
        PlaceTile(centerTile, _dungeonSize / 2, _dungeonSize / 2, Quaternion.identity);
        _tileQueue.Push(centerTile);
        ActivateRowVisualizaiton();
        SpawnNext();
    }
    private void SpawnNext()
    {
        if(spawnCount > 0)
        {
            print($"Tile queue count: {_tileQueue.Count}");
            if (_tileQueue.Count > 0)
            {
                Tile previousTile = _tileQueue.Peek();
                int spawn = previousTile.GetNextSpawn();
                // calculate grid positon
                float f = Mathf.PI * (spawn + previousSpawn - 1) / 2;
                _currentX += (int)Mathf.Cos(f);
                _currentY += (int)Mathf.Sin(f);
                bool inRange = _currentX > -1 && _currentX < _dungeonSize && _currentY > -1 && _currentY < _dungeonSize;
                print($"Function: {f}");
                print($"Current spawn: {spawn}");
                print($"previous spawn: {spawn}");
                print($"x: {_currentX}, y: {_currentY}");
                print($"In range: {inRange}");
                if (spawn >= 0 && inRange)
                {
                    // calculate rotation of a new tile
                    Vector3 spawnEuler = Vector3.up * (spawn - 1) * -90 + previousTile.transform.localEulerAngles;
                    // randomly choose a new tile
                    int index = Random.Range(0, _tile.Length);
                    Tile newTile = Instantiate(_tile[index]).GetComponent<Tile>();
                    PlaceTile(newTile, _currentX, _currentY, Quaternion.Euler(spawnEuler));
                    _tileQueue.Push(newTile);
                    ActivateRowVisualizaiton();
                }
                else
                {
                    // Place a wall

                    // Remove tile from queue
                    _tileQueue.Pop();
                }
                SpawnNext();
                previousSpawn = spawn;
                spawnCount--;
            }
        }

    }
    private void PlaceTile(Tile tile, int x, int y, Quaternion rotation)
    {
        tile.transform.position = (Vector3.right * x + Vector3.forward * y) * _tileScale;
        tile.transform.rotation = rotation;
        _tileArray[x, y] = true;
    }
    private void PlaceTile(Tile tile, Transform spawnTransform, int x, int y)
    {
        tile.transform.position = spawnTransform.position;
        tile.transform.rotation = spawnTransform.rotation;
        _tileArray[x, y] = tile;
    }
    private void ActivateRowVisualizaiton()
    {
        for (int i = 0; i < _row0.Length; i++) _row0[i].SetActive(_tileArray[0, i]);
        for (int i = 0; i < _row1.Length; i++) _row1[i].SetActive(_tileArray[1, i]);
        for (int i = 0; i < _row2.Length; i++) _row2[i].SetActive(_tileArray[2, i]);
    }
}
