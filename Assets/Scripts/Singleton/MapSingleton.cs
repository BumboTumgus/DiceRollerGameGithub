using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapSingleton : MonoBehaviour
{
    private const int MAP_DIVISION_COUNT = 10;
    private const int MAP_DIVISION_REST_OVERRIDE = 5;
    private const int MAX_NODE_COUNT_PER_DIVISION = 4;

    private const float MAP_CONTENT_WIDTH = 1240;
    private const float MAP_NODE_VERTICAL_SPACING = 300;
    private const float MAP_SIDE_MARGINS = 200;
    private const float MAP_TOP_MARGINS = 200;
    private const float MAP_CONNECTION_EDGE_TRIM = 65;
    private const float MAP_NODE_RANDOMNESS= 50;

    public static MapSingleton Instance;

    [SerializeField] private GameObject _mapNodeToInstantiate;
    [SerializeField] private GameObject _mapNodeConnectionToInstantiate;
    [SerializeField] private Transform _mapNodeParent;
    [SerializeField] private Sprite[] _mapNodeArtwork;
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
        ClearMap();
        CreateNewMapData();
        DrawMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            Start();
    }

    public void CreateNewMapData()
    {
        // Create all our nodes for the map
        _mapNodes = new MapNode[MAP_DIVISION_COUNT + 1][];

        _mapNodes[0] = new MapNode[] { new MapNode(MapNode.MapNodeType.StartingNode, 0) };
        _mapNodes[MAP_DIVISION_COUNT] = new MapNode[] { new MapNode(MapNode.MapNodeType.BossNode, 0) };
        Debug.Log("TOTAL NODE COUNT IS " + _mapNodes.Length);
        for(int mapDivisionIndex = 1; mapDivisionIndex < MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            _mapNodes[mapDivisionIndex] = new MapNode[UnityEngine.Random.Range(2, MAX_NODE_COUNT_PER_DIVISION + 1)];
            Debug.Log("length of mapnode at index " + mapDivisionIndex + " is " + _mapNodes[mapDivisionIndex].Length);
            for (int nodeIndex = 0; nodeIndex < _mapNodes[mapDivisionIndex].Length; nodeIndex++)
            {
                _mapNodes[mapDivisionIndex][nodeIndex] = new MapNode(nodeIndex);
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
        for (int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            Debug.LogFormat("CHECKING IF ANY OFT HESE ARE NULL: NEXT NODE: {0} || CURRENT NODE: {1}", _mapNodes[mapDivisionIndex + 1][0], _mapNodes[mapDivisionIndex][0]);
            _mapNodes[mapDivisionIndex + 1][0].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][0]);
            Debug.LogFormat("Connecting node {0}:{1} to {2}:{3}", mapDivisionIndex, _mapNodes[mapDivisionIndex].Length - 1, mapDivisionIndex + 1, _mapNodes[mapDivisionIndex + 1].Length - 1);
            _mapNodes[mapDivisionIndex + 1][_mapNodes[mapDivisionIndex + 1].Length - 1].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][_mapNodes[mapDivisionIndex].Length -1]);
        }

        // Backtrack a connection from any central nodes ahead that don't have a connection
        for (int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT - 1; mapDivisionIndex++)
        {
            Debug.LogFormat("<color=#ff0000>checking if we ned to backtrack link mapdiv index {0} with a length of {1} to div index {2}</color>", mapDivisionIndex + 1, _mapNodes[mapDivisionIndex + 1].Length, mapDivisionIndex);
            // Start connecting any central 1 ahead of the count backwards towards one of these nodes
            if (_mapNodes[mapDivisionIndex + 1].Length > 2)
            {
                Debug.Log("Linking central nodes");
                for(int nodeIndex = 1; nodeIndex < _mapNodes[mapDivisionIndex + 1].Length - 1; nodeIndex++) 
                {
                    bool nodeConnectionSuccess = false;
                    int currentNodeToCheck = UnityEngine.Random.Range(0,_mapNodes[mapDivisionIndex].Length);
                    Debug.LogFormat("Backlinking node " + nodeIndex + "to our rnadom target of " + currentNodeToCheck);
                    while (!nodeConnectionSuccess)
                    {
                        bool crossCheckSuccessful = true;

                        if (_mapNodes[mapDivisionIndex].Length - 1 == currentNodeToCheck)
                        { 
                            nodeConnectionSuccess = true;
                            break;
                        }

                        for (int nodeCrissCrossIndex = currentNodeToCheck + 1; nodeCrissCrossIndex < _mapNodes[mapDivisionIndex].Length; nodeCrissCrossIndex++)
                            if (_mapNodes[mapDivisionIndex][nodeCrissCrossIndex].LowestOutgoingConnectedNodeIndex < nodeIndex)
                            {
                                Debug.Log("node cross detected, changing index from " + currentNodeToCheck + " to " + nodeCrissCrossIndex);
                                currentNodeToCheck = nodeCrissCrossIndex;
                                crossCheckSuccessful = false;
                                break;
                            }

                        if (!crossCheckSuccessful)
                            continue;

                        nodeConnectionSuccess = true;
                    }

                    // Build the connection here
                    Debug.LogFormat("Reverse Connecting node {0}:{1} to {2}:{3}", mapDivisionIndex, nodeIndex, mapDivisionIndex + 1, currentNodeToCheck);
                    _mapNodes[mapDivisionIndex + 1][nodeIndex].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][currentNodeToCheck]);
                }
            }
        }
    }

    public void DrawMap()
    {
        RectTransform rectTransform = _mapNodeParent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, MAP_TOP_MARGINS * 2 + MAP_NODE_VERTICAL_SPACING * MAP_DIVISION_COUNT);

        // Draw Nodes
        for(int mapDivisionIndex = 0; mapDivisionIndex <= MAP_DIVISION_COUNT; mapDivisionIndex++) 
        { 
            for(int mapNodeIndex = 0; mapNodeIndex < _mapNodes[mapDivisionIndex].Length; mapNodeIndex++) 
            {
                RectTransform mapNode = Instantiate(_mapNodeToInstantiate, _mapNodeParent).GetComponent<RectTransform>();
                if (mapNode != null)
                {
                    mapNode.localPosition = new Vector2((MAP_CONTENT_WIDTH - MAP_SIDE_MARGINS * 2) * (((float)mapNodeIndex + 1) / ((float)_mapNodes[mapDivisionIndex].Length + 1)) - (MAP_CONTENT_WIDTH / 2 - MAP_SIDE_MARGINS),
                        (MAP_DIVISION_COUNT - mapDivisionIndex) * -MAP_NODE_VERTICAL_SPACING - MAP_TOP_MARGINS);
                    mapNode.localPosition += new Vector3(UnityEngine.Random.Range(-MAP_NODE_RANDOMNESS, MAP_NODE_RANDOMNESS), UnityEngine.Random.Range(-MAP_NODE_RANDOMNESS, MAP_NODE_RANDOMNESS));

                    SetMapNodeImageBasedOnNodeType(mapNode.GetComponent<Image>(), _mapNodes[mapDivisionIndex][mapNodeIndex].AssignedMapNodeType);
                }
                _mapNodes[mapDivisionIndex][mapNodeIndex].MapNodeUI = mapNode;
            }
        }

        // Draw Lines Connecting Map Options
        for (int mapDivisionIndex = 0; mapDivisionIndex <= MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            for (int mapNodeIndex = 0; mapNodeIndex < _mapNodes[mapDivisionIndex].Length; mapNodeIndex++)
            {
                MapNode currentNode = _mapNodes[mapDivisionIndex][mapNodeIndex];
                foreach (MapNode outgoingConnectedNode in currentNode.OutgoingNodeConnections)
                {
                    RectTransform connectionRectTransform = Instantiate(_mapNodeConnectionToInstantiate, _mapNodeParent).GetComponent<RectTransform>();
                    UILineRenderer lineRenderer = connectionRectTransform.GetComponent<UILineRenderer>();

                    connectionRectTransform.localPosition = new Vector2(0, 0);

                    lineRenderer.points[0] = currentNode.MapNodeUI.localPosition + Vector3.Normalize(outgoingConnectedNode.MapNodeUI.localPosition - currentNode.MapNodeUI.localPosition) * MAP_CONNECTION_EDGE_TRIM;
                    lineRenderer.points[1] = outgoingConnectedNode.MapNodeUI.localPosition + Vector3.Normalize(currentNode.MapNodeUI.localPosition - outgoingConnectedNode.MapNodeUI.localPosition) * MAP_CONNECTION_EDGE_TRIM;
                }
            }
        }
    }

    private void ClearMap()
    {
        for(int childIndex = 0;  childIndex < _mapNodeParent.childCount; childIndex++)
            Destroy(_mapNodeParent.GetChild(childIndex).gameObject);
    }

    private void SetMapNodeImageBasedOnNodeType(Image mapNodeArt, MapNode.MapNodeType mapNodeType)
    {
        switch (mapNodeType)
        {
            case MapNode.MapNodeType.CombatNode:
                mapNodeArt.sprite = _mapNodeArtwork[0];
                break;
            case MapNode.MapNodeType.EliteCombatNode:
                mapNodeArt.sprite = _mapNodeArtwork[1];
                break;
            case MapNode.MapNodeType.EventNode:
                mapNodeArt.sprite = _mapNodeArtwork[2];
                break;
            case MapNode.MapNodeType.ShopNode:
                mapNodeArt.sprite = _mapNodeArtwork[3];
                break;
            case MapNode.MapNodeType.RestNode:
                mapNodeArt.sprite = _mapNodeArtwork[4];
                break;
            case MapNode.MapNodeType.BossNode:
                mapNodeArt.sprite = _mapNodeArtwork[5];
                break;
            case MapNode.MapNodeType.StartingNode:
                mapNodeArt.sprite = _mapNodeArtwork[6];
                break;
            default:
                break;
        }
    }

}
