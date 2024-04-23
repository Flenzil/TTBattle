using System.Collections;
using System.Collections.Generic;
using CreatureUtils;
using UnityEngine;

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
    //public string occupiedBy = "none";
    //public string occupiedBySize = "none";
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

    /*
    public void SetOccupiedState(string occupiedBy){
        this.occupiedBy = occupiedBy;
        grid.TriggerGridObjectChanged(x, y);
    }

    public string GetOccupiedState(){
        return occupiedBy;
    }

    public void SetOccupiedSize(string creatureSize){
        this.occupiedBySize = creatureSize;
        grid.TriggerGridObjectChanged(x, y);
    }

    public string GetOccupiedSize(){
        return occupiedBySize;
    }
    */

    public void SetOccupyingCreature(GameObject creature){
        this.occupyingCreature = creature;
        isOccupied = true;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void ClearOccupyingCreature(){
        this.occupyingCreature = null;
        isOccupied = false;
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
        return GetOccupyingCreature().name;
    }
}
