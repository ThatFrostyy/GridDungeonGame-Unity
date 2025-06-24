using UnityEngine;
using System.Collections.Generic;
public static class AStar
{
    private static Vector2 gridSize;

    private class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public int GCost; // Cost from start to this node
        public int HCost; // Heuristic cost to end node
        public int FCost => GCost + HCost; // Total cost
        public bool isObstacle;

        public Node(Vector2Int position, Node parent, int gCost, int hCost, bool isObstacle)
        {
            this.Position = position;
            this.Parent = parent;
            this.GCost = gCost;
            this.HCost = hCost;
            this.isObstacle = isObstacle;
        }
    }

    public static List<Vector2> FindPath(Vector2 start, Vector2 target, Vector2 gridCellSize, System.Func<Vector2, bool>isObstacle)
    {
        gridSize = gridCellSize;

        Vector2Int startGridPos = GridUtils.WorldToGrid(start);
        Vector2Int targetGridPos = GridUtils.WorldToGrid(target);

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Node startNode = new Node(startGridPos, null, 0, CalculateHCost(startGridPos, targetGridPos), false);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);

            openList.Remove(currentNode);
            closedList.Add(currentNode.Position);

            if (currentNode.Position == targetGridPos)
            {
                return GeneratePath(currentNode);
            }

            foreach (Vector2Int adjacentPos in GetAdjacentPositions(currentNode.Position))
            {
                if (closedList.Contains(adjacentPos) || isObstacle(GridUtils.GridToWorld(adjacentPos)))
                {
                    continue; // Skip already evaluated nodes and obstacles
                }
                
                int gCost = currentNode.GCost + 1; // Assuming uniform cost for each step
                int hCost = CalculateHCost(adjacentPos, targetGridPos);

                Node adjacentNode = new Node(adjacentPos, currentNode, gCost, hCost, false);

                int index = openList.FindIndex(node => node.Position == adjacentNode.Position);
                if (index != -1)
                {
                    // If the node is already in the open list, check if this path is better
                    if (gCost < openList[index].GCost)
                    {
                        openList[index].Parent = currentNode;
                        openList[index].GCost = gCost;
                    }
                }
                else
                {
                    // If not in open list, add it
                    openList.Add(adjacentNode);
                }
            }
        }
         
        return null; // No path found
    }

    private static Node GetLowestFCostNode(List<Node> nodes)
    {
        Node lowest = nodes[0];
     
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].FCost < lowest.FCost || (nodes[i].FCost == lowest.FCost && nodes[i].HCost < lowest.HCost))
            {
                lowest = nodes[i];
            }
        }
        
        return lowest;
    }

    private static int CalculateHCost(Vector2Int current, Vector2Int target)
    {
        // Using Manhattan distance as heuristic
        return Mathf.Abs(current.x - target.x) + Mathf.Abs(current.y - target.y);
    }

    private static List<Vector2> GeneratePath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(GridUtils.GridToWorld(currentNode.Position));
            currentNode = currentNode.Parent;
        }

        path.Reverse(); // Reverse the path to get it from start to end
        return path;
    }

    private static List<Vector2Int> GetAdjacentPositions(Vector2Int position)
    {
        return new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right,    
        };
    }
}
