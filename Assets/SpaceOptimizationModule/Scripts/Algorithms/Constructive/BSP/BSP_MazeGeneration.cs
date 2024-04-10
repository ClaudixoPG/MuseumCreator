using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using static PCG.Utilities;

namespace PCG
{
    public class BSP_MazeGeneration : MonoBehaviour
    {
        public int width = 20;
        public int height = 20;

        public int maximalRoomSize = 5;

        //To restart binary partition
        public List<GameObject> containers;
        public void Build()
        {

            RestartContainers();

            var matrix = new int[width, height];

            //Initialize matrix
            InitializeMatrix(matrix);

            //Create root Node
            Node root = new Node(new Tuple<int, int>(0, 0), new Tuple<int, int>(matrix.GetLength(0) - 1, matrix.GetLength(1) - 1));
            Split(matrix, root);
            //CreateHallway(matrix, root);

            //Write Matrix to file in var path = Application.dataPath + "/SpaceOptimizationModule/Resources/Maps/" + filename + ".txt";
            string filename = "Maze";
            string path = Application.dataPath + "/SpaceOptimizationModule/Resources/Maps/" + filename + ".txt";
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
                if (i == root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 4 || i == root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) * 3 / 4)
                //if (i == root.InitPoint.Item2 + (root.EndPoint.Item2 - root.InitPoint.Item2) / 2)
                {
                    matrix[splitPoint, i] = 3;
                    matrix[splitPoint + 1, i] = 3;
                }
                else
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
                if (i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 4 || i == root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) * 3 / 4)
                //if(i ==root.InitPoint.Item1 + (root.EndPoint.Item1 - root.InitPoint.Item1) / 2)
                {
                    matrix[i, splitPoint] = 3;
                    matrix[i, splitPoint + 1] = 3;
                }
                else
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
        void RestartContainers()
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

    }
}
