using UnityEngine;

public class Tile : MonoBehaviour
{
    // Every next tile is connected to the previous one by the south side,
    // so this direction should always be unavalable for spawn
    // The spawn direction is counterclockwise starting from the east
    [Tooltip("0 - east, 1 - north, 2 - west")]
    [SerializeField] private bool[] _hasSpawn = new bool[3];
    private Grid _gridPosition;
    [HideInInspector] public int SpawnsAvalable { get; private set; }
    [SerializeField] private int _width, _length;
    private void Awake()
    {
        for (int i = 0; i < 3; i++) 
            if (_hasSpawn[i]) 
                SpawnsAvalable++;
    }
    public void SetGridPosition(Grid gridPosition) =>
        _gridPosition = gridPosition;
    public Grid GetGridPosition() =>
        _gridPosition;
    public int GetWidth() =>
        _width;
    public int GetLength() =>
        _length;
    public int GetNextSpawn()
    {
        int spawnIndex = -1;
        if(SpawnsAvalable > 0)
        {
            spawnIndex = Random.Range(0, 3);
            while (!_hasSpawn[spawnIndex] || spawnIndex == -1) 
                spawnIndex = Random.Range(0, 3);
            SpawnsAvalable--;
            _hasSpawn[spawnIndex] = false;
        }
        return spawnIndex;
    }
    public bool IsLarge() =>
        _width + _length != 2;
}
