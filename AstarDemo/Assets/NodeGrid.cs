using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;//area of the grid
    public float nodeRadius;// how large each node is
    Node[,] grid;//2d array representing the grid


    float nodeDiamater;
    int gridSizeX, gridSizeY;//based off node radius, number of nodes we can fit in both x and y

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
        CreateGrid();
    }


    //given a node, return its neighbors as a list
    public List<Node> GetNeigbors (Node node){ 
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++){
            for (int y = -1; y <= 1; y++){
                if (x == 0 && y == 0) { //node == current node; not neighbor
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeY && checkY >=0 && checkY < gridSizeY) {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    void CreateGrid(){
        grid = new Node[gridSizeX, gridSizeY];
        //basically (0,0,0) - (gridWorldSize.x,0,0) - (0, 0, gridWorldSize.y) = (-gridWorldSize.x,0,-gridWorldsize.y)
        Vector3 worldBottomLeft = transform.position + (Vector3.left * (gridWorldSize.x/2)) - (Vector3.forward * (gridWorldSize.y/2));

        for (int x = 0; x < gridSizeX; x++){
            for (int y = 0; y < gridSizeX; y++){
                //getting a point smack in the middle of each node in our grid
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiamater + nodeRadius) + Vector3.forward * (y * nodeDiamater + nodeRadius);

                //checks point of size nodeRadius centered around worldPoint (our node) collides with anything with our unwalkable mask. If it does then walkable is false
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);

                grid[x,y] = new Node(walkable, worldPoint, x, y);

            }
        }

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

    
    //draw out grid for visualizations sake
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos){
            foreach (Node n in grid) {
                Gizmos.color = (n.walkable)?Color.white: Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiamater-.1f));
            }
        } 
    }
}
