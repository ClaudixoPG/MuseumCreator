using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static Node;

namespace SpaceOptimization{
    public class MapCreator : MonoBehaviour
    {
        public GameObject[] prefabs;
        public GameObject[] containers;
        public int scale; //scale of each node
        public List<GameObject> openNodes = new List<GameObject>();
        public List<GameObject> closedNodes = new List<GameObject>();


        //read a matrix from a text file and return it as a matrix of integers, considering the first line as the number of lines and columns
        public int[,] ReadMatrixWithSize(string path)
        {
            //check if the file exists
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("File not found: " + path);
                return null;
            }

            string[] lines = System.IO.File.ReadAllLines(path);
            int[,] matrix = new int[int.Parse(lines[0].Split(' ')[0]), int.Parse(lines[0].Split(' ')[1])];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                string[] line = lines[i + 1].Split(' ');
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = int.Parse(line[j]);
                }
            }
            return matrix;
        }


        public int[,] Creator(string filename = "map")
        {
            var path = Application.dataPath + "/SpaceOptimization/Resources/Maps/" + filename + ".txt";
            int[,] matrix = ReadMatrixWithSize(path);
            for (int i = 0; i < matrix.GetLength(0); i++) {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    switch(matrix[i,j])
                    {
                        //non Transitable
                        case 0:
                            var nonTransitable = Instantiate(prefabs[0], new Vector3(i* scale, 0, j* scale), Quaternion.identity, containers[0].transform);
                            var nodeNonTransitable = nonTransitable.AddComponent<Node>();
                            nodeNonTransitable.SetData(new Vector2Int(i, j), matrix[i,j]);
                            closedNodes.Add(nonTransitable);
                        break;
                        //wall
                        case 1:
                            var wall = Instantiate(prefabs[1], new Vector3(i* scale, 0, j* scale), Quaternion.identity,containers[1].transform);
                            var nodeWall = wall.AddComponent<Node>();
                            nodeWall.SetData(new Vector2Int(i, j), matrix[i,j]);
                            openNodes.Add(wall);

                            //Turn on External walls
                            //check if up position to wall is 0 or inside array, if it is, then, activate the wall
                            if(i-1 < 0 || matrix[i-1,j] == 0) wall.transform.GetChild(0).gameObject.SetActive(true);
                            //check if right position to wall is 0 or inside array, if it is, then, activate the wall
                            if(j+1 >= matrix.GetLength(1) || matrix[i,j+1] == 0) wall.transform.GetChild(1).gameObject.SetActive(true);
                            //check if down position to wall is 0 or inside array, if it is, then, activate the wall
                            if(i+1 >= matrix.GetLength(0) || matrix[i+1,j] == 0) wall.transform.GetChild(2).gameObject.SetActive(true);
                            //check if left position to wall is 0 or inside array, if it is, then, activate the wall
                            if(j-1 < 0 || matrix[i,j-1] == 0) wall.transform.GetChild(3).gameObject.SetActive(true);

                            //Turn on Internal walls (check 4 directions)
                            //check if up position is 2 and down position is 1 and left position is 1 or 3 and right position is 1 or 3, if it is, then, activate the down wall
                            if(i-1 >= 0 && matrix[i-1,j] == 2 && i+1 < matrix.GetLength(0) && matrix[i+1,j] == 1 && j-1 >= 0 && (matrix[i,j-1] == 1 || matrix[i,j-1] == 3) && j+1 < matrix.GetLength(1) && (matrix[i,j+1] == 1 || matrix[i,j+1] == 3)) wall.transform.GetChild(2).gameObject.SetActive(true);
                            //check if up position is 1 and down position is 2 and left position is 1 or 3 and right position is 1 or 3, if it is, then, activate the up wall
                            if(i-1 >= 0 && matrix[i-1,j] == 1 && i+1 < matrix.GetLength(0) && matrix[i+1,j] == 2 && j-1 >= 0 && (matrix[i,j-1] == 1 || matrix[i,j-1] == 3) && j+1 < matrix.GetLength(1) && (matrix[i,j+1] == 1 || matrix[i,j+1] == 3)) wall.transform.GetChild(0).gameObject.SetActive(true);
                            //check if right position is 2 and left position is 1 and up position is 1 or 3 and down position is 1 or 3, if it is, then, activate the left wall
                            if(j+1 < matrix.GetLength(1) && matrix[i,j+1] == 2 && j-1 >= 0 && matrix[i,j-1] == 1 && i-1 >= 0 && (matrix[i-1,j] == 1 || matrix[i-1,j] == 3) && i+1 < matrix.GetLength(0) && (matrix[i+1,j] == 1 || matrix[i+1,j] == 3)) wall.transform.GetChild(3).gameObject.SetActive(true);
                            //check if right position is 1 and left position is 2 and up position is 1 or 3 and down position is 1 or 3, if it is, then, activate the right wall
                            if(j+1 < matrix.GetLength(1) && matrix[i,j+1] == 1 && j-1 >= 0 && matrix[i,j-1] == 2 && i-1 >= 0 && (matrix[i-1,j] == 1 || matrix[i-1,j] == 3) && i+1 < matrix.GetLength(0) && (matrix[i+1,j] == 1 || matrix[i+1,j] == 3)) wall.transform.GetChild(1).gameObject.SetActive(true);

                            //Turn on Internal corners
                            //check right down corner
                            if(i+1 < matrix.GetLength(0) && j+1 < matrix.GetLength(1) && (matrix[i+1,j] == 1 || matrix[i+1,j] == 3) && (matrix[i,j+1] == 1 || matrix[i,j+1] == 3) && matrix[i+1,j+1] == 2) 
                            {
                                wall.transform.GetChild(0).gameObject.SetActive(true);
                                wall.transform.GetChild(3).gameObject.SetActive(true);
                            }
                            //check left down corner
                            if(i+1 < matrix.GetLength(0) && j-1 >= 0 && (matrix[i+1,j] == 1 || matrix[i+1,j] == 3) && (matrix[i,j-1] == 1 || matrix[i,j-1] == 3) && matrix[i+1,j-1] == 2)
                            {
                                wall.transform.GetChild(0).gameObject.SetActive(true);
                                wall.transform.GetChild(1).gameObject.SetActive(true);
                            } 
                            //check right up corner
                            if(i-1 >= 0 && j+1 < matrix.GetLength(1) && (matrix[i-1,j] == 1 || matrix[i-1,j] == 3) && (matrix[i,j+1] == 1 || matrix[i,j+1] == 3) && matrix[i-1,j+1] == 2)
                            {
                                wall.transform.GetChild(2).gameObject.SetActive(true);
                                wall.transform.GetChild(3).gameObject.SetActive(true);
                            } //check left up corner
                            if(i-1 >= 0 && j-1 >= 0 && (matrix[i-1,j] == 1 || matrix[i-1,j] == 3) && (matrix[i,j-1] == 1 || matrix[i,j-1] == 3) && matrix[i-1,j-1] == 2) 
                            {
                                wall.transform.GetChild(2).gameObject.SetActive(true);
                                wall.transform.GetChild(1).gameObject.SetActive(true);
                            }

                            break;
                        //floor
                        case 2:
                            var floor = Instantiate(prefabs[2], new Vector3(i* scale, 0, j* scale), Quaternion.identity,containers[2].transform);
                            var nodeFloor = floor.AddComponent<Node>();
                            nodeFloor.SetData(new Vector2Int(i, j),matrix[i,j]);
                            openNodes.Add(floor);
                            break;
                        //door
                        case 3:
                            var door = Instantiate(prefabs[3], new Vector3(i* scale, 0, j* scale), Quaternion.identity,containers[3].transform);
                            var nodeDoor = door.AddComponent<Node>();
                            nodeDoor.SetData(new Vector2Int(i, j),matrix[i,j]);
                            closedNodes.Add(door);

                            //External doors
                            //check if down position is outside the matrix, if it is, then, activate the down door
                            if(i+1 >= matrix.GetLength(0)) door.transform.GetChild(2).gameObject.SetActive(true);
                            //check if up position is outside the matrix, if it is, then, activate the up door
                            if(i-1 < 0) door.transform.GetChild(0).gameObject.SetActive(true);
                            //check if right position is outside the matrix, if it is, then, activate the right door
                            if(j+1 >= matrix.GetLength(1)) door.transform.GetChild(1).gameObject.SetActive(true);
                            //check if left position is outside the matrix, if it is, then, activate the left door
                            if(j-1 < 0) door.transform.GetChild(3).gameObject.SetActive(true);

                            //Internal doors
                            //check if down position is 3 and up position is 2 or 0, if it is, then, activate the down door
                            if(i+1 < matrix.GetLength(0) && matrix[i+1,j] == 3 && (i-1 < 0 || matrix[i-1,j] == 2 || matrix[i-1,j] == 0)) door.transform.GetChild(2).gameObject.SetActive(true);
                            //check if up position is 3 and down position is 2 or 0, if it is, then, activate the up door
                            if(i-1 >= 0 && matrix[i-1,j] == 3 && (i+1 >= matrix.GetLength(0) || matrix[i+1,j] == 2 || matrix[i+1,j] == 0)) door.transform.GetChild(0).gameObject.SetActive(true);
                            //check if right position is 3 and left position is 2 or 0, if it is, then, activate the right door
                            if(j+1 < matrix.GetLength(1) && matrix[i,j+1] == 3 && (j-1 < 0 || matrix[i,j-1] == 2 || matrix[i,j-1] == 0)) door.transform.GetChild(1).gameObject.SetActive(true);
                            //check if left position is 3 and right position is 2 or 0, if it is, then, activate the left door
                            if(j-1 >= 0 && matrix[i,j-1] == 3 && (j+1 >= matrix.GetLength(1) || matrix[i,j+1] == 2 || matrix[i,j+1] == 0)) door.transform.GetChild(3).gameObject.SetActive(true);

                            break;
                        case 8:
                            var start = Instantiate(prefabs[4], new Vector3(i* scale, 0, j* scale), Quaternion.identity,containers[3].transform);
                            var nodeStart = start.AddComponent<Node>();
                            nodeStart.SetData(new Vector2Int(i, j),matrix[i,j]);
                            closedNodes.Add(start);
                            break;

                        case 9:
                            var end = Instantiate(prefabs[5], new Vector3(i* scale, 0, j* scale), Quaternion.identity,containers[3].transform);
                            var nodeEnd = end.AddComponent<Node>();
                            nodeEnd.SetData(new Vector2Int(i, j),matrix[i,j]);
                            closedNodes.Add(end);
                            break;
                        default: 
                            //print Error
                            Debug.Log("Error: " + matrix[i,j] + " is not a valid value");
                            break;
                        //añadir esquinas internas y externas
                    }
                }
            }

            //podría crear una clase nodo que guarde el valor x e y de la posición, además de un array de 4 posiciones que guarde los valores de los vecinos (arriba, abajo, izquierda, derecha)
            //y luego recorrer la matriz y crear un nodo por cada posición, y luego recorrer los nodos y activar las paredes correspondientes
            //también podría crear una clase pared que guarde el valor de la posición y el valor de la pared (0,1,2,3) y luego recorrer la matriz y crear una pared por cada posición
            //y luego recorrer las paredes y activar las paredes correspondientes

            return matrix;
        }

        public void SetNeighbors(int[,] matrix)
        {
            foreach(GameObject node in openNodes)
            {
                node.GetComponent<Node>().SetNeighbors(GetNeighbors(matrix, (int)node.GetComponent<Node>().Position.x, (int)node.GetComponent<Node>().Position.y));
            }
        }

        public List<GameObject> GetOpenNodes()
        {
            return openNodes;
        }

        public List<GameObject> GetClosedNodes()
        {
            return closedNodes;
        }

        private List<GameObject> GetNeighbors(int[,] matrix, int xPos, int yPos)
        {
            GameObject node = null;
            List<GameObject> neighbors = new List<GameObject>();

            for(int i = -1; i <=1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    if(i == 0 && j == 0) continue; //skip the current node

                    if(xPos + i >= 0 && xPos + i < matrix.GetLength(0) && yPos + j >= 0 && yPos + j < matrix.GetLength(1)) //check if the node is inside the matrix
                    {
                        //check if node exist on openNodes or closedNodes, if it does, then, return the object
                        if(openNodes.Exists(go => go.GetComponent<Node>().Position.x == xPos + i && go.GetComponent<Node>().Position.y == yPos + j)) //check if the node exist on openNodes
                        {
                            node = openNodes.Find(go => go.GetComponent<Node>().Position.x == xPos + i && go.GetComponent<Node>().Position.y == yPos + j); //get the node
                        }else
                        if(closedNodes.Exists(go => go.GetComponent<Node>().Position.x == xPos + i && go.GetComponent<Node>().Position.y == yPos + j)) //check if the node exist on closedNodes
                        {
                            node = closedNodes.Find(go => go.GetComponent<Node>().Position.x == xPos + i && go.GetComponent<Node>().Position.y == yPos + j); //get the node
                        }
                        //Debug.Log("node found on openNodes" + node.GetComponent<Node>().Position);
                        neighbors.Add(node); //add the node to the neighbors list
                    }else
                    {
                        neighbors.Add(null); //add null to the neighbors list
                    }
                }
            }
            return neighbors;
        }
    }
}