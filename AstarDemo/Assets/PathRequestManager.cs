using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//class that helps dictate when to draw paths for optimizations sake
public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;
    bool isProcessingPath;
    void Awake(){
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action <Vector3[], bool> callback) {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
        
    }

    void TryProcessNext(){
        if (!isProcessingPath && pathRequestQueue.Count > 0) {//if not currently processing path and pathQueue not empty
            //start processing next path
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);//begins A* algorithm as coroutine
        }
    }

    //called by Pathfinding once A* has finished running.
    public void FinishedProcessingPath (Vector3[] path, bool success) {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();

    }

    struct PathRequest {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action <Vector3[], bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback){
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
        }
    }
}
