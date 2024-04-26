using System.Collections.Generic;
using UnityEngine;
using PathingUtils;
using System;
using GameUtils;
using CreatureUtils;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Linq;

public class Pathfinding {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding Instance {get; private set;}
    private Grid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height, float cellSize) 
    {
        Instance = this;
        grid = new Grid<PathNode>(width, height, cellSize, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    public Grid<PathNode> GetGrid() {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
        grid.GetXY(startWorldPosition , out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path;

        if (IsInsideGrid(endX, endY)){
            path = FindPath(startX, startY, endX, endY);
        } else {
            path = null;
        }
        
        if (path == null) { 
            return null;
        } else {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path) {
                vectorPath.Add(UPathing.XZPlane(pathNode.x, pathNode.y) * grid.GetCellSize() + UPathing.XZPlane(1, 1) * grid.GetCellSize() * 0.5f);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY){
        PathNode startNode = GetNode(startX, startY);
        PathNode endNode = GetNode(endX, endY);

        if (!endNode.isWalkable) return null;

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        // If pathing towards a targetted creature, the endNode is placed inside the creature and the
        // pathfinding manager makes sure that the active creature doesn't path inside it. However,
        // if there is another creature between them the active creature will stop at the edge of this
        // creature instead. So we add any occupied spaces around the target to the closed list.
        List<PathNode> occupiedByAdjacentCreature = new();
        if (endNode.isOccupied){
            if (endNode.GetOccupyingCreature() != UGame.GetActiveCreature()) {

                List<PathNode> surroundingNodes = GetGridObjectsSurroundingCreature(
                    endNode.GetOccupyingCreature(), 
                    UCreature.CreatureWidthAsInt(UGame.GetActiveCreature().GetComponent<CreatureStats>().GetSize()),
                    UGame.GetActiveAttack().GetWeaponRange()
                );
                
                foreach(PathNode node in surroundingNodes) {
                    if (
                        node.isOccupied 
                        && node.GetOccupyingCreature() != UGame.GetActiveCreature()
                        && node.GetOccupyingCreature() != endNode.GetOccupyingCreature()
                    ){
                        occupiedByAdjacentCreature.Add(node);
                    }
                }
            }
        }

        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = GetNode(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode  = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0) {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (
                IsAnyPartOfCreatureAtDestination(currentNode, endNode)
            ) {
                return CalculatePath(currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighbourList(currentNode)) {

                if (!neighborNode.isWalkable){
                    closedList.Add(currentNode);
                    continue;
                }

                if (closedList.Contains(neighborNode)) { 
                    continue;
                }

                if (IsCreatureSpaceInAdjacentCreatureSpace(UGame.GetActiveCreature(), neighborNode, occupiedByAdjacentCreature)){
                    continue;
                }

                if (currentNode == null || neighborNode == null) {
                    closedList.Add(currentNode);
                    continue;
                }

                if (!CanFit(neighborNode, endNode) && !IsAnyPartOfCreatureAtDestination(currentNode, endNode)) { 
                    closedList.Add(currentNode);
                    continue;
                }

                if (CutsCorner(currentNode, neighborNode)) {
                    closedList.Add(currentNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                if (tentativeGCost < neighborNode.gCost) {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode)) {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        // No path found
        Debug.Log("No Path Found");
        return null;
    }
    
    private bool IsCreatureSpaceInAdjacentCreatureSpace(GameObject creature, PathNode currentNode, List<PathNode> adjacentCreatures){
        CreatureSize creatureSize = creature.GetComponent<CreatureStats>().GetSize();
        UPathing.GetSeekRadius(creatureSize, out int seekRadiusStart, out int seekRadiusEnd);
        PathNode centre = currentNode;

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++)
            {
                if (!IsInsideGrid(centre.x + i, centre.y + j))
                {
                    continue;
                }
                if (adjacentCreatures.Contains(GetNode(centre.x + i, centre.y + j))){
                    return true;
                }
            }
        }
        return false;
    }

    private List<PathNode> GetGridObjectsSurroundingCreature(GameObject creature, int searchWidth, int weaponRange){
        List<PathNode> surroundingGridObjects = new();

        UPathing.GetSeekRadius(creature.GetComponent<CreatureStats>().GetSize(), out int seekRadiusStart, out int seekRadiusEnd);
        weaponRange /= 5;

        PathNode centre = grid.GetGridObject(creature.transform.GetChild(0).transform.position);

        for (int i = seekRadiusStart - weaponRange - searchWidth; i <= seekRadiusEnd + weaponRange+ searchWidth; i++){
            for (int j = seekRadiusStart - weaponRange - searchWidth; j <= seekRadiusEnd + weaponRange + searchWidth; j++){
                if (!IsInsideGrid(centre.x + i - searchWidth, centre.y + j - searchWidth)) {
                    continue;
                }
                if (!IsInsideGrid(centre.x + i, centre.y + j)) {
                    continue;
                }
                if (GetNode(centre.x + i, centre.y + j).GetOccupyingCreature() == creature) {
                    continue;
                }

                if (
                    Math.Abs(centre.x + i) < centre.x + searchWidth / 2 + weaponRange 
                    || Math.Abs(centre.y + j) < centre.y + searchWidth / 2 + weaponRange
                    ){
                        continue;
                    }

                surroundingGridObjects.Add(GetNode(centre.x + i, centre.y + j));
            }
        }
        return surroundingGridObjects;
    }

    private bool IsAnyPartOfCreatureAtDestination(PathNode currentNode, PathNode endNode){
        UPathing.GetSeekRadius(
            UGame.GetActiveCreatureSize(),
            out int attackerSeekRadiusStart, 
            out int attackerSeekRadiusEnd
            );

        int weaponRange = 0;
        int targetSeekRadiusStart = 0;
        int targetSeekRadiusEnd = 0;

        if (
            UGame.GetActiveAttack() != null
            && endNode.isOccupied
            && endNode.GetOccupyingCreature() != UGame.GetActiveCreature()
        ){
            UPathing.GetSeekRadius(
                endNode.GetOccupyingCreature().GetComponent<CreatureStats>().GetSize(),
                out targetSeekRadiusStart, 
                out targetSeekRadiusEnd
                );
            weaponRange = UGame.GetActiveAttack().GetWeaponRange() / 5;
        }

        int seekRadiusStart = attackerSeekRadiusStart - targetSeekRadiusStart - weaponRange;
        int seekRadiusEnd = attackerSeekRadiusEnd + targetSeekRadiusEnd + weaponRange;

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                if (GetNode(endNode.x + i, endNode.y + j) == currentNode) {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsOccupiedByPassableCreature(PathNode node, GameObject activeCreature){
        if (!node.isOccupied) {
            return true;
        }

        if (!UPathing.IsOccupiedByEnemy(node, activeCreature)) {
            return true;
        }

        if (Math.Abs(UCreature.CreatureSizesDifference(
            node.GetOccupyingCreatureSize(),
            activeCreature.GetComponent<CreatureStats>().GetSize()
            )) >= 2
        ) {
            return true;
        }

        return false;
    }

    private bool CanFit(PathNode destinationNode, PathNode endNode){

        // Checks if models that take up more than one square can fit in the destination location
        UPathing.GetSeekRadius(UGame.GetActiveCreatureSize(), out int seekRadiusStart, out int seekRadiusEnd);

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                PathNode surroundingNode = GetNode(destinationNode.x + i, destinationNode.y + j);
                if (!IsInsideGrid(destinationNode.x + i, destinationNode.y + j)){
                    return false;
                }
                if (!surroundingNode.isWalkable){
                    return false;
                }
                if (endNode.isOccupied && surroundingNode.isOccupied){
                    if (
                        !IsOccupiedByPassableCreature(surroundingNode, UGame.GetActiveCreature()) 
                        && endNode.GetOccupyingCreature() != surroundingNode.GetOccupyingCreature()
                    ){
                        return false;
                    }
                } else {
                    if (!IsOccupiedByPassableCreature(surroundingNode, UGame.GetActiveCreature())) {
                        return false;
                    }

                }

            }
        }
        return true;
    }

    private bool CutsCorner(PathNode currentNode, PathNode destinationNode){
        // Checks if path would cut through the corner of an unwalkable tile.
        if (currentNode.x == destinationNode.x || currentNode.y == destinationNode.y) return false;

        UPathing.GetSeekRadius(UGame.GetActiveCreatureSize(), out int seekRadiusStart, out int seekRadiusEnd);

        bool horizontalNeighborNodeIsWalkable;
        bool verticalNeighborNodeIsWalkable;

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                if (!IsInsideGrid(destinationNode.x + i, destinationNode.y + j)){
                    continue;
                }
                if (!IsInsideGrid(destinationNode.x + i + 1, destinationNode.y + j + 1)){
                    continue;
                }
                if (!IsInsideGrid(destinationNode.x + i - 1, destinationNode.y + j - 1)){
                    continue;
                }

                //Going down left
                if (currentNode.x + i > destinationNode.x + i && currentNode.y + j > destinationNode.y + j) {
                    // if (destinationNode.x == 0 || destinationNode.y + j == 0) return false;
                    horizontalNeighborNodeIsWalkable = GetNode(destinationNode.x + i + 1, destinationNode.y + j).isWalkable;
                    verticalNeighborNodeIsWalkable = GetNode(destinationNode.x + i, destinationNode.y + j + 1).isWalkable;
                }
                //Going up left
                else if (currentNode.x + i > destinationNode.x + i && currentNode.y + j < destinationNode.y + j) {
                    // if (destinationNode.x + i == 0 || destinationNode.y + j == grid.GetHeight() - 1) return false;
                    horizontalNeighborNodeIsWalkable = GetNode(destinationNode.x + i + 1, destinationNode.y + j).isWalkable;
                    verticalNeighborNodeIsWalkable = GetNode(destinationNode.x + i, destinationNode.y + j - 1).isWalkable;
                }
                //Going up right
                else if (currentNode.x + i < destinationNode.x + i && currentNode.y + j < destinationNode.y + j) {
                    // if (destinationNode.x + i == grid.GetWidth() - 1 || destinationNode.y + j == grid.GetHeight() - 1) return false;
                    horizontalNeighborNodeIsWalkable= GetNode(destinationNode.x + i - 1, destinationNode.y + j).isWalkable;
                    verticalNeighborNodeIsWalkable = GetNode(destinationNode.x + i, destinationNode.y + j - 1).isWalkable;
                }
                //Going down right
                else if (currentNode.x + i < destinationNode.x + i && currentNode.y + j > destinationNode.y + j) {
                    // if (destinationNode.x + i == grid.GetWidth() - 1 || destinationNode.y + j == 0) return false;
                    horizontalNeighborNodeIsWalkable = GetNode(destinationNode.x + i - 1, destinationNode.y + j).isWalkable;
                    verticalNeighborNodeIsWalkable = GetNode(destinationNode.x + i, destinationNode.y + j + 1).isWalkable;
                }
                else {
                    horizontalNeighborNodeIsWalkable = true;
                    verticalNeighborNodeIsWalkable = true;
                }

                if (!horizontalNeighborNodeIsWalkable || !verticalNeighborNodeIsWalkable){
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsInsideGrid(int x, int y){
        if (x < grid.GetWidth() && y < grid.GetHeight() && x >= 0 && y >= 0){
            return true;
        } else {
            return false;
        }
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode) {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.x - 1 >= 0) {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Left Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            // Left Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth()) {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Right Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        // Down
        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Up
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }


    public PathNode GetNode(int x, int y){
        return grid.GetGridObject(x, y);
    }


    private List<PathNode> CalculatePath(PathNode endNode){
        List<PathNode> path = new(){ endNode };
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null){
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        

        /*
        if (
            path.Last().GetOccupyingCreature() != null
            && path.Last().GetOccupyingCreature() != UGame.GetActiveCreature()
        ){
            Debug.Log(path.Last());
            path = EnsurePathEndsOnEdgeOfWeaponRange(path, endNode);
        }
        */
        path = EnsurePathEndIsNotInsideCreature(path);
        return path;
    }

    private List<PathNode> EnsurePathEndsOnEdgeOfWeaponRange(List<PathNode> path, PathNode endNode){
        UPathing.GetSeekRadius(
            endNode.GetOccupyingCreature().GetComponent<CreatureStats>().GetSize()
            , out int targetSeekRadiusStart
            , out int targetSeekRadiusEnd
        );
        UPathing.GetSeekRadius(
            UGame.GetActiveCreatureStats().GetSize()
            , out int attackerSeekRadiusStart
            , out int attackerSeekRadiusEnd
        );

        
        int weaponRange = UGame.GetActiveAttack().GetWeaponRange() / 5 - 1;

        int seekRadiusStart = targetSeekRadiusStart - attackerSeekRadiusStart - weaponRange + 5;
        int seekRadiusEnd = targetSeekRadiusEnd + attackerSeekRadiusEnd + weaponRange + 5;

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                path.Remove(GetNode(endNode.x + i, endNode.y + j));

            }

        }
        return path;
    }

    private List<PathNode> EnsurePathEndIsNotInsideCreature(List<PathNode> path){
        while (
            path != null
            && path.Count != 0
            && IsAnyPartOfCreatureSpaceOccupied(UGame.GetActiveCreature(), path.Last().x, path.Last().y) 
            ){
                path = CutOccupiedSpaceFromPath(path);
            }
        return path;
    }

    private List<PathNode> CutOccupiedSpaceFromPath(List<PathNode> path){
        path.RemoveAt(path.Count - 1);
        if (path.Count == 0){
            path = null;
        }
        return path;
    }

    private bool IsAnyPartOfCreatureSpaceOccupied(GameObject creature, int x, int y){
        UPathing.GetSeekRadius(creature.GetComponent<CreatureStats>().GetSize(), out int seekRadiusStart, out int seekRadiusEnd);
        //int weaponRange = UGame.GetActiveAttack().GetWeaponRange() / 5 - 1;
        int weaponRange = 0;
        for (int i = seekRadiusStart - weaponRange; i <= seekRadiusEnd + weaponRange; i++){
            for (int j = seekRadiusStart - weaponRange; j <= seekRadiusEnd + weaponRange; j++){
                if (!IsInsideGrid(x + i, y + j)){
                    continue;
                }
                if (GetNode(x + i, y + j).isOccupied ){
                    if(GetNode(x + i, y + j).GetOccupyingCreature() != creature){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList){ 
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++){
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;

    }

}
