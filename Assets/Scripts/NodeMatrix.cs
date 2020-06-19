using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    public List<Node> neighbourNodes = new List<Node>();
    //if oppositeVertex is (0,0,0) it's missing
    public Vector3 vertexPosition, oppositeVertex;

    public Node(Vector3 vertexPosition, List<Node> neighbourNodes)
    {
        this.vertexPosition = vertexPosition;
        this.neighbourNodes = neighbourNodes;
    }
    public Node(Vector3 vertexPosition, Vector3 oppositeVertex, List<Node> neighbourNodes)
    {
        this.vertexPosition = vertexPosition;
        this.neighbourNodes = neighbourNodes;
        this.oppositeVertex = oppositeVertex;
    }
    public Node(Vector3 vertexPosition)
    {
        this.vertexPosition = vertexPosition;
    }

}

public class NodeMatrix : MonoBehaviour
{
    public Vector3 positionOffest;
    [Range(1, 100)]
    public int matrixSize = 10;
    Vector3[,,] matrix;
    [Range(1, 25)]
    public int sectionLength = 5;
    [Range(1, 100)]
    public float sphereRadius = 5;
    public bool drawLines = true, drawMidpoint = true, drawSpheres = true, drawWorldCentre;
    public List<Vector3> positions = new List<Vector3>();
    public List<Node> nodes = new List<Node>();
    void OnDrawGizmos()
    {
        matrix = new Vector3[3, 3, 3];
        CreateMatrix();
    }

    public void CreateMatrix()
    {
        nodes.Clear();
        positions.Clear();
        int loops = 0;
        for (int i = 0; i < matrix.Length; i += sectionLength)
        {
            loops++;
            for (int j = 0; j < matrix.Length; j += sectionLength)
            {
                for (int k = 0; k < matrix.Length; k += sectionLength)
                {
                    Gizmos.color = Color.red;
                    var nodePos = new Vector3(i - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize;

                    if (drawSpheres) Gizmos.DrawSphere(nodePos, sphereRadius);
                    var lineLength = matrixSize * sectionLength;
                    var oppositeVertex = Vector3.zero;

                    if (i + sectionLength < matrix.Length && j + sectionLength < matrix.Length && k + sectionLength < matrix.Length)
                    {
                        oppositeVertex = new Vector3(i + sectionLength - positionOffest.x, j + sectionLength - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize;
                        if (drawMidpoint)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(nodePos, oppositeVertex);
                        }
                    }

                    List<Node> tempNodes = new List<Node>();
                    if (i + sectionLength < matrix.Length)
                    {
                        Node x = new Node(new Vector3(i + sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize);
                        tempNodes.Add(x);
                    }
                    if (j + sectionLength < matrix.Length)
                    {
                        Node x = new Node(new Vector3(i - positionOffest.x, j + sectionLength - positionOffest.y, k - positionOffest.z) * matrixSize);
                        if (!tempNodes.Exists(y => y.vertexPosition == x.vertexPosition)) tempNodes.Add(x);
                    }
                    if (k + sectionLength < matrix.Length)
                    {
                        Node x = new Node(new Vector3(i - positionOffest.x, j - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize);
                        if (!tempNodes.Exists(y => y.vertexPosition == x.vertexPosition)) tempNodes.Add(x);
                    }

                    Node posNode = new Node(nodePos, oppositeVertex, tempNodes);
                    nodes.Add(posNode);
                }

            }
        }
        if (drawLines) DrawLines();

        //Debuggind purposes
        nodes.ForEach(n =>
        {
            positions.Add(n.vertexPosition);
        });

        if (drawWorldCentre)
        {
            Gizmos.color = Color.yellow;
            var firstNode = new Vector3(nodes.Min(n => n.vertexPosition.x), nodes.Min(n => n.vertexPosition.y), nodes.Min(n => n.vertexPosition.z));
            var lastNode = new Vector3(nodes.Max(n => n.vertexPosition.x), nodes.Max(n => n.vertexPosition.y), nodes.Max(n => n.vertexPosition.z));
            Gizmos.DrawSphere((lastNode + firstNode) / 2, sphereRadius * 2);
        }
    }

    public void DrawLines()
    {
        Gizmos.color = Color.blue;
        nodes.ForEach(node =>
        {
            node.neighbourNodes.ForEach(neighbour =>
            {
                Gizmos.DrawLine(node.vertexPosition, neighbour.vertexPosition);
            });
        });
    }

}