using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour{
    private Grid<PathNode> grid;
    [SerializeField] Object floorTile;

    public void Start() {
        for (int x = 0; x < grid.GetWidth(); x++){
            for (int y = 0; y < grid.GetHeight(); y++){
                Instantiate(floorTile, new Vector3(x, 0, y) * grid.GetCellSize(), Quaternion.identity);
            }
        }
    }

}
