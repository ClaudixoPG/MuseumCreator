using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using SpaceOptimization.BSP;

namespace SpaceOptimization{
    public class Map : MonoBehaviour
    {
        [Header("Map Configuration")]
        public string filename;

        [Header("References")]

        public MapCreator mapCreator;
        public FloorSolver floorSolver;
        public BSP_MazeGeneration bspMazeGeneration;
        public CanvasController canvasController;
        public RoomRecognition roomRecognition;
        
        [Header("Debug Lists")]
        public List<GameObject> debugFloorObjects;
        public List<GameObject> floorObjects;
        public List<GameObject> wallObjects;
        public List<GameObject> artworkObjects;

        [Header("Debug Materials")]
        public Material neighborBlockedMaterial;
        public Material floorBlockedMaterial;
        public Material wallBlockedMaterial;
        public Material unblockedMaterial;

        private int[,] matrix;
        private List<GameObject> openNodes = new List<GameObject>();
        private List<GameObject> closedNodes = new List<GameObject>();
        private float timer = 0f;
        
        //Map data and configuration
        private int width;
        private int height;
        private int populationSize;
        private int generations;
        private bool haveElitism;
        private Genetic.CrossoverType crossoverType;
        private Genetic.MutationType mutationType;
        private Genetic.SelectionType selectionType;

        private bool alreadyGenerated = false;

        //save data
        private List<Data> data = new List<Data>();

        private void Awake() {
            openNodes = mapCreator.GetOpenNodes();
            closedNodes = mapCreator.GetClosedNodes();
        }

        //function to remove all elements from the map
        public void RestartToInitialStage()
        {
            //Seek map containers and remove all children objects of them
            GameObject wallsContainer = GameObject.Find("WallsContainer");
            GameObject floorsContainer = GameObject.Find("FloorsContainer");
            GameObject nonTraversableContainer = GameObject.Find("NonTraversableContainer");
            GameObject doorsContainer = GameObject.Find("DoorsContainer");

            foreach (Transform child in wallsContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in floorsContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in nonTraversableContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in doorsContainer.transform)
            {
                Destroy(child.gameObject);
            }
            RestartMatrix();
        }


        public void PopulateMap() {

            //if matrix is empty, load the example matrix
            if (matrix == null)
            {
                filename = "ExampleMap";
                matrix = mapCreator.Creator(filename);
                mapCreator.SetNeighbors(matrix);
                width = matrix.GetLength(0);
                height = matrix.GetLength(1);
            }

            //search for panel with the name GeneticsPanel and convert it to GeneticsPanel
            var panel = canvasController.panels.Find(p => p.name == "PanelStep2") as GeneticsPanel;
            var panelConfiguration = panel.GetPanelData();
            
            //get values from panel
            populationSize = int.TryParse(panelConfiguration[0], out int popSize) ? popSize : 0;
            generations = int.TryParse(panelConfiguration[1], out int gen) ? gen : 0;
            haveElitism = panelConfiguration[2] == "True" ? true : false;
            crossoverType = (Genetic.CrossoverType)int.Parse(panelConfiguration[3]);
            mutationType = (Genetic.MutationType)int.Parse(panelConfiguration[4]);
            selectionType = (Genetic.SelectionType)int.Parse(panelConfiguration[5]);

            //search for invalid values of genetics configuration
            if(populationSize < 1 || generations < 1){
                Debug.LogError("Invalid values for population size or generations");
                return;
            }

            //Genetic Algorithm

            //Data with Elitism
            /* for(int i = 0; i < 1; i++)
            {
                var dataWithElitism = new Data();

                foreach(var crossover in System.Enum.GetValues(typeof(Genetic.CrossoverType)))
                {
                    foreach(var mutation in System.Enum.GetValues(typeof(Genetic.MutationType)))
                    {
                        foreach(var selection in System.Enum.GetValues(typeof(Genetic.SelectionType)))
                        {
                            dataWithElitism = floorSolver.GeneticAlgorithm(10,100,true,
                            (Genetic.CrossoverType)crossover,
                            (Genetic.MutationType)mutation,
                            (Genetic.SelectionType)selection);

                            data.Add(dataWithElitism);
                        }
                    }
                }
            } */
            /* var dataWithElitism = new Data();
            dataWithElitism = floorSolver.GeneticAlgorithm(10,100,true,
            Genetic.CrossoverType.SinglePoint,
            Genetic.MutationType.RandomResetting,
            Genetic.SelectionType.Roulette);
             */

            //var dataWithElitism = new Data();
            var dataWithElitism = floorSolver.GeneticAlgorithm(populationSize,generations,haveElitism,crossoverType,mutationType,selectionType);

            data.Add(dataWithElitism);
            RestartMatrix();

            //CANCER PARA CAMBIAR EL COLOR DE LOS NODOS PUERTA (Feature *)

            CreateFloorElementByMatrixIndex(floorObjects,dataWithElitism.individual.GetCromosome());
            do{
                CreateWallElement(wallObjects[Random.Range(0,wallObjects.Count)]);
                //Debug.Log("Wall");
            }while(AreWallNodeRemaining());

            foreach(GameObject node in closedNodes){
                if(node.GetComponent<Node>().Type == Node.NodeType.Door)
                node.GetComponent<Renderer>().material = floorBlockedMaterial;
            }

            //Save artworks distrubution to reconstruct the rooms in the museum
            SaveArtworksDistribution(dataWithElitism.individual.GetCromosome());

            SaveData(data,"MazeData.csv");
            data.Clear();
        }

