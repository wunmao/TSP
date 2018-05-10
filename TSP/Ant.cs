using System;
using System.Collections.Generic;

namespace TSP
{
    public class Ant
    {
        private readonly List<int> Path;
        private readonly int TourSize;
        public HashSet<int> AllowedNodes;
        public int CurrentStartNode;

        public bool PathFound => Path.Count >= TourSize;

        public Ant(Ant ant)
        {
            Path = new List<int>(ant.Path);
            TourSize = ant.TourSize;
            AllowedNodes = ant.AllowedNodes;
            CurrentStartNode = ant.CurrentStartNode;
        }

        public Ant(int tourSize)
        {
            Path = new List<int>();
            TourSize = tourSize;
        }

        public Ant(int tourSize, Random r)
        {
            AllowedNodes = new HashSet<int>();
            Path = new List<int>();
            TourSize = tourSize;

            for (var i = 0; i < TourSize; i++)
            {
                AllowedNodes.Add(i);
            }

            UpdatePath(r.Next(TourSize));
        }

        public bool InTour(int start, int end)
        {
            var size = Path.Count - 1;

            for (var i = 0; i < size; ++i)
            {
                if (Path[i] == start && Path[i + 1] == end || Path[i + 1] == start && Path[i] == end)
                {
                    return true;
                }
            }

            return Path[0] == start && Path[size] == end || Path[size] == start && Path[0] == end;
        }

        public double Distance(CoordMatrix matrix)
        {
            var totalDist = 0.0;

            for (var i = 0; i < Path.Count - 1; ++i)
            {
                var dist = matrix.Distance(Path[i], Path[i + 1]);
                totalDist += dist;
            }

            totalDist += matrix.Distance(Path[0], Path[Path.Count - 1]);

            return totalDist;
        }

        public void Reset()
        {
            var r = new Random();

            var first = r.Next(TourSize);

            Path.Clear();

            for (var i = 0; i < TourSize; i++)
            {
                AllowedNodes.Add(i);
            }

            UpdatePath(first);
        }

        public int GetNode(int index)
        {
            return Path[index];
        }

        public void UpdatePath(int node)
        {
            if (PathFound)
            {
                return;
            }

            CurrentStartNode = node;
            Path.Add(CurrentStartNode);
            RemoveAllowedNode(CurrentStartNode);
        }

        public void RemoveAllowedNode(int i)
        {
            AllowedNodes?.Remove(i);
        }

        public List<int> GetAllowedDestinationNodes(int startNode, int destNode)
        {
            var allowedDestNodes = new List<int>();

            foreach (var node in AllowedNodes)
            {
                if (node != startNode && node != destNode)
                {
                    allowedDestNodes.Add(node);
                }
            }

            return allowedDestNodes;
        }
    }
}