using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Axis
{
    x, y, z
}

public class Node
{
    public List<Node> neighbourNodes = new List<Node>();
    //if oppositeVertex is (0,0,0) it's missing
    public Vector3 vertexPosition, oppositeVertex;

    public Node(Node node)
    {

        this.neighbourNodes = node.neighbourNodes;
        this.vertexPosition = node.vertexPosition;
        this.oppositeVertex = node.oppositeVertex;
    }

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
    [HideInInspector]
    public static NodeMatrix Instance;
    [Header("Reposition")]
    public Vector3 positionOffest;
    [Range(1, 100)]
    public int matrixSize = 10;
    Vector3[,,] matrix;
    [Range(1, 25)]
    public int sectionLength = 5;
    [Range(1, 100)]
    public float sphereRadius = 5;
    public bool drawEdges = true, drawMidpoint = true, drawVertices = true, drawMatrixCentre = true, drawDiagonals = true, drawClosestNode = true;
    public List<Vector3> positions = new List<Vector3>();
    public List<Node> nodes = new List<Node>();
    public List<Node> midNodes = new List<Node>();
    public List<Node> allNodes = new List<Node>();
    public GameObject target;
    public Node targetsClosestNode;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        matrix = new Vector3[3, 3, 3];
        CreateMatrix();
        allNodes = nodes.Concat(midNodes).ToList();
        targetsClosestNode = FindClosestNode(target.transform.position, nodes);
    }

    void Update()
    {
        targetsClosestNode = FindClosestNode(target.transform.position, nodes);
    }

    void OnDrawGizmos()
    {
        matrix = new Vector3[3, 3, 3];
        DrawMatrix();
        Gizmos.color = new Color32(255, 165, 0, 255);
        if (allNodes.Count > 0 && drawClosestNode)
            Gizmos.DrawLine(target.transform.position, targetsClosestNode.vertexPosition);
    }

    public void DrawMatrix()
    {
        positions.Clear();
        nodes.Clear();
        for (int i = 0; i < matrix.Length; i += sectionLength)
        {
            for (int j = 0; j < matrix.Length; j += sectionLength)
            {
                for (int k = 0; k < matrix.Length; k += sectionLength)
                {
                    Gizmos.color = Color.red;
                    var nodePos = new Vector3(i - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize;

                    if (drawVertices) Gizmos.DrawSphere(nodePos, sphereRadius);
                    var lineLength = matrixSize * sectionLength;
                    var oppositeVertex = Vector3.zero;

                    if (i + sectionLength < matrix.Length && j + sectionLength < matrix.Length && k + sectionLength < matrix.Length)
                    {
                        oppositeVertex = new Vector3(i + sectionLength - positionOffest.x, j + sectionLength - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize;

                        var diag1 = new Vector3(oppositeVertex.x - sectionLength * matrixSize, oppositeVertex.y, oppositeVertex.z);
                        var diag2 = new Vector3(nodePos.x + sectionLength * matrixSize, nodePos.y, nodePos.z);
                        var midPos = (diag1 + diag2) / 2;

                        if (drawMidpoint)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(nodePos, oppositeVertex);
                            Gizmos.DrawSphere(midPos, sphereRadius * .5f);
                            if (drawDiagonals)
                            {
                                Gizmos.DrawLine(diag1, diag2);

                            }
                        }
                    }

                    List<Node> tempNodes = new List<Node>() {
                        new Node (new Vector3(i - sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i + sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j - sectionLength - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j + sectionLength - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k - sectionLength - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize)
                    };

                    if (i <= 0 || i + sectionLength <= 0 && i + sectionLength < matrix.Length - 1)
                        tempNodes[0] = null;
                    if (i > matrix.Length || i + sectionLength > matrix.Length - 1)
                        tempNodes[1] = null;
                    if (j <= 0 || j + sectionLength <= 0 && j + sectionLength < matrix.Length - 1)
                        tempNodes[2] = null;
                    if (j > matrix.Length || j + sectionLength > matrix.Length - 1)
                        tempNodes[3] = null;
                    if (k <= 0 || k + sectionLength <= 0 && k + sectionLength < matrix.Length - 1)
                        tempNodes[4] = null;
                    if (k > matrix.Length || k + sectionLength > matrix.Length - 1)
                        tempNodes[5] = null;

                    Node posNode = new Node(nodePos, oppositeVertex, tempNodes.Where(q => q != null).ToList());
                    nodes.Add(posNode);
                }

            }
        }
        if (drawEdges) DrawLines();

        //Debuggind purposes
        nodes.ForEach(n =>
        {
            positions.Add(n.vertexPosition);
        });

        var firstNode = new Vector3(nodes.Min(n => n.vertexPosition.x), nodes.Min(n => n.vertexPosition.y), nodes.Min(n => n.vertexPosition.z));
        var lastNode = new Vector3(nodes.Max(n => n.vertexPosition.x), nodes.Max(n => n.vertexPosition.y), nodes.Max(n => n.vertexPosition.z));
        var midPoint = (lastNode + firstNode) / 2;

        if (drawMatrixCentre)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(midPoint, sphereRadius * 2);
        }

    }

    public Node GetNodeByPosition(Vector3 pos)
    {
        Node node = nodes.Find(n => n.vertexPosition == pos);
        return node;
    }

    public void CreateMatrix()
    {
        positions.Clear();
        nodes.Clear();
        midNodes.Clear();
        int loops = 0;
        for (int i = 0; i < matrix.Length; i += sectionLength)
        {
            loops++;
            for (int j = 0; j < matrix.Length; j += sectionLength)
            {
                for (int k = 0; k < matrix.Length; k += sectionLength)
                {
                    var nodePos = new Vector3(i - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize;

                    var lineLength = matrixSize * sectionLength;
                    var oppositeVertex = Vector3.zero;

                    if (i + sectionLength < matrix.Length && j + sectionLength < matrix.Length && k + sectionLength < matrix.Length)
                    {
                        oppositeVertex = new Vector3(i + sectionLength - positionOffest.x, j + sectionLength - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize;
                        var diag1 = new Vector3(oppositeVertex.x - sectionLength * matrixSize, oppositeVertex.y, oppositeVertex.z);
                        var diag2 = new Vector3(nodePos.x + sectionLength * matrixSize, nodePos.y, nodePos.z);
                        var midPos = (diag1 + diag2) / 2;
                        Node mid = new Node(midPos, new Node[] { GetNodeByPosition(diag1), GetNodeByPosition(diag2), GetNodeByPosition(nodePos), GetNodeByPosition(oppositeVertex) }.ToList());

                        midNodes.Add(mid);
                    }

                    List<Node> tempNodes = new List<Node>();
                    tempNodes = FindNeighbourNodes(i, j, k);

                    Node posNode = new Node(nodePos, oppositeVertex, tempNodes.Where(q => q != null).ToList());
                    nodes.Add(posNode);
                }

            }
        }

        //Debuggind purposes
        nodes.ForEach(n =>
        {
            positions.Add(n.vertexPosition);
        });
    }

    public List<Node> FindNeighbourNodes(float i, float j, float k)
    {
        List<Node> tempNodes = new List<Node>() {
                        new Node (new Vector3(i - sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i + sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j - sectionLength - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j + sectionLength - positionOffest.y, k - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k - sectionLength - positionOffest.z) * matrixSize),
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize)
                    };

        if (i <= 0 || i + sectionLength <= 0)
            tempNodes[0] = null;
        if (i > matrix.Length || i + sectionLength > matrix.Length)
            tempNodes[1] = null;
        if (j <= 0 || j + sectionLength <= 0)
            tempNodes[2] = null;
        if (j > matrix.Length || j + sectionLength > matrix.Length)
            tempNodes[3] = null;
        if (k <= 0 || k + sectionLength <= 0)
            tempNodes[4] = null;
        if (k > matrix.Length || k + sectionLength > matrix.Length)
            tempNodes[5] = null;

        return tempNodes;
    }

    public List<Node> FindNeighbourNodes(Vector3 input)
    {
        var i = input.x;
        var j = input.y;
        var k = input.z;
        List<Node> tempNodes = new List<Node>() {
                        new Node (new Vector3(i - sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z)),
                        new Node (new Vector3(i + sectionLength - positionOffest.x, j - positionOffest.y, k - positionOffest.z)),
                        new Node (new Vector3(i - positionOffest.x, j - sectionLength - positionOffest.y, k - positionOffest.z)),
                        new Node (new Vector3(i - positionOffest.x, j + sectionLength - positionOffest.y, k - positionOffest.z)),
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k - sectionLength - positionOffest.z)),
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k + sectionLength - positionOffest.z))
                    };

        if (i <= 0 || i + sectionLength <= 0)
            tempNodes[0] = null;
        if (i > matrix.Length || i + sectionLength > matrix.Length)
            tempNodes[1] = null;
        if (j <= 0 || j + sectionLength <= 0)
            tempNodes[2] = null;
        if (j > matrix.Length || j + sectionLength > matrix.Length)
            tempNodes[3] = null;
        if (k <= 0 || k + sectionLength <= 0)
            tempNodes[4] = null;
        if (k > matrix.Length || k + sectionLength > matrix.Length)
            tempNodes[5] = null;

        return tempNodes;
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

    public Node FindClosestNode(Vector3 inputPosition)
    {
        var listX = nodes.OrderBy(n => n.vertexPosition.x).Select(s => s.vertexPosition.x).Distinct().ToList();

        float closeX = QuickSorting(listX, inputPosition.x);
        var temp = nodes.FindAll(n => n.vertexPosition.x == closeX).OrderBy(n => n.vertexPosition.x).ToList();

        var listY = temp.OrderBy(n => n.vertexPosition.y).Select(s => s.vertexPosition.y).Distinct().ToList();
        float closeY = QuickSorting(listY, inputPosition.y);

        temp = temp.FindAll(n => n.vertexPosition.y == closeY).ToList();
        var listZ = temp.OrderBy(n => n.vertexPosition.z).Select(s => s.vertexPosition.z).Distinct().ToList();

        float closeZ = QuickSorting(listZ, inputPosition.z);
        temp = temp.FindAll(n => n.vertexPosition.z == closeZ).ToList();

        return temp.Find(n => n.vertexPosition == new Vector3(closeX, closeY, closeZ));
    }
    public Node FindClosestNode(Vector3 inputPosition, List<Node> nodes)
    {
        var listX = nodes.OrderBy(n => n.vertexPosition.x).Select(s => s.vertexPosition.x).Distinct().ToList();

        float closeX = QuickSorting(listX, inputPosition.x);
        var temp = nodes.FindAll(n => n.vertexPosition.x == closeX).OrderBy(n => n.vertexPosition.x).ToList();

        var listY = temp.OrderBy(n => n.vertexPosition.y).Select(s => s.vertexPosition.y).Distinct().ToList();
        float closeY = QuickSorting(listY, inputPosition.y);

        temp = temp.FindAll(n => n.vertexPosition.y == closeY).ToList();
        var listZ = temp.OrderBy(n => n.vertexPosition.z).Select(s => s.vertexPosition.z).Distinct().ToList();

        float closeZ = QuickSorting(listZ, inputPosition.z);
        temp = temp.FindAll(n => n.vertexPosition.z == closeZ).ToList();

        var node = temp.Find(n => n.vertexPosition == new Vector3(closeX, closeY, closeZ));

        return node;

    }

    public Vector3 FindClosestVector3(Vector3 inputPosition)
    {
        return FindClosestNode(inputPosition, allNodes.OrderBy(x => x.vertexPosition.x).OrderBy(y => y.vertexPosition.y).OrderBy(z => z.vertexPosition.z).Distinct().ToList()).vertexPosition;
    }

    /// <summary>
    /// This function does a quick sorting search to find the closest element to this target in that list
    /// </summary>
    /// <param name="list">List of items to be searched through - Must be ordered</param>
    /// <param name="target">Target element</param>
    /// <typeparam name="T">Element typing</typeparam>
    /// <returns></returns>
    public float QuickSorting(List<float> list, float pivot)
    {
        pivot = Mathf.Round(pivot);
        float finalPos = 0;
        //gets midPoint, if its even, there are two. If its odd, jut the one
        var midPoint = list.Count % 2;
        var end = list.Count;

        //even midPoint - Two cases
        if (midPoint == 0)
        {
            var firstMid = (int)end / 2 - 1;
            var secondMid = (int)end / 2;

            if (end == 2)
            {
                // - 36 pivot x  - 9 pivot z
                var dist1 = pivot - list[0];
                var dist2 = list[1] - pivot;

                var dist = dist1 > dist2 ? list[1] : list[0];
                return dist;
            }

            // if by any chance the pivot is already one of the midpoints
            if (pivot == list[firstMid])
            {
                finalPos = list[firstMid];
                return finalPos;
            }
            else if (pivot == list[secondMid])
            {
                finalPos = list[secondMid];
                return finalPos;
            }
            else if (pivot < list[firstMid])
            {
                var temp = list.GetRange(0, firstMid + 1);
                finalPos = QuickSorting(temp, pivot);
            }
            else if (pivot > list[secondMid])
            {
                var temp = list.GetRange(secondMid, end - secondMid);
                finalPos = QuickSorting(temp, pivot);
            }
            else if (pivot > list[secondMid - 1] && pivot < list[firstMid + 1])
            {
                // - 36 pivot x  - 9 pivot z
                var dist1 = pivot - list[secondMid - 1];
                var dist2 = list[firstMid + 1] - pivot;

                var dist = dist1 > dist2 ? list[firstMid + 1] : list[secondMid - 1];
                return dist;
            }
        }
        else
        {
            var mid = (int)end / 2;

            if (end == 1)
            {
                finalPos = list[0];
                return finalPos;
            }

            // if by any chance the pivot is already one of the midpoints
            if (pivot == list[mid])
            {
                finalPos = list[mid];
                return finalPos;
            }
            else if (pivot < list[mid])
            {
                var temp = list.GetRange(0, mid + 1);
                finalPos = QuickSorting(temp, pivot);
            }
            else if (pivot > list[mid])
            {
                var temp = list.GetRange(mid, end - mid);
                finalPos = QuickSorting(temp, pivot);
            }
        }

        return finalPos;
    }

}

