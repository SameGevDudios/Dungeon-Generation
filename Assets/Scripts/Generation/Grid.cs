using UnityEngine;

public struct Grid
{
    public int x, z;
    public Grid(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public Vector3 ToVector3() => new Vector3(this.x, 0, this.z);
    public Vector3 ToVector3(Grid grid) => new Vector3(grid.x, 0, grid.z);
}
