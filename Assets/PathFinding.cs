using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class that handles finding the best paths from the player to a node positioning system
/// </summary>
public class PathFinding : MonoBehaviour
{
    //List of nodes that make up the path
    List<Node> path = new List<Node>();
    // The destination node and a hard memoized copy of it
    Node to = null, memoizedFromNode = null, from = null;
    Vector3 memoizedPosition = Vector3.positiveInfinity, closestDistance = Vector3.positiveInfinity;
    float closestDistanceToTarget = Mathf.Infinity;
    Color randColour;
    bool atSamePosition = false;

    bool drawLines = false;
    IEnumerator Start()
    {
        yield return new WaitUntil(() => CheckpointManager.Instance.GetGeneratedPoints() != null);
        //Sets up the 'to Node' to a random position
        to = NodeMatrix.Instance.FindClosestNode(this.transform.position, NodeMatrix.Instance.allNodes);
        memoizedPosition = transform.position;
        //'From' node is the closest node to the target
        from = NodeMatrix.Instance?.targetsClosestNode;
        closestDistanceToTarget = Vector3.Distance(from.vertexPosition, this.transform.position);
        //Gets the path
        path = FindPath(from, to);
        //Stores the from position
        memoizedFromNode = from;
        randColour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
    }

    void Update()
    {
        if (transform.position != memoizedPosition && !memoizedPosition.Equals(Vector3.positiveInfinity))
        {
            path.Clear();
            to = NodeMatrix.Instance.FindClosestNode(this.transform.position, NodeMatrix.Instance.allNodes);
            memoizedPosition = to.oppositeVertex;
            path = FindPath(NodeMatrix.Instance?.targetsClosestNode, to);
        }

        //checks if the targetsClosestNode changed
        if (memoizedFromNode?.vertexPosition != NodeMatrix.Instance?.targetsClosestNode?.vertexPosition && to != null && !memoizedPosition.Equals(Vector3.positiveInfinity))
        {
            closestDistanceToTarget = Vector3.Distance(from.vertexPosition, this.transform.position);
            if (closestDistanceToTarget <= NodeMatrix.Instance.sectionLength * NodeMatrix.Instance.matrixSize)
            {
                CalculateClosestDistance();
            }
            else
            {

                //Clears up the path
                path.Clear();
                NodeMatrix.Instance.SetNeighbourPosition();
                memoizedFromNode = NodeMatrix.Instance?.targetsClosestNode;
                //gets the path
                path = FindPath(NodeMatrix.Instance?.targetsClosestNode, to);
            }

        }
    }

    public void CalculateClosestDistance()
    {
        if (closestDistanceToTarget < NodeMatrix.Instance.matrixSize)
        {
            atSamePosition = true;
            print("same pos");
        }
    }

    //Draws the path
    void OnDrawGizmos()
    {

        Gizmos.color = new Color32(255, 165, 0, 255);
        if (from != null && to != null)
            Gizmos.DrawLine(to.vertexPosition, transform.position);

        if (path.Count > 0)
        {
            //Draws the starting point cube
            Gizmos.color = Color.white;
            Gizmos.DrawCube(path[0].vertexPosition, Vector3.one * NodeMatrix.Instance.sphereRadius);

            //Draws the ending point cube
            Gizmos.color = Color.black;
            Gizmos.DrawCube(path[path.Count - 1].vertexPosition, Vector3.one * NodeMatrix.Instance.sphereRadius);
            Gizmos.color = randColour;
            //Draws path
            for (int i = 0; i < path.Count; i++)
            {
                if (i + 1 < path.Count)
                    Gizmos.DrawLine(path[i].vertexPosition, path[i + 1].vertexPosition);
            }
        }

    }

    /// <summary>
    /// Algorithm that gets the shortest path from one input to the other based on the node system
    /// </summary>
    /// <param name="from">Starting point Node</param>
    /// <param name="to">Ending point Node</param>
    /// <returns>Returns a list of nodes that makes up the path</returns>
    public List<Node> FindPath(Node from, Node to)
    {

        List<Node> path = new List<Node>();
        Node loopedNode = new Node(from);

        path.Add(from);
        if (from.Equals(to))
            return path;

        while (!path.Contains(to))
        {
            Node next = GetPath(to, loopedNode, ref path);
            loopedNode = NodeMatrix.Instance.GetNodeByPosition(next.vertexPosition, NodeMatrix.Instance.allNodes);

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
            Node con = path.Find(x => x.vertexPosition == loopedNode.neighbourNodes[i].vertexPosition);
            if (Vector3.Distance(loopedNode.neighbourNodes[i].vertexPosition, to.vertexPosition) < dist && con == null)
            {
                dist = Vector3.Distance(loopedNode.neighbourNodes[i].vertexPosition, to.vertexPosition);

                lastNode = new Node(loopedNode.neighbourNodes[i]);
            }
        }
        if (!path.Any(l => l?.vertexPosition == lastNode?.vertexPosition))
        {
            path.Add(lastNode);
        }

        return lastNode;

    }

    public float FindDistance(Vector3 one, Vector3 two)
    {
        return Vector3.Distance(one, two);
    }

    public Node GetRandomNode()
    {
        int indexer = Random.Range(0, NodeMatrix.Instance.nodes.Count - 1);
        return NodeMatrix.Instance?.nodes?[indexer];
    }

}
