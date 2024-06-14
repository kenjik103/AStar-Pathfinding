using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;//area of the grid
    public float nodeRadius;// how large each node is
    public int obstacleProximityPenalty = 10;
    public int blurStrength = 5;
    
    Node[,] grid;//2d array representing the grid

    public TerrainType[] walkableRegions;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();


    float nodeDiamater;
    int gridSizeX, gridSizeY;//based off node radius, number of nodes we can fit in both x and y

    int penaltyMin;
    int penaltyMax;

    public int MaxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }
    
    //calculate number of nodes 
    void Awake(){
        nodeDiamater = nodeRadius*2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiamater);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiamater);

        foreach (TerrainType region in walkableRegions) {
            walkableMask.value |= region.terrainMask.value; //adding each layer within our walkable regions to the walkable mask
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
        CreateGrid();
    }

    void CreateGrid(){
        grid = new Node[gridSizeX, gridSizeY];
        //basically (0,0,0) - (gridWorldSize.x/2,0,0) - (0, 0, gridWorldSize.y/2) = (-gridWorldSize.x/2,0,-gridWorldsize.y/2)
        Vector3 worldBottomLeft = transform.position + (Vector3.left * (gridWorldSize.x/2)) - (Vector3.forward * (gridWorldSize.y/2));

        for (int x = 0; x < gridSizeX; x++){
            for (int y = 0; y < gridSizeY; y++){
                //getting a point smack in the middle of each node in our grid
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiamater + nodeRadius) + Vector3.forward * (y * nodeDiamater + nodeRadius);
                //checks point of size nodeRadius centered around worldPoint (our node) collides with anything with our unwalkable mask. If it does then walkable is false
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);

                int movementPenalty = 0;
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down); //a ray directly above the node fired straight down
                if (Physics.Raycast(ray, out RaycastHit hit, 100, walkableMask)) {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }
                if (!walkable) {
                    movementPenalty += obstacleProximityPenalty;
                }

                grid[x,y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }
        BlurPenaltyMap(blurStrength);
    }

    void BlurPenaltyMap(int blurSize) {
        int kernalSize = (blurSize * 2) + 1; //ensures kernalSize is always odd
        int kernalExtents = (kernalSize - 1) / 2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++) {
            for (int x = -kernalExtents; x <= kernalExtents; x++) {
                int sampleX = Mathf.Clamp(x, 0, kernalExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX,y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++) {
                int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernalExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }
        
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = -kernalExtents; y <= kernalExtents; y++) {
                int sampleY = Mathf.Clamp(y, 0, kernalExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }
            
            int roundedValue = Mathf.RoundToInt((float)penaltiesVerticalPass[x,0] / (kernalSize * kernalSize));
            grid[x,0].movementPenalty = roundedValue;
            for (int y = 1; y< gridSizeY; y++) {
                int removeIndex = Mathf.Clamp(y - kernalExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernalExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass [x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                
                roundedValue = Mathf.RoundToInt((float)penaltiesVerticalPass[x,y] / (kernalSize * kernalSize));
                grid[x,y].movementPenalty = roundedValue;
                if (roundedValue > penaltyMax)
                    penaltyMax = roundedValue;
                if (roundedValue < penaltyMin)
                    penaltyMin = roundedValue;
            }
        }
    }


    //given a node, return its neighbors as a list
    public List<Node> GetNeighbors (Node node){ 
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++){
            for (int y = -1; y <= 1; y++){
                if (x == 0 && y == 0) { //node == current node; not neighbor
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >=0 && checkY < gridSizeY) {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }


    //returns a Node within the NodeGrid at whatever the provided position is
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        //NOTE: center of grid is at (0,0,0).
        //ANOTHER NOTE: plane in unity goes along x and z planes. Thus, worldPosition.y shouldnt be touched.
        float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;//gets position as a percent. far right = 1 far left = 0
        float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
        //making sure values are clamped from 0 to 1
        percentX = Math.Clamp(percentX, 0, 1);
        percentY = Math.Clamp(percentY, 0, 1);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x,y];
    }

    
    //draw out grid for visualization’s sake
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos){
            foreach (Node n in grid) {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                
                Gizmos.color = (n.walkable)?Gizmos.color: Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiamater));
            }
        } 
    }
    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;

    }
}