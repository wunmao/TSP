using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TSP
{
    public class FastEuclideanPath
    {
        private readonly float[] _distanceArray;
        protected int _numberOfPoints;

        public FastEuclideanPath(IReadOnlyList<Point> points)
        {
            _numberOfPoints = points.Count;
            _distanceArray = GetDistanceArray(points);
        }

        public double GetAveragedeDistance()
        {
            throw new NotImplementedException();
        }

        public double GetDistance(int i, int j)
        {
            return _distanceArray[j + i * _numberOfPoints];
        }

        public double GetPathLength(int[] sequence, bool closedPath)
        {
            //float pathLength = 0f;

            //unsafe
            //{
            //    fixed (int* seqpointer = sequence)
            //    {
            //        int* element = seqpointer;
            //        var remaining = sequence.Length - 1;
            //        while (remaining-- > 0)
            //        {
            //            pathLength += *element;
            //            element++;
            //        }
            //    }
            //}

            var pathLength = 0f;
            for (var i = 0; i < sequence.Length - 1; i++)
            {
                pathLength += GetDistanceFloat(sequence[i], sequence[i + 1]);
            }

            // closed hamiltonian path
            if (closedPath)
            {
                pathLength += GetDistanceFloat(sequence[0], sequence[_numberOfPoints - 1]);
            }

            return pathLength;
        }

        public double GetSubPathLength(int[] sequence, int maxIndex)
        {
            var currentPathLength = 0f;

            for (var i = 0; i < maxIndex; i++)
            {
                currentPathLength += GetDistanceFloat(sequence[i], sequence[i + 1]);
            }

            return currentPathLength;
        }

        private float GetDistanceFloat(int i, int j)
        {
            return _distanceArray[j + i * _numberOfPoints];
        }

        private float[] GetDistanceArray(IReadOnlyList<Point> points)
        {
            var distanceArray = new float[_numberOfPoints * _numberOfPoints];

            for (var row = 0; row < _numberOfPoints; row++)
            {
                for (var column = 0; column < _numberOfPoints; column++)
                {
                    // euclidean distance
                    distanceArray[column + row * _numberOfPoints] = (float)Point.Subtract(points[row], points[column]).Length;
                }
            }

            return distanceArray;
        }
    }

    public class SimulatedAnnealingOptimizer
    {
        private readonly double CoolingRate;
        public bool ClosedPath;
        public FastEuclideanPath EuclideanPath;
        public List<Point> Points;
        public int[] StartPermutation;

        public SimulatedAnnealingOptimizer(List<Point> _Points, double _CoolingRate, bool _ClosedPath)
        {
            StartPermutation = Enumerable.Range(0, _Points.Count).ToArray(); ;
            Points = _Points;
            EuclideanPath = new FastEuclideanPath(_Points);
            CoolingRate = _CoolingRate;
            ClosedPath = _ClosedPath;
        }

        public async Task<(List<Point>, double)> Run()
        {
            return await Task.Factory.StartNew(() =>
                                               {
                                                   var rand = new Random();
                                                   var minPathLength = double.MaxValue;
                                                   var curPathLength = double.MaxValue;
                                                   var currentSequence = StartPermutation.ToArray();

                                                   var temperature = double.MaxValue;

                                                   var n = 0;
                                                   var index = 0;
                                                   while (true)
                                                   {
                                                       int cp1, cp2;

                                                       do
                                                       {
                                                           cp1 = rand.Next(0, currentSequence.Length - 1);
                                                           cp2 = rand.Next(1, currentSequence.Length);
                                                       } while (cp1 == cp2 || cp1 > cp2);

                                                       var nextSequence = TwoOptSwap(currentSequence, cp1, cp2);

                                                       var nextPathLength = EuclideanPath.GetPathLength(nextSequence, ClosedPath);
                                                       var difference = nextPathLength - curPathLength;
                                                       curPathLength = nextPathLength;

                                                       if (curPathLength < minPathLength)
                                                       {
                                                           currentSequence = nextSequence.ToArray();
                                                           minPathLength = curPathLength;
                                                           index = n;
                                                       }
                                                       else if (difference > 0 && GetProbability(difference, temperature) > rand.NextDouble())
                                                       {
                                                           currentSequence = nextSequence.ToArray();
                                                           minPathLength = curPathLength;
                                                           index = n;
                                                       }
                                                       else if (n - index > 5000)
                                                       {
                                                           break;
                                                       }

                                                       temperature = temperature * CoolingRate;
                                                       n++;
                                                   }

                                                   var first = currentSequence.TakeWhile(x => x != 0);
                                                   var second = currentSequence.SkipWhile(x => x != 0);
                                                   var final = second.Concat(first);

                                                   var pts = final.Select(i => Points[i]).ToList();

                                                   return (pts, minPathLength);
                                               },
                                               TaskCreationOptions.LongRunning);
        }

        public static int[] TwoOptSwap(int[] sequence, int i, int k)
        {
            var nextSequence = new int[sequence.Length];

            // 1. take sequence[0] to sequence[posA-1] and add them in order to nextSequence
            // 2. take sequence[posA] to sequence[posB] and add them in reverse order to nextSequence
            // 3. take sequence[posB+1] to end and add them in order to nextSequence
            Array.Copy(sequence, nextSequence, sequence.Length);
            Array.Reverse(nextSequence, i, k - i + 1);

            return nextSequence;
        }

        private double GetProbability(double difference, double temperature)
        {
            return Math.Exp(-difference / temperature);
        }
    }
}