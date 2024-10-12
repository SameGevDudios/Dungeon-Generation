using UnityEngine;

public class BorderPlacement : MonoBehaviour
{
    [SerializeField] private GameObject wall;
    public void PlaceBorders(int dungeonSize, int tileScale)
    {
        int position = dungeonSize * tileScale / 2 - 1;
        wall.transform.position = new Vector3(position, 0, position);
        wall.transform.localScale = (Vector3.forward + Vector3.right) * dungeonSize + Vector3.up;
    }
}
