//using SpaceOptimization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

namespace SpaceOptimization
{
    public class RoomRecognition : MonoBehaviour
    {
        int[,] matrix;
        int rows;
        int cols;
        bool[,] visited;
        Dictionary<int, Room> rooms = new Dictionary<int, Room>();
        public GameObject weightCanvas ;
        int[] distribution;

        public void TSP(int[,] matrix)//, TSPSolver solver = TSPSolver.instance)
        {

            //Identify the rooms in the matrix
            InitializeData(matrix);
            IdentifyRooms();
            //PrintRoomsCoordinates();

            //Load a distribution of artworks from a file
            LoadChromosome();

            ReconstructMatrix();


            //PrintGraph();
            
            //InstantiateTheData();
            
            //Save the graph in a file

            IdentifyConnections();
            
            //Create the graph new knew the rooms and the connections between them, and number of artworks in each room
            CreateTheGraph();
            
            //SaveGraph(rooms);
            //solver.Calculate();
        }

        void LoadChromosome()
        {
            //Load the chromosome from the file
            string path = Application.dataPath + "/SpaceOptimization/Resources/Maps/ArtworksDistribution.csv";
            string[] lines = File.ReadAllLines(path);
            string[] values = lines[0].Split(';');
            distribution = new int[values.Length];
            for(int i = 0; i < values.Length; i++)
            {
                distribution[i] = int.Parse(values[i]);
            }
        }


