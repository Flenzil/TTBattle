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
    public Creature activeCreature {get; set;}
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
        if (Input.GetKey(KeyCode.B)) {
            UGame.GetActiveCreature().SetCondition(Condition.blinded);
        }

        if (Input.GetKey(KeyCode.I)) {
            UGame.GetActiveCreature().SetCondition(Condition.invisible);
        }
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
        
        // Set random grid position 
        for (int i = 0; i < creatures.Count; i++){
            System.Random random = new();
            int creatureLocX = random.Next(1, floorWidth - 1);
            int creatureLocY = random.Next(1, floorHeight - 1);

            // Ensure creatures don't spawn on top of each other
            while (Pathfinding.GetGrid().GetGridObject(random.Next(0,floorWidth), random.Next(0, floorHeight)).isOccupied){
                creatureLocX = random.Next(1, floorWidth - 1);
                creatureLocY = random.Next(1, floorHeight - 1);
            }

            // Translate the model so that the anchor (child object) is centred on a grid node
            Vector3 distanceToAnchor;
            distanceToAnchor.x = Math.Abs(creatures[i].transform.position.x - creatures[i].transform.GetChild(0).transform.position.x); 
            distanceToAnchor.y = Math.Abs(creatures[i].transform.position.y - creatures[i].transform.GetChild(0).transform.position.y); 
            distanceToAnchor.z = Math.Abs(creatures[i].transform.position.z - creatures[i].transform.GetChild(0).transform.position.z); 

            Vector3 placementPosition = Pathfinding.GetGrid().GetWorldPosition(creatureLocX, creatureLocY) + UPathing.XZPlane(1, 1) * 0.5f;

            Debug.Log(creatures[i]);
            GameObject creatureObject = Instantiate(creatures[i], placementPosition, Quaternion.identity);
            creatureObject.transform.Translate(distanceToAnchor);
            
            // Set layer, name and default weapon
            creatureObject.layer = LayerMask.NameToLayer("Creatures");
            creatureObject.name = creatures[i].name;
            creatureObject.GetComponent<Actions>().SetActiveAttack(creatureObject.GetComponent<Actions>().GetAttacks()[0]);
            
            // Set creatures space to occupied
            Pathfinding.GetGrid().GetXY(GetPosition(creatureObject), out int x, out int y);
            creatureObject.GetComponent<Creature>().SetCreatureSpaceToOccupied(x, y);
        }
    }
}



