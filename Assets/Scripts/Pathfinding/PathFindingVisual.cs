
using System.Collections.Generic;
using UnityEngine;
using PathingUtils;
using Unity.VisualScripting;
using System.Linq;
using System;
using GameUtils;
using UnityEngine.EventSystems;

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

        for (int i = 1; i < path.Count(); i++){
            PathNode node = path[i];
            int speed = UGame.GetActiveCreature().GetComponent<CreatureStats>().GetMovementSpeed();
            grid.GetXY(UGame.GetActiveCreature().transform.position, out int ObjectX, out int ObjectY);

            Color colour;

            UPathing.GetSeekRadius(UGame.GetActiveCreatureSize(), out int pathStart, out int pathEnd);
            for (int j = pathStart; j <= pathEnd; j++){
                for (int k = pathStart; k <= pathEnd; k++) {
                    if (
                        !IsInsideGrid(node.x + j, node.y + k) 
                        || (node.isOccupied && node.GetOccupyingCreature() != UGame.GetActiveCreature())
                    ) {
                        continue;
                    }

                    // Colour tiles beyond the creature's walking speed red.
                    if (
                        Math.Abs(ObjectX - (node.x + j)) + pathStart  > speed / 5.0f 
                        || Math.Abs(ObjectY - (node.y + k)) + pathStart > speed / 5.0f
                    ){
                        colour = Color.red;
                    } 
                    else {
                        colour = Color.yellow;
                    }

                    HighlightTile(node.x + j, node.y + k, colour);
                    highlightedPath.Add(grid.GetGridObject(node.x + j, node.y + k));
                }
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
