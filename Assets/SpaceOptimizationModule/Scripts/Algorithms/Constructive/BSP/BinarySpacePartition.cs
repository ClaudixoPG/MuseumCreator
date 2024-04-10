using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using static PCG.Utilities;

//REFS
// https://gamedevelopment.tutsplus.com/how-to-use-bsp-trees-to-generate-game-maps--gamedev-12268t
// https://medium.com/@guribemontero/dungeon-generation-using-binary-space-trees-47d4a668e2d0
// https://bfnightly.bracketproductions.com/chapter_25.html
// 

namespace PCG
{
    public class BinarySpacePartition : MonoBehaviour
    {
        //Implementation of Binary Space Partitioning
        //https://en.wikipedia.org/wiki/Binary_space_partitioning

        public int width = 20;
        public int height = 20;
        public int maximalRoomSize = 5;
        //public int minimalRoomSize = 2;
        public Vector2Int widthRoom;
        public Vector2Int heightRoom;

        //To restart binary partition
        public List<GameObject> containers;

        private void Start()
        {
            if (widthRoom.x > maximalRoomSize) widthRoom.x = maximalRoomSize/2;
            if (widthRoom.y > maximalRoomSize) widthRoom.y = maximalRoomSize;
            if (heightRoom.x > maximalRoomSize) heightRoom.x = maximalRoomSize/2;
            if (heightRoom.y > maximalRoomSize) heightRoom.y = maximalRoomSize;
        }

        public void Build() {

            RestartContainers();

            var matrix = new int[width, height];
            
            //Initialize matrix
            InitializeMatrix(matrix);

            //Create root Node
            Node root = new Node(new Tuple<int, int>(0,0), new Tuple<int, int>(matrix.GetLength(0) -1 , matrix.GetLength(1) - 1 ));
            Split(matrix, root);
            CreateHallway(matrix, root);
            
            //Write Matrix to file in var path = Application.dataPath + "/SpaceOptimizationModule/Resources/Maps/" + filename + ".txt";
            string filename = "BSP";
            string path = Application.dataPath + "/SpaceOptimizationModule/Resources/Maps/" + filename + ".txt";
            WriteMatrixToFile(matrix, path);
        }

