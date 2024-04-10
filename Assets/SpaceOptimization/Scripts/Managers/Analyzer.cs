using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace SpaceOptimization{
    public class Analyzer : MonoBehaviour
    {
        private float spaceOptimization = 0;
        Map map;
        private void Awake() {
            map = GameObject.Find("Map").GetComponent<Map>();
        }

        public float CalculateEfficiency()
        {
            float efficiency = 100 * (SumAllCerosOnMatrix() / (MatrixArea() - SumNtandD()));
            Debug.Log("Efficiency: " + efficiency + "%");
            return efficiency;
        }



        public float CalculateEfficiency(int[,] matrix)
        {
            float efficiency = 100 * (SumAllPiecesOnMatrix(matrix) / (MatrixArea() - SumNtandD()));
            return efficiency;
        }

        public float CalculateEfficiency(int[] array)
        {
            float efficiency = 100 * (SumAllPiecesOnArray(array) / (MatrixArea() - SumNtandD()));
            Debug.Log("Efficiency: " + efficiency + "%");
            return efficiency;
        }

        public float CalculateEfficiency(Vector3Int[] array)
        {
            float efficiency = 100 * (SumAllPiecesOnArray(array) / (MatrixArea() - SumNtandD()));
            //Debug.Log("Efficiency: " + efficiency + "%");
            return efficiency;
        }
            public float CalculateEfficiency(Vector2Int[] array)
        {
            float efficiency = 0;
            Debug.Log("Efficiency: " + efficiency + "%");
            return efficiency;
        }


        private float MatrixArea()
        {
            return map.GetMatrix().GetLength(0) * map.GetMatrix().GetLength(1);
        }
        private float SumNtandD()
        {
            float sum = 0;
            for(int i = 0; i < map.GetMatrix().GetLength(0); i++)
            {
                for(int j = 0; j < map.GetMatrix().GetLength(1); j++)
                {
                    if(map.GetMatrix()[i,j] == 0 || map.GetMatrix()[i,j] == 3)
                    {
                        sum++;
                    }
                }
            }
            return sum;
        }
        private float SumAllCerosOnMatrix()
        {
            float sum = 0;
            for(int i = 0; i < map.GetMatrix().GetLength(0); i++)
            {
                for(int j = 0; j < map.GetMatrix().GetLength(1); j++)
                {
                    if(map.GetMatrix()[i,j] == 0)
                    {
                        sum++;
                    }
                }
            }
            return sum;
        }
        private float SumAllCerosOnMatrix(int[,] matrix)
        {
            float sum = 0;
            for(int i = 0; i < matrix.GetLength(0); i++)
            {
                for(int j = 0; j < matrix.GetLength(1); j++)
                {
                    if(matrix[i,j] == 0)
                    {
                        sum++;
                    }
                }
            }
            return sum;
        }
        private float SumWandFOnMatrix()
        {
            float sum = 0;
            for(int i = 0; i < map.GetMatrix().GetLength(0); i++)
            {
                for(int j = 0; j < map.GetMatrix().GetLength(1); j++)
                {
                    if(map.GetMatrix()[i,j] == 1 || map.GetMatrix()[i,j] == 2)
                    {
                        sum++;
                    }
                }
            }
            return sum;
        }
        private float SumWandFOnMatrix(List<GameObject> nodes)
        {
            float sum = 0;
            foreach(GameObject n in nodes)
            {
                if(n.GetComponent<Node>().IsBlocked)
                {
                    sum++;
                }
            }
            return sum;
        }
        private float SumAllPiecesOnMatrix()
        {
            float sum = 0;
            for(int i = 0; i < map.GetMatrix().GetLength(0); i++)
            {
                for(int j = 0; j < map.GetMatrix().GetLength(1); j++)
                {
                    //plus floor pieces value
                    if(map.GetMatrix()[i,j] == 4) sum++;
                    if(map.GetMatrix()[i,j] == 5) sum+=2;
                    if(map.GetMatrix()[i,j] == 6) sum+=4;
                    //plus wall pieces value
                    if(map.GetMatrix()[i,j] == 7) sum++;
                }
            }
            return sum;
        }
        private float SumAllPiecesOnArray(int[] array)
        {
            float sum = 0;
            foreach(int i in array) if(array[i] == 1)sum++;
            return sum;
        }
        private float SumAllPiecesOnMatrix(int[,] matrix)
        {
            float sum = 0;
            for(int i = 0; i < matrix.GetLength(0); i++)
            {
                for(int j = 0; j < matrix.GetLength(1); j++)
                {
                    //plus floor pieces value
                    if(matrix[i,j] == 4) sum++;
                    if(matrix[i,j] == 5) sum+=2;
                    if(matrix[i,j] == 6) sum+=4;
                    //plus wall pieces value
                    if(matrix[i,j] == 7) sum++;
                }
            }
            return sum;
        }

        private float SumAllPiecesOnArray(Vector3Int[] array)
        {
            float sum = 0;

            for(int i = 0; i < array.Length; i++)
            {
                if(array[i].x == 1) sum++;
            }
            return sum;
        }


        public bool CheckIfIndividualIsLegal(Vector3Int[] individual)
        {
            //get map size
            int width = map.GetMatrix().GetLength(0);
            int height = map.GetMatrix().GetLength(1);

            //check tetra pieces
            for(int i = 0; i < individual.Length; i ++)
            {
                //if gene is 0, continue
                if(individual[i].x == 0) continue;
                
                //check tetra pieces

                //check if diagonal gene from individual contains a value on y axis 
                if(individual.Contains(new Vector3Int(1,i,0)) && individual.Contains(new Vector3Int(1,i+width+1,0)))
                {
                    //check if lateral genes contains a value on x axis
                    if(individual.Contains(new Vector3Int(1,i+1,0)) &&
                    individual.Contains(new Vector3Int(1,i+width,0)))
                    {
                        if(individual.Contains(new Vector3Int(1,i-(width)-1,0)) || // 1 to left and 1 up
                        individual.Contains(new Vector3Int(1,i-(width),0)) || // 1 up
                        individual.Contains(new Vector3Int(1,i-(width)+1,0)) || // 1 up and 1 right
                        individual.Contains(new Vector3Int(1,i-(width)+2,0)) || // 1 up and 2 right
                        individual.Contains(new Vector3Int(1,i-1,0)) || // 1 to the left
                        individual.Contains(new Vector3Int(1,i+2,0)) || // 2 to the right
                        individual.Contains(new Vector3Int(1,i+(width)-1,0)) || // 1 down and 1 left
                        individual.Contains(new Vector3Int(1,i+(width)+2,0)) || // 2 to the right and 1 down
                        individual.Contains(new Vector3Int(1,i+(width*2)-1,0)) || // 2 down and 1 left
                        individual.Contains(new Vector3Int(1,i+(width*2),0)) || // 2 down
                        individual.Contains(new Vector3Int(1,i+(width*2)+1,0)) || // 2 down and 1 right
                        individual.Contains(new Vector3Int(1,i+(width*2)+2,0))) // 2 down and 2 right
                        {
                            //Debug.Log("Pieza Ilegal");
                            return false;
                        }else{
                            //Debug.Log("Hay espacio para la pieza de 2x2");
                            //check if it´s a tetra piece (check diagonal, horizontal and vertical pieces )
                            var ind1 = Array.Find(individual, element => element.y == i);
                            var ind2 = Array.Find(individual, element => element.y == i + 1);
                            var ind3 = Array.Find(individual, element => element.y == i + width);
                            var ind4 = Array.Find(individual, element => element.y == i + width + 1);

                            ind1 = new Vector3Int(1,i,1);
                            ind2 = new Vector3Int(1,i + 1,1);
                            ind3 = new Vector3Int(1,i + width,1);
                            ind4 = new Vector3Int(1,i + width + 1,1);
                        }
                    }else
                    {
                        //Debug.Log("Pieza Ilegal");
                        return false;
                    }
                }
            }
            //Check horizontal double pieces
            for(int i = 0; i < individual.Length; i ++)
            {
                //if gene is 0 or was checked before, continue
                if(individual[i].x == 0) continue;

                //check if right gen from individual contains a value on y axis
                if(individual.Contains(new Vector3Int(1,i,0)) && 
                individual.Contains(new Vector3Int(1,i+1,0)))
                {
                    //Debug.Log("Hay espacio para la pieza de 1x2");

                    //check if next gene in right side contains 1 value on x axis
                    if(individual.Contains(new Vector3Int(1,i-(width)-1,0)) || // 1 to left and 1 up
                    individual.Contains(new Vector3Int(1,i-(width),0)) || // 1 up
                    individual.Contains(new Vector3Int(1,i-(width)+1,0)) || // 1 up and 1 right
                    individual.Contains(new Vector3Int(1,i-(width)+2,0)) || // 1 up and 2 right
                    individual.Contains(new Vector3Int(1,i-1,0)) || // 1 to the left
                    individual.Contains(new Vector3Int(1,i+2,0)) || // 2 to the right
                    individual.Contains(new Vector3Int(1,i+(width)-1,0)) || // 1 down and 1 left
                    individual.Contains(new Vector3Int(1,i+width,0)) || // 1 down
                    individual.Contains(new Vector3Int(1,i+width+1,0)) || // 1 down and 1 right
                    individual.Contains(new Vector3Int(1,i+width+2,0))) // 1 down and 2 right
                    {
                        //Debug.Log("Pieza Ilegal");
                        return false;
                    }else
                    {
                        var ind1 = Array.Find(individual, element => element.y == i);
                        var ind2 = Array.Find(individual, element => element.y == i + 1);
                        ind1 = new Vector3Int(1,i,1);
                        ind2 = new Vector3Int(1,i + 1,1);
                    }
                }
            }
            //Check vertical double pieces
            for(int i = 0; i < individual.Length; i ++)
            {
                //if gene is 0 or was checked before, continue
                if(individual[i].x == 0) continue;

                //check if down gen from individual contains a value on y axis
                if(individual.Contains(new Vector3Int(1,i,0)) &&
                individual.Contains(new Vector3Int(1,i+width,0)))
                {
                    //Debug.Log("Hay espacio para la pieza de 2x1");
                    //check if next gene in down side contains 1 value on x axis
                    if(individual.Contains(new Vector3Int(1,i-(width)-1,0)) || // 1 to left and 1 up
                    individual.Contains(new Vector3Int(1,i-(width),0)) || // 1 up
                    individual.Contains(new Vector3Int(1,i-(width)+1,0)) || // 1 up and 1 right
                    individual.Contains(new Vector3Int(1,i-1,0)) || // 1 to the left
                    individual.Contains(new Vector3Int(1,i+1,0)) || // 1 to the right
                    individual.Contains(new Vector3Int(1,i+(width)-1,0)) || // 1 down and 1 left
                    individual.Contains(new Vector3Int(1,i+width+1,0)) || // 1 down and 1 right
                    individual.Contains(new Vector3Int(1,i+(width*2)-1,0)) || // 2 down and 1 left
                    individual.Contains(new Vector3Int(1,i+width*2,0)) || // 2 down
                    individual.Contains(new Vector3Int(1,i+width*2+1,0))) // 2 down and 1 right
                    {
                        //Debug.Log("Pieza Ilegal");
                        return false;
                    }else{
                        var ind1 = Array.Find(individual, element => element.y == i);
                        var ind2 = Array.Find(individual, element => element.y == i + width);
                        ind1 = new Vector3Int(1,i,1);
                        ind2 = new Vector3Int(1,i + width,1);
                    }

                    
                    //continue;
                }
            }
            //check single piece
            for(int i = 0; i < individual.Length; i ++)
            {
                //if gene is 0 or was checked before, continue
                if(individual[i].x == 0) continue;

                if(individual.Contains(new Vector3Int(1,i,0)))
                {
                    //Debug.Log("Hay espacio para la pieza de 1x1");
                    //check if its eight neighbor genes contains 1 value on x axis
                    if(individual.Contains(new Vector3Int(1,i+1,1)) || //right
                    individual.Contains(new Vector3Int(1,i-1,1)) || //left
                    individual.Contains(new Vector3Int(1,i+width,1)) || //down
                    individual.Contains(new Vector3Int(1,i-width,1)) || //up
                    individual.Contains(new Vector3Int(1,i+width+1,1)) || //down right
                    individual.Contains(new Vector3Int(1,i+width-1,1)) || //down left
                    individual.Contains(new Vector3Int(1,i-width+1,1)) || //up right
                    individual.Contains(new Vector3Int(1,i-width-1,1))) //up left
                    {
                        //Debug.Log("Pieza Ilegal");
                        return false;
                    }else
                    {
                        var ind1 = Array.Find(individual, element => element.y == i);
                        ind1 = new Vector3Int(1,i,1);
                    }
                }


            }
            return true;
        }
    } 
}