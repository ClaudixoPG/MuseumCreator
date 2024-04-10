using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOptimization
{
    [System.Serializable]
    public class Node : MonoBehaviour
    {
        public enum NodeType { NonTraversable, Floor, Wall, Door, Start, End, SimplePiece,DoublePiece, TetraPiece, WallPiece};
        [SerializeField]
        private Vector2Int position;
        [SerializeField]
        public Vector2Int Position { get { return position; } }
        [SerializeField]
        private List<GameObject> neighbors = new List<GameObject>();
        [SerializeField]
        private NodeType type;
        public NodeType Type { get { return type; } }
        [SerializeField]
        private bool isBlocked = false;
        public bool IsBlocked { get { return isBlocked; } set { isBlocked = value; } }
        private double gCost;
        public double GCost { get { return gCost; } set { gCost = value; } }
        private double hCost;
        public double HCost { get { return hCost; } set { hCost = value; } }
        private double fCost;
        public double FCost { get { return fCost; } set { fCost = value; } }
        private Node parent;
        public Node Parent { get { return parent; } set { parent = value; } }
        
        public void SetData(Vector2Int position, int type)
        {
            this.position = position;
            this.type = GetNodeType(type);
        }
        public void SetNeighbors(List<GameObject> neighbors)
        {
            this.neighbors = neighbors;
        }

        public NodeType GetNodeType(int value)
        {
            switch (value)
            {
                case 0:
                    return NodeType.NonTraversable;
                case 1:
                    return NodeType.Wall;
                case 2:
                    return NodeType.Floor;
                case 3:
                    return NodeType.Door;
                case 8:
                    return NodeType.Start;
                case 9:
                    return NodeType.End;
                default:
                    return NodeType.NonTraversable;
            }
        }
        public List<GameObject> GetNeighbors()
        {
            return neighbors;
        }

        public List<GameObject> GetCrossNeighbors()
        {
            List<GameObject> neighborsInFourDirections = new List<GameObject>();
            foreach (GameObject neighbor in neighbors)
            {
                if(neighbor == null) continue;
                
                if (neighbor.GetComponent<Node>().position.x == position.x + 1 && neighbor.GetComponent<Node>().position.y == position.y)
                {
                    neighborsInFourDirections.Add(neighbor);
                }
                if (neighbor.GetComponent<Node>().position.x == position.x - 1 && neighbor.GetComponent<Node>().position.y == position.y)
                {
                    neighborsInFourDirections.Add(neighbor);
                }
                if (neighbor.GetComponent<Node>().position.x == position.x && neighbor.GetComponent<Node>().position.y == position.y + 1)
                {
                    neighborsInFourDirections.Add(neighbor);
                }
                if (neighbor.GetComponent<Node>().position.x == position.x && neighbor.GetComponent<Node>().position.y == position.y - 1)
                {
                    neighborsInFourDirections.Add(neighbor);
                }
            }
            return neighborsInFourDirections;
        }

        public List<GameObject> GetDiagonalNeighbors()
        {
            List<GameObject> diagonalNeighbors = new List<GameObject>();
            foreach (GameObject neighbor in neighbors)
            {
                if(neighbor == null) continue;
                if (neighbor.GetComponent<Node>().position.x == position.x + 1 && neighbor.GetComponent<Node>().position.y == position.y + 1)
                {
                    diagonalNeighbors.Add(neighbor);
                }
                else if (neighbor.GetComponent<Node>().position.x == position.x - 1 && neighbor.GetComponent<Node>().position.y == position.y - 1)
                {
                    diagonalNeighbors.Add(neighbor);
                }
                else if (neighbor.GetComponent<Node>().position.x == position.x + 1 && neighbor.GetComponent<Node>().position.y == position.y - 1)
                {
                    diagonalNeighbors.Add(neighbor);
                }
                else if (neighbor.GetComponent<Node>().position.x == position.x - 1 && neighbor.GetComponent<Node>().position.y == position.y + 1)
                {
                    diagonalNeighbors.Add(neighbor);
                }
            }
            return diagonalNeighbors;
        }

        //set the node type
        public void SetNodeType(NodeType type)
        {
            this.type = type;
        }

    }
}