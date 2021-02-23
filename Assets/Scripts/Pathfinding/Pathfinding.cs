﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding {
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;
    public Field<PathNode> field {
        get;
        private set;
    }

    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height) {
        this.field = new Field<PathNode>(width,
                                         height,
                                         1,
                                         Utils.TILE_RATIO,
                                         Vector2.zero,
                                         (Field<PathNode> field, int x, int y) => new PathNode(field, x, y));
        

    }

    public List<PathNode> ComputePath(int startX, int startY, int endX, int endY) {
        // Instantiate lists.
        this.openList = new List<PathNode>();
        this.closedList = new List<PathNode>();

        // Retrieve element at start position.
        PathNode startNode = this.field.GetElement(startX, startY);
        PathNode endNode = this.field.GetElement(endX, endY);
        this.openList.Add(startNode);

        // Initialise the field.
        for (int i = 0; i < this.field.width; i++) {
            for (int j = 0; j < this.field.height; j++) {
                PathNode pathNode = this.field.GetElement(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.ComputeFCost();
                pathNode.previous = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = this.ComputeHCost(startNode, endNode);
        startNode.ComputeFCost();

        while (this.openList.Count > 0) {
            PathNode currentNode = this.GetFastestNode(this.openList);

            if (currentNode == endNode) {
                // Found the solution, so trace back and return the complete path.
                return RetrievePath(endNode);
            } else {
                this.openList.Remove(currentNode);
                this.closedList.Add(currentNode);

                foreach (PathNode neighbor in currentNode.GetNeighbors()) {
                    if (!closedList.Contains(neighbor)) {
                        int tentativeGCost = currentNode.gCost + this.ComputeHCost(currentNode, neighbor);

                        if (tentativeGCost < neighbor.gCost) {
                            neighbor.previous = currentNode;
                            neighbor.gCost = tentativeGCost;
                            neighbor.hCost = this.ComputeHCost(neighbor, endNode);
                            neighbor.ComputeFCost();

                            if (!openList.Contains(neighbor)) {
                                openList.Add(neighbor);
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    private List<PathNode> RetrievePath(PathNode end) {
        List<PathNode> path = new List<PathNode>();

        // Trace back from end to start.
        PathNode currentNode = end;
        path.Add(currentNode);
        while (currentNode.previous != null) {
            path.Add(currentNode.previous);
            currentNode = currentNode.previous;
        }

        // Reverse the found path in order to return it start to end.
        path.Reverse();

        return path;
    }

    private PathNode GetFastestNode(List<PathNode> nodes) {
        PathNode result = nodes[0];

        for (int i = 0; i < nodes.Count; i++) {
            if (nodes[i].fCost < result.fCost) {
                result = nodes[i];
            }
        }

        return result;
    }

    private int ComputeHCost(PathNode start, PathNode end) {
        int xDistance = Mathf.Abs(start.x - end.x);
        int yDistance = Mathf.Abs(start.y - end.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return STRAIGHT_COST * Mathf.Min(xDistance, yDistance) + DIAGONAL_COST * remaining;
    }
}
