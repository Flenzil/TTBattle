using System.Collections.Generic;
using PathingUtils;
using GameUtils;
using UnityEngine;
using System;
using System.IO;

public class GameManager : MonoBehaviour {

    private Pathfinding pathfinding;
    //[SerializeField] private PathFindingVisual pathFindingVisual;
    [SerializeField] GameObject floorTile;
    [SerializeField] GameObject wallTile;
    [SerializeField] List<GameObject> creatures;

    public PathFindingVisual pathFindingVisual;
    public int floorWidth = 15;
    public int floorHeight = 15;
    public float cellSize = 1f;
    public bool visualiseGrid = true;

    private static GameManager instance;
    public GameObject activePlayer {get; set;}
    List<GameObject> initiativeOrder;

    enum states{
        moving,
        idle
    }

    private void Awake(){
        instance = this;

        pathfinding = new Pathfinding(floorWidth, floorHeight, cellSize);
        pathFindingVisual = new PathFindingVisual();
        if (visualiseGrid){
            pathFindingVisual.SetFloorTile(floorTile);
            pathFindingVisual.SetWallTile(wallTile);
            pathFindingVisual.SetGrid(Pathfinding.GetGrid());
        }

        //states state = states.idle;

        SpawnCreatures();

    }

    private void Update() {
    }


    public static GameManager Instance {
        get {
            if (instance == null)
                Debug.Log("Game Manager is null");
            return instance;
        }
    }


    private Vector3 GetPosition(GameObject creature){
        return creature.transform.GetChild(0).transform.position;
    }

    private void SpawnCreatures(){
        for (int i = 0; i < creatures.Count; i++){
            System.Random random = new();
            int creatureLocX = random.Next(1, floorWidth - 1);
            int creatureLocY = random.Next(1, floorHeight - 1);

            while (Pathfinding.GetGrid().GetGridObject(random.Next(0,floorWidth), random.Next(0, floorHeight)).isOccupied){
                creatureLocX = random.Next(1, floorWidth - 1);
                creatureLocY = random.Next(1, floorHeight - 1);
            }

            Vector3 distanceToAnchor;
            distanceToAnchor.x = Math.Abs(creatures[i].transform.position.x - creatures[i].transform.GetChild(0).transform.position.x); 
            distanceToAnchor.y = Math.Abs(creatures[i].transform.position.y - creatures[i].transform.GetChild(0).transform.position.y); 
            distanceToAnchor.z = Math.Abs(creatures[i].transform.position.z - creatures[i].transform.GetChild(0).transform.position.z); 

            Debug.Log(distanceToAnchor);
            Vector3 placementPosition = Pathfinding.GetGrid().GetWorldPosition(creatureLocX, creatureLocY) + UPathing.XZPlane(1, 1) * 0.5f;

            GameObject creatureObject = Instantiate(creatures[i], placementPosition, Quaternion.identity);
            
            creatureObject.layer = LayerMask.NameToLayer("Creatures");
            creatureObject.name = creatures[i].name;

            Vector3 creaturePosition = GetPosition(creatureObject);

            creatureObject.transform.Translate(distanceToAnchor);

            
            Pathfinding.GetGrid().GetXY(GetPosition(creatureObject), out int x, out int y);
            UPathing.SetCreatureSpaceToOccupied(creatureObject, Pathfinding.GetGrid(), x, y);
        }
    }
}



