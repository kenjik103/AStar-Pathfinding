using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool displayPathGizmos;
    
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;
    
    public Transform target;
    public float speed = 20;
    public float turnDst = 5;
    public float turnSpeed = 3;
    public float stoppindDst = 10;

    Path path;

    void Start() {
        StartCoroutine(UpdatePath());
    }
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            path = new Path(waypoints, transform.position, turnDst, stoppindDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetOldPos = target.position;
        while (true) {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - targetOldPos).sqrMagnitude > sqrMoveThreshold) {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetOldPos = target.position;
            }
        }
    }

    IEnumerator FollowPath() {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookpoints[0]);

        float speedPercent = 1;
        
        while (followingPath) {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            //if a unit moves at a speed that breaches multiple turn boundaries in one frame, updating the path index multiple times is necessary;
            //thus the while loop
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
                if (pathIndex == path.finishLineIndex) {
                    followingPath = false;
                    break;
                } 
                pathIndex++;
                
            }

            if (followingPath) {
                if (pathIndex >= path.slowDownIndex && stoppindDst > 0) {
                    speedPercent = Math.Clamp(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppindDst, 0, 1);
                    if (speedPercent < 0.1f) {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookpoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * (Time.deltaTime * speed * speedPercent), Space.Self);
            }
            
            yield return null;
        }
    }

    public void OnDrawGizmos(){
        if (path != null && displayPathGizmos) {
            path.OnDrawWithGizmos();
        }
    }
}