        void SaveArtworksDistribution(Vector3Int[] chromosome)
        {
            if(File.Exists(Application.dataPath + "/SpaceOptimization/Resources/Maps/ArtworksDistribution.csv"))
            {
                File.Delete(Application.dataPath + "/SpaceOptimization/Resources/Maps/ArtworksDistribution.csv");
            }
            else
            {
                Debug.Log("File not found");
            }

            Debug.Log("Saving Artworks Distribution");
            //create a CSV file to save data from the genetic algorithm, use the a List of class Data to save the data
            string path = Application.dataPath + "/SpaceOptimization/Resources/Maps/ArtworksDistribution.csv";
            StreamWriter writer = new StreamWriter(path, false);
            
            var values = new List<string>();

            for(int i = 0; i < chromosome.Length; i++)
            {
                if (chromosome[i].x == 0) continue;
                values.Add(chromosome[i].y.ToString());
                //writer.WriteLine(chromosome[i].y);
            }
            //write values to file as a row in the CSV file
            writer.WriteLine(string.Join(";",values));


            writer.Close();
        }   


        public void BuildMap(string filename)
        {
            var panel = canvasController.panels.Find(p => p.name == "PanelStep1") as BSPPanel;
            var panelConfiguration = panel.GetPanelData();
            var width = int.TryParse(panelConfiguration[0], out int w) ? w : 0;
            var height = int.TryParse(panelConfiguration[1], out int h) ? h : 0;
            var minLeafSize = int.TryParse(panelConfiguration[2], out int min) ? min : 0;

            //use the BSP algorithm to generate the map
            bspMazeGeneration.CreateMaze(w,h,min,filename);

            matrix = bspMazeGeneration.GetMatrix();

            //patch to fix the bug of the BSP algorithm
            //mapCreator.openNodes.Clear();
            //mapCreator.closedNodes.Clear();

            mapCreator.SetNeighbors(matrix);
            this.width = matrix.GetLength(0);
            this.height = matrix.GetLength(1);

            /*matrix = mapCreator.Creator(filename);
            mapCreator.SetNeighbors(matrix);
            width = matrix.GetLength(0);
            height = matrix.GetLength(1);*/

        }

        public void TSP()
        {
            //if matrix is empty, load the example matrix
            if (matrix == null)
            {
                filename = "ExampleMap";
                matrix = mapCreator.Creator(filename);
                mapCreator.SetNeighbors(matrix);
                width = matrix.GetLength(0);
                height = matrix.GetLength(1);
            }

            /*var iterations = 1;
            var panel = canvasController.panels.Find(p => p.name == "PanelStep3") as TSPPanel;
            var panelConfiguration = panel.GetPanelData();
            for (int i = 0; i < iterations; i++)
            {
                BuildMap("BSP_Maze");
                //search for panel with the name TSPPanel and convert it to TSPPanel
                roomRecognition.TSP(matrix);
            }*/

            
            //search for panel with the name TSPPanel and convert it to TSPPanel
            var panel = canvasController.panels.Find(p => p.name == "PanelStep3") as TSPPanel;
            var panelConfiguration = panel.GetPanelData();
            roomRecognition.TSP(matrix);
        }


        //create a CSV file to save data from the genetic algorithm, use the a List of class Data to save the data
        public void SaveData(List<Data> data, string filename = "Data.csv")
        {
            string path = Application.dataPath + "/"+filename;
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine("Have Elitism;Crossover;Mutation;Selection;Valid Chromosomes;Invalid Chromosomes;Time;Best Fitness");
            foreach(Data d in data)
            {
                writer.WriteLine(d.haveElitism + ";" + d.crossover + ";" + d.mutation + ";" + d.selection + ";" + d.validChromosomes + ";" + d.invalidChromosomes + ";" + d.time + ";" + (100 - d.bestFitness));
            }
            writer.Close();
        }

