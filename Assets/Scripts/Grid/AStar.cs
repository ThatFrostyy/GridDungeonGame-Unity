using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4-directional A* pathfinding on the tile grid.
/// </summary>
public static class AStar
{
    // The map has no hard bounds, so unreachable targets must not search forever.
    private const int MaxExpandedNodes = 2048;

    private sealed class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
    }

    /// <summary>
    /// Finds a path from start to target, both inclusive, as tile-center world positions.
    /// Returns null if the target cannot be reached.
    /// </summary>
    public static List<Vector2> FindPath(Vector2 start, Vector2 target, Func<Vector2, bool> isObstacle)
    {
        Vector2Int startGrid = GridUtils.WorldToGrid(start);
        Vector2Int targetGrid = GridUtils.WorldToGrid(target);

        var open = new List<Node>();
        var openLookup = new Dictionary<Vector2Int, Node>();
        var closed = new HashSet<Vector2Int>();

        var startNode = new Node { Position = startGrid, HCost = GridUtils.ManhattanDistance(startGrid, targetGrid) };
        open.Add(startNode);
        openLookup[startGrid] = startNode;

        while (open.Count > 0 && closed.Count < MaxExpandedNodes)
        {
            Node current = PopLowestCost(open);
            openLookup.Remove(current.Position);
            closed.Add(current.Position);

            if (current.Position == targetGrid)
            {
                return BuildPath(current);
            }

            foreach (Vector2Int direction in GridUtils.CardinalDirections)
            {
                Vector2Int neighbour = current.Position + direction;
                if (closed.Contains(neighbour) || isObstacle(GridUtils.GridToWorld(neighbour)))
                {
                    continue;
                }

                int gCost = current.GCost + 1;
                if (openLookup.TryGetValue(neighbour, out Node existing))
                {
                    if (gCost < existing.GCost)
                    {
                        existing.GCost = gCost;
                        existing.Parent = current;
                    }
                    continue;
                }

                var node = new Node
                {
                    Position = neighbour,
                    Parent = current,
                    GCost = gCost,
                    HCost = GridUtils.ManhattanDistance(neighbour, targetGrid),
                };
                open.Add(node);
                openLookup[neighbour] = node;
            }
        }

        return null;
    }

    private static Node PopLowestCost(List<Node> nodes)
    {
        int best = 0;
        for (int i = 1; i < nodes.Count; i++)
        {
            Node candidate = nodes[i];
            Node lowest = nodes[best];
            if (candidate.FCost < lowest.FCost || (candidate.FCost == lowest.FCost && candidate.HCost < lowest.HCost))
            {
                best = i;
            }
        }

        Node result = nodes[best];
        nodes[best] = nodes[nodes.Count - 1];
        nodes.RemoveAt(nodes.Count - 1);
        return result;
    }

    private static List<Vector2> BuildPath(Node end)
    {
        var path = new List<Vector2>();
        for (Node node = end; node != null; node = node.Parent)
        {
            path.Add(GridUtils.GridToWorld(node.Position));
        }

        path.Reverse();
        return path;
    }
}
