using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    private const int NODE_RC_COMBAT = 30;
    private const int NODE_RC_ELITE_COMBAT = 10;
    private const int NODE_RC_EVENT = 40;
    private const int NODE_RC_SHOP = 10;
    private const int NODE_RC_REST = 10;

    public enum MapNodeType { CombatNode, EliteCombatNode, EventNode, ShopNode, RestNode, BossNode }
    public MapNodeType AssignedMapNodeType { get => _assignedMapNodeType; set => _assignedMapNodeType = value; }
    public List<MapNode> IncomingConnections { get => _incomingNodeConnections; set => _incomingNodeConnections = value; }
    public List<MapNode> OutgoingNodeConnections { get => _outgoingNodeConnections; set => _outgoingNodeConnections = value; }
    public RectTransform MapNodeUI { get => _mapNodeUI; set => _mapNodeUI = value; }
    public int HighestIncomingConnectedNodeIndex { get => _highestIncomingConnectedNodeIndex; set => _highestIncomingConnectedNodeIndex = value; }
    public int LowestIncomingConnectedNodeIndex { get => _lowestIncomingConnectedNodeIndex; set => _lowestIncomingConnectedNodeIndex = value; }
    public int HighestOutgoingConnectedNodeIndex { get => _highestOutgoingConnectedNodeIndex; set => _highestOutgoingConnectedNodeIndex = value; }
    public int LowestOutgoingConnectedNodeIndex { get => _lowestOutgoingConnectedNodeIndex; set => _lowestOutgoingConnectedNodeIndex = value; }
    public int NodeIndex { get => _nodeIndex; set => _nodeIndex = value; }

    private MapNodeType _assignedMapNodeType;
    private List<MapNode> _incomingNodeConnections = new List<MapNode>();
    private List<MapNode> _outgoingNodeConnections = new List<MapNode>();
    private int _highestIncomingConnectedNodeIndex = -999;
    private int _lowestIncomingConnectedNodeIndex = 999;
    private int _highestOutgoingConnectedNodeIndex = -999;
    private int _lowestOutgoingConnectedNodeIndex = 999;
    private int _nodeIndex = 0;
    private RectTransform _mapNodeUI;

    public MapNode(MapNodeType assignedMapNodeType, int mapNodeIndex)
    {
        _assignedMapNodeType = assignedMapNodeType;
        _nodeIndex = mapNodeIndex;
    }

    public MapNode(int mapNodeIndex)
    {
        _nodeIndex = mapNodeIndex;
    }

    public void RandomizeNode()
    {
        int randomNodeIndex = Random.Range(0, NODE_RC_COMBAT + NODE_RC_ELITE_COMBAT + NODE_RC_EVENT + NODE_RC_REST + NODE_RC_SHOP);
        
        if(randomNodeIndex < NODE_RC_SHOP)
        {
            _assignedMapNodeType = MapNodeType.CombatNode;
            return;
        }
        if (randomNodeIndex < NODE_RC_SHOP + NODE_RC_ELITE_COMBAT)
        {
            _assignedMapNodeType = MapNodeType.EliteCombatNode;
            return;
        }
        if (randomNodeIndex < NODE_RC_COMBAT + NODE_RC_ELITE_COMBAT + NODE_RC_EVENT)
        {
            _assignedMapNodeType = MapNodeType.EventNode;
            return;
        }
        if (randomNodeIndex < NODE_RC_COMBAT + NODE_RC_ELITE_COMBAT + NODE_RC_EVENT + NODE_RC_REST)
        {
            _assignedMapNodeType = MapNodeType.ShopNode;
            return;
        }

        _assignedMapNodeType = MapNodeType.RestNode;
        return;
    }

    public void AddConnectionNodeIncoming(MapNode incomingNode)
    {
        _incomingNodeConnections.Add(incomingNode);
        incomingNode.OutgoingNodeConnections.Add(this);

        if (incomingNode.NodeIndex < _lowestIncomingConnectedNodeIndex)
            _lowestIncomingConnectedNodeIndex = incomingNode.NodeIndex;
        if (incomingNode.NodeIndex > _highestIncomingConnectedNodeIndex)
            _highestIncomingConnectedNodeIndex = incomingNode.NodeIndex;

        if (NodeIndex < incomingNode.LowestOutgoingConnectedNodeIndex)
            incomingNode.LowestOutgoingConnectedNodeIndex = NodeIndex;
        if (NodeIndex > incomingNode.HighestOutgoingConnectedNodeIndex)
            incomingNode.HighestOutgoingConnectedNodeIndex = NodeIndex;
    }
}
