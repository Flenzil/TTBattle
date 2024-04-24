using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PathingUtils;
using Unity.VisualScripting;
using System.Linq;
using GameUtils;
using System.Collections;
using UnityEngine.Analytics;

public class PathFindingManager : MonoBehaviour {

    private const float speed = 4f;


    private int currentPathIndex;
    private List<Vector3> pathVectorList;
    //private Grid<PathNode> grid = Pathfinding.Instance.GetGrid(); 
    private Vector3 lastMousePosition;
    private Vector3 lastObjectPosition;
    private static PathFindingManager instance;

    public List<PathNode> path;


    [SerializeField] PathFindingVisual pathFindingVisual;

    public static PathFindingManager Instance {
        get {
            if (instance == null)
                Debug.Log("Pathfinding Manager is null");
            return instance;
        }
    }

    private void Awake(){
        instance = this;
    }

    private void Update() {

        if (EventSystem.current.IsPointerOverGameObject()){
            pathFindingVisual.UnHighlightPath(pathFindingVisual.highlightedPath);
            HandleMovement();
            return;
        }

        HighLightPath();
        HandleMovement();

        if (Input.GetMouseButtonDown(0)) {

            // Find object under mouse position
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
            GameObject hitObject = hit.transform.GameObject();



            // If the object is the active player, pathfind to ground behind it, as if the object wasn't there.
            if (hitObject.layer == LayerMask.NameToLayer("Creatures") && IsActiveCreature(hitObject)){
                SetTargetPosition(UGame.GetMousePosition3D(Camera.main, "Ground"));
            }

            // If the object is the ground, pathfind to that point
            if (hitObject.layer == LayerMask.NameToLayer("Ground")){
                SetTargetPosition(hit.point);
            }
        }
    }

    private Grid<PathNode> GetGrid() {
        return Pathfinding.Instance.GetGrid();
    }

    private void SetPath(int currentX, int currentY, int endX, int endY){
        path = Pathfinding.Instance.FindPath(currentX, currentY, endX, endY);
    }

    private void SetPath(Vector3 startPosition, Vector3 endPosition){
        List<Vector3> vectorPath = Pathfinding.Instance.FindPath(startPosition, endPosition);
        if (vectorPath != null){
            path = Vector3ListToPathNodeList(vectorPath);
        } 
    }

    private void HighLightPath(){
        if (UGame.GetActiveCreature() == null){
            return;
        }
        if (!IsMouseOverNewGridNode() && !ObjectHasMoved()) {
            return;
        }

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

        GetGrid().GetXY(GetPosition(UGame.GetActiveCreature()), out int currentX, out int currentY);

        int endX, endY;
        if (IsActiveCreature(hit.transform.GameObject())) {
            GetGrid().GetXY(UGame.GetMousePosition3D(Camera.main, "Ground"), out endX, out endY);
        } else {
            GetGrid().GetXY(hit.point, out endX, out endY);
        }

        if (!IsInsideGrid(endX, endY)) {
            return;
        }
        if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures") && !IsActiveCreature(hit.transform.GameObject())) {
            GetGrid().GetXY(GetPosition(hit.transform.GameObject()), out endX, out endY);
        }

        SetPath(currentX, currentY, endX, endY);

        pathFindingVisual.SetPath(path);
        pathFindingVisual.TileHighlighting();
    }


    private Vector3 GetPosition(GameObject player){
        if (player.transform.childCount == 0) {
            return player.transform.position;
        } else {
            return player.transform.GetChild(0).transform.position;
        }
    }

    private bool IsMouseOverNewGridNode(){
        if (GetGrid().GetGridObject(UGame.GetMousePosition3D(Camera.main)) != GetGrid().GetGridObject(lastMousePosition)){
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
        if (x < GetGrid().GetWidth() && y < GetGrid().GetHeight() && x >= 0 && y >= 0){
            return true;
        } else {
            return false;
        }
    }
    private bool IsActiveCreature(GameObject creature){
        if (creature == UGame.GetActiveCreature()) return true;
        return false;
    }
    
    private void HandleMovement() {
        if (pathVectorList != null) {
            Vector3 targetPosition = pathVectorList[currentPathIndex];
            if (Vector3.Distance(GetPosition(), targetPosition) > 0.05f) {
                Vector3 moveDir = (targetPosition - GetPosition()).normalized;

                UGame.GetActiveCreature().transform.position = UGame.GetActiveCreature().transform.position + moveDir * speed * Time.deltaTime;
            } else {
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count) {
                    StopMoving();
                }
            }
        } 
    }

    private void StopMoving() {
        pathVectorList = null;
    }

    private void SetSpaceToOccupied(PathNode positionNode, GameObject creature){
        Grid<PathNode> grid = Pathfinding.Instance.GetGrid();

        UPathing.SetCreatureSpaceToOccupied(creature, grid, positionNode.x, positionNode.y);
    }

    private void ClearCreatureSpace(GameObject creature){
        Grid<PathNode> grid = Pathfinding.Instance.GetGrid();
        GameObject anchor = creature.transform.GetChild(0).GameObject();
        grid.GetXY(anchor.transform.position, out int x, out int y);

        UPathing.SetCreatureSpaceToUnoccupied(creature, grid, x, y);
    }


    public Vector3 GetPosition() {
        return UGame.GetActiveCreature().transform.GetChild(0).transform.position;
    }

    private List<Vector3> PathNodeListToVector3List(List<PathNode> pathNodeList){

        List<Vector3> vector3List = new();
        float cellSize = Pathfinding.Instance.GetGrid().GetCellSize();
        foreach(PathNode node in pathNodeList){
            vector3List.Add(
                GetGrid().GetWorldPosition(node.x, node.y) 
                + UPathing.XZPlane(cellSize, cellSize) * 0.5f
            );
        }
        return vector3List;
    }

    private List<PathNode> Vector3ListToPathNodeList(List<Vector3> vectorList){

        List<PathNode> pathNodeList = new();
        float cellSize = Pathfinding.Instance.GetGrid().GetCellSize();
        foreach(Vector3 position in vectorList){
            GetGrid().GetXY(position, out int x, out int y);
            pathNodeList.Add(
                GetGrid().GetGridObject(x, y)
            );
        }
        return pathNodeList;
    }

    public void SetTargetPosition(Vector3 targetPosition) {

        currentPathIndex = 0;

        SetPath(GetPosition(), targetPosition);

        if (path != null && path.Count > 1) {

            path.RemoveAt(0);

            if (path != null) {
                ClearCreatureSpace(UGame.GetActiveCreature());
                SetSpaceToOccupied(path.Last(), UGame.GetActiveCreature());
                pathFindingVisual.SetPath(path);
                pathFindingVisual.TileHighlighting();
                pathVectorList = PathNodeListToVector3List(path);
                }
        }
    }
}
