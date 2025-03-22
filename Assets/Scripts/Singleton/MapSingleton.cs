using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSingleton : MonoBehaviour
{
    private const int MAP_DIVISION_COUNT = 10;
    private const int MAP_DIVISION_REST_OVERRIDE = 5;
    private const int MAX_NODE_COUNT_PER_DIVISION = 4;

    private const float MAP_CONTENT_WIDTH = 1240;
    private const float MAP_NODE_VERTICAL_SPACING = 300;
    private const float MAP_SIDE_MARGINS = 150;
    private const float MAP_TOP_MARGINS = 350;
    private const float MAP_CONNECTION_EDGE_TRIM = 65;
    private const float MAP_NODE_RANDOMNESS = 50;
    private const float MAP_DISSAPEAR_DELAY_AFTER_BUTTON_PRESS = 1.5f;

    private const string MAP_APPEAR_ANIM_NAME = "Ui_MapAppear";
    private const string MAP_DISAPPEAR_ANIM_NAME = "Ui_MapDisappear";

    public static MapSingleton Instance;

    [SerializeField] private GameObject _mapNodeToInstantiate;
    [SerializeField] private GameObject _mapNodeConnectionToInstantiate;
    [SerializeField] private Transform _mapNodeParent;
    [SerializeField] private Transform _mapContentBox;
    [SerializeField] private UiAnimationPlayer _mapAnimation;
    [SerializeField] private Sprite[] _mapNodeArtwork;
    [SerializeField] private CanvasGroup _mapInteractibilityCanvasGroup;
    [SerializeField] private CanvasGroup _mapScrollRectRaycastBlockingCanvasGroup;

    private MapNode[][] _mapNodes;
    private bool _mapCurrentlyShowing = false;
    private float _contentPreviousHeightWhenOpened = 99999f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    // THIS IS FOR TESTING  UKE THIS AFTER
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
        if (Input.GetKeyDown(KeyCode.O))
            SetMapInteractibility(true);
    }

    public IEnumerator SetMapShowStatusDelayed(bool showStatus, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetMapShowStatus(showStatus);
    }

    public void SetMapShowStatus(bool showMap)
    {
        if (showMap == _mapCurrentlyShowing)
            return;

        _mapCurrentlyShowing = showMap;
        _mapScrollRectRaycastBlockingCanvasGroup.interactable = showMap;
        _mapScrollRectRaycastBlockingCanvasGroup.blocksRaycasts =  showMap;

        if (_mapCurrentlyShowing)
        {
            _mapAnimation.PlayAnimationByName(MAP_APPEAR_ANIM_NAME);
            _mapContentBox.localPosition = new Vector2(_mapContentBox.localPosition.x, _contentPreviousHeightWhenOpened);
        }
        else
        {
            _mapAnimation.PlayAnimationByName(MAP_DISAPPEAR_ANIM_NAME);
            _contentPreviousHeightWhenOpened = _mapContentBox.localPosition.y;
        }
    }

    public void SetMapInteractibility(bool canInteract)
    {
        _mapInteractibilityCanvasGroup.interactable = canInteract;
    }

    private void CreateNewMapData()
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

    private void DrawMap()
    {
        RectTransform rectTransform = _mapContentBox.GetComponent<RectTransform>();
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
                    //UILineRenderer lineRenderer = connectionRectTransform.GetComponent<UILineRenderer>();

                    connectionRectTransform.localPosition = (currentNode.MapNodeUI.localPosition + outgoingConnectedNode.MapNodeUI.localPosition) / 2;
                    connectionRectTransform.SetAsFirstSibling();

                    float hypotenuseLength = (currentNode.MapNodeUI.localPosition - outgoingConnectedNode.MapNodeUI.localPosition).magnitude;
                    float oppositeLength = Mathf.Abs(currentNode.MapNodeUI.localPosition.y - outgoingConnectedNode.MapNodeUI.localPosition.y);
                    connectionRectTransform.sizeDelta = new Vector2(connectionRectTransform.sizeDelta.x, hypotenuseLength - MAP_CONNECTION_EDGE_TRIM * 2);
                    float angle = Mathf.Asin(oppositeLength / hypotenuseLength) * Mathf.Rad2Deg;

                    if (currentNode.MapNodeUI.localPosition.x - outgoingConnectedNode.MapNodeUI.localPosition.x < 0)
                        connectionRectTransform.rotation = Quaternion.Euler(0, 0, (90 - angle) * -1);
                    else
                        connectionRectTransform.rotation = Quaternion.Euler(0, 0, 90 - angle);
                }
            }
        }
    }

    private void SetupMapButtons()
    {
        // basic setup, adding callbacks and disabling buttons.
        for (int mapDivisionIndex = 0; mapDivisionIndex <= MAP_DIVISION_COUNT; mapDivisionIndex++)
        {
            for (int mapNodeIndex = 0; mapNodeIndex < _mapNodes[mapDivisionIndex].Length; mapNodeIndex++)
            {
                Button nodeButton = _mapNodes[mapDivisionIndex][mapNodeIndex].MapNodeUI.GetComponent<Button>();
                nodeButton.enabled = false;
                UiNodeButtonAddDelegate_EnableConnectedButtons(nodeButton, mapDivisionIndex, mapNodeIndex);
                UiNodeButtonAddDelegate_DisableAllButtons(nodeButton, mapDivisionIndex);
                UiNodeButtonAddDelegate_LockMapInteractibility(nodeButton);
                UiNodeButtonAddDelegate_HideMap(nodeButton);
                switch (_mapNodes[mapDivisionIndex][mapNodeIndex].AssignedMapNodeType)
                {
                    case MapNode.MapNodeType.CombatNode:
                        UiNodeButtonAddDelegate_StartCombat(nodeButton, LevelDataSingleton.Instance.GetBasicEncounter());
                        break;
                    case MapNode.MapNodeType.EliteCombatNode:
                        UiNodeButtonAddDelegate_StartCombat(nodeButton, LevelDataSingleton.Instance.GetEliteEncounter());
                        break;
                    case MapNode.MapNodeType.EventNode:
                        UiNodeButtonAddDelegate_StartEvent(nodeButton, LevelDataSingleton.Instance.GetRandomEventDetails());
                        break;
                    case MapNode.MapNodeType.ShopNode:
                        UiNodeButtonAddDelegate_OpenShop(nodeButton);
                        break;
                    case MapNode.MapNodeType.RestNode:
                        UiNodeButtonAddDelegate_Rest(nodeButton);
                        break;
                    case MapNode.MapNodeType.BossNode:
                        UiNodeButtonAddDelegate_StartCombat(nodeButton, LevelDataSingleton.Instance.GetBossEncounter());
                        break;
                    default:
                        break;
                }
            }
        }

        // enable the first division buttons.
        for (int mapNodeIndex = 0; mapNodeIndex < _mapNodes[0].Length; mapNodeIndex++)
            _mapNodes[0][mapNodeIndex].MapNodeUI.GetComponent<Button>().enabled = true ;
    }

    #region Ui Button Delegate Containers
    private void UiNodeButtonAddDelegate_EnableConnectedButtons(Button nodeButton, int mapDivisionIndex, int mapNodeIndex)
    {
        nodeButton.onClick.AddListener(delegate { UiEnableConnectedButtonsInNextMapDivisonCallback(mapDivisionIndex, mapNodeIndex); });
    }
    private void UiNodeButtonAddDelegate_DisableAllButtons(Button nodeButton, int mapDivisionIndex)
    {
        nodeButton.onClick.AddListener(delegate { UiDisableAllButtonsInSameMapDivisionCallback(mapDivisionIndex); });
    }
    private void UiNodeButtonAddDelegate_StartCombat(Button nodeButton, EncounterScriptableObject encounter)
    {
        nodeButton.onClick.AddListener(delegate { UiStartEncounterCallback(encounter); });
    }
    private void UiNodeButtonAddDelegate_StartEvent(Button nodeButton, EventData eventSO)
    {
        nodeButton.onClick.AddListener(delegate { UiStartEventCallback(eventSO); });
    }
    private void UiNodeButtonAddDelegate_OpenShop(Button nodeButton)
    {
        nodeButton.onClick.AddListener(delegate { UiRollShopContents(nodeButton); });
        nodeButton.onClick.AddListener(delegate { UiOpenShopCallback(); });
    }
    private void UiNodeButtonAddDelegate_Rest(Button nodeButton)
    {
        nodeButton.onClick.AddListener(delegate { UiStartRestCallback(); });
    }
    private void UiNodeButtonAddDelegate_LockMapInteractibility(Button nodeButton)
    {
        nodeButton.onClick.AddListener(delegate { SetMapInteractibility(false); });
    }
    private void UiNodeButtonAddDelegate_HideMap(Button nodeButton)
    {
        nodeButton.onClick.AddListener(delegate { UiHideMapWithDelay(MAP_DISSAPEAR_DELAY_AFTER_BUTTON_PRESS); });
    }
    #endregion

    #region Ui Button Delegate
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
    public void UiStartEncounterCallback(EncounterScriptableObject encounter)
    {
        CombatManagerSingleton.Instance.StartCombat(encounter);
    }
    public void UiStartEventCallback(EventData eventSO)
    {
        EventSingelton.Instance.StartEvent(eventSO, 1.5f);
    }
    public void UiOpenShopCallback()
    {
        ShopSingleton.Instance.OpenShopWithDelay(1.5f);
    }
    public void UiRollShopContents(Button nodeButton)
    {
        ShopSingleton.Instance.RollShopContents();
        nodeButton.onClick.RemoveListener(delegate { UiRollShopContents(nodeButton); });
    }
    public void UiStartRestCallback()
    {
        RestingSingleton.Instance.ShowRestScreenWithDelay(1.5f);
    }
    public void UiHideMapWithDelay(float delayInSeconds)
    {
        StartCoroutine(SetMapShowStatusDelayed(false, delayInSeconds));
    }
    public void UiShowMapWithDelay(float delayInSeconds)
    {
        StartCoroutine(SetMapShowStatusDelayed(true, delayInSeconds));
    }
    #endregion

    public void UiShowMapButtonPress()
    {
        SetMapShowStatus(true);
    }
    public void UiHideMapButtonPress()
    {
        SetMapShowStatus(false);
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
            default:
                break;
        }
    }

}
