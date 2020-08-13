using UnityEngine;
using System.Collections;

public class MeshGenerator : MonoBehaviour
{

    public SquareGrid squareGrid;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        //set square grid variable
        squareGrid = new SquareGrid(map, squareSize);
    }

    //Method that draws in the gizmos our current version of implementation
    void OnDrawGizmos()
    {
        if (squareGrid != null)
        {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                {

                    Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * .4f);


                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * .15f);

                }
            }
        }
    }

    public class SquareGrid
    {
        /***************************************************/
        //Class that represents the square grid in our mesh
        //which holds a 2d array of swquares	
        /***************************************************/

        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    //Calculate position of control node
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2,
                                               0,
                                               -mapHeight / 2 + y * squareSize + squareSize / 2);

                    //wall if its == 1 (active)
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }

        }
    }

    public class Square
    {
        /***********************************************/
        //Class that represents every Square in our mesh
        /***********************************************/

        //Reference the control nodes (edges of square)
        //and node (mid points of the squares)
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;

        //Constructor:
        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            //Set Control Nodes:
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            //Set nodes:
            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;
        }
    }

    public class Node
    {
        /***********************************************/
        //Class that represents every node in our mesh
        /***********************************************/

        public Vector3 position;    //Keep track of its position in the world
        public int vertexIndex = -1;    //Keep track of the index

        //Constructor:
        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        /***********************************************/
        //  Class that represents every 
        //  switch (control node) in our mesh
        /***********************************************/

        public bool active; //active = wall // not active = emplty tile	
        public Node above, right;

        //Constructor:
        //position is set by base constructor
        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }

    }
}