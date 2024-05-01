using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PathingUtils;
using Unity.VisualScripting;
using System.Linq;
using GameUtils;
using System.Collections;
using UnityEngine.Analytics;
using Unity.Collections;

public class PathFindingManager : MonoBehaviour {

    // Manages the pathfinding calculation, creature movement and tile highlighting

    private const float moveSpeed = 4f;

    private int currentPathIndex;
    private List<Vector3> pathVectorList;
    private Vector3 lastMousePosition;
    private Vector3 lastObjectPosition;
    private static PathFindingManager instance;
    private PathFindingVisual pathFindingVisual;
    public int remainingMovement;
    public List<PathNode> path; 

    public static PathFindingManager Instance {
        get {
            if (instance == null)
                Debug.Log("Pathfinding Manager is null");
            return instance;
        }
    }

    private void Start(){
        pathFindingVisual = GameManager.Instance.pathFindingVisual;
        instance = this;
    }

    private void Update() {


        // Remove highlighted tiles if mouse is hovering over UI element
        if (EventSystem.current.IsPointerOverGameObject()){
            pathFindingVisual.UnHighlightPath(pathFindingVisual.highlightedPath);
            HandleMovement();
            return;
        }


        HighLightPath();
        HandleMovement();

        if (Input.GetMouseButtonDown(0)) {

            if (UGame.GetActiveCreatureStats().GetRemainingMovement() == 0){
                Debug.Log("Out of movement!");
                return;
            }

            // Find object under mouse position
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
            GameObject hitObject = hit.transform.GameObject();

            // If the object is the active player, pathfind to ground behind it, as if the object wasn't there.
            if (hitObject.layer == LayerMask.NameToLayer("Creatures") && IsActiveCreature(hitObject)){
                SetTargetPosition(UGame.GetMousePosition3D(Camera.main, "Ground"), false);
            }

            // If the object is the ground, pathfind to that point
            if (hitObject.layer == LayerMask.NameToLayer("Ground")){
                SetTargetPosition(hit.point, false);
            }
        }

        if (Input.GetMouseButtonDown(4)){
            UGame.GetActiveCreatureStats().SetReaminingMovement(UGame.GetActiveCreatureStats().GetMovementSpeed());
        }

        if (Input.GetMouseButtonDown(3)){
            Vector3 mousePosition = UGame.GetMousePosition3D(Camera.main, "Ground");

            PathNode node = GetGrid().GetGridObject(mousePosition);
            node.isWalkable = !node.isWalkable;
            pathFindingVisual.UpdateFloorTile(node.x, node.y);
        }
    }

    public void SetTargetPosition(Vector3 targetPosition, bool isAttacking) {

        // Sets the target position for the pathfinding. Also sets the pathNodes
        // at the creature's current position to unoccupied, sets the pathNodes
        // at the target position to occupied and updates the highlighting.

        currentPathIndex = 0;

        SetPath(GetPosition(), targetPosition, isAttacking);

        if (path != null && path.Count > 1) {

            path.RemoveAt(0);

            if (path != null) {
                ClearCreatureSpace(UGame.GetActiveCreature());
                //SetSpaceToOccupied(path.Last(), UGame.GetActiveCreature());
                pathFindingVisual.SetPath(path);
                pathFindingVisual.TileHighlighting();
                pathVectorList = PathNodeListToVector3List(path);
                }
        }
    }

    private void HandleMovement() {

        // Move the active creature towards the next pathNode in the path.
        // When sufficiently close, move on to the next pathNode until the
        // creature is at the end of the path or has run out of movement.

        if (pathVectorList != null) {
            Vector3 targetPosition = pathVectorList[currentPathIndex];
            if (Vector3.Distance(GetPosition(), targetPosition) > 0.05f) {
                Vector3 moveDirection = (targetPosition - GetPosition()).normalized;

                UGame.GetActiveCreature().transform.position 
                    += moveSpeed * Time.deltaTime * moveDirection;

            } else {
                
                GetGrid().GetXY(pathVectorList[currentPathIndex], out int x, out int y);
                UGame.GetActiveCreatureStats().DecreaseRemainingMovement(5);
                if (IsDifficultTerrain(x, y)){
                    UGame.GetActiveCreatureStats().DecreaseRemainingMovement(5);
                }

                currentPathIndex++;
                
                if (
                    currentPathIndex >= pathVectorList.Count
                    || UGame.GetActiveCreatureStats().GetRemainingMovement() == 0
                    ) {
                    StopMoving();
                }
            }
        } 
    }

    private bool IsDifficultTerrain(int x, int y){
        return GetGrid().GetGridObject(x, y).isDifficultTerrain;
    }

    private void StopMoving() {
        GetGrid().GetXY(
            pathVectorList[currentPathIndex - 1],
            out int x,
            out int y
        );
        
        UPathing.SetCreatureSpaceToOccupied(
            UGame.GetActiveCreature(),
            GetGrid(),
            x,
            y
        );

        currentPathIndex = 0;

        pathVectorList = null;
    }

    public static Grid<PathNode> GetGrid() {
        return Pathfinding.GetGrid();
    }

    private void SetPath(int currentX, int currentY, int endX, int endY, bool isAttacking){
        
        // Calls the pathfinding script to calculate path from (currentX, currentY)
        // to (endX, endY)

        path = Pathfinding.Instance.FindPath(currentX, currentY, endX, endY, isAttacking);
    }