        public int[,] GetMatrix()
        {
            return matrix;
        }
        public Vector3Int[] GenerateMap(){

            RestartMatrix();
            //create random floor nodes
            do{
                var randomFloorType = Random.Range(0, 3);
                switch(randomFloorType)
                {
                    case 0:
                        CreateFloorElement(debugFloorObjects[0]);
                        break;
                    case 1:
                        CreateFloorDoubleElement(debugFloorObjects[0]);
                        break;
                    case 2:
                        CreateFloorTetraElement(debugFloorObjects[0]);
                        break;
                    default:
                        break;
                }
            }while(AreFloorNodeRemaining());
            //create random wall nodes
            /* do{
                CreateWallElement(wallObjects[Random.Range(0,wallObjects.Count)]);
                //Debug.Log("Wall");
            }while(AreWallNodeRemaining()); */

            //Get Floor Map
            var floorMap = GetFloorsMap();
            //Create piece map
            var pieceMap = GetAllPieces();
            return pieceMap;
        }
        public int[] GetFloorsMap()
        {
            //copy closed nodes to avoid modifying the original list
            var copyClosedNodes = new List<GameObject>(closedNodes);
            //add open nodes to the new list
            copyClosedNodes.AddRange(openNodes);

            //create map
            int[] map = new int[width * height];
            //set map
            //1 = floor
            //0 = not floor
            foreach(GameObject g in copyClosedNodes)
            {
                var node = g.GetComponent<Node>();
                //https://stackoverflow.com/questions/2151084/map-a-2d-array-onto-a-1d-array
                var index = width * node.Position.x + node.Position.y;
                
                if(node.Type == Node.NodeType.Floor ||
                node.Type == Node.NodeType.SimplePiece ||
                node.Type == Node.NodeType.DoublePiece ||
                node.Type == Node.NodeType.TetraPiece) map[index] = 1;
                else map[index] = 0;
            }

            //clear list
            copyClosedNodes.Clear();

            //return map
            return map;
        }
        public Vector3Int[] GetAllPieces()
        {
            //copy closed nodes to avoid modifying the original list
            var copyClosedNodes = new List<GameObject>(closedNodes);
            //add open nodes to the new list
            copyClosedNodes.AddRange(openNodes);
            var floorsAmount = 0;

            //count pieces
            foreach(GameObject g in copyClosedNodes)
            {
                var node = g.GetComponent<Node>();
                if(node.Type == Node.NodeType.Floor ||
                node.Type == Node.NodeType.SimplePiece ||
                node.Type == Node.NodeType.DoublePiece ||
                node.Type == Node.NodeType.TetraPiece) floorsAmount++;
            }

            //create map
            Vector3Int[] map = new Vector3Int[floorsAmount];
            //set map values x = is piece? 1 = yes, 0 = no; y = matrix index; z = is visited? 1 = yes, 0 = no
            
            var pieceIndex = 0;

            foreach(GameObject g in copyClosedNodes)
            {
                var node = g.GetComponent<Node>();
                var index = width * node.Position.x + node.Position.y;
                if(node.Type == Node.NodeType.SimplePiece ||
                node.Type == Node.NodeType.DoublePiece ||
                node.Type == Node.NodeType.TetraPiece){
                    map[pieceIndex] = new Vector3Int(1,index,0);
                    pieceIndex++;
                } 
                if(node.Type == Node.NodeType.Floor)
                {
                    map[pieceIndex] = new Vector3Int(0,index,0);
                    pieceIndex++;
                }
            }
            //clear list
            copyClosedNodes.Clear();
            
            //return map
            return map.OrderBy(v => v.y).ToArray();
        }

