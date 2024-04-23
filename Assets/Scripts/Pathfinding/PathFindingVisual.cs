
using System.Collections.Generic;
using UnityEngine;
using PathingUtils;
using Unity.VisualScripting;
using System.Linq;
using System;
using GameUtils;
using UnityEngine.EventSystems;

public class PathFindingVisual : MonoBehaviour {

    private Grid<PathNode> grid;
    private Mesh mesh;
    private bool updateMesh;
    private GameObject floorTile;
    private GameObject wallTile;
    private GameObject[] floor;
    private int floorTileToUpdateX;
    private int floorTileToUpdateY;
    public List<PathNode> highlightedPath = new List<PathNode>();
    private Color startingColour;
    private Vector3 lastMousePosition;
    private Vector3 lastObjectPosition;
    private List<PathNode> path;


    public void SetPath(List<PathNode> path){
        this.path = path;
    }

    private void Awake() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        lastMousePosition = UGame.GetMousePosition3D(Camera.main);
        lastObjectPosition = Vector3.one * float.MaxValue;
    }

    public void SetGrid(Grid<PathNode> grid) {
        this.grid = grid;
        floor = new GameObject[grid.GetHeight() * grid.GetWidth()];
        ScaleTileToGrid(floorTile);
        ScaleTileToGrid(wallTile);
        CreateFloor();

        grid.OnGridValueChanged += Grid_OnGridValueChanged;
    }

    private void Grid_OnGridValueChanged(object sender, Grid<PathNode>.OnGridValueChangedEventArgs e){
        // UpdatePathfindingVisual(); 
        floorTileToUpdateX = e.x;
        floorTileToUpdateY = e.y;
        updateMesh = true;
    }

    private void LateUpdate() {
        if (updateMesh ) {
            updateMesh = false;
            UpdateFloorTile(floorTileToUpdateX, floorTileToUpdateY);
        }
        if (GameManager.Instance.activePlayer != null) TileHighlighting();
    }
    
    private Vector3 GetPosition(GameObject player){
        if (player.transform.childCount == 0) return player.transform.position;
        else return player.transform.GetChild(0).transform.position;
    }

    private bool IsActiveCreature(GameObject creature) {
        if (creature == UGame.GetActiveCreature()) return true;
        return false;
    }

    /*
    private void TileHighlighting(){
        if (!IsMouseOverNewGridNode() && !ObjectHasMoved()) return;

        if (highlightedPath.Count() > 0) UnHighlightPath();

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

        if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures") && !IsActiveCreature(hit.transform.GameObject())) return;

        grid.GetXY( GetPosition(UGame.GetActiveCreature()), out int currentX, out int currentY);

        int endX, endY;
        if (IsActiveCreature(hit.transform.GameObject())) {
            grid.GetXY(UGame.GetMousePosition3D(Camera.main, "Ground"), out endX, out endY);
        } else {
            grid.GetXY(hit.point, out endX, out endY);
        }

        if (!IsInsideGrid(endX, endY)) return;

        List<PathNode> path = PathFindingManager.;

        if (path == null) return;

        HighLightPath(path);
    }
    */

    public void TileHighlighting(){
        if (highlightedPath.Count() > 0) {
            UnHighlightPath();
        }
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
        if (IsInsideGrid(x, y) ){
            if (startingColour == default(Color)) {
                startingColour = floor[y + grid.GetHeight() * x].GetComponent<Renderer>().material.color;
            }
            floor[y + grid.GetHeight() * x].GetComponent<Renderer>().material.color = colour;
        }
    }

    private void UnHighlightTile(int x, int y){
        if (IsInsideGrid(x, y) ){
            floor[y + grid.GetHeight() * x].GetComponent<Renderer>().material.color = startingColour;
        }
    }

    private void HighLightPath(List<PathNode> path){
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

                    if (Math.Abs(ObjectX - (node.x + j)) + pathStart  > speed / 5.0f || Math.Abs(ObjectY - (node.y + k)) + pathStart > speed / 5.0f) colour = Color.red;
                    else colour = Color.yellow;

                    HighlightTile(node.x + j, node.y + k, colour);
                    highlightedPath.Add(grid.GetGridObject(node.x + j, node.y + k));
                }
            }
        }
    }


    private void UnHighlightPath(){
        for (int i = highlightedPath.Count() - 1; i >= 0; i--){
            UnHighlightTile(highlightedPath[i].x, highlightedPath[i].y);
            highlightedPath.RemoveAt(i);
        }
    }

    public void UnHighlightPath(List<PathNode> pathToUnHighlight){
        for (int i = pathToUnHighlight.Count() - 1; i >= 0; i--){
            UnHighlightTile(pathToUnHighlight[i].x, pathToUnHighlight[i].y);
            pathToUnHighlight.RemoveAt(i);
        }
    }

    private bool IsMouseOverNewGridNode(){
        if (grid.GetGridObject(UGame.GetMousePosition3D(Camera.main)) != grid.GetGridObject(lastMousePosition)){
            lastMousePosition = UGame.GetMousePosition3D(Camera.main);
            return true;
        } else return false;
    }

    private bool ObjectHasMoved(){
        if (UGame.GetActiveCreature().transform.position != lastObjectPosition){
            lastObjectPosition = UGame.GetActiveCreature().transform.position;
            return true;
        } else return false;
    }

    private bool IsInsideGrid(int x, int y){
        if (x < grid.GetWidth() && y < grid.GetHeight() && x >= 0 && y >= 0){
            return true;
        } else {
            return false;
        }
    }

    private bool IsOnGridEdge(int x, int y){
        if (x == 0 || x == grid.GetWidth() - 1 || y == 0 || y == grid.GetHeight() - 1){
            return true;
        } else {
            return false;
        }
    }

    public void SetFloorTile(GameObject floorTile){
        this.floorTile = floorTile;
    }

    public void SetWallTile(GameObject wallTile){
        this.wallTile = wallTile;
    }

     private void ScaleTileToGrid(GameObject tile){
        Vector3 tileSize = tile.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        tile.transform.localScale = Vector3.one * grid.GetCellSize() / tileSize.x; // Assumes floor tile is square!
     }

    private void UpdateFloorTile(int x, int y) { 
        Destroy(floor[y + grid.GetHeight() * x]);
        PathNode pathNode = grid.GetGridObject(x, y);

        if (pathNode.isWalkable){
            floor[y + grid.GetHeight() * x] = Instantiate(floorTile,
            grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
            Quaternion.identity);

            floor[y + grid.GetHeight() * x].layer = LayerMask.NameToLayer("Ground");
        } else {
            floor[y + grid.GetHeight() * x] = Instantiate(wallTile,
            grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
            Quaternion.identity);
        }
    }

    private void CreateFloor() {
        for (int x = 0; x < grid.GetWidth(); x++){
            for (int y = 0; y < grid.GetHeight(); y++){
                PathNode pathNode = grid.GetGridObject(x, y);

                if (pathNode.isWalkable){
                    floor[y + grid.GetHeight() * x] = Instantiate(floorTile,
                    grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
                    Quaternion.identity);

                    floor[y + grid.GetHeight() * x].layer = LayerMask.NameToLayer("Ground");
                } else {
                    floor[y + grid.GetHeight() * x] = Instantiate(wallTile,
                    grid.GetCellSize() * (UPathing.XZPlane(x,y) + UPathing.XZPlane(1,1) * 0.5f),
                    Quaternion.identity);
                }
            }
        }
    }

    /*
    private void UpdatePathfindingVisual() {
        MeshUtil.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < grid.GetWidth(); x++){
            for (int y = 0; y < grid.GetHeight(); y++){

                int index = x * grid.GetHeight() + y;
                Vector3 quadSize = new Vector3(1, 0, 1) * grid.GetCellSize();
                float yOffset = 0.01f;

                PathNode pathNode = grid.GetGridObject(x, y);

                if (pathNode.isWalkable) {
                    quadSize = Vector3.zero;
                }

                MeshUtil.AddToMeshArrays(vertices, 
                uv, 
                triangles, 
                index, 
                grid.GetWorldPosition(x, y) + quadSize * 0.5f, 
                0f, //Rotation
                quadSize,
                Vector2.zero, // UV00
                Vector2.zero, // U11
                yOffset); // Amount that mesh is raised in y direcion i.e above the grid
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
    */
}
