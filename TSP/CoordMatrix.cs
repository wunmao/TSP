using System.Collections.Generic;
using System.Windows;

namespace TSP
{
    public class CoordMatrix
    {
        private readonly List<Point> coords = new List<Point>();
        private readonly Dictionary<string, double> distMatrix = new Dictionary<string, double>();

        public int TourSize => coords.Count;

        public CoordMatrix(IEnumerable<Point> points)
        {
            foreach (var pt in points)
            {
                coords.Add(pt);
            }

            for (var i = 0; i < coords.Count - 1; i++)
            {
                var p1 = coords[i];
                for (var j = i + 1; j < coords.Count; j++)
                {
                    var p2 = coords[j];
                    var dist = (p1 - p2).Length;

                    var s = i + "," + j;
                    distMatrix[s] = dist;
                }
            }
        }

        public void ClearCoordinates()
        {
            coords.Clear();
        }

        public Point GetCoordinate(int index)
        {
            return coords[index];
        }

        public double Distance(int c1, int c2)
        {
            if (c1 < c2)
            {
                var s = c1 + "," + c2;
                var dist = distMatrix[s];
                return dist;
            }
            else
            {
                var s = c2 + "," + c1;
                var dist = distMatrix[s];
                return dist;
            }
        }
    }
}