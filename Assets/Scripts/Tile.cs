using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Every next tile is connected to the previous one by the south side,
    // so this direction should always be unavalable for spawn
    // The spawn direction is counterclockwise starting from the east
    [Tooltip("0 - east, 1 - north, 2 - west")]
    [SerializeField] private bool[] _hasSpawn = new bool[3];
    private int _spawnsAvalable;
    private void Awake()
    {
        for (int i = 0; i < 3; i++) if (_hasSpawn[i]) _spawnsAvalable++;
    }
    public int GetNextSpawn()
    {
        if(_spawnsAvalable > 0)
        {
            int spawnIndex = Random.Range(0, 3);
            while (!_hasSpawn[spawnIndex]) spawnIndex = Random.Range(0, 3);
            _spawnsAvalable--;
            _hasSpawn[spawnIndex] = false;
            return spawnIndex;
        }
        else
        {
            return -1;
        }
    }
}