        void RestartContainers()
        {
            foreach(GameObject container in containers)
            {
                foreach(Transform child in container.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        void WriteMatrixToFile(int[,] matrix, string path)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            StringBuilder sb = new StringBuilder();
            /// Append width and height to the first row
            sb.Append(rows + " " + cols + "\n");
            
            //Append matrix to the string builder, each row separated by a new line
            for (int i = 0; i < rows; i++)
            {
                string row = "";
                for (int j = 0; j < cols; j++)
                {
                    row += matrix[i, j] + " ";
                }
                sb.Append(row + "\n");
            }

            //Write string builder to file
            File.WriteAllText(path, sb.ToString());
        }
        void InitializeMatrix(int[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++) 
                {
                    matrix[i, j] = 0;
                }
            }
        }
        void Split(int[,] matrix, Node root){//, splitDirection = SplitDirection.Default){

            var width = root.EndPoint.Item1 - root.InitPoint.Item1;
            var height = root.EndPoint.Item2 - root.InitPoint.Item2;

            if(width > maximalRoomSize * 2 && height > maximalRoomSize * 2)
            {
                //Get random split direction
                var splitDirection = (SplitDirection)UnityEngine.Random.Range(0, 2);

                if(splitDirection == SplitDirection.Horizontal)
                {
                    SplitHorizontally(matrix, root);
                }
                else
                {
                    SplitVertically(matrix, root);
                }
            }else if(width<= maximalRoomSize * 2 && height > maximalRoomSize * 2)
            {
                SplitHorizontally(matrix, root);
            }else if(width > maximalRoomSize * 2 && height <= maximalRoomSize * 2)
            {
                SplitVertically(matrix, root);
            }
            else
            {
                CreateRoom(matrix, root);
                return;
            }
        }
        void CreateRoom(int[,] matrix, Node root)
        {
            Node node = root;

            //If node is leaf
            if(node.LeftChild == null && node.RightChild == null)
            {
                //set room size 
                int roomWidth = UnityEngine.Random.Range(widthRoom.x, widthRoom.y);
                int roomHeight = UnityEngine.Random.Range(heightRoom.x, heightRoom.y);

                //set room position in random position in node area, check that room is inside node boundaries
                int roomInitX = UnityEngine.Random.Range(node.InitPoint.Item1, node.EndPoint.Item1 - roomWidth);
                int roomInitY = UnityEngine.Random.Range(node.InitPoint.Item2, node.EndPoint.Item2 - roomHeight);

                //Preguntar a Nico porqué no se el límite superior está capeado a un valor menor que el que debería ser (las habitaciones no llegan al final del nodo)

                //Create room
                Room room = new Room(new Tuple<int, int>(roomInitX, roomInitY), new Tuple<int, int>(roomInitX + roomWidth, roomInitY + roomHeight));
                node.Room = room;

                //Fill matrix with room coordinates
                foreach (var coordinate in room.RoomCoordinates())
                {
                    matrix[coordinate.Item1, coordinate.Item2] = 2;
                }
            }
        }
        void CreateHallway(int[,] matrix, Node root)
        {
            if (root.LeftChild == null && root.RightChild == null) return;

            //Debug.Log("root direction: " + root.SplitDirection + " root init: " + root.InitPoint + " root end: " + root.EndPoint);

            if (root.SplitDirection == SplitDirection.Horizontal) VerticalCheck(matrix,root);
            else if (root.SplitDirection == SplitDirection.Vertical) HorizontalCheck(matrix,root);
            //if (root.SplitDirection == SplitDirection.Vertical) HorizontalCheck(matrix, root);
        }
        //Create Hallway Class and Constructor
        public class Hallway
        {
            public List<Tuple<int, int>> Coordinates { get; set; }

            public Hallway(List<Tuple<int, int>> coordinates)
            {
                Coordinates = coordinates;
            }
        }

        void HorizontalCheck(int[,] matrix, Node root)
        {
            int firstQuadrant = 0;//root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 4 - 1;
            //int middlePoint = root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 2;
            int thirdQuadrant = 0;//root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) * 3 / 4 + 1;

            int cutPoint = root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 2;
            //int firstQuadrant = 0;
            //int thirdQuadrant = 0;

            var auxList = new List<Hallway>();

            //check vertical path from first quadrant to third quadrant
            for (int i = root.InitPoint.Item2; i < root.EndPoint.Item2; i++)
            {
                for (int j = cutPoint-1; j >= root.InitPoint.Item1; j--)
                {
                    if (matrix[j, i] == 2)
                    {
                        firstQuadrant = j;
                        break;
                    }
                }
                for (int j = cutPoint; j <= root.EndPoint.Item1; j++)
                {
                    if (matrix[j, i] == 2)
                    {
                        thirdQuadrant = j;
                        break;
                    }
                }

                if (matrix[firstQuadrant, i] == 2 && matrix[thirdQuadrant, i] == 2)
                {
                    var coordinates = new List<Tuple<int, int>>();
                    for (int j = firstQuadrant; j <= thirdQuadrant; j++)
                    {
                        coordinates.Add(new Tuple<int, int>(j, i));
                    }
                    auxList.Add(new Hallway(coordinates));
                }else if (auxList.Count > 0)
                {
                    //select random hallway from aux list, change coordinates in matrix and clear aux list
                    int randomIndex = UnityEngine.Random.Range(0, auxList.Count);
                    var hallway = auxList[randomIndex];
                    foreach (var coordinate in hallway.Coordinates)
                    {
                        matrix[coordinate.Item1, coordinate.Item2] = 2;
                    }
                    auxList.Clear();
                }
            }

            ///////////////////////////////
            
            CreateHallway(matrix,root.LeftChild);
            CreateHallway(matrix,root.RightChild);
        }
        
        void VerticalCheck(int [,] matrix, Node root)
        {
            int firstQuadrant = root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 4 - 1;
            //int middlePoint = root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 2;
            int thirdQuadrant = root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) * 3 / 4 + 1;
            int cutPoint = root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 2;

            var auxList = new List<Hallway>();

            //check horizontal path from first quadrant to third quadrant
            for(int i  = root.InitPoint.Item1; i < root.EndPoint.Item1; i++)
            {
                for (int j = cutPoint - 1; j >= root.InitPoint.Item2; j--)
                {
                    if (matrix[i,j] == 2)
                    {
                        firstQuadrant = j;
                        break;
                    }
                }
                for (int j = cutPoint; j <= root.EndPoint.Item2; j++)
                {
                    if (matrix[i,j] == 2)
                    {
                        thirdQuadrant = j;
                        break;
                    }
                }

                if (matrix[i, firstQuadrant] == 2 && matrix[i, thirdQuadrant] == 2)
                {
                    var coordinates = new List<Tuple<int, int>>();
                    for(int j = firstQuadrant; j < thirdQuadrant; j++)
                    {
                        coordinates.Add(new Tuple<int, int>(i, j));
                    }
                    auxList.Add(new Hallway(coordinates));
                }else if(auxList.Count > 0)
                {
                    //select random hallway from aux list, change coordinates in matrix and clear aux list
                    int randomIndex = UnityEngine.Random.Range(0, auxList.Count);
                    var hallway = auxList[randomIndex];
                    foreach(var coordinate in hallway.Coordinates)
                    {
                        matrix[coordinate.Item1, coordinate.Item2] = 2;
                    }
                    auxList.Clear();
                }
            }

            ///////////////////////////////

            CreateHallway(matrix,root.LeftChild);
            CreateHallway(matrix,root.RightChild);
        }