        void InitializeData(int[,] matrix)
        {
            this.matrix = matrix;
            rows = matrix.GetLength(0);
            cols = matrix.GetLength(1);
            visited = new bool[rows, cols];
        }
        void IdentifyRooms()
        {
            int roomId = 0;

            // Identify rooms
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (matrix[i, j] == 2 && !visited[i, j])
                    {
                        roomId++;
                        rooms[roomId] = new Room();
                        DFS(i, j, roomId);
                    }
                }
            }
        }
        void IdentifyConnections()
        {
            // Identify connections
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (matrix[i, j] == 3) // Check if it is a door and get the connected rooms
                    {
                        var connectedRooms = GetConnectedRooms(i, j);
                        foreach (var pair in connectedRooms)
                        {
                            rooms[pair.Item1].ConnectedRooms.Add(pair.Item2);
                            rooms[pair.Item2].ConnectedRooms.Add(pair.Item1);
                        }
                    }
                }
            }
        }
        void PrintRoomsCoordinates()
        {
            // Print the rooms coordinates of each node in each room
            foreach (var room in rooms)
            {
                //Debug.Log($"Room {room.Key}: x={room.Value.x}, y={room.Value.y}, width={room.Value.width}, height={room.Value.height}");
                foreach (var node in room.Value.nodes)
                {
                    Debug.Log("Room " + room.Key + " Node: " + node.Position);
                }
            }
        }
        void PrintGraph()
        {
            // Print the graph
            foreach (var room in rooms)
            {
                Debug.Log("Room " + room.Key + " is connected to rooms: ");
                foreach (var connectedRoom in room.Value.ConnectedRooms)
                {
                    Debug.Log(connectedRoom);
                }
            }
        }

        void CreateTheGraph()
        {
            //Create the graph
            foreach (var room in rooms)
            {
                foreach (var connectedRoom in room.Value.ConnectedRooms)
                {
                    var x = room.Value.nodes[0].Position.x;
                    var y = room.Value.nodes[0].Position.y;
                    var connectedX = rooms[connectedRoom].nodes[0].Position.x;
                    var connectedY = rooms[connectedRoom].nodes[0].Position.y;
                    var scale = 10f;

                    Debug.DrawLine(new Vector3(x, 0, y) * scale, new Vector3(connectedX, 0, connectedY) * scale, Color.green, 1000f);

                    //Debug.DrawLine(new Vector3(room.Value.nodes[0].Position.x, 0, room.Value.nodes[0].Position.y), new Vector3(rooms[connectedRoom].nodes[0].Position.x, 0, rooms[connectedRoom].nodes[0].Position.y), Color.green, 1000f);
                    //Create a text displaying the weight between the rooms in the scene, for debugging purposes
                    //var text = new TextMeshProUGUI();
                    //text.text = CalculateWeightBetweenRooms(room.Value, rooms[connectedRoom]).ToString();
                    //text.transform.position = new Vector3((room.Value.nodes[0].Position.x + rooms[connectedRoom].nodes[0].Position.x) / 2, 0, (room.Value.nodes[0].Position.y + rooms[connectedRoom].nodes[0].Position.y) / 2);
                    var text = Instantiate(weightCanvas, new Vector3((room.Value.nodes[0].Position.x + rooms[connectedRoom].nodes[0].Position.x) / 2, 0, (room.Value.nodes[0].Position.y + rooms[connectedRoom].nodes[0].Position.y) / 2), Quaternion.identity);
                    text.GetComponentInChildren<TextMeshProUGUI>().text = CalculateWeightBetweenRooms(room.Value, rooms[connectedRoom]).ToString();
                    text.transform.LookAt(Camera.main.transform);

                    //Debug.Log("The weight between room " + room.Key + " and room " + connectedRoom + " is " + CalculateWeightBetweenRooms(room.Value, rooms[connectedRoom]));
                }
            }
            //Remove the cases when room is connected to itself


            //Create a graph with the rooms and the weights between them
        }

        int CalculateWeightBetweenRooms(Room room1, Room room2)
        {
            //Calculate the weight between two rooms
            int weight = 0;
            foreach (var node1 in room1.nodes)
            {
                if(node1.Occupied)
                {
                    weight++;
                }
            }
            foreach (var node2 in room2.nodes)
            {
                if (node2.Occupied)
                {
                    weight++;
                }
            }
            return weight;
        }

        void ReconstructMatrix()
        {
            var newMatrix = new string[rows, cols];

            // Initialize the new matrix with 0
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newMatrix[i, j] = "0";
                }
            }

            //Take the rooms data and use to fill the new matrix with the rooms
            foreach (var room in rooms)
            {
                foreach (var node in room.Value.nodes)
                {
                    newMatrix[node.Position.x, node.Position.y] = room.Key.ToString();
                }
            }

            //Add the occupied nodes to the matrix
            for (int i = 0; i < distribution.Length; i++)
            {
                var xPosition = distribution[i] / cols;
                var yPosition = distribution[i] % cols;

                newMatrix[xPosition, yPosition] = "*";

                //Search for the node in the room and set it as occupied
                foreach (var room in rooms)
                {
                    if (room.Value.nodes.Exists(node => node.Position == new Vector2Int(xPosition, yPosition)))
                    {
                        room.Value.nodes.Find(node => node.Position == new Vector2Int(xPosition, yPosition)).Occupied = true;
                    }
                }

                //rooms[matrix[xPosition, yPosition]].nodes.Find(node => node.Position == new Vector2Int(xPosition, yPosition)).Occupied = true;
            }

            //Save the new matrix to a file

            string filename = "RoomRecognition";
            string path = Application.dataPath + "/SpaceOptimization/Resources/Maps/" + filename + ".txt";
            WriteMatrixToFile(newMatrix, path);

        }

        void WriteMatrixToFile(string[,] matrix, string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

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
        void SaveGraph(Dictionary<int, Room> rooms)
        {
            string path = "Assets/Graph.txt";
            //Write some text to the test.txt file
            StreamWriter writer;
            writer = new StreamWriter(path, true);
        }
        //Use DFS to identify rooms given a matrix, identify rooms and assign a room id to each room, besides, store the room's position
        void DFS(int row, int col, int roomId)
        {
            if (row < 0 || col < 0 || row >= rows || col >= cols || visited[row, col] || matrix[row, col] != 2)
            {
                return;
            }

            visited[row, col] = true;
            matrix[row, col] = roomId;

            //Save the nodes of the room
            var newNode = new Node(new Vector2Int(row, col));
        
            //add the node to the room only if not already added
            if(!rooms[roomId].nodes.Contains(newNode))
            {
                rooms[roomId].nodes.Add(newNode);
            }

            DFS(row + 1, col, roomId);
            DFS(row - 1, col, roomId);
            DFS(row, col + 1, roomId);
            DFS(row, col - 1, roomId);
        }   

        //Get the connected rooms of a door
        List<Tuple<int, int>> GetConnectedRooms(int row, int col)
        {
            //Check if the door is connected to two rooms, if so, return the connected rooms and the weight of the edge

            var connectedRooms = new List<Tuple<int, int>>();
            if (row > 0 && row < rows - 1 && matrix[row - 1, col] > 0 && matrix[row + 1, col] > 0)
            {
                connectedRooms.Add(Tuple.Create(matrix[row - 1, col], matrix[row + 1, col]));
            }
            if (col > 0 && col < cols - 1 && matrix[row, col - 1] > 0 && matrix[row, col + 1] > 0)
            {
                connectedRooms.Add(Tuple.Create(matrix[row, col - 1], matrix[row, col + 1]));
            }
            return connectedRooms;
        }

        class Room
        {
            public List<int> ConnectedRooms { get; set; } = new List<int>();
            public List<Node> nodes = new List<Node>();
        }

        class Node
        {
            public Vector2Int Position { get; set; }
            public Node(Vector2Int position)
            {
                Position = position;
            }
            public bool Occupied { get; set; }
        }

    }
}
