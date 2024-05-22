using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace SpaceOptimization.BSP
{
    public class BSP_MazeGeneration : MonoBehaviour
    {
        private int width = 20;
        private int height = 20;
        private int maximalRoomSize = 5;

        public MapCreator mapCreator;

        //To restart binary partition
        public List<GameObject> containers;
        public enum SplitDirection
        {
            Horizontal,
            Vertical,
            Default
        }

        private int[,] matrix;

        //get matrix
        public int[,] GetMatrix()
        {
            return matrix;
        }


        public void CreateMaze(int width, int height, int maximalRoomSize, string filename = "Maze")
        {
            //restart containers
            RestartContainers();

            //Debug Partition
            Debug.Log("Creating Maze Map");
            this.width = width;
            this.height = height;
            this.maximalRoomSize = maximalRoomSize;

            Build(filename);
            matrix = mapCreator.Creator(filename);
            mapCreator.SetNeighbors(matrix);
            //width = matrix.GetLength(0);
            //height = matrix.GetLength(1);
        }

        public void Build(string filename = "Maze")
        {
            RestartContainers();

            var matrix = new int[width, height];

            //Initialize matrix
            InitializeMatrix(matrix);

            //Create root Node
            Node root = new Node(new Tuple<int, int>(0, 0), new Tuple<int, int>(matrix.GetLength(0) - 1, matrix.GetLength(1) - 1));
            Split(matrix, root);
            //CreateHallway(matrix, root);
            //RandomDoors(matrix);
            IntercalatedDoors(matrix);
            //Write Matrix to file in var path = Application.dataPath + "/SpaceOptimization/Resources/Maps/" + filename + ".txt";
            string path = Application.dataPath + "/SpaceOptimization/Resources/Maps/" + filename + ".txt";
            WriteMatrixToFile(matrix, path);
        }
        void Split(int[,] matrix, Node root)
        {//, splitDirection = SplitDirection.Default){

            var width = root.EndPoint.Item1 - root.InitPoint.Item1;
            var height = root.EndPoint.Item2 - root.InitPoint.Item2;

            if (width > maximalRoomSize * 2 && height > maximalRoomSize * 2)
            {
                //Get random split direction
                var splitDirection = (SplitDirection)UnityEngine.Random.Range(0, 2);

                if (splitDirection == SplitDirection.Horizontal)
                {
                    SplitHorizontally(matrix, root);
                }
                else
                {
                    SplitVertically(matrix, root);
                }
            }
            else if (width <= maximalRoomSize * 2 && height > maximalRoomSize * 2)
            {
                SplitHorizontally(matrix, root);
            }
            else if (width > maximalRoomSize * 2 && height <= maximalRoomSize * 2)
            {
                SplitVertically(matrix, root);
            }
            /*else
            {
                CreateRoom(matrix, root);
                return;
            }*/
        }
        void SplitVertically(int[,] matrix, Node root)
        {
            root.SplitDirection = SplitDirection.Vertical;

            //Get middle split point
            //int splitPoint = ((root.EndPoint.Item1 - root.InitPoint.Item1) / 2) + root.InitPoint.Item1;

            int distance = root.EndPoint.Item1 - root.InitPoint.Item1;
            int randomSplitPoint = UnityEngine.Random.Range(maximalRoomSize, distance-maximalRoomSize);

            if(randomSplitPoint < maximalRoomSize)
            {
                return;
            }
            
            int splitPoint = root.InitPoint.Item1 + randomSplitPoint;

            //Get random split point in matrix, check that it is possible have a room of minimal size in both sides
            //int splitPoint = root.InitPoint.Item1 + UnityEngine.Random.Range(root.InitPoint.Item1 + minimalRoomSize, root.EndPoint.Item1 - minimalRoomSize);

            

            //Change the value of each cell in the split point of the matrix to 1 (wall)
            for (int i = root.InitPoint.Item2; i <= root.EndPoint.Item2; i++)
            {
                //create 2 doors in the wall, one in the first quarter and another in the third quarter
                //if (i == root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 4 || i == root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) * 3 / 4)
                /*if (i == root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 2)
                {
                    //i = random position between the first and third quarter
                    var randomPoint = UnityEngine.Random.Range(0, (root.EndPoint.Item2 - root.InitPoint.Item2));
                    i = randomPoint;

                    //check left, right, up and down cells, if only one is a wall, create a door between them
                    if (matrix[splitPoint, i - 1] == 1 && matrix[splitPoint, i + 1] == 2 && matrix[splitPoint - 1, i] == 2 && matrix[splitPoint + 1, i] == 2)
                    {
                        matrix[splitPoint, i] = 3;
                        matrix[splitPoint + 1, i] = 3;
                    }
                    else if (matrix[splitPoint, i + 1] == 1 && matrix[splitPoint, i - 1] == 2 && matrix[splitPoint - 1, i] == 2 && matrix[splitPoint + 1, i] == 2)
                    {
                        matrix[splitPoint, i] = 3;
                        matrix[splitPoint + 1, i] = 3;
                    }
                    else if (matrix[splitPoint - 1, i] == 1 && matrix[splitPoint + 1, i] == 2 && matrix[splitPoint, i - 1] == 2 && matrix[splitPoint, i + 1] == 2)
                    {
                        matrix[splitPoint, i] = 3;
                        matrix[splitPoint + 1, i] = 3;
                    }
                    else if (matrix[splitPoint + 1, i] == 1 && matrix[splitPoint - 1, i] == 2 && matrix[splitPoint, i - 1] == 2 && matrix[splitPoint, i + 1] == 2)
                    {
                        matrix[splitPoint, i] = 3;
                        matrix[splitPoint + 1, i] = 3;
                    }
                    else
                    {
                        matrix[splitPoint, i] = 1;
                        matrix[splitPoint + 1, i] = 1;

                        //move the iteration to the left to create a door
                        matrix[splitPoint, i-1] = 3;
                        matrix[splitPoint + 1, i - 1] = 3;

                    }
                }
                else*/
                {
                    matrix[splitPoint, i] = 1;
                    matrix[splitPoint + 1, i] = 1;
                }

                //matrix[splitPoint, i] = 1;
                //matrix[splitPoint + 1, i] = 1;
            }

            //Create left child
            Node leftChild = new Node(root.InitPoint, new Tuple<int, int>(splitPoint - 1, root.EndPoint.Item2));

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
            //int splitPoint = ((root.EndPoint.Item2 - root.InitPoint.Item2) / 2) + root.InitPoint.Item2;

            int distance = root.EndPoint.Item2 - root.InitPoint.Item2;

            int randomSplitPoint = UnityEngine.Random.Range(maximalRoomSize, distance-maximalRoomSize);

            if(randomSplitPoint < maximalRoomSize)
            {
                return;
            }

            int splitPoint = root.InitPoint.Item2 + randomSplitPoint;

            //Get random split point in matrix, check that it is possible have a room of minimal size in both sides
            //int splitPoint = root.InitPoint.Item2 + UnityEngine.Random.Range(root.InitPoint.Item2 + minimalRoomSize, root.EndPoint.Item2 - minimalRoomSize);

            //Change the value of each cell in the split point of the matrix to 1 (wall)
            for (int i = root.InitPoint.Item1; i <= root.EndPoint.Item1; i++)
            {
                //create 2 doors in the wall, one in the first quarter and another in the third quarter
                /*if (i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 4 || i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) * 3 / 4)
                //if(i ==root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 2)
                {
                    matrix[i, splitPoint] = 3;
                    matrix[i, splitPoint + 1] = 3;
                }
                else*/
                /*if(i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 2)
                //if (i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 4 || i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) * 3 / 4)
                {
                    //i = random position between the first and third quarter
                    var randomPoint = UnityEngine.Random.Range(0, (root.EndPoint.Item1 - root.InitPoint.Item1));
                    i = randomPoint;
                    //check left, right, up and down cells, if only one is a wall, create a door between them
                    if (matrix[i - 1, splitPoint] == 1 && matrix[i + 1, splitPoint] == 2 && matrix[i, splitPoint - 1] == 2 && matrix[i, splitPoint + 1] == 2)
                    {
                        matrix[i, splitPoint] = 3;
                        matrix[i, splitPoint + 1] = 3;
                    }
                    else if (matrix[i + 1, splitPoint] == 1 && matrix[i - 1, splitPoint] == 2 && matrix[i, splitPoint - 1] == 2 && matrix[i, splitPoint + 1] == 2)
                    {
                        matrix[i, splitPoint] = 3;
                        matrix[i, splitPoint + 1] = 3;
                    }
                    else if (matrix[i, splitPoint - 1] == 1 && matrix[i, splitPoint + 1] == 2 && matrix[i - 1, splitPoint] == 2 && matrix[i + 1, splitPoint] == 2)
                    {
                        matrix[i, splitPoint] = 3;
                        matrix[i, splitPoint + 1] = 3;
                    }
                    else if (matrix[i, splitPoint + 1] == 1 && matrix[i, splitPoint - 1] == 2 && matrix[i - 1, splitPoint] == 2 && matrix[i + 1, splitPoint] == 2)
                    {
                        matrix[i, splitPoint] = 3;
                        matrix[i, splitPoint + 1] = 3;
                    }
                    else
                    {
                        matrix[i, splitPoint] = 1;
                        matrix[i, splitPoint + 1] = 1;

                        //move the iteration to the left to create a door
                        matrix[i-1, splitPoint] = 3;
                        matrix[i-1, splitPoint + 1] = 3;

                    }
                }else*/
                {
                    matrix[i, splitPoint] = 1;
                    matrix[i, splitPoint + 1] = 1;
                }
                //matrix[i, splitPoint] = 1;
                //matrix[i, splitPoint + 1] = 1;
            }


            //Create left child
            Node leftChild = new Node(root.InitPoint, new Tuple<int, int>(root.EndPoint.Item1, splitPoint - 1));

            //Create right child
            Node rightChild = new Node(new Tuple<int, int>(root.InitPoint.Item1, splitPoint), root.EndPoint);

            //Assign left child to root and split it
            root.LeftChild = leftChild;
            Split(matrix, leftChild);

            //Assign right child to root and split it
            root.RightChild = rightChild;
            Split(matrix, rightChild);
        }
        public void RestartContainers()
        {
            foreach (GameObject container in containers)
            {
                foreach (Transform child in container.transform)
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
                //Change the value of each cell in the first and last row of the matrix to 1 (wall)
                matrix[i, 0] = 1;
                matrix[i, matrix.GetLength(1) - 1] = 1;
            }

            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                //Change the value of each cell in the first and last column of the matrix to 1 (wall)
                matrix[0, i] = 1;
                matrix[matrix.GetLength(0) - 1, i] = 1;
            }

            for (int i = 1; i < matrix.GetLength(0)-1; i++)
            {
                for (int j = 1; j < matrix.GetLength(1)-1; j++)
                {
                    //Initialize matrix with 2 (floor)
                    matrix[i, j] = 2;
                }
            }
        }

        void IntercalatedDoors(int[,] matrix)
        {
            for (int i = 1; i < matrix.GetLength(0) - 1; i++)
            {
                //Horizontal doors
                for (int j = 1; j < matrix.GetLength(1) - 1; j++)
                {
                    var actualCell = matrix[i, j];
                    var rightCell = matrix[i, j + 1];
                    var actualUpCell = matrix[i - 1, j];
                    var actualDownCell = matrix[i + 1, j];
                    var rightUpCell = matrix[i - 1, j + 1];
                    var rightDownCell = matrix[i + 1, j + 1];

                    //check if 2 walls are in the same row 
                    //if (matrix[i,j] == 1 && matrix[i,j+1] == 1)
                    if (actualCell == 1 && actualUpCell == 1 && actualDownCell == 1 &&
                        rightCell == 1 && rightUpCell == 1 && rightDownCell == 1)
                    {
                        var leftCell = matrix[i, j - 1];
                        var twoLeftCell = matrix[i, j + 2];
                        //check if the cell in the left is a floor and the cell in the right is a wall
                        //if (matrix[i,j-1] == 2 && matrix[i,j+2] == 2)
                        if (leftCell == 2 && twoLeftCell == 2)
                        {
                            //Debug.Log("Horizontal Door");
                            //valid door
                            matrix[i, j] = 3;
                            matrix[i, j + 1] = 3;
                        }
                    }
                }
            }

            for (int i = 1; i < matrix.GetLength(0) - 2; i++)
            {
                //Vertical doors
                for (int j = 1; j < matrix.GetLength(1) - 2; j++)
                {
                    var actualCell = matrix[i, j];
                    var downCell = matrix[i + 1, j];
                    var actualRightCell = matrix[i, j + 1];
                    var actualLeftCell = matrix[i, j - 1];
                    var downRightCell = matrix[i + 1, j + 1];
                    var downLeftCell = matrix[i + 1, j - 1];

                    //check if 2 walls are in the same column
                    if (actualCell == 1 && actualLeftCell == 1 && actualRightCell == 1 &&
                        downCell == 1 && downRightCell == 1 && downLeftCell == 1)
                    {
                        var upCell = matrix[i - 1, j];
                        var DownCell2 = matrix[i + 2, j];
                        //check if the cell in the up is a floor and the cell in the down is a wall
                        if (upCell == 2 && DownCell2 == 2)
                        {
                            //Debug.Log("Vertical Door");
                            //valid door
                            matrix[i, j] = 3;
                            matrix[i + 1, j] = 3;
                        }
                    }
                }
            }
            
        }
    }
}