    private void SetPath(Vector3 startPosition, Vector3 endPosition, bool isAttacking){

        // Overload for SetPath that accepts world position instead of grid position

        List<Vector3> vectorPath = Pathfinding.Instance.FindPath(startPosition, endPosition, isAttacking);
        if (vectorPath != null){
            path = Vector3ListToPathNodeList(vectorPath);
        } 
    }

    private void HighLightPath(){

        // Each time the mouse hovers over a new tile in the grid or the active creature moves, 
        //the pathfinding is called and the resulting path is used to highlight the tiles that 
        // make up the path. The actual highlighting is done in PathfindingVisual.cs.

        // Can't pathfind if there's no creature selected
        if (UGame.GetActiveCreature() == null){
            return;
        }

        // Don't highlight if nothing has changed
        if (!IsMouseOverNewGridNode() && !HasCreatureMoved()) {
            return;
        }

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
        GetGrid().GetXY(GetPosition(UGame.GetActiveCreature()), out int currentX, out int currentY);

        int endX, endY;

        // Allow player to click through the active creature so that they can pathfind to a space
        // behind it.
        if (IsActiveCreature(hit.transform.GameObject())) {
            GetGrid().GetXY(UGame.GetMousePosition3D(Camera.main, "Ground"), out endX, out endY);
        } else {
            GetGrid().GetXY(hit.point, out endX, out endY);
        }

        if (!IsInsideGrid(endX, endY)) {
            return;
        }

        // If the mouse is hovering over another creature, then set the pathfiding destination
        // to the anchor point of that creature.
        bool isAttacking = false;
        if (
            hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures") 
            && !IsActiveCreature(hit.transform.GameObject())
        ) {
            isAttacking = true;
            GetGrid().GetXY(GetPosition(hit.transform.GameObject()), out endX, out endY);
        }

        // Find path to endX, endY
        SetPath(currentX, currentY, endX, endY, isAttacking);

        // Update highlighted tiles
        pathFindingVisual.SetPath(path);
        pathFindingVisual.TileHighlighting();
    }


    private Vector3 GetPosition(GameObject player){

        // Pathfinding for creatures is based on an anchor game object which is a child
        // of the creature object, so return the position of that instead of the position
        // of the creature

        if (player.transform.childCount == 0) {
            return player.transform.position;
        } else {
            return player.transform.GetChild(0).transform.position;
        }
    }

    private bool IsMouseOverNewGridNode(){

        // Check if mouse has moved over a new tile to avoid unnecessary pathfinding

        if (
            GetGrid().GetGridObject(UGame.GetMousePosition3D(Camera.main)) 
            != GetGrid().GetGridObject(lastMousePosition)
        ){
            lastMousePosition = UGame.GetMousePosition3D(Camera.main);
            return true;
        } else {
            return false;
        }
    }

    private bool HasCreatureMoved(){

        // Check if the active creature has moved to update highlighted tiles as it
        // travels along its path

        if (UGame.GetActiveCreature().transform.position != lastObjectPosition){
            lastObjectPosition = UGame.GetActiveCreature().transform.position;
            return true;
        } else {
            return false;
        }
    }

    private bool IsInsideGrid(int x, int y){
        return (
            x < GetGrid().GetWidth() 
            && y < GetGrid().GetHeight() 
            && x >= 0 
            && y >= 0
        );
    }
    private bool IsActiveCreature(GameObject creature){
        return creature == UGame.GetActiveCreature();
    }
    

    private void SetSpaceToOccupied(PathNode positionNode, GameObject creature){
        
        // Set every pathNode in creature's space to occupied

        Grid<PathNode> grid = GetGrid();
        UPathing.SetCreatureSpaceToOccupied(creature, grid, positionNode.x, positionNode.y);
    }

    private void ClearCreatureSpace(GameObject creature){

        // Set every pathNode in creature's space to unoccupied

        Grid<PathNode> grid = GetGrid();
        GameObject anchor = creature.transform.GetChild(0).GameObject();
        grid.GetXY(anchor.transform.position, out int x, out int y);

        UPathing.SetCreatureSpaceToUnoccupied(creature, grid, x, y);
    }


    public Vector3 GetPosition() {
        return UGame.GetActiveCreature().transform.GetChild(0).transform.position;
    }

    private List<Vector3> PathNodeListToVector3List(List<PathNode> pathNodeList){

        // Convert list of pathNode to list of Vector3
        
        List<Vector3> vector3List = new();
        float cellSize = GetGrid().GetCellSize();

        foreach(PathNode node in pathNodeList){
            vector3List.Add(
                GetGrid().GetWorldPosition(node.x, node.y) 
                + UPathing.XZPlane(cellSize, cellSize) * 0.5f // Offset to middle of tile
            );
        }
        return vector3List;
    }

    private List<PathNode> Vector3ListToPathNodeList(List<Vector3> vectorList){

        // Convert list of PathNode to list of Vector3

        List<PathNode> pathNodeList = new();
        foreach(Vector3 position in vectorList){
            GetGrid().GetXY(position, out int x, out int y);
            pathNodeList.Add(
                GetGrid().GetGridObject(x, y)
            );
        }
        return pathNodeList;
    }

}
