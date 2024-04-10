using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOptimization{

    public class Spawner : MonoBehaviour
    {
        Map map;
        public GameObject[] floorObjects;
        public GameObject[] wallObjects;

        private void Awake() {
            map = GameObject.Find("Map").GetComponent<Map>();
        }
    /* 
        void Start()
        {
            SpawnFloor();
            //SpawnWalls();
        }   

        void SpawnFloor() {
            for (int x = 0; x < map.GetMatrix().GetLength(0); x++) {
                for (int y = 0; y < map.GetMatrix().GetLength(1); y++) {
                    Vector3 spawnPos = new Vector3(x, 0, y);
                    Instantiate(floorObjects[Random.Range(0, floorObjects.Length)], spawnPos, Quaternion.identity);
                }
            }
        } */
    }
}
