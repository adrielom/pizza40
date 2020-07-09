using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Axis Enumerator
/// </summary>
public enum Axis
{
    x, y, z
}

/// <summary>
/// Node class - Parent class to all nodes
/// </summary>
public class Node
{
    /// <summary>
    /// Neighbour nodes - connected tree list
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <returns></returns>
    public List<Node> neighbourNodes = new List<Node>();
    //if oppositeVertex is (0,0,0) it's missing

    ///Vertex position and opposite Vertex
    public Vector3 vertexPosition, oppositeVertex;

    public Node(Node node)
    {
        this.vertexPosition = node.vertexPosition;
        this.oppositeVertex = node.oppositeVertex;
        this.neighbourNodes = node.neighbourNodes;
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

/// <summary>
/// Mid points of a cube section - Class inherits from Node
/// </summary>
public class MidNode : Node
{
    public MidNode(Node node) : base(node)
    {

    }

    public MidNode(Vector3 vertexPosition) : base(vertexPosition)
    {
        this.vertexPosition = vertexPosition;
    }
    public MidNode(Vector3 vertexPosition, List<Node> corners) : base(vertexPosition)
    {
        this.vertexPosition = vertexPosition;
        this.neighbourNodes = corners;
    }
}

/// <summary>
/// Node Matrix - Creates the matrix, its drawing behaviours and its searching methods
/// </summary>
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
    public List<MidNode> midNodes = new List<MidNode>();
    public List<Node> allNodes = new List<Node>();
    public GameObject target;
    public Node targetsClosestNode;

    void Awake()
    {
        //Singleton
        if (Instance == null)
            Instance = this;
        //Start up a 3D matrix
        matrix = new Vector3[3, 3, 3];
        //Populate the matrix with nodes
        CreateMatrix();
        //Gets all nodes as a list format
        allNodes = nodes.Concat(midNodes).ToList();
        //Find the closest node to a Target
        targetsClosestNode = FindClosestNode(target.transform.position, allNodes);

    }

    void Update()
    {

        //Find the closest node to a Target
        targetsClosestNode = FindClosestNode(target.transform.position, allNodes);
    }

    /// <summary>
    /// Method draws the gizmos relative to each Node - Vertices, Edges and Midpoints
    /// </summary>
    void OnDrawGizmos()
    {
        //Another matrix is created
        matrix = new Vector3[3, 3, 3];
        //Draws in the points
        DrawMatrix();
        //Draws the distance from the target to its closest node
        Gizmos.color = new Color32(255, 165, 0, 255);
        if (allNodes.Count > 0 && drawClosestNode)
            Gizmos.DrawLine(target.transform.position, targetsClosestNode.vertexPosition);
    }

    /// <summary>
    /// Method responsible for debuging the gizmos visually, drawing the matrix
    /// </summary>
    public void DrawMatrix()
    {
        //Clear the list of position - Debugging - and the nodes.
        positions.Clear();
        nodes.Clear();
        //loops through the matrix
        for (int i = 0; i < matrix.Length; i += sectionLength)
        {
            for (int j = 0; j < matrix.Length; j += sectionLength)
            {
                for (int k = 0; k < matrix.Length; k += sectionLength)
                {
                    //Sets a color and a starting position
                    Gizmos.color = Color.red;
                    var nodePos = new Vector3(i - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize;

                    //Draws a sphere for each vertex
                    if (drawVertices) Gizmos.DrawSphere(nodePos, sphereRadius);
                    //Size of the edge
                    var lineLength = matrixSize * sectionLength;
                    //initializes the oppositeVertex
                    var oppositeVertex = Vector3.zero;

                    //Checks if the section determined from this node position to the next node is contained withing the boundaries of the matrix
                    if (i + sectionLength < matrix.Length && j + sectionLength < matrix.Length && k + sectionLength < matrix.Length)
                    {
                        //sets the opposite vertex
                        oppositeVertex = new Vector3(i + sectionLength - positionOffest.x, j + sectionLength - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize;

                        //gets the other 2 missing vertices that make up the section
                        var diag1 = new Vector3(oppositeVertex.x - sectionLength * matrixSize, oppositeVertex.y, oppositeVertex.z);
                        var diag2 = new Vector3(nodePos.x + sectionLength * matrixSize, nodePos.y, nodePos.z);
                        //gets section midpoint
                        var midPos = (diag1 + diag2) / 2;

                        //Based on the Inspector values, displays the edges and midpoints for each node
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

                    //Gets a list of neighbour nodes to perform the right search algorithm
                    List<Node> tempNodes = FindNeighbourNodes(nodePos);
                    //Creates the node and populate its neighbours
                    Node posNode = new Node(nodePos, oppositeVertex, tempNodes.Where(q => q != null).ToList());
                    //adds it to the nodes list
                    nodes.Add(posNode);
                }

            }
        }
        //Draws edges
        if (drawEdges) DrawLines();

        //Debuggind purposes - Shows up in inspector
        nodes.ForEach(n =>
        {
            positions.Add(n.vertexPosition);
        });

        //Draws matrix center - Gets first and last node of matrix and gets the midpoint of that diagonal
        var firstNode = new Vector3(nodes.Min(n => n.vertexPosition.x), nodes.Min(n => n.vertexPosition.y), nodes.Min(n => n.vertexPosition.z));
        var lastNode = new Vector3(nodes.Max(n => n.vertexPosition.x), nodes.Max(n => n.vertexPosition.y), nodes.Max(n => n.vertexPosition.z));
        var midPoint = (lastNode + firstNode) / 2;

        if (drawMatrixCentre)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(midPoint, sphereRadius * 2);
        }

    }

    /// <summary>
    /// Gets the node found at the pos position
    /// </summary>
    /// <param name="pos">Searched Position</param>
    /// <returns>Returns the right node, else returns null</returns>
    public Node GetNodeByPosition(Vector3 pos)
    {
        Node node = nodes.Find(n => n.vertexPosition == pos);
        return node;
    }

    /// <summary>
    /// Gets the node found at the pos position
    /// </summary>
    /// <param name="pos">Searched Position</param>
    /// <param name="nodes">List to be searched</param>
    /// <returns>Returns the right node, else returns null</returns>
    public Node GetNodeByPosition(Vector3 pos, List<Node> nodes)
    {
        Node node = nodes.Find(n => n.vertexPosition == pos);
        return node;
    }

    /// <summary>
    /// Creates the actual matrix
    /// </summary>
    public void CreateMatrix()
    {
        //Clear the lists of position - Debugging - nodes and midpoints.
        positions.Clear();
        nodes.Clear();
        midNodes.Clear();
        //loops matrix
        for (int i = 0; i < matrix.Length; i += sectionLength)
        {
            for (int j = 0; j < matrix.Length; j += sectionLength)
            {
                for (int k = 0; k < matrix.Length; k += sectionLength)
                {
                    //This node's iteration position
                    var nodePos = new Vector3(i - positionOffest.x, j - positionOffest.y, k - positionOffest.z) * matrixSize;

                    var lineLength = matrixSize * sectionLength;
                    var oppositeVertex = Vector3.zero;

                    //Checks if the section determined from this node position to the next node is contained withing the boundaries of the matrix
                    if (i + sectionLength < matrix.Length && j + sectionLength < matrix.Length && k + sectionLength < matrix.Length)
                    {
                        oppositeVertex = new Vector3(i + sectionLength - positionOffest.x, j + sectionLength - positionOffest.y, k + sectionLength - positionOffest.z) * matrixSize;
                        var diag1 = new Vector3(oppositeVertex.x - sectionLength * matrixSize, oppositeVertex.y, oppositeVertex.z);
                        var diag2 = new Vector3(nodePos.x + sectionLength * matrixSize, nodePos.y, nodePos.z);
                        var midPos = (diag1 + diag2) / 2;

                        //Creates a midnode object
                        MidNode mid = new MidNode(midPos);
                        var tem = FindNeighbourMidNodes(mid);
                        //Inicializes the object's neighbour nodes
                        mid.neighbourNodes = tem;
                        //Adds it to the list
                        midNodes.Add(mid);
                    }

                    //Gets the neighbour nodes of this iteration's node
                    List<Node> tempNodes = new List<Node>();
                    tempNodes = FindNeighbourNodes(nodePos);
                    //Creates the actual node and adds it to the list of nodes
                    Node posNode = new Node(nodePos, oppositeVertex, tempNodes.Where(q => q != null).ToList());
                    nodes.Add(posNode);
                }

            }
        }

    }

    /// <summary>
    /// Finds the Neighbour nodes of a certain position
    /// </summary>
    /// <param name="i">x axis position</param>
    /// <param name="j">y axis position</param>
    /// <param name="k">z axis position</param>
    /// <returns>Returns a list of nodes</returns>
    public List<Node> FindNeighbourNodes(float i, float j, float k)
    {
        //Every node has a fixed amount of starting nodes - 6. Two for each axis.
        List<Node> tempNodes = new List<Node>() {
                        //Grows to the left on the x axis
                        new Node (new Vector3(i - sectionLength * matrixSize - positionOffest.x, j - positionOffest.y, k - positionOffest.z)),
                        //Grows to the right on the x axis
                        new Node (new Vector3(i + sectionLength * matrixSize - positionOffest.x, j - positionOffest.y, k - positionOffest.z)),
                        //Grows to the left on the y axis
                        new Node (new Vector3(i - positionOffest.x, j - sectionLength * matrixSize - positionOffest.y, k - positionOffest.z)),
                        //Grows to the right on the y axis
                        new Node (new Vector3(i - positionOffest.x, j + sectionLength * matrixSize - positionOffest.y, k - positionOffest.z)),
                        //Grows to the left on the z axis
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k - sectionLength * matrixSize - positionOffest.z)),
                        //Grows to the right on the z axis
                        new Node (new Vector3(i - positionOffest.x, j - positionOffest.y, k + sectionLength * matrixSize - positionOffest.z))
                    };

        //According to its inicial position, some of the nodes don't have certain neighbour nodes. They go past beyond the bounds of the matrix, so they have to be removed from the list
        if (i <= 0)
            tempNodes[0] = null;
        if (i > sectionLength * matrixSize)
            tempNodes[1] = null;
        if (j <= 0)
            tempNodes[2] = null;
        if (j > sectionLength * matrixSize)
            tempNodes[3] = null;
        if (k <= 0)
            tempNodes[4] = null;
        if (k > sectionLength * matrixSize)
            tempNodes[5] = null;

        return tempNodes.Where(t => t != null).ToList();
    }

    /// <summary>
    /// Finds all the neighbour nodes of a mid Node
    /// </summary>
    /// <param name="mid">Mid Node</param>
    /// <returns>Returns a list of neighbour nodes</returns>
    public List<Node> FindNeighbourMidNodes(MidNode mid)
    {
        //Half of the section length
        float halfLength = Mathf.Floor(sectionLength * matrixSize / 2);
        //Actual length
        float length = sectionLength;
        //the vector3 input
        Vector3 input = mid.vertexPosition;

        //Every node has a fixed amount of starting nodes - 8. Four vertices on top and four on the bottom.
        List<Node> tempNodes = new List<Node>() {
                        new Node (new Vector3(input.x - halfLength - positionOffest.x, input.y + halfLength - positionOffest.y, input.z - halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x - halfLength - positionOffest.x, input.y + halfLength - positionOffest.y, input.z + halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x + halfLength - positionOffest.x, input.y + halfLength - positionOffest.y, input.z - halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x + halfLength - positionOffest.x, input.y + halfLength - positionOffest.y, input.z + halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x - halfLength - positionOffest.x, input.y - halfLength - positionOffest.y, input.z - halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x - halfLength - positionOffest.x, input.y - halfLength - positionOffest.y, input.z + halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x + halfLength - positionOffest.x, input.y - halfLength - positionOffest.y, input.z - halfLength - positionOffest.z)),
                        new Node (new Vector3(input.x + halfLength - positionOffest.x, input.y - halfLength - positionOffest.y, input.z + halfLength - positionOffest.z)),

                    };

        //For each element different than null in the list, add the midnode to it - The tempnode are the midnode neighbours, this code makes the back relation
        tempNodes.Where(t => t != null).ToList().ForEach(x =>
        {
            List<Node> ne = FindNeighbourNodes(x.vertexPosition);
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.transform.position = x.vertexPosition;
            x.neighbourNodes.Clear();
            x.neighbourNodes = ne;
            x.neighbourNodes.Add(mid);
        });

        return tempNodes.Where(t => t != null).ToList();
    }

    /// <summary>
    /// Finds the Neighbour nodes of a certain position
    /// </summary>
    /// <param name="input">position</param>
    /// <returns>Returns a list of nodes</returns>
    public List<Node> FindNeighbourNodes(Vector3 input)
    {
        var i = input.x;
        var j = input.y;
        var k = input.z;
        return FindNeighbourNodes(i, j, k);
    }

    /// <summary>
    /// Draws an gizmo line from every node in the matrix to its neighbours
    /// </summary>
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

    /// <summary>
    /// Finds closest node given an specific input position 
    /// This method may be biased because it searches x, y, z axis.
    /// </summary>
    /// <param name="inputPosition">Vector3 position</param>
    /// <param name="nodes">List of nodes to search on</param>
    /// <returns>return closest Node</returns>
    public Node FindClosestNode(Vector3 inputPosition, List<Node> nodes)
    {
        //Ordering the values here is crucial due the quicksort algorithm applied afterwards
        //Gets the distinct elements of said node list ordering by x axis value
        var listX = nodes.OrderBy(n => n.vertexPosition.x).Select(s => s.vertexPosition.x).Distinct().ToList();

        //gets closest X position
        float closeX = QuickSorting(listX, inputPosition.x);
        //Gets all the elements in the list that has the x value equals to the close X variable found
        var temp = nodes.FindAll(n => n.vertexPosition.x == closeX).OrderBy(n => n.vertexPosition.x).ToList();

        //Gets the distinct elements of said node list ordering by y axis value - Already filtered on x
        var listY = temp.OrderBy(n => n.vertexPosition.y).Select(s => s.vertexPosition.y).Distinct().ToList();
        //gets closest Y position
        float closeY = QuickSorting(listY, inputPosition.y);

        //Gets all the elements in the list that has the y value equals to the close Y variable found
        temp = temp.FindAll(n => n.vertexPosition.y == closeY).ToList();
        //Gets the distinct elements of said node list ordering by z axis value - Already filtered on x and y
        var listZ = temp.OrderBy(n => n.vertexPosition.z).Select(s => s.vertexPosition.z).Distinct().ToList();

        //gets closest Z position
        float closeZ = QuickSorting(listZ, inputPosition.z);
        //Gets all the elements in the list that has the z value equals to the close Z variable found
        temp = temp.FindAll(n => n.vertexPosition.z == closeZ).ToList();

        //Finds in the filtered list a node element that has the vertex position equals to the close variables found
        var node = temp.Find(n => n.vertexPosition == new Vector3(closeX, closeY, closeZ));

        return node;

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
        //Rounds up or down the pivot value
        pivot = Mathf.Round(pivot);
        float finalPos = 0;
        //gets midPoint, if its even, there are two. If its odd, jut the one
        var midPoint = list.Count % 2;
        var end = list.Count;

        //even midPoint - Two cases
        if (midPoint == 0)
        {
            //last element of first section
            var firstMid = (int)end / 2 - 1;
            // first element of second section
            var secondMid = (int)end / 2;

            //there are only two elements left in the list - it's either one of them
            if (end == 2)
            {
                // finds distance of each one from the pivot point
                var dist1 = pivot - list[0];
                var dist2 = list[1] - pivot;

                //returns the one with smaller distance
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
            //pivot point is less than the last element of first section
            else if (pivot < list[firstMid])
            {
                //breaks the first section into a new list and calls in the function recursivelly based on the new list
                var temp = list.GetRange(0, firstMid + 1);
                finalPos = QuickSorting(temp, pivot);
            }
            //pivot point is bigger than the last element of first section
            else if (pivot > list[secondMid])
            {
                //breaks the second section into a new list and calls in the function recursivelly based on the new list
                var temp = list.GetRange(secondMid, end - secondMid);
                finalPos = QuickSorting(temp, pivot);
            }
            //pivot point between the two mid points of the section - Just two to be searched
            else if (pivot > list[secondMid - 1] && pivot < list[firstMid + 1])
            {
                // finds distance of each one from the pivot point
                var dist1 = pivot - list[secondMid - 1];
                var dist2 = list[firstMid + 1] - pivot;

                //returns the one with smaller distance
                var dist = dist1 > dist2 ? list[firstMid + 1] : list[secondMid - 1];
                return dist;
            }
        }
        //odd midPoint
        else
        {
            //gets the middle of array
            var mid = (int)end / 2;

            //if there's only one element in the array, returns it
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
            // pivot point is less than the middle point
            else if (pivot < list[mid])
            {
                //new list from - first section
                var temp = list.GetRange(0, mid + 1);
                //Recursivly calling the function
                finalPos = QuickSorting(temp, pivot);
            }
            // pivot point is bigger than the middle point
            else if (pivot > list[mid])
            {
                //new list from - first section
                var temp = list.GetRange(mid, end - mid);
                //Recursivly calling the function
                finalPos = QuickSorting(temp, pivot);
            }
        }

        //returns final element
        return finalPos;
    }

}

