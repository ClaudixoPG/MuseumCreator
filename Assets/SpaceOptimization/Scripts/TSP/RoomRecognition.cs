//using SpaceOptimization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
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
        public GameObject weightCanvas;
        int[] distribution;
        float scale = 10f;

        public void TSP(int[,] matrix)//, TSPSolver solver = TSPSolver.instance)
        {
            InitializeData(matrix);
            IdentifyRooms();
            LoadChromosome();
            ReconstructMatrix();
            IdentifyConnections();
            CreateTheGraph();
            DistanceBetweenDoors(matrix);

        }
        void LoadChromosome()
        {
            //Load the chromosome from the file
            string path = Application.dataPath + "/SpaceOptimization/Resources/Maps/ArtworksDistribution.csv";
            string[] lines = File.ReadAllLines(path);
            string[] values = lines[0].Split(';');
            distribution = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                distribution[i] = int.Parse(values[i]);
            }
        }
        void InitializeData(int[,] matrix)
        {
            this.matrix = matrix;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 3)
                    {
                        matrix[i, j] = -1;
                    }
                }
            }

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
        /// <summary>
        /// Recognize the connections between rooms and store them in the rooms dictionary
        /// </summary>
        void IdentifyConnections()
        {
            //Identify connections from left to right and top to bottom
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < cols - 1; j++)
                {
                    //Draw a line if 2 doors are connected
                    if (matrix[i, j] == -1 && matrix[i, j + 1] == -1)
                    {
                        Debug.DrawLine(new Vector3(i, 1, j) * scale, new Vector3(i, 1, j + 1) * scale, Color.yellow, 1000f);
                    }
                    if (matrix[i, j] == -1 && matrix[i + 1, j] == -1)
                    {
                        Debug.DrawLine(new Vector3(i, 1, j) * scale, new Vector3(i + 1, 1, j) * scale, Color.yellow, 1000f);
                    }

                    if (matrix[i, j] == -1 && matrix[i, j + 1] == -1)
                    {
                        var leftRoom = matrix[i, j - 1];
                        var rightRoom = matrix[i, j + 2];

                        rooms[leftRoom].ConnectedRooms.Add(rightRoom);
                        rooms[rightRoom].ConnectedRooms.Add(leftRoom);
                    }

                    if (matrix[i, j] == -1 && matrix[i + 1, j] == -1)
                    {
                        var topRoom = matrix[i - 1, j];
                        var bottomRoom = matrix[i + 2, j];

                        rooms[topRoom].ConnectedRooms.Add(bottomRoom);
                        rooms[bottomRoom].ConnectedRooms.Add(topRoom);

                    }

                }
            }
        }
        void CreateTheGraph()
        {
            //Create the graph
            foreach (var room in rooms)
            {
                var initX = room.Value.nodes[0].Position.x;
                var initY = room.Value.nodes[0].Position.y;
                var endX = room.Value.nodes[room.Value.nodes.Count - 1].Position.x;
                var endY = room.Value.nodes[room.Value.nodes.Count - 1].Position.y;

                foreach (var node in room.Value.nodes)
                {
                    if(initX > node.Position.x)
                    {
                        initX = node.Position.x;
                    }
                    if (initY > node.Position.y)
                    {
                        initY = node.Position.y;
                    }
                    if (endX < node.Position.x)
                    {
                        endX = node.Position.x;
                    }
                    if (endY < node.Position.y)
                    {
                        endY = node.Position.y;
                    }
                }

                Vector2 roomMiddlePoint = new Vector2((initX + endX) / 2, (initY + endY) / 2);

                foreach (var connectedRoom in room.Value.ConnectedRooms)
                {
                    var connectedInitX = rooms[connectedRoom].nodes[0].Position.x;
                    var connectedInitY = rooms[connectedRoom].nodes[0].Position.y;
                    var connectedEndX = rooms[connectedRoom].nodes[rooms[connectedRoom].nodes.Count - 1].Position.x;
                    var connectedEndY = rooms[connectedRoom].nodes[rooms[connectedRoom].nodes.Count - 1].Position.y;

                    foreach (var node in rooms[connectedRoom].nodes)
                    {
                        if (connectedInitX > node.Position.x)
                        {
                            connectedInitX = node.Position.x;
                        }
                        if (connectedInitY > node.Position.y)
                        {
                            connectedInitY = node.Position.y;
                        }
                        if (connectedEndX < node.Position.x)
                        {
                            connectedEndX = node.Position.x;
                        }
                        if (connectedEndY < node.Position.y)
                        {
                            connectedEndY = node.Position.y;
                        }
                    }

                    Vector2 connectedRoomMiddlePoint = new Vector2((connectedInitX + connectedEndX) / 2, (connectedInitY + connectedEndY) / 2);

                    //Draw a line between the middle points of the rooms
                    /*Debug.DrawLine(new Vector3(roomMiddlePoint.x, 0, roomMiddlePoint.y) * scale, 
                        new Vector3(connectedRoomMiddlePoint.x, 0, connectedRoomMiddlePoint.y) * scale, 
                        Color.green, 1000f);*/

                }
            }
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

            //DEBUG
            /*for (int i = 0; i < rows; i++)
            {
                string row = "";
                for (int j = 0; j < cols; j++)
                {
                    row += newMatrix[i, j] + " ";
                }
                Debug.Log(row);
            }*/

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
            if (!rooms[roomId].nodes.Contains(newNode))
            {
                rooms[roomId].nodes.Add(newNode);
            }

            DFS(row + 1, col, roomId);
            DFS(row - 1, col, roomId);
            DFS(row, col + 1, roomId);
            DFS(row, col - 1, roomId);
        }
        
        void DistanceBetweenDoors(int[,] matrix)
        {
            //Calculate the distance between doors
            Graph graph = new Graph(matrix);
            foreach(var room in rooms)
            {
                List<Node> doorsInRoom = new List<Node>();
                List<Node> OccupiedNodes = new List<Node>();

                //print values of the room

                foreach (var node in room.Value.nodes)
                {

                    //Get lateral nodes of the current node
                    Node leftNode = new Node(new Vector2Int(node.Position.x, node.Position.y - 1));
                    Node rightNode = new Node(new Vector2Int(node.Position.x, node.Position.y + 1));
                    Node topNode = new Node(new Vector2Int(node.Position.x - 1, node.Position.y));
                    Node bottomNode = new Node(new Vector2Int(node.Position.x + 1, node.Position.y));

                    //check if node around (4 connected) is a door, if so, add it to the list
                    if(!doorsInRoom.Contains(leftNode) && matrix[leftNode.Position.x, leftNode.Position.y] == -1)
                    {
                        doorsInRoom.Add(leftNode);
                    }
                    if (!doorsInRoom.Contains(rightNode) && matrix[rightNode.Position.x, rightNode.Position.y] == -1)
                    {
                        doorsInRoom.Add(rightNode);
                    }
                    if (!doorsInRoom.Contains(topNode) && matrix[topNode.Position.x, topNode.Position.y] == -1)
                    {
                        doorsInRoom.Add(topNode);
                    }
                    if (!doorsInRoom.Contains(bottomNode) && matrix[bottomNode.Position.x, bottomNode.Position.y] == -1)
                    {
                        doorsInRoom.Add(bottomNode);
                    }
                    if(node.Occupied)
                    {
                        OccupiedNodes.Add(node);
                    }
                }

                //Draw the lines between the doors
                for (int i = 0; i < doorsInRoom.Count; i++)
                {
                    for (int j = 0; j < doorsInRoom.Count; j++)
                    {
                        var city1 = graph.cities.Find(city => city.x == doorsInRoom[i].Position.x && city.y == doorsInRoom[i].Position.y);
                        var city2 = graph.cities.Find(city => city.x == doorsInRoom[j].Position.x && city.y == doorsInRoom[j].Position.y);

                        if (city1 == null || city2 == null || city1.Equals(city2))
                        {
                            continue;
                        }

                        //add first city at beginning of the matrix and the last city at the end of the matrix

                        //check if the nodes are already added to the graph, using find method to check if the city is already in the list
                        var firstValue = new Node(new Vector2Int(city1.x, city1.y));
                        var lastValue = new Node(new Vector2Int(city2.x, city2.y));

                        //Check if firstValue is already in the list of occupied nodes
                        if (!OccupiedNodes.Exists(node => node.Position == firstValue.Position))
                        {
                            OccupiedNodes.Insert(0, firstValue);
                        }
                        if(!OccupiedNodes.Exists(node => node.Position == lastValue.Position))
                        {
                            OccupiedNodes.Add(lastValue);
                        }

                        //OccupiedNodes.Insert(0, new Node(new Vector2Int(city1.x, city1.y)));
                        //OccupiedNodes.Add(new Node(new Vector2Int(city2.x, city2.y)));

                        var dMatrix = new int[OccupiedNodes.Count, OccupiedNodes.Count];

                        for (int k = 0; k < OccupiedNodes.Count; k++)
                        {
                            for (int l = 0; l < OccupiedNodes.Count; l++)
                            {
                                dMatrix[k, l] = Mathf.Abs(OccupiedNodes[k].Position.x - OccupiedNodes[l].Position.x) + Mathf.Abs(OccupiedNodes[k].Position.y - OccupiedNodes[l].Position.y);
                                Debug.DrawLine(new Vector3(OccupiedNodes[k].Position.x, 0, OccupiedNodes[k].Position.y) * scale,
                                new Vector3(OccupiedNodes[l].Position.x, 0, OccupiedNodes[l].Position.y) * scale, Color.white, 2f);
                            }
                        }

                        //Calculate distance using the Held-Karp algorithm
                        var artworksPath = HeldKarp.CalculateValue(dMatrix);

                        //draw the line of each connection returned by the algorithm (Held-Karp)
                        /*for (int k = 0;k < artworksPath.Item2.Count - 1; k++)
                        {
                            //Draw first line as green
                            if (k == 0)
                            {
                                Debug.DrawLine(new Vector3(OccupiedNodes[artworksPath.Item2[k]].Position.x, 1, OccupiedNodes[artworksPath.Item2[k]].Position.y) * scale,
                                new Vector3(OccupiedNodes[artworksPath.Item2[k + 1]].Position.x, 1, OccupiedNodes[artworksPath.Item2[k + 1]].Position.y) * scale, Color.green, 1000f);
                            }
                            //Draw the last line as red
                            else if (k == artworksPath.Item2.Count - 2)
                            {
                                Debug.DrawLine(new Vector3(OccupiedNodes[artworksPath.Item2[k]].Position.x, 1, OccupiedNodes[artworksPath.Item2[k]].Position.y) * scale,
                                new Vector3(OccupiedNodes[artworksPath.Item2[k + 1]].Position.x, 1, OccupiedNodes[artworksPath.Item2[k + 1]].Position.y) * scale, Color.red, 1000f);
                            }
                            //Draw all the other lines as yellow
                            else
                            {
                                Debug.DrawLine(new Vector3(OccupiedNodes[artworksPath.Item2[k]].Position.x, 1, OccupiedNodes[artworksPath.Item2[k]].Position.y) * scale,
                                new Vector3(OccupiedNodes[artworksPath.Item2[k + 1]].Position.x, 1, OccupiedNodes[artworksPath.Item2[k + 1]].Position.y) * scale, Color.yellow, 1000f);
                            }
                           
                            //Debug.DrawLine(new Vector3(OccupiedNodes[distance.Item2[k]].Position.x, 0, OccupiedNodes[distance.Item2[k]].Position.y) * scale,
                            //new Vector3(OccupiedNodes[distance.Item2[k + 1]].Position.x, 0, OccupiedNodes[distance.Item2[k + 1]].Position.y) * scale, Color.yellow, 1000f);
                        }*/

                        graph.edges.Add(new Edge { StartCity = city1, EndCity = city2, Artworks = OccupiedNodes, weight = artworksPath.Item1, path = artworksPath.Item2 });
                    }
                }
            }

            SolveTSP(graph);
            
        }

        void SolveTSP(Graph graph)
        {
            //Calculate the distance matrix
            int[,] distanceMatrix = graph.GraphToDistanceMatrix();

            //Save the distance matrix to a file to be used in the TSP
            var copyMatrix = new string[distanceMatrix.GetLength(0), distanceMatrix.GetLength(1)];

            for (int j = 0; j < distanceMatrix.GetLength(0); j++)
            {
                for (int k = 0; k < distanceMatrix.GetLength(1); k++)
                {
                    copyMatrix[j, k] = distanceMatrix[j, k].ToString();
                }
            }

            string mdfilename = "TSP_DATA_" + distanceMatrix.GetLength(0) + "x" + distanceMatrix.GetLength(1) + "_" +
                    DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            WriteMatrixToFile(copyMatrix, Application.dataPath + "/SpaceOptimization/Experiments/" +
                matrix.GetLength(0) + "x" + matrix.GetLength(1) + "/" + mdfilename + ".txt");


            //Create a list of solvers data
            List<Tuple<string, int, List<int>, double>> solversData = new List<Tuple<string, int, List<int>, double>>();

            //Initialize the solvers
            ACO_TSP aco_TSP = new ACO_TSP(graph.cities.Count / 2, 10, 1.0f, 2.0f);
            SA_TSP sa_TSP = new SA_TSP(graph.cities.Count / 2, 1000, 1.0f, 0.001f);
            PSO_TSP pSO_TSP = new PSO_TSP(graph.cities.Count / 2, 10, 100, 0.5f, 0.7f, 1.5f, 1.5f);
            GA_TSP gA_TSP = new GA_TSP(graph.cities.Count / 2, 100, 0.1f, 100);
            //experiment with the solvers
            int iterations = 1;

            for (int i = 0; i < iterations; i++)
            {
                //Get the solvers data
                var acoSolver = aco_TSP.Solver(distanceMatrix);
                //var saSolver = sa_TSP.Solver(distanceMatrix);
                //var psoSolver = pSO_TSP.Solver(distanceMatrix);
                //var gaSolver = gA_TSP.Solver(distanceMatrix);

                //Add the solvers data to the list
                solversData.Add(acoSolver);
                //solversData.Add(saSolver);
                //solversData.Add(psoSolver);
                //solversData.Add(gaSolver);
                PrintDebugLinesInEditor(graph, acoSolver);
                //PrintDebugLinesInEditor(graph, gaSolver);
                //SaveTSPData(mdfilename, solversData, 1, "TSP_DATA_" + matrix.GetLength(0) + "x" + matrix.GetLength(1)); //aco_TSP.executionTime, aco_TSP.iterations);
                solversData.Clear();
            }
            Debug.Log("Saved Data");
        }
        
        void PrintDebugLinesInEditor(Graph graph,Tuple<string,int,List<int>,double> data)
        {
            for(int i = 0; i < data.Item3.Count - 1; i++)
            {
                //Find the city in the graph
                var city1 = graph.cities.Find(city => city.id == data.Item3[i]);
                var city2 = graph.cities.Find(city => city.id == data.Item3[i + 1]);

                //Find the edge that connects the cities in the graph
                var edge = graph.edges.Find(e => e.StartCity.id == city1.id && e.EndCity.id == city2.id);

                //Draw a line between each artwork in the path of the edge
                for (int j = 0; j < edge.Artworks.Count - 1; j++)
                {
                    Debug.DrawLine(new Vector3(edge.Artworks[j].Position.x, 1, edge.Artworks[j].Position.y) * scale,
                    new Vector3(edge.Artworks[j + 1].Position.x, 1, edge.Artworks[j + 1].Position.y) * scale, Color.yellow, 1000f);
                }
            }

            //Draw the las connection between the last and the first city
            var lastCity = graph.cities.Find(city => city.id == data.Item3[data.Item3.Count - 1]);
            var firstCity = graph.cities.Find(city => city.id == data.Item3[0]);

            var lastEdge = graph.edges.Find(e => e.StartCity.id == lastCity.id && e.EndCity.id == firstCity.id);

            for (int j = 0; j < lastEdge.Artworks.Count - 1; j++)
            {
                Debug.DrawLine(new Vector3(lastEdge.Artworks[j].Position.x, 1, lastEdge.Artworks[j].Position.y) * scale,
                new Vector3(lastEdge.Artworks[j + 1].Position.x, 1, lastEdge.Artworks[j + 1].Position.y) * scale, Color.yellow, 1000f);
            }

            /*foreach (var edge in graph.edges)
            {
                Debug.DrawLine
            }*/
        }   
        void SaveTSPData(string dmfilename,List<Tuple<string,int,List<int>,double>> data,int interations,string filename = "TSP_Data")
        {
            //Save the rooms data to a file
            string path = Application.dataPath + "/SpaceOptimization/Resources/Maps/" + filename + ".csv";

            if (!File.Exists(path))
            {
                //Create the file if it doesn't exist and write the header
                File.Create(path).Dispose();
            }

            //use writer to write the data to the file, first line have the values TourLength, Tour, ExecutionTime, Iterations, 
            using (StreamWriter writer = new StreamWriter(path,append:true))
            {
                //check if first line is empty, if so, write the header
                if (new FileInfo(path).Length == 0)
                {
                    string header = "dmfilename;";

                    foreach (var item in data)
                    {
                        //if item is the last one, don't add the separator
                        if (item.Equals(data[data.Count - 1]))
                        {
                            header += item.Item1 + "TourLength" + ";" + item.Item1 + "Tour" + ";" + item.Item1 + "ExecutionTime" + ";" + item.Item1 + "Iterations";
                        }
                        else
                        {
                            header += item.Item1 + "TourLength" + ";" + item.Item1 + "Tour" + ";" + item.Item1 + "ExecutionTime" + ";" + item.Item1 + "Iterations;";
                        }
                    }
                    writer.WriteLine(header);
                }

                var line = dmfilename + ";";

                foreach (var item in data)
                {
                    //if item is the last one, don't add the separator
                    if (item.Equals(data[data.Count - 1]))
                    {
                        line += item.Item2 + ";" + string.Join(",", item.Item3) + ";" + item.Item4 + ";" + interations;
                    }
                    else
                    {
                        line += item.Item2 + ";" + string.Join(",", item.Item3) + ";" + item.Item4 + ";" + interations + ";";
                    }
                }
                writer.WriteLine(line);
            }

        }

        class Graph
        {
            public List<City> cities = new List<City>();
            public List<Edge> edges = new List<Edge>();

            public Graph(int[,]matrix)
            {
                DetectCities(matrix);
            }
            public void DetectCities(int[,] matrix)
            {
                int cityId = 0;

                //Save each door as a city in the graph, if 2 doors are connected, save them with the same id (horizontal)
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] == -1 && matrix[i, j + 1] == -1)
                        {
                            City firstHalfCity = new City { id = cityId, x = i, y = j };
                            City secondHalfCity = new City { id = cityId, x = i, y = j + 1 };

                            //Check if the nodes are already added to the graph, using find method to check if the city is already in the list
                            if (!cities.Exists(city => city.x == firstHalfCity.x && city.y == firstHalfCity.y))
                            {
                                cities.Add(firstHalfCity);
                                cities.Add(secondHalfCity);
                                cityId++;
                            }
                        }
                    }
                }

                //Save each door as a city in the graph, if 2 doors are connected, save them with the same id (vertical)
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] == -1 && matrix[i + 1, j] == -1)
                        {
                            City firstHalfCity = new City { id = cityId, x = i, y = j };
                            City secondHalfCity = new City { id = cityId, x = i + 1, y = j };

                            //Check if the nodes are already added to the graph, using find method to check if the city is already in the list
                            if (!cities.Exists(city => city.x == firstHalfCity.x && city.y == firstHalfCity.y))
                            {
                                cities.Add(firstHalfCity);
                                cities.Add(secondHalfCity);
                                cityId++;
                            }
                        }
                    }
                }

            }

            public int[,] GraphToDistanceMatrix()
            { 
                var n = cities.Count/2;

                int[,] distanceMatrix = new int[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        //distanceMatrix[i, j] = int.MaxValue;
                        distanceMatrix[i, j] = 100;
                    }
                }
                
                foreach (var edge in edges)
                {
                    distanceMatrix[edge.StartCity.id, edge.EndCity.id] = edge.weight;
                }

                return distanceMatrix;
            }
        }
        
        class City
        {
            public int id;
            public int x;
            public int y;
        }
        
        class Edge
        {
            public City StartCity;
            public City EndCity;
            public List<Node> Artworks = new List<Node>();
            public int weight;
            public List<int> path;
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


