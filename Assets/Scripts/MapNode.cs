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

    public enum MapNodeType { CombatNode, EliteCombatNode, EventNode, ShopNode, RestNode, BossNode, StartingNode }
    public MapNodeType AssignedMapNodeType { get => _assignedMapNodeType; set => _assignedMapNodeType = value; }
    public List<MapNode> IncomingConnections { get => _incomingNodeConnections; set => _incomingNodeConnections = value; }
    public List<MapNode> OutgoingNodeConnections { get => _outgoingNodeConnections; set => _outgoingNodeConnections = value; }
    public RectTransform MapNodeUI { get => _mapNodeUI; set => _mapNodeUI = value; }

    private MapNodeType _assignedMapNodeType;
    private List<MapNode> _incomingNodeConnections = new List<MapNode>();
    private List<MapNode> _outgoingNodeConnections = new List<MapNode>();
    private RectTransform _mapNodeUI;

    public MapNode(MapNodeType assignedMapNodeType)
    {
        _assignedMapNodeType = assignedMapNodeType;
    }

    public MapNode()
    {

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
    }
    public void RemoveConnectionNodeIncoming(MapNode incomingNode)
    {
        _incomingNodeConnections.Remove(incomingNode);
        incomingNode.OutgoingNodeConnections.Remove(this);
    }
}
