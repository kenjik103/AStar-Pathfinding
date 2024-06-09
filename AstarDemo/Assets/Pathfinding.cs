using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Runtime.ExceptionServices;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    Grid grid;
    void Awake () {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath (Vector3 startPosition, Vector3 endPosition){
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        
        Node startNode = grid.NodeFromWorldPoint(startPosition);
        Node targetNode = grid.NodeFromWorldPoint(endPosition);
        if (startNode.walkable && targetNode.walkable) {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);//Set of nodes to be evaluated
            HashSet<Node> closedSet = new HashSet<Node>();//set of nodes already evaluated

            //Add start node to open
            openSet.Add(startNode);

            //loop
            while (openSet.Count > 0) {
                // current = node in OPEN with lowest f_cost
                //remove current from OPEN
                Node currentNode = openSet.RemoveFirst();

                //add current to CLOSED
                closedSet.Add(currentNode);


                //if path has been found return
                if (currentNode == targetNode){
                    sw.Stop();
                    UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
                    pathSuccess = true;
                    break;
                }
    
                foreach (Node neighbor in grid.GetNeigbors(currentNode)) {

                    //if neighbor is not traversable or neigbor is in CLOSED
                    if (!neighbor.walkable || closedSet.Contains(neighbor)) {
                        //skip to next neighbor
                        continue;
                    }

                    //if new path to neigbor is shorter or neigbor is not in OPEN
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                        //set f_cost of neighbor
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);

                        //set parent of neighbor to current
                        neighbor.parent = currentNode;

                        //if neighbor is not in OPEN add neigbor to OPEN
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }

                }
            }
        }
        yield return null;
        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode; 

        while (currentNode != startNode){
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[]waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX, path[i-1].gridY - path[i].gridY);
            if (directionNew != directionOld) {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }

        return waypoints.ToArray();
    } 

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Math.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Math.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14*dstY + 10*(dstX-dstY);
        return 14*dstX + 10*(dstY-dstX);
    }
}
