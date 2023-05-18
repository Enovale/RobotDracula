using System.Collections.Generic;
using Dungeon.Map;

namespace RobotDracula.Dungeon.Automation
{
    public class DijkstraNode
    {
        public int Distance;
        public readonly NodeModel NodeModel;
        public DijkstraNode Previous;
        public readonly List<DijkstraNode> Neighbors;

        public DijkstraNode(NodeModel model)
        {
            NodeModel = model;
            Neighbors = new();
            Distance = int.MaxValue;
        }

        public static bool operator ==(DijkstraNode obj1, DijkstraNode obj2)
        {
            return obj1?.NodeModel.id == obj2?.NodeModel.id;
        }

        public static bool operator !=(DijkstraNode obj1, DijkstraNode obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator ==(DijkstraNode obj1, NodeModel obj2)
        {
            return obj1?.NodeModel.id == obj2?.id;
        }

        public static bool operator !=(DijkstraNode obj1, NodeModel obj2)
        {
            return !(obj1 == obj2);
        }
        
        public static implicit operator NodeModel(DijkstraNode node) => node.NodeModel;

        public override bool Equals(object obj)
        {
            if (obj is DijkstraNode d)
                return NodeModel.Equals(d.NodeModel);
            else if (obj is NodeModel n)
                return NodeModel.Equals(n);
            return false;
        }

        public override int GetHashCode()
        {
            return NodeModel.GetHashCode();
        }
    }
}