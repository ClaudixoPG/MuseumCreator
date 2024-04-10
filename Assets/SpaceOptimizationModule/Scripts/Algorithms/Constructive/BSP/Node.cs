using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PCG.Utilities;

namespace PCG{
    public class Node
    {
        Tuple<int, int> initPoint;
        Tuple<int, int> endPoint;
        Room room;
        Node leftChild;
        Node rightChild;
        SplitDirection splitDirection;

        public Node(Tuple<int, int> initPoint, Tuple<int, int> endPoint, SplitDirection splitDirection = SplitDirection.Default, Node leftChild = null, Node rightChild = null, Room room = null)
        {
            this.InitPoint = initPoint;
            this.EndPoint = endPoint;
            this.SplitDirection = splitDirection;
            this.LeftChild = leftChild;
            this.RightChild = rightChild;
            this.Room = room;
        }

        public Node LeftChild { get => leftChild; set => leftChild = value; }
        public Node RightChild { get => rightChild; set => rightChild = value; }
        public Tuple<int, int> InitPoint { get => initPoint; set => initPoint = value; }
        public Tuple<int, int> EndPoint { get => endPoint; set => endPoint = value; }
        public Room Room { get => room; set => room = value; }
        public SplitDirection SplitDirection { get => splitDirection; set => splitDirection = value; }
    }

    

    public class Room
    {
        Tuple<int, int> initPoint;
        Tuple<int, int> endPoint;

        public Room(Tuple<int, int> initPoint, Tuple<int, int> endPoint)
        {
            this.initPoint = initPoint;
            this.endPoint = endPoint;
        }

        public List<Tuple<int,int>> RoomCoordinates()
        {
            List<Tuple<int, int>> coordinates = new List<Tuple<int, int>>();
            for (int i = initPoint.Item1; i < endPoint.Item1; i++)
            {
                for (int j = initPoint.Item2; j < endPoint.Item2; j++)
                {
                    coordinates.Add(new Tuple<int, int>(i, j));
                }
            }
            return coordinates;
        }
    }
}

