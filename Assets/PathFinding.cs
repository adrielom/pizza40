using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding : MonoBehaviour
{

    List<Node> path = new List<Node>();
    Node to = null;
    void Start()
    {
        to = GetRandomNode();
        Node from = NodeMatrix.Instance?.targetsClosestNode;
        path = FindPath(from, to);

    }


    void OnDrawGizmos()
    {
        if (path.Count > 0)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawCube(path[0].vertexPosition, Vector3.one * NodeMatrix.Instance.sphereRadius);

            Gizmos.color = Color.black;
            Gizmos.DrawCube(path[path.Count - 1].vertexPosition, Vector3.one * NodeMatrix.Instance.sphereRadius);
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count; i++)
            {
                if (i + 1 < path.Count)
                    Gizmos.DrawLine(path[i].vertexPosition, path[i + 1].vertexPosition);
            }
        }

    }



    public List<Node> FindPath(Node from, Node to)
    {
        List<Node> path = new List<Node>();
        List<Node> unvisited = NodeMatrix.Instance.allNodes;
        Node loopedNode = new Node(from);

        path.Add(from);
        if (from.Equals(to))
            return path;

        while (!path.Contains(to))
        {
            Node next = GetPath(to, loopedNode, ref path);
            loopedNode = NodeMatrix.Instance.GetNodeByPosition(next.vertexPosition);
        }

        return path;
    }

    Node GetPath(Node to, Node from, ref List<Node> path)
    {
        Node loopedNode = new Node(from);
        float dist = Mathf.Infinity;
        Node lastNode = null;

        if (from.Equals(to))
        {
            path.Add(to);
            return to;
        }

        for (int i = 0; i < loopedNode.neighbourNodes.Count; i++)
        {
            if (loopedNode.neighbourNodes[i] == null)
            {
                loopedNode.neighbourNodes = NodeMatrix.Instance.FindNeighbourNodes(loopedNode.vertexPosition).Where(t => t != null).ToList();
                break;
            }
        }

        for (int i = 0; i < loopedNode.neighbourNodes.Count; i++)
        {
            if (Vector3.Distance(loopedNode.neighbourNodes[i].vertexPosition, to.vertexPosition) < dist)
            {
                dist = Vector3.Distance(loopedNode.neighbourNodes[i].vertexPosition, to.vertexPosition);
                lastNode = new Node(loopedNode.neighbourNodes[i]);
            }
        }

        if (!path.Contains(lastNode))
            path.Add(lastNode);

        return lastNode;

    }

    public Node GetRandomNode()
    {
        int indexer = Random.Range(0, NodeMatrix.Instance.nodes.Count - 1);
        return NodeMatrix.Instance?.nodes?[indexer];
    }

}