        void SplitVertically(int[,] matrix, Node root)
        {
            root.SplitDirection = SplitDirection.Vertical;
            
            //Get middle split point
            int splitPoint = ((root.EndPoint.Item1 - root.InitPoint.Item1) / 2) + root.InitPoint.Item1;

            //Get random split point in matrix, check that it is possible have a room of minimal size in both sides
            //int splitPoint = root.InitPoint.Item1 + UnityEngine.Random.Range(root.InitPoint.Item1 + minimalRoomSize, root.EndPoint.Item1 - minimalRoomSize);

            //Create left child
            Node leftChild = new Node(root.InitPoint, new Tuple<int, int>(splitPoint-1, root.EndPoint.Item2));

            //Create right child
            Node rightChild = new Node(new Tuple<int, int>(splitPoint, root.InitPoint.Item2), root.EndPoint);

            //Assign left child to root and split it
            root.LeftChild = leftChild;
            Split(matrix, leftChild);

            //Assign right child to root and split it
            root.RightChild = rightChild;
            Split(matrix, rightChild);
        }

        void SplitHorizontally(int[,] matrix, Node root)
        {
            root.SplitDirection = SplitDirection.Horizontal;

            //Get middle split point
            int splitPoint = ((root.EndPoint.Item2 - root.InitPoint.Item2) / 2) + root.InitPoint.Item2;

            //Get random split point in matrix, check that it is possible have a room of minimal size in both sides
            //int splitPoint = root.InitPoint.Item2 + UnityEngine.Random.Range(root.InitPoint.Item2 + minimalRoomSize, root.EndPoint.Item2 - minimalRoomSize);

            //Create left child
            Node leftChild = new Node(root.InitPoint, new Tuple<int, int>(root.EndPoint.Item1, splitPoint-1));

            //Create right child
            Node rightChild = new Node(new Tuple<int, int>(root.InitPoint.Item1, splitPoint), root.EndPoint);

            //Assign left child to root and split it
            root.LeftChild = leftChild;
            Split(matrix, leftChild);

            //Assign right child to root and split it
            root.RightChild = rightChild;
            Split(matrix, rightChild);
        }

    }

}