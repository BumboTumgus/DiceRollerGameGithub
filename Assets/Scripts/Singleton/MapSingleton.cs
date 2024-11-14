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
    private const float MAP_NODE_VERTICAL_SPACING = 350;
    private const float MAP_SIDE_MARGINS = 100;
    private const float MAP_TOP_MARGINS = 200;
    private const float MAP_CONNECTION_EDGE_TRIM = 65;
    private const float MAP_NODE_RANDOMNESS= 50;

    public static MapSingleton Instance;

    [SerializeField] private GameObject _mapNodeToInstantiate;
    [SerializeField] private GameObject _mapNodeConnectionToInstantiate;
    [SerializeField] private Transform _mapNodeParent;
    [SerializeField] private Sprite[] _mapNodeArtwork;
    private MapNode[][] _mapNodes;

    private int _currentExplorationIndex = 0;

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
        SetupMapButtons();
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

        //_mapNodes[0] = new MapNode[] { new MapNode(MapNode.MapNodeType.StartingNode, 0) };
        _mapNodes[MAP_DIVISION_COUNT] = new MapNode[] { new MapNode(MapNode.MapNodeType.BossNode, 0) };
        for(int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            _mapNodes[mapDivisionIndex] = new MapNode[UnityEngine.Random.Range(2, MAX_NODE_COUNT_PER_DIVISION + 1)];
            for (int nodeIndex = 0; nodeIndex < _mapNodes[mapDivisionIndex].Length; nodeIndex++)
            {
                _mapNodes[mapDivisionIndex][nodeIndex] = new MapNode(nodeIndex);
                _mapNodes[mapDivisionIndex][nodeIndex].RandomizeNode();
            }
        }

        // Make sure a division of the nodes are all rests
        for (int nodeIndex = 0; nodeIndex < _mapNodes[MAP_DIVISION_REST_OVERRIDE].Length; nodeIndex++)
            _mapNodes[MAP_DIVISION_REST_OVERRIDE][nodeIndex].AssignedMapNodeType = MapNode.MapNodeType.RestNode;

        // Connect every node on the outsides of the map.
        for (int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            _mapNodes[mapDivisionIndex + 1][0].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][0]);
            _mapNodes[mapDivisionIndex + 1][_mapNodes[mapDivisionIndex + 1].Length - 1].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][_mapNodes[mapDivisionIndex].Length -1]);
        }


        // Backtrack a connection from any central nodes ahead that don't have a connection
        for (int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            // Start connecting any central 1 ahead of the count backwards towards one of these nodes
            if (_mapNodes[mapDivisionIndex + 1].Length > 2)
            {
                for(int nodeIndex = 1; nodeIndex < _mapNodes[mapDivisionIndex + 1].Length - 1; nodeIndex++) 
                {
                    bool nodeConnectionSuccess = false;
                    int currentNodeToCheck = UnityEngine.Random.Range(0,_mapNodes[mapDivisionIndex].Length);
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
                                currentNodeToCheck = nodeCrissCrossIndex;
                                crossCheckSuccessful = false;
                                break;
                            }

                        if (!crossCheckSuccessful)
                            continue;

                        nodeConnectionSuccess = true;
                    }

                    // Build the connection here
                    _mapNodes[mapDivisionIndex + 1][nodeIndex].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][currentNodeToCheck]);
                }
            }


            // Start connecting the current central nodes with no connections to any of the ahead ones.
            if (_mapNodes[mapDivisionIndex].Length > 2)
            {
                for (int nodeIndex = 1; nodeIndex < _mapNodes[mapDivisionIndex].Length - 1; nodeIndex++)
                {
                    if (_mapNodes[mapDivisionIndex][nodeIndex].OutgoingNodeConnections.Count > 0)
                    {
                        continue;
                    }

                    bool nodeConnectionSuccess = false;
                    int currentNodeToCheck = UnityEngine.Random.Range(0, _mapNodes[mapDivisionIndex + 1].Length);
                    bool incrementUp = currentNodeToCheck < nodeIndex;
                    while (!nodeConnectionSuccess)
                    {
                        bool crossCheckSuccessful = true;

                        if (incrementUp)
                        {
                            for (int nodeCrissCrossIndex = currentNodeToCheck + 1; nodeCrissCrossIndex < _mapNodes[mapDivisionIndex + 1].Length; nodeCrissCrossIndex++)
                                if (_mapNodes[mapDivisionIndex + 1][nodeCrissCrossIndex].LowestIncomingConnectedNodeIndex < nodeIndex)
                                {
                                    currentNodeToCheck = nodeCrissCrossIndex;
                                    crossCheckSuccessful = false;
                                    break;
                                }
                        }
                        else
                        {
                            for (int nodeCrissCrossIndex = currentNodeToCheck - 1; nodeCrissCrossIndex >= 0; nodeCrissCrossIndex--)
                                if (_mapNodes[mapDivisionIndex + 1][nodeCrissCrossIndex].HighestIncomingConnectedNodeIndex > nodeIndex)
                                {
                                    currentNodeToCheck = nodeCrissCrossIndex;
                                    crossCheckSuccessful = false;
                                    break;
                                }
                        }

                        if (!crossCheckSuccessful)
                            continue;

                        nodeConnectionSuccess = true;
                    }

                    // Build the connection here
                    _mapNodes[mapDivisionIndex + 1][currentNodeToCheck].AddConnectionNodeIncoming(_mapNodes[mapDivisionIndex][nodeIndex]);
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
                    connectionRectTransform.SetAsFirstSibling();

                    lineRenderer.points[0] = currentNode.MapNodeUI.localPosition + Vector3.Normalize(outgoingConnectedNode.MapNodeUI.localPosition - currentNode.MapNodeUI.localPosition) * MAP_CONNECTION_EDGE_TRIM;
                    lineRenderer.points[1] = outgoingConnectedNode.MapNodeUI.localPosition + Vector3.Normalize(currentNode.MapNodeUI.localPosition - outgoingConnectedNode.MapNodeUI.localPosition) * MAP_CONNECTION_EDGE_TRIM;
                }
            }
        }
    }

    private void SetupMapButtons()
    {
        // basic setup, adding callbacks and disabling buttons.
        for (int mapDivisionIndex = 0; mapDivisionIndex < MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            for (int mapNodeIndex = 0; mapNodeIndex < _mapNodes[mapDivisionIndex].Length; mapNodeIndex++)
            {
                Button nodeButton = _mapNodes[mapDivisionIndex][mapNodeIndex].MapNodeUI.GetComponent<Button>();
                nodeButton.enabled = false;
                UiNodeButtonAddDelegate_EnableConnectedButtons(nodeButton, mapDivisionIndex, mapNodeIndex);
                UiNodeButtonAddDelegate_DisableAllButtons(nodeButton, mapDivisionIndex);
            }
        }

        // enable the first division buttons.
        for (int mapNodeIndex = 0; mapNodeIndex < _mapNodes[0].Length; mapNodeIndex++)
            _mapNodes[0][mapNodeIndex].MapNodeUI.GetComponent<Button>().enabled = true ;
    }

    private void UiNodeButtonAddDelegate_EnableConnectedButtons(Button nodeButton, int mapDivisionIndex, int mapNodeIndex)
    {
        nodeButton.onClick.AddListener(delegate { UiEnableConnectedButtonsInNextMapDivisonCallback(mapDivisionIndex, mapNodeIndex); });
    }

    private void UiNodeButtonAddDelegate_DisableAllButtons(Button nodeButton, int mapDivisionIndex)
    {
        nodeButton.onClick.AddListener(delegate { UiDisableAllButtonsInSameMapDivisionCallback(mapDivisionIndex); });
    }

    public void UiDisableAllButtonsInSameMapDivisionCallback(int mapDivision)
    {
        for(int mapNodeIndex = 0; mapNodeIndex < _mapNodes[mapDivision].Length; mapNodeIndex++) 
        {
            _mapNodes[mapDivision][mapNodeIndex].MapNodeUI.GetComponent<Button>().enabled = false;
        }
    }

    public void UiEnableConnectedButtonsInNextMapDivisonCallback(int mapDivision, int nodeIndex)
    {
        for (int outgoingConnectionIndex = 0; outgoingConnectionIndex < _mapNodes[mapDivision][nodeIndex].OutgoingNodeConnections.Count; outgoingConnectionIndex++)
        {
            Button connectedButton = _mapNodes[mapDivision + 1][_mapNodes[mapDivision][nodeIndex].OutgoingNodeConnections[outgoingConnectionIndex].NodeIndex].MapNodeUI.GetComponent<Button>();
            connectedButton.enabled = true;
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
