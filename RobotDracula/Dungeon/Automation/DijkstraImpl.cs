using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Map;

namespace RobotDracula.Dungeon.Automation
{
    public static class DijkstraImpl
    {
        private static DijkstraNode IterateNode(DijkstraNode node, List<DijkstraNode> nodes, DijkstraNode destination = null)
        {
            if (!nodes.Contains(node))
                nodes.Add(node);
            foreach (var valueTuple in node.NodeModel.next)
            {
                var neighbor = nodes.FirstOrDefault(n => n == valueTuple.Item1) ?? new(valueTuple.Item1);
                if (!node.Neighbors.Contains(neighbor))
                    node.Neighbors.Add(neighbor);
                destination = IterateNode(neighbor, nodes, destination);
            }

            if (node.Neighbors.Count <= 0)
                destination = node;
            
            return destination;
        }

        public static List<DijkstraNode> RunDijkstra()
        {
            return Dijkstra(DungeonHelper.CurrentNodeModel);
        }

        public static List<DijkstraNode> Dijkstra(NodeModel currentModel)
        {
            var allNodes = new List<DijkstraNode>();
            var destination = IterateNode(new(currentModel), allNodes);
            if (destination is null)
                throw new Exception("Cannot reach destination from current!");
            
            var unvisitedNodes = new List<DijkstraNode>(allNodes);
            var current = unvisitedNodes.First();
            current.Distance = 0;

            while (unvisitedNodes.Count > 0)
            { 
                var smallest = unvisitedNodes.MinBy(n => n.Distance);
                unvisitedNodes.Remove(smallest);
                foreach (var neighbor in smallest.Neighbors.Where(n => unvisitedNodes.Contains(n)))
                {
                    var newCost = smallest.Distance + DungeonAutomation.GetCost(smallest.NodeModel, neighbor.NodeModel);
                    if (newCost < neighbor.Distance)
                    {
                        neighbor.Distance = newCost;
                        neighbor.Previous = smallest;
                    }
                }
            }
            
            var path = new List<DijkstraNode>();
            var prev = destination;
            while (prev.Previous is not null)
            {
                path.Add(prev);
                prev = prev.Previous;
            }

            path.Reverse();
            Plugin.PluginLog.LogDebug("Path:");
            Plugin.PluginLog.LogDebug("\n" + string.Join("\n", path.Select(d => $"{d.NodeModel.id}: {d.Distance}")));
            return path;
        }
    }
}