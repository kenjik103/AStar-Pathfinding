using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line
{
    const float verticalLineSlope = 1e5f;//arbitrary big number. can't divide by zero, so divide by a really big number instead.
    
    float slope;
    float y_intercept;
    
    float slopePerpendicular;
    
    //two arbitrary points on our line used in GetSide().
    Vector2 pointOnLine_1;
    Vector2 pointOnLine_2;
    
    //bool that keeps track of whether or not a point is before (true) or after (false) our turn boundary
    bool approachSide; 


    //the perpendicular line is the line from node to node. the regular line is the turn boundary itself.
    //lines intersect at pointOnLine
    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine) { //assumes that perpendicular line intersects at pointOnLine.
        float dx = pointOnLine.x - pointPerpendicularToLine.x;      
        float dy = pointOnLine.y - pointPerpendicularToLine.y;      //all this math is basically rise over run on the perpendicular line

        if (dx == 0) {
            slopePerpendicular = dy / verticalLineSlope;
        } else {
            slopePerpendicular = dy / dx;
        }

        if (slopePerpendicular == 0) {
            slope = -1 / verticalLineSlope;
        } else {
            slope = -1 / slopePerpendicular; //basic math fact. slope is equal to negative reciprocal of perpendicular slope
        }

        y_intercept = pointOnLine.y - slope * pointOnLine.x; //y=mx+b rearranged to solve for b
        
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1,slope);
        
        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }

    //returns true if p is on one side of our line, and false if on other side
    //in the case of our program, true if point before our turn boundary, false if after.
    bool GetSide(Vector2 p) {
        return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    //if our point has crossed over the turn boundary, return true;
    public bool HasCrossedLine(Vector2 p) {
        return GetSide(p) != approachSide;
    }
    
    //distance between a point and our line
    public float DistanceFromPoint(Vector2 p) {
        float yInterceptPerpendicular = p.y - p.x * slopePerpendicular;
        float intersectX = (yInterceptPerpendicular - y_intercept) / (slope - slopePerpendicular);
        float intersectY = (slopePerpendicular * p.x) + yInterceptPerpendicular;
        return Vector2.Distance(p, new Vector2(intersectX, intersectY));
    }

    public void DrawWithGizmos(float length) {
        Vector3 lineDir = new Vector3(1, 0, slope).normalized;
        Vector3 lineCenter = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine(lineCenter - lineDir * length/2f, lineCenter + lineDir * length/2f);
    }
}