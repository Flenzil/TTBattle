
using System.Collections.Generic;
using UnityEngine;
using PathingUtils;
using Unity.VisualScripting;
using System.Linq;
using System;
using GameUtils;
using UnityEngine.EventSystems;
using Mono.Cecil;
using CreatureUtils;

public class PathFindingVisual{

    // Handles the visual elements of the pathfinding, mainly highlighting the tiles
    // to visual the path and the GameObjects that make up the grid itself.

    private Grid<PathNode> grid;
    private GameObject floorTile;
    private GameObject wallTile;
    private GameObject[] floor;
    public List<PathNode> highlightedPath = new();
    private Color startingColour;
    private List<PathNode> path;


    public void SetPath(List<PathNode> path){
        this.path = path;
    }

    public void SetGrid(Grid<PathNode> grid) {
        this.grid = grid;
        floor = new GameObject[grid.GetHeight() * grid.GetWidth()];
        ScaleTileToGrid(floorTile);
        ScaleTileToGrid(wallTile);
        CreateFloor();

    }

    public void SetFloorTile(GameObject floorTile){
        this.floorTile = floorTile;
    }

    public void SetWallTile(GameObject wallTile){
        this.wallTile = wallTile;
    }

    public void TileHighlighting(){

        // Controls when tiles should be highlighted an unhighlighted

        if (highlightedPath.Count() > 0) {
            UnHighlightPath();
        }

        // Unhighlight if mouse is hovering over UI element
        if (EventSystem.current.IsPointerOverGameObject()){
            UnHighlightPath();
            return;
         }

        if (path == null) {
            return;
        }

        HighLightPath(path);
    }

    private void HighlightTile(int x, int y, Color colour){

        // Highlight a single tile at grid position (x, y)

        if (IsInsideGrid(x, y) ){
            if (startingColour == default) {
                startingColour = floor[y + grid.GetHeight() * x].GetComponent<Renderer>().material.color;
            }
            floor[y + grid.GetHeight() * x].GetComponent<Renderer>().material.color = colour;
        }
    }

    private void UnHighlightTile(int x, int y){

        // Returns tile to original colour 

        if (IsInsideGrid(x, y) ){
            floor[y + grid.GetHeight() * x].GetComponent<Renderer>().material.color = startingColour;
        }
    }

    private void HighLightPath(List<PathNode> path){

        // Highlights all PathNodes in path and highlights surrounding tiles for creatures that are larger
        // than 1 tile wide.
        int remainingMovement = UGame.GetActiveCreatureStats().GetRemainingMovement() / 5;
        Debug.Log(remainingMovement);

        for (int i = 1; i < path.Count(); i++){
            PathNode node = path[i];

            grid.GetXY(
                UGame.GetActiveCreature().transform.position,
                out int destinationX,
                out int destinationY
            );

            Color colour;

            UPathing.GetSeekRadius(UGame.GetActiveCreatureSize(), out int seekRadiusStart, out int seekRadiusEnd);
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                for (int k = seekRadiusStart; k <= seekRadiusEnd; k++) {
                    if (
                        !IsInsideGrid(node.x + j, node.y + k) 
                        || grid.GetGridObject(node.x + j, node.y + k).GetOccupyingCreature() == UGame.GetActiveCreature()
                    ) {
                        continue;
                    }
                    Debug.Log(remainingMovement);

                    grid.GetXY(UGame.GetActiveCreature().transform.GetChild(0).position, out int x, out int y);
                    // Colour tiles beyond the creature's walking speed red.
                    int offset = 0;
                    if ( node.x < x || node.y < y) {
                        offset = -seekRadiusStart;
                    } else if (node.x > x || node.y > y) {
                        offset = seekRadiusEnd;
                    }
                    if ( remainingMovement + offset <= 0){
                        colour = Color.red;
                    } 
                    else {
                        colour = Color.yellow;
                    }

                    if (!highlightedPath.Contains(grid.GetGridObject(node.x + j, node.y + k))){
                        HighlightTile(node.x + j, node.y + k, colour);
                    }
                    highlightedPath.Add(grid.GetGridObject(node.x + j, node.y + k));
                }
            }
            remainingMovement--;
            if (node.isDifficultTerrain){
                remainingMovement--;
            }
        }
    }


    private void UnHighlightPath(){

        // Unhighlight all previously highlighted tiles.

        for (int i = highlightedPath.Count() - 1; i >= 0; i--){
            UnHighlightTile(highlightedPath[i].x, highlightedPath[i].y);
            highlightedPath.RemoveAt(i);
        }
    }

    public void UnHighlightPath(List<PathNode> pathToUnHighlight){

        // Unhighlight given pathNodes

        for (int i = pathToUnHighlight.Count() - 1; i >= 0; i--){
            UnHighlightTile(pathToUnHighlight[i].x, pathToUnHighlight[i].y);
            pathToUnHighlight.RemoveAt(i);
        }
    }

    private int MovementNeededToTraversePath(List<PathNode> path){

        // Some tiles can be difficult terrain and thus cost twice as much
        // movement to path through. This returns the amount of "effective"
        // tiles are needed to path.

        int movementNeeded = 0;
        foreach (PathNode node in path){
            movementNeeded++;
            if (node.isDifficultTerrain){
                movementNeeded++;
            }
        }
        return movementNeeded;
    }

    private bool IsInsideGrid(int x, int y){
        return (
            x < grid.GetWidth() 
            && y < grid.GetHeight() 
            && x >= 0 
            && y >= 0
        );
    }

     private void ScaleTileToGrid(GameObject tile){

        // Scales the floor and wall tiles so that they fit the grid, regardless of cellSize

        Vector3 tileSize = tile.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        tile.transform.localScale = Vector3.one * grid.GetCellSize() / tileSize.x; // Assumes floor tile is square!
     }

    public void UpdateFloorTile(int x, int y) { 

        // Swaps floor tile with wall tile depending on whether that PathNode is walkable or not

        UnityEngine.Object.Destroy(floor[y + grid.GetHeight() * x]);
        PathNode pathNode = grid.GetGridObject(x, y);

        if (pathNode.isWalkable){
            floor[y + grid.GetHeight() * x] = UnityEngine.Object.Instantiate(floorTile,
            grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
            Quaternion.identity);

            floor[y + grid.GetHeight() * x].layer = LayerMask.NameToLayer("Ground");
        } else {
            floor[y + grid.GetHeight() * x] = UnityEngine.Object.Instantiate(wallTile,
            grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
            Quaternion.identity);
        }
    }

    private void CreateFloor() {

        // Instantiate floor tiles for each PathNode in grid when game first launches

        for (int x = 0; x < grid.GetWidth(); x++){
            for (int y = 0; y < grid.GetHeight(); y++){
                PathNode pathNode = grid.GetGridObject(x, y);

                if (pathNode.isWalkable){
                    floor[y + grid.GetHeight() * x] = UnityEngine.Object.Instantiate(floorTile,
                    grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
                    Quaternion.identity);

                    floor[y + grid.GetHeight() * x].layer = LayerMask.NameToLayer("Ground");
                } else {
                    floor[y + grid.GetHeight() * x] = UnityEngine.Object.Instantiate(wallTile,
                    grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
                    Quaternion.identity);
                }
            }
        }
    }
}
