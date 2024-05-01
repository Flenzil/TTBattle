using System.Collections;
using System.Collections.Generic;
using CreatureUtils;
using UnityEngine;
using UnityEngine.TextCore;

public class PathNode
{
    private Grid<PathNode> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public bool isOccupied = false;
    public bool isDifficultTerrain = false;
    private GameObject occupyingCreature = null;

    public PathNode cameFromNode;

    public PathNode(Grid<PathNode> grid, int x, int y) {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public void CalculateFCost() {
        fCost = gCost + hCost;
    }

    public void SetIsWalkable(bool isWalkable) {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    public bool GetIsWalkable(){
        return isWalkable;
    }

    public void SetOccupyingCreature(GameObject creature){
        occupyingCreature = creature;
        isOccupied = true;
        isDifficultTerrain = true;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void ClearOccupyingCreature(){
        occupyingCreature = null;
        isOccupied = false;
        isDifficultTerrain = false;
        grid.TriggerGridObjectChanged(x, y);
    }

    public GameObject GetOccupyingCreature(){
        return occupyingCreature;
    }

    public CreatureSize GetOccupyingCreatureSize(){
        return occupyingCreature.GetComponent<CreatureStats>().GetSize();
    }

    public override string ToString()
    {
        return x + ", " + y;
    }
}
