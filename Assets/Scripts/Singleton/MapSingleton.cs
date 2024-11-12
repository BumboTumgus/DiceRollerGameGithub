using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapSingleton : MonoBehaviour
{
    private const int MAP_DIVISION_COUNT = 10;
    private const int MAP_DIVISION_REST_OVERRIDE = 5;
    private const int MAX_NODE_COUNT_PER_DIVISION = 4;

    public static MapSingleton Instance;

    private MapNode[][] _mapNodes;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    //THIS IS FOR TESTING  UKE THIS AFTER
    private void Start()
    {
        CreateNewMap();
    }

    public void CreateNewMap()
    {
        // Create all our nodes for the map
        _mapNodes = new MapNode[MAP_DIVISION_COUNT + 1][];

        _mapNodes[0] = new MapNode[] { new MapNode(MapNode.MapNodeType.StartingNode)};
        _mapNodes[MAP_DIVISION_COUNT] = new MapNode[] { new MapNode(MapNode.MapNodeType.BossNode) };
        for(int mapDivisionIndex = 1; mapDivisionIndex < MAP_DIVISION_COUNT - 1; mapDivisionIndex++)
        {
            _mapNodes[mapDivisionIndex] = new MapNode[UnityEngine.Random.Range(2, MAX_NODE_COUNT_PER_DIVISION + 1)];
            Debug.Log("length of mapnode at index " + mapDivisionIndex + " is " + _mapNodes[mapDivisionIndex].Length);
            for (int nodeIndex = 0; nodeIndex < _mapNodes[mapDivisionIndex].Length; nodeIndex++)
            {
                _mapNodes[mapDivisionIndex][nodeIndex] = new MapNode();
                _mapNodes[mapDivisionIndex][nodeIndex].RandomizeNode();
                Debug.Log("Our rolled nodetype for index " + nodeIndex + " is " + _mapNodes[mapDivisionIndex][nodeIndex].AssignedMapNodeType);
            }
        }
        Debug.Log("NODES ALL CREATED AND ROLLED");

        Debug.Log("length of mapnodes at 5 is " + _mapNodes[MAP_DIVISION_REST_OVERRIDE].Length);
        // Make sure a division of the nodes are all rests
        for (int nodeIndex = 0; nodeIndex < _mapNodes[MAP_DIVISION_REST_OVERRIDE].Length; nodeIndex++)
        {
            Debug.Log("IS this node null? " + _mapNodes[MAP_DIVISION_REST_OVERRIDE][nodeIndex]);
            _mapNodes[MAP_DIVISION_REST_OVERRIDE][nodeIndex].AssignedMapNodeType = MapNode.MapNodeType.RestNode;
        }

        // Connect every node on the outsides of the map.
        for (int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT - 1; mapDivisionIndex++)
        {
            Debug.LogFormat("Connecting node {0}:{1} to {2}:{3}", mapDivisionIndex, 0, mapDivisionIndex + 1, 0);
            _mapNodes[mapDivisionIndex + 1][0].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][0]);
            Debug.LogFormat("Connecting node {0}:{1} to {2}:{3}", mapDivisionIndex, _mapNodes[mapDivisionIndex].Length - 1, mapDivisionIndex + 1, _mapNodes[mapDivisionIndex + 1].Length - 1);
            _mapNodes[mapDivisionIndex + 1][_mapNodes[mapDivisionIndex + 1].Length - 1].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][_mapNodes[mapDivisionIndex].Length -1]);
        }

    }

    private void GetPathData(int startingNodeCount, int endingNodeCount)
    {

    }
}