        //GLOBAL METHODS TO DETECT IF ARE REMAINING NODES, HELPERS TO REMAINING NODES
        public void RestartMatrix()
        {
            //restart timer and add closed nodes to open nodes
            timer = 0f;
            foreach(GameObject g in closedNodes)
            {
                openNodes.Add(g);
                g.GetComponent<Node>().IsBlocked = false;
                if(g.GetComponent<Node>().Type == Node.NodeType.Floor || 
                g.GetComponent<Node>().Type == Node.NodeType.SimplePiece ||
                g.GetComponent<Node>().Type == Node.NodeType.DoublePiece ||
                g.GetComponent<Node>().Type == Node.NodeType.TetraPiece) {
                    g.GetComponentInChildren<Renderer>().material = unblockedMaterial;
                    g.GetComponent<Node>().SetNodeType(Node.NodeType.Floor);
                    if(g.transform.childCount == 0) continue;
                    var spawnArea = g.transform.GetChild(0);
                    if(spawnArea == null) continue;
                    foreach(Transform child in spawnArea)
                    {
                        //Debug.Log("Destroying child");
                        //Debug.Log("child name: " +  child.gameObject);
                        Destroy(child.gameObject);
                    }
                }

                //mirar en un futuro
                if(g.GetComponent<Node>().Type == Node.NodeType.Wall||
                    g.GetComponent<Node>().Type == Node.NodeType.WallPiece) {
                    g.GetComponentInChildren<Renderer>().material = unblockedMaterial;
                    g.GetComponent<Node>().SetNodeType(Node.NodeType.Wall); 
                    if(g.transform.childCount == 0) continue;
                    foreach(Transform dir in g.transform)
                    {
                        var spawnArea = dir.transform.GetChild(0);
                        if(spawnArea == null) continue;
                        foreach(Transform child in spawnArea)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }
            }
            closedNodes.Clear();

            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Door)
                {
                    g.GetComponentInChildren<Renderer>().material = unblockedMaterial;
                    closedNodes.Add(g);
                } 
                if(g.GetComponent<Node>().Type == Node.NodeType.NonTraversable) closedNodes.Add(g);
            }
            openNodes.RemoveAll(n => closedNodes.Contains(n));
        }
        private bool AreFloorNodeRemaining()
        {
            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Floor) return true;
            }
            return false;
        }
        private bool AreWallNodeRemaining()
        {
            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Wall) return true;
            }
            return false;
        }
        private bool AreChildActives(GameObject node)
        {
            foreach(Transform child in node.transform)
            {
                if(child.gameObject.activeSelf) return true;
            }
            return false;
        }
        private bool AreDoubleFloorNodeRemaining()
        {
            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Floor)
                {
                    var neighbors = g.GetComponent<Node>().GetCrossNeighbors();
                    foreach(GameObject n in neighbors)
                    {
                        if(n == null) continue;
                        if(openNodes.Contains(n) && n.GetComponent<Node>().Type == Node.NodeType.Floor) return true;
                    }
                }
            }
            return false;
        }
        private List<List<GameObject>> DoubleFloorNodeRemaining()
        {
            var doubleSpaces = new List<List<GameObject>>();
            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Floor)
                {
                    var neighbors = g.GetComponent<Node>().GetCrossNeighbors();
                    foreach(GameObject n in neighbors)
                    {
                        if(n == null || 
                        n.GetComponent<Node>().Type != Node.NodeType.Floor ||
                        !openNodes.Contains(n)) continue;

                        var doubleNodes = new List<GameObject>();
                        doubleNodes.Add(g);
                        doubleNodes.Add(n);
                        doubleSpaces.Add(doubleNodes);
                    }
                }
            }
            return doubleSpaces;
        }
        private bool AreDiagonalFloorNodeRemaining()
        {
            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Floor)
                {
                    var neighbors = g.GetComponent<Node>().GetDiagonalNeighbors();
                    foreach(GameObject n in neighbors)
                    {
                        if(n == null) continue;
                        if(openNodes.Contains(n) && n.GetComponent<Node>().Type == Node.NodeType.Floor) return true;
                    }
                }
            }
            return false;
        }
        private List<List<GameObject>> TetraFloorNodeRemaining()
        {
            var tetraSpaces = new List<List<GameObject>>();
            foreach(GameObject g in openNodes)
            {
                if(g.GetComponent<Node>().Type == Node.NodeType.Floor)
                {
                    //get diagonal neighbors
                    var diagonalNeighbors = g.GetComponent<Node>().GetDiagonalNeighbors();
                    //get cross neighbors
                    var crossNeighbors = g.GetComponent<Node>().GetCrossNeighbors();

                    foreach(GameObject d in diagonalNeighbors)
                    {
                        if(d == null || 
                        d.GetComponent<Node>().Type != Node.NodeType.Floor || 
                        !openNodes.Contains(d)) continue;
                        
                        List<GameObject> tetra = new List<GameObject>();

                        //compare diagonal neighbors with neighbors, if they have the same neighbor, add to list
                        foreach(GameObject c in crossNeighbors)
                        {
                            if(c == null || 
                            c.GetComponent<Node>().Type != Node.NodeType.Floor || 
                            !openNodes.Contains(c)) continue;

                            if(d.GetComponent<Node>().GetNeighbors().Contains(c))
                            {
                                tetra.Add(c);
                            }
                        }
                        tetra.Add(g);
                        tetra.Add(d);
                        if(tetra.Count == 4) tetraSpaces.Add(tetra);
                    }
                }
            }
            return tetraSpaces;
        }
        public void CreateFloorElementByMatrixIndex(List<GameObject> floorObjects,Vector3Int[] individual)
        {
            GameObject go;
            
            //copy floor elements list into elements list
            var elements = new List<GameObject>(floorObjects);

            if(individual.Length == 0) return;
            //positioning pieces
            foreach(var i in individual)
            {
                if(i.x == 0) continue;
                //convert i x value from 1 dimension to 2 dimension matrix index
                var x = i.y / matrix.GetLength(0);
                //convert i y value from 1 dimension to 2 dimension matrix index
                var y = i.y % matrix.GetLength(0);

                var element = elements[Random.Range(0,elements.Count)];
                //elements.Remove(element);

                //get node from matrix
                go = openNodes.Find(n => n.GetComponent<Node>().Position == new Vector2Int(x,y));
                //set material to blocked
                go.GetComponentInChildren<Renderer>().material = floorBlockedMaterial;
                //set node to blocked
                go.GetComponent<Node>().IsBlocked = true;
                //create element
                var spawnArea = go.transform.GetChild(0);
                var a = go.transform.GetChild(0).localScale.y;
                var b = element.transform.localScale.y;
                var obj = Instantiate(element, go.GetComponentInChildren<Transform>().position + new Vector3(0,(a*b)/2,0), Quaternion.identity,spawnArea);

                //Insttantate an artwork over the pedestal
                var artwork = artworkObjects[Random.Range(0,artworkObjects.Count)];

                var spawnAreaArtwork = obj.transform.GetChild(0);
                var aArtwork = obj.transform.localScale.y;
                var bArtwork = artwork.transform.localScale.y;
                var objArtwork = Instantiate(artwork, obj.GetComponentInChildren<Transform>().position + new Vector3(0,(aArtwork*bArtwork),0), Quaternion.identity,spawnAreaArtwork);

                
                //CANCER PARA DEBUGEAR
                //CANCER PARA DEBUGEAR
                //CANCER PARA DEBUGEAR
                /* var objCanvas = obj.GetComponent<InteractiveCanvas>();
                objCanvas.canvas_ui = canvasGroup;
                objCanvas.piecenameText = pieceName;
                objCanvas.museumNameText = museumName;
                objCanvas.map = markers;
                objCanvas.audioPlayer = audioPlayer; */
                //obj.GetComponent<InteractiveCanvas>().enabled = false;

                //CANCER PARA DEBUGEAR
                //CANCER PARA DEBUGEAR
                //CANCER PARA DEBUGEAR

                //obj.transform.localScale = new Vector3(1,1,1) * .2f;

                //Translate to real position

                //add to closed list
                closedNodes.Add(go);
                //remove from open list
                openNodes.Remove(go);

                //set node type to floor        
                go.GetComponent<Node>().SetNodeType(Node.NodeType.SimplePiece);
            }

            //blocking neighbors
            foreach(var i in individual)
            {
                if(i.x == 0) continue;
                //convert i x value from 1 dimension to 2 dimension matrix index
                var x = i.y / matrix.GetLength(0);
                //convert i y value from 1 dimension to 2 dimension matrix index
                var y = i.y % matrix.GetLength(0);

                //get node from matrix
                go = closedNodes.Find(n => n.GetComponent<Node>().Position == new Vector2Int(x,y));

                foreach (GameObject n in go.GetComponent<Node>().GetNeighbors()) {

                    //if node is null, or is not a floor, or is already in closed list, continue
                    if(n == null ||
                    n.GetComponent<Node>().Type != Node.NodeType.Floor ||
                    closedNodes.Contains(n)) continue;

                    //set material to blocked
                    n.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                    //add to closed list
                    closedNodes.Add(n);
                    //remove from open list
                    openNodes.Remove(n);
                }
            }
        }

        //RANDOM GENERATOR
        //create floor element
        public void CreateFloorElement(GameObject element)
        {
            //if there are no more open nodes, return
            if(openNodes.Count == 0 || !AreFloorNodeRemaining()) {
                //Debug.Log("No more open nodes");
                return;
            }

            //create gameobject
            GameObject go;
            
            //get random node from open list, then save it to go
            do
            {
                go = openNodes[Random.Range(0, openNodes.Count)];
            }while(go.GetComponent<Node>().Type != Node.NodeType.Floor);
            
            //set material to blocked
            go.GetComponentInChildren<Renderer>().material = floorBlockedMaterial;
            //set node to blocked
            go.GetComponent<Node>().IsBlocked = true;

            //create element
            var spawnArea = go.transform.GetChild(0);
            var a = go.transform.GetChild(0).localScale.y;
            var b = element.transform.localScale.y;
            Instantiate(element, go.GetComponentInChildren<Transform>().position + new Vector3(0,(a*b)/2,0), Quaternion.identity,spawnArea);
            //add to closed list
            closedNodes.Add(go);
            //remove from open list
            openNodes.Remove(go);

            //set node type to floor        
            go.GetComponent<Node>().SetNodeType(Node.NodeType.SimplePiece);

            foreach (GameObject n in go.GetComponent<Node>().GetNeighbors()) {

                //if node is null, or is not a floor, or is already in closed list, continue
                if(n == null ||
                n.GetComponent<Node>().Type != Node.NodeType.Floor ||
                closedNodes.Contains(n)) continue;

                //set material to blocked
                n.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                //add to closed list
                closedNodes.Add(n);
                //remove from open list
                openNodes.Remove(n);
            }
        }
        public void CreateFloorDoubleElement(GameObject element)
        {
            //if there are no more double open nodes, try create a single element
            if(openNodes.Count == 0 || DoubleFloorNodeRemaining().Count == 0) {

                //Debug.Log("No more double open nodes");
                CreateFloorElement(element);
                return;
            }

            //get all double spaces
            var doubleSpaces = DoubleFloorNodeRemaining();

            //get random double space
            var randomDoubleNodes = doubleSpaces[Random.Range(0, doubleSpaces.Count)];
            
            //foreach node in double space, set material to blocked
            foreach(GameObject n in randomDoubleNodes)
            {
                //set material to blocked
                n.GetComponentInChildren<Renderer>().material = floorBlockedMaterial;
                //set node to blocked
                n.GetComponent<Node>().IsBlocked = true;
                //create element
                var spawnArea = n.transform.GetChild(0);
                var a = n.transform.GetChild(0).localScale.y;
                var b = element.transform.localScale.y;
                Instantiate(element, n.GetComponentInChildren<Transform>().position + new Vector3(0,(a*b)/2,0), Quaternion.identity,spawnArea);
                //add to closed list
                closedNodes.Add(n);
                //remove from open list
                openNodes.Remove(n);
            }

    /*         var aux = randomDoubleNodes[0];        

            for(int i = 0; i < randomDoubleNodes.Count; i++)
            {
                if(randomDoubleNodes[i].GetComponent<Node>().Position.x < aux.GetComponent<Node>().Position.x ||
                randomDoubleNodes[i].GetComponent<Node>().Position.y < aux.GetComponent<Node>().Position.y)
                aux = randomDoubleNodes[i];
            } 

            aux.GetComponent<Node>().SetNodeType(Node.NodeType.DoublePiece); */

            foreach(GameObject n in randomDoubleNodes)
            {
                n.GetComponent<Node>().SetNodeType(Node.NodeType.DoublePiece);
            }
            
            //foreach neighbor of double space, set material to blocked
            foreach(GameObject rn in randomDoubleNodes)
            {
                foreach(GameObject nb in rn.GetComponent<Node>().GetNeighbors())
                {
                    //if neighbor is null, or neighbor is not a floor, or neighbor is already closed, continue
                    if(nb == null ||
                    nb.GetComponent<Node>().Type != Node.NodeType.Floor || 
                    closedNodes.Contains(nb)) continue;
                    //set material to blocked
                    nb.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                    //add to closed list
                    closedNodes.Add(nb);
                    //remove from open list
                    openNodes.Remove(nb);
                }
            }
        }
        public void CreateFloorTetraElement(GameObject element)
        {
            //if there are no more tetra open nodes, try create a double element
            if(openNodes.Count == 0 || TetraFloorNodeRemaining().Count == 0) {
                //Debug.Log("No more tetra open nodes");
                CreateFloorDoubleElement(element);
                return;
            }
            //get all tetra spaces
            var tetraSpaces = TetraFloorNodeRemaining();
            //get random tetra space
            var randomTetraNodes = tetraSpaces[Random.Range(0, tetraSpaces.Count)];
            //foreach node in tetra space, set material to blocked
            foreach(GameObject n in randomTetraNodes)
            {
                //set material to blocked
                n.GetComponentInChildren<Renderer>().material = floorBlockedMaterial;
                //set node to blocked
                n.GetComponent<Node>().IsBlocked = true;
                //create element
                var spawnArea = n.transform.GetChild(0);
                var a = n.transform.GetChild(0).localScale.y;
                var b = element.transform.localScale.y;
                Instantiate(element, n.GetComponentInChildren<Transform>().position + new Vector3(0,(a*b)/2,0), Quaternion.identity,spawnArea);
                //add to closed list
                closedNodes.Add(n);
                //remove from open list
                openNodes.Remove(n);
            }

    /*         var aux = randomTetraNodes[0];        

            for(int i = 0; i < randomTetraNodes.Count; i++)
            {
                if(randomTetraNodes[i].GetComponent<Node>().Position.x < aux.GetComponent<Node>().Position.x ||
                randomTetraNodes[i].GetComponent<Node>().Position.y < aux.GetComponent<Node>().Position.y)
                aux = randomTetraNodes[i];
            }

            aux.GetComponent<Node>().SetNodeType(Node.NodeType.TetraPiece); */

            foreach(GameObject n in randomTetraNodes)
            {
                n.GetComponent<Node>().SetNodeType(Node.NodeType.TetraPiece);
            }

            //foreach neighbor of tetra space, set material to blocked
            foreach(GameObject n in randomTetraNodes)
            {
                foreach(GameObject nb in n.GetComponent<Node>().GetNeighbors())
                {
                    //if neighbor is null, or neighbor is not a floor, or neighbor is already closed, continue
                    if(nb == null || 
                    nb.GetComponent<Node>().Type != Node.NodeType.Floor || 
                    closedNodes.Contains(nb)) continue;
                    //set material to blocked
                    nb.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                    //add to closed list
                    closedNodes.Add(nb);
                    //remove from open list
                    openNodes.Remove(nb);
                }
            }
        }
        
        //create wall element
        public void CreateWallElement(GameObject element)
        {
            //if there are no more open nodes, return
            if(openNodes.Count == 0 || !AreWallNodeRemaining()) {
                Debug.Log("No more open nodes");
                return;
            }

            //create gameobject
            GameObject go;
            //get random node from open list, then save it to go
            do
            {
                go = openNodes[Random.Range(0, openNodes.Count)];
            }while(go.GetComponent<Node>().Type != Node.NodeType.Wall);
            
            //nodos marcados como pared pero que no tienen paredes visibles
            if(!AreChildActives(go)){
                go.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                closedNodes.Add(go);
                openNodes.Remove(go);
            }else
            {
                //set material to blocked
                go.GetComponentInChildren<Renderer>().material = wallBlockedMaterial;
                //set node to blocked
                go.GetComponent<Node>().IsBlocked = true;

                //create element
                //var spawnArea = go.transform.GetChild(0);
                var dirs = new List<Transform>();
                var spawns = new List<Transform>();
                foreach(Transform d in go.transform)
                {
                    if(d.gameObject.activeSelf)
                    {
                        dirs.Add(d);
                    }
                }
                foreach(Transform d in dirs)
                {
                    foreach(Transform s in d)
                    {
                        spawns.Add(s);
                    }
                }

                var spawnArea = spawns[Random.Range(0, spawns.Count)];
                var a = spawnArea.localScale.y;
                var b = element.transform.localScale.y;
                var obj = Instantiate(element, spawnArea.transform.position + new Vector3(0,(a*b)/2,0), Quaternion.identity,spawnArea);
                //obj.GetComponent<InteractiveCanvas>().enabled = false;
                
                //set object rotation based on parent direction
                switch(spawnArea.parent.name)
                {
                    case "Down":
                        obj.transform.rotation = Quaternion.Euler(0,180,0);
                        break;
                    case "Right":
                        obj.transform.rotation = Quaternion.Euler(0,90,0);
                        break;
                    case "Left":
                        obj.transform.rotation = Quaternion.Euler(0,270,0);
                        break;
                    case "Up":
                        obj.transform.rotation = Quaternion.Euler(0,0,0);
                        break;
                    default:
                        break;
                }
                
                //add to closed list
                closedNodes.Add(go);
                //remove from open list
                openNodes.Remove(go);

                //set node type to wall piece            
                go.GetComponent<Node>().SetNodeType(Node.NodeType.WallPiece);

                /* foreach (GameObject n in go.GetComponent<Node>().GetNeighbors()) {

                    //if node is null, or is not a floor, or is already in closed list, continue
                    if(n == null ||
                    n.GetComponent<Node>().Type != Node.NodeType.Wall ||
                    closedNodes.Contains(n)) continue;

                    //set material to blocked
                    n.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                    //add to closed list
                    closedNodes.Add(n);
                    //remove from open list
                    openNodes.Remove(n);
                } */

                var nb = go.GetComponent<Node>().GetNeighbors();
                var goPos = go.GetComponent<Node>().Position;
                var right = nb.Find(x => x != null && x.GetComponent<Node>().Position.x == goPos.x + 1) ? nb.Find(x => x != null && x.GetComponent<Node>().Position.x == goPos.x + 1) : null;
                var left = nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x - 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x - 1) : null;
                var up = nb.Find(x =>x != null && x.GetComponent<Node>().Position.y == goPos.y + 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.y == goPos.y + 1) : null;
                var down = nb.Find(x =>x != null && x.GetComponent<Node>().Position.y == goPos.y - 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.y == goPos.y - 1) : null;
                var upRight = nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x + 1 && x.GetComponent<Node>().Position.y == goPos.y + 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x + 1 && x.GetComponent<Node>().Position.y == goPos.y + 1) : null;
                var upLeft = nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x - 1 && x.GetComponent<Node>().Position.y == goPos.y + 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x - 1 && x.GetComponent<Node>().Position.y == goPos.y + 1) : null;
                var downRight = nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x + 1 && x.GetComponent<Node>().Position.y == goPos.y - 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x + 1 && x.GetComponent<Node>().Position.y == goPos.y - 1) : null;
                var downLeft = nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x - 1 && x.GetComponent<Node>().Position.y == goPos.y - 1) ? nb.Find(x =>x != null && x.GetComponent<Node>().Position.x == goPos.x - 1 && x.GetComponent<Node>().Position.y == goPos.y - 1) : null;

                var conectionType = "";

                //check conection type (necesito la diagonal para reconocer la direccion de la pared) la diagonal pueden ser de tipo floor o no traversable, los laterales tambien pueden ser de tipo door

                if(right != null && up != null && upRight != null &&
                (right.GetComponent<Node>().Type == Node.NodeType.Wall || right.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (up.GetComponent<Node>().Type == Node.NodeType.Wall || up.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (upRight.GetComponent<Node>().Type == Node.NodeType.Floor || upRight.GetComponent<Node>().Type == Node.NodeType.NonTraversable)) conectionType = "UpRight";

      /*           if(right != null && up != null &&
                right.GetComponent<Node>().Type == Node.NodeType.Wall && up.GetComponent<Node>().Type == Node.NodeType.Wall) conectionType = "UpRight";

       */          /* else if(right != null && down != null &&
                right.GetComponent<Node>().Type == Node.NodeType.Wall && down.GetComponent<Node>().Type == Node.NodeType.Wall) conectionType = "DownRight"; */
                else if(right != null && down != null && downRight != null &&
                (right.GetComponent<Node>().Type == Node.NodeType.Wall || right.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (down.GetComponent<Node>().Type == Node.NodeType.Wall || down.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (downRight.GetComponent<Node>().Type == Node.NodeType.Floor || downRight.GetComponent<Node>().Type == Node.NodeType.NonTraversable)) conectionType = "DownRight";

                /* else if(left != null && up != null &&
                left.GetComponent<Node>().Type == Node.NodeType.Wall && up.GetComponent<Node>().Type == Node.NodeType.Wall) conectionType = "UpLeft";
 */
                else if(left != null && up != null && upLeft != null &&
                (left.GetComponent<Node>().Type == Node.NodeType.Wall || left.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (up.GetComponent<Node>().Type == Node.NodeType.Wall || up.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (upLeft.GetComponent<Node>().Type == Node.NodeType.Floor || upLeft.GetComponent<Node>().Type == Node.NodeType.NonTraversable)) conectionType = "UpLeft";
/* 
                else if(left != null && down != null &&
                left.GetComponent<Node>().Type == Node.NodeType.Wall && down.GetComponent<Node>().Type == Node.NodeType.Wall) conectionType = "DownLeft";
 */
                else if(left != null && down != null && downLeft != null &&
                (left.GetComponent<Node>().Type == Node.NodeType.Wall || left.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (down.GetComponent<Node>().Type == Node.NodeType.Wall || down.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (downLeft.GetComponent<Node>().Type == Node.NodeType.Floor || downLeft.GetComponent<Node>().Type == Node.NodeType.NonTraversable)) conectionType = "DownLeft";

/*                 else if(right != null && left != null && 
                right.GetComponent<Node>().Type == Node.NodeType.Wall && left.GetComponent<Node>().Type == Node.NodeType.Wall) conectionType = "Horizontal";
 */
                else if(right != null && left != null &&
                (right.GetComponent<Node>().Type == Node.NodeType.Wall || right.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (left.GetComponent<Node>().Type == Node.NodeType.Wall || left.GetComponent<Node>().Type == Node.NodeType.Door)) conectionType = "Horizontal";

/* 
                else if(up != null && down != null &&
                up.GetComponent<Node>().Type == Node.NodeType.Wall && down.GetComponent<Node>().Type == Node.NodeType.Wall) conectionType = "Vertical";
 */
                else if(up != null && down != null &&
                (up.GetComponent<Node>().Type == Node.NodeType.Wall || up.GetComponent<Node>().Type == Node.NodeType.Door) &&
                (down.GetComponent<Node>().Type == Node.NodeType.Wall || down.GetComponent<Node>().Type == Node.NodeType.Door)) conectionType = "Vertical";

                switch(conectionType)
                {
                    case "Horizontal":
                        //set material to blocked
                        right.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        left.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        //add to closed list
                        closedNodes.Add(right);
                        closedNodes.Add(left);
                        //remove from open list
                        openNodes.Remove(right);
                        openNodes.Remove(left);
                        break;

                    case "Vertical":
                        //set material to blocked
                        up.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        down.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        //add to closed list
                        closedNodes.Add(up);
                        closedNodes.Add(down);
                        //remove from open list
                        openNodes.Remove(up);
                        openNodes.Remove(down);
                        break;
                    
                    case "UpRight":
                        //set material to blocked
                        right.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        up.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        //add to closed list
                        closedNodes.Add(right);
                        closedNodes.Add(up);
                        //remove from open list
                        openNodes.Remove(right);
                        openNodes.Remove(up);
                        break;
                    
                    case "DownRight":
                        //set material to blocked
                        right.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        down.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        //add to closed list
                        closedNodes.Add(right);
                        closedNodes.Add(down);
                        //remove from open list
                        openNodes.Remove(right);
                        openNodes.Remove(down);
                        break;

                    case "UpLeft":
                        //set material to blocked
                        left.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        up.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        //add to closed list
                        closedNodes.Add(left);
                        closedNodes.Add(up);
                        //remove from open list
                        openNodes.Remove(left);
                        openNodes.Remove(up);
                        break;
                    
                    case "DownLeft":
                        //set material to blocked
                        left.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        down.GetComponentInChildren<Renderer>().material = neighborBlockedMaterial;
                        //add to closed list
                        closedNodes.Add(left);
                        closedNodes.Add(down);
                        //remove from open list
                        openNodes.Remove(left);
                        openNodes.Remove(down);
                        break;
                    
                    default:
                        break;
                }
            }
        }
    }
}