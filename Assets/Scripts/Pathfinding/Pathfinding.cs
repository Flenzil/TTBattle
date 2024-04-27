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

    // Implements a* pathfinding while also making sure creatures can/can't pass through
    // eachother's spaces in the appropriate situations, creatures do/don't cut corners
    // through occupied spaces and move only as much as they need to in order to get their 
    // target within range of their attack. 

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding Instance {get; private set;}
    private Grid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height, float cellSize) 
    {
        Instance = this;
        grid = new Grid<PathNode>(
            width, // Grid width
            height, // Grid height
            cellSize, // Grid cell size
            Vector3.zero, // Grid origin point
            (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y)); // Instantiating grid object
    }

    public Grid<PathNode> GetGrid() {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {

        // Finds path from startWorldPosition to endWorldPosition using a* pathfinding. 
        // Path length equates to how many grid spaces the path takes. Essentially calls the 
        // overload FindPath which returns a list of PathNodes and converts to 
        // Vector3.

        grid.GetXY(startWorldPosition , out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path;

        // Don't path outside grid
        if (IsInsideGrid(endX, endY)){
            path = FindPath(startX, startY, endX, endY);
        } else {
             return null;
        }
        
        List<Vector3> vectorPath = new List<Vector3>();
        foreach (PathNode pathNode in path) {
            vectorPath.Add(
                UPathing.XZPlane(pathNode.x, pathNode.y) 
                * grid.GetCellSize() // Convert grid position to Vector3
                + 0.5f * grid.GetCellSize() // Offset to middle of node
                * UPathing.XZPlane(1, 1) // But not in the y direction
            );
        }
        return vectorPath;
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY){

        // Finds path from grid position (startX, startY) to (endX, endY) as a 
        // list of PathNodes using a* pathfinding.

        PathNode startNode = GetNode(startX, startY);
        PathNode endNode = GetNode(endX, endY);

        // Can't path to unwalkable space
        if (!endNode.isWalkable) {
            return null;
        }

        openList = new List<PathNode> { startNode }; // List of nodes to consider for pathing
        closedList = new List<PathNode>(); // List of nodes out of consideration

        // If pathing towards a targetted creature, the endNode is placed inside the creature and the
        // pathfinding makes sure that the active creature doesn't path inside it. However,
        // if there is another creature between them, the active creature will stop at the edge of this
        // creature instead. So premtively remove those occupied from consideration.

        List<PathNode> occupiedByAdjacentCreature = new();
        if (endNode.isOccupied){
            if (endNode.GetOccupyingCreature() != UGame.GetActiveCreature()) {

                // When attacking, creatures only path as much as they need to in order to get into range
                // of their attack, so the search radius needs to include the attack range too.
                int attackRange = 0;
                if (UGame.GetActiveAttack() != null) {
                    attackRange = UGame.GetActiveAttack().GetWeaponRange();
                }

                List<PathNode> surroundingNodes = GetGridObjectsSurroundingCreature(
                    endNode.GetOccupyingCreature(), 
                    UCreature.CreatureWidthAsInt(UGame.GetActiveCreature().GetComponent<CreatureStats>().GetSize()),
                    attackRange
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

        // Initialise the grid for a* pafinding
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

            // Exit point for while loop
            if (
                IsAnyPartOfCreatureAtDestination(currentNode, endNode)
            ) {
                return CalculatePath(currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighbourList(currentNode)) {

                if (closedList.Contains(neighborNode)) { 
                    continue;
                }

                if (!neighborNode.isWalkable){
                    closedList.Add(currentNode);
                    continue;
                }


                if (IsCreatureSpaceInAdjacentCreatureSpace(UGame.GetActiveCreature(), neighborNode, occupiedByAdjacentCreature)){
                    closedList.Add(currentNode);
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

                int neighborGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                if (neighborGCost < neighborNode.gCost) {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = neighborGCost;
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

    private int CalculateDistanceCost(PathNode a, PathNode b) {

        // Calulcates the hCost from a to b for use in the a* pathfinding.

        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private List<PathNode> CalculatePath(PathNode endNode){
        // Loops from endNode backwards through the cameFromNode paramter to build the 
        // correct path.
        List<PathNode> path = new(){ endNode };
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null){
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        
        path = EnsurePathEndIsNotInsideCreature(path);

        return path;
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
    
    private bool IsCreatureSpaceInAdjacentCreatureSpace(GameObject creature, PathNode currentNode, List<PathNode> adjacentCreatures){

        // Checks if any part of the active creature is occupying the same space as one
        // of the creatures adjacent to the target, if attacking.

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

        // Returns the nodes surround the creature in a radius determined by the active creature's size and the
        // active creature's attack range. This is used to remove occupied spaces from considering in the pathfinding.

        List<PathNode> surroundingGridObjects = new();

        // SeekRadius is determined by the creature's size so that the creature's entire space is explored.
        UPathing.GetSeekRadius(
            creature.GetComponent<CreatureStats>().GetSize(),
            out int seekRadiusStart,
            out int seekRadiusEnd
        );

        // The centre of a creature is determined by an anchor GameObject which is parented to the creature.
        PathNode centre = grid.GetGridObject(creature.transform.GetChild(0).transform.position);

        seekRadiusStart = seekRadiusStart - weaponRange / 5 - searchWidth;
        seekRadiusEnd = seekRadiusEnd + weaponRange / 5 + searchWidth;

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                if (!IsInsideGrid(centre.x + i, centre.y + j)) {
                    continue;
                }

                if (GetNode(centre.x + i, centre.y + j).GetOccupyingCreature() == creature) {
                    continue;
                }

                // Only check in a ring around the target creature, the inner radius of which is
                // determined by the width of the attacking creature and it's weapon range. Any
                // nodes within that radius aren't going to be pathed to anyway since attacking
                // creatures stop pathing once their are within attacking range.
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
        
        // Checks if any node occupied by the active creature is at the endNode. Without this,
        // a player in control of a 3x3 creature, for example, wouldn't be able to click to path to
        // the edge of the map, since the centre of the creature can't make it there. If the active
        // creature is attacking, this also takes into account the attack range of the active creature.

        UPathing.GetSeekRadius(
            UGame.GetActiveCreatureSize(),
            out int attackerSeekRadiusStart, 
            out int attackerSeekRadiusEnd
            );

        int weaponRange = 0;
        int targetSeekRadiusStart = 0;
        int targetSeekRadiusEnd = 0;

        // Only give non-zero value to the above variables if the active creature is attacking
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

        // Creatures can pass through allies
        // Creatures cannot pass through enemies
        // Creatures can pass through enemies as long as they are more than two size catagories 
        // apart e.g huge vs medium or large vs small.

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

        // Checks if creatures that take up more than one square can fit in the destination location
        // either because of unwalkable tiles or tiles occupied by enemies.

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

        // Creatures can't cut the corner through an unwalkable tile but they can cut the corner through
        // another creature. For each possible diagonal that a creature can move, we only need to check
        // the adjacent two tiles for walkability.. 

        if (currentNode.x == destinationNode.x || currentNode.y == destinationNode.y) {
            return false;
        } 

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
                //Not moving diagonally
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

        // Avoids the null reference exceptions of chosing an x and y positin that doesn't exist.

        return (
            x < grid.GetWidth() 
            && y < grid.GetHeight() 
            && x >= 0 
            && y >= 0
        );
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode) {

        // Returns list of all nodes surrounding the currenNode for use in the pathfinding.

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

    private List<PathNode> EnsurePathEndIsNotInsideCreature(List<PathNode> path){

        // While creatures can pass through other creatures in certain circumstances, they can never end
        // their movement in another creature's space. Starting at the end of the path, this checks if the
        // node is occupied and if it is, remove that node from the path. Repeat until the end point is no
        // longer occupied or until the path list is empty.

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

        // Checks if any part of the creature's space is occupied by another creature

        UPathing.GetSeekRadius(creature.GetComponent<CreatureStats>().GetSize(), out int seekRadiusStart, out int seekRadiusEnd);
        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
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



}
