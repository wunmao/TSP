using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TSP
{
    public class AntHandler
    {
        private readonly int tourSize;
        public List<Ant> Ants;

        public AntHandler(int Size)
        {
            tourSize = Size;
            Ants = new List<Ant>();

            var size = tourSize * 1.2;

            for (var i = 0; i < size; ++i)
            {
                var r = new Random();
                Ants.Add(new Ant(tourSize, r));
            }
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public async Task<(Ant, double)> Run(PheromoneMatrix pheromoneMatrix, CoordMatrix coordMatrix, double alpha, double beta, double evaporationRate, double q)
        {
            return await Task.Factory.StartNew(() =>
                                               {
                                                   var minDistance = double.MaxValue;
                                                   var bestAntFound = new Ant(tourSize, new Random());

                                                   // Find the tour taken by the ant
                                                   while (!bestAntFound.PathFound)
                                                   {
                                                       var nextNode = GetNextPathNode(bestAntFound, pheromoneMatrix, coordMatrix, alpha, beta, q);
                                                       bestAntFound.UpdatePath(nextNode);
                                                   }

                                                   for (var i = 0; i < 1000; i++)
                                                   {
                                                       foreach (var ant in Ants)
                                                       {
                                                           while (!ant.PathFound)
                                                           {
                                                               var nextNode = GetNextPathNode(ant, pheromoneMatrix, coordMatrix, alpha, beta, q);
                                                               ant.UpdatePath(nextNode);
                                                           }

                                                           var pathDistance = ant.Distance(coordMatrix);
                                                           if (pathDistance < minDistance)
                                                           {
                                                               minDistance = pathDistance;
                                                               bestAntFound = new Ant(ant);
                                                           }
                                                       }

                                                       UpdateTrails(coordMatrix, evaporationRate, pheromoneMatrix);

                                                       // reset paths
                                                       Reset();
                                                   }

                                                   for (var j = 0; j < 1000; j++)
                                                   {
                                                       for (var i = 1; i < tourSize - 1; i++)
                                                       {
                                                           for (var k = i + 1; k < tourSize; k++)
                                                           {
                                                               var newAnt = TwoOptSwap(i, k, bestAntFound);

                                                               var newDistance = newAnt.Distance(coordMatrix);

                                                               if (newDistance < minDistance)
                                                               {
                                                                   minDistance = newDistance;
                                                                   bestAntFound = newAnt;
                                                               }
                                                           }
                                                       }
                                                   }

                                                   return (bestAntFound, minDistance);
                                               },
                                               TaskCreationOptions.LongRunning);
        }

        public void UpdateTrails(CoordMatrix coordMatrix, double evaporationRate, PheromoneMatrix pheromoneMatrix)
        {
            var size = coordMatrix.TourSize;
            // evaporation
            for (var x = 0; x < size - 1; ++x)
            {
                for (var y = x + 1; y < size; ++y)
                {
                    pheromoneMatrix.Evaporate(evaporationRate, x, y);
                }
            }

            // contribution
            foreach (var ant in Ants)
            {
                var contribution = 1 / ant.Distance(coordMatrix);

                for (var i = 0; i < size - 1; ++i)
                {
                    pheromoneMatrix.Contribute(ant.GetNode(i), ant.GetNode(i + 1), contribution);
                }

                pheromoneMatrix.Contribute(ant.GetNode(0), ant.GetNode(size - 1), contribution);
            }
        }

        private void Reset()
        {
            foreach (var ant in Ants)
            {
                ant.Reset();
            }
        }

        private int GetNearestNode(Ant ant, CoordMatrix coordMatrix)
        {
            var allowedSet = ant.AllowedNodes;

            var currentStartNode = ant.CurrentStartNode;

            var minDistance = 99999999999.9;
            var minNode = -1;

            foreach (var otherNode in allowedSet)
            {
                var distance = coordMatrix.Distance(currentStartNode, otherNode);

                if (distance < minDistance)
                {
                    minNode = otherNode;
                    minDistance = distance;
                }
            }

            return minNode;
        }

        private int GetNextPathNode(Ant ant, PheromoneMatrix pheromoneMatrix, CoordMatrix coordMatrix, double alpha, double beta, double q)
        {
            var allowedSet = ant.AllowedNodes;
            var currentStartNode = ant.CurrentStartNode;
            var totalPheromoneXZProduct = 0.0;

            var probLookup = new List<Tuple<int, double>>();
            var maxProb = -1.0;
            var cumProb = 0.0;

            foreach (var otherNode in allowedSet)
            {
                var pheromoneXY = Math.Pow(pheromoneMatrix.Pheromone(currentStartNode, otherNode), alpha);
                var distance = coordMatrix.Distance(currentStartNode, otherNode);
                var attractivenessXY = Math.Pow(2 / distance, beta);
                var pheromoneXYproduct = pheromoneXY * attractivenessXY;

                totalPheromoneXZProduct += pheromoneXYproduct;
            }

            foreach (var otherNode in allowedSet)
            {
                var pheromoneXY = Math.Pow(pheromoneMatrix.Pheromone(currentStartNode, otherNode), alpha);
                var attractivenessXY = Math.Pow(q / coordMatrix.Distance(currentStartNode, otherNode), beta);
                var pheromoneXYproduct = pheromoneXY * attractivenessXY;

                var probability = pheromoneXYproduct / totalPheromoneXZProduct;

                if (probability > maxProb)
                {
                    maxProb = probability;
                }

                cumProb += probability;

                probLookup.Add(new Tuple<int, double>(otherNode, cumProb));
            }

            var value = new Random().NextDouble() * cumProb;

            // locate the random value based on the weights
            // a roulette-wheel selection
            foreach (var prob in probLookup)
            {
                var weight = prob.Item2;
                value -= weight;

                if (value <= 0)
                {
                    return prob.Item1;
                }
            }

            // when rounding errors occur, we return the last item's index 
            return probLookup.Count - 1;
        }

        private Ant TwoOptSwap(int i, int k, Ant ant)
        {
            var copy = new Ant(tourSize);

            for (var c = 0; c <= i - 1; ++c)
            {
                copy.UpdatePath(ant.GetNode(c));
            }

            var dec = 0;
            for (var c = i; c <= k; ++c)
            {
                copy.UpdatePath(ant.GetNode(k - dec));
                dec++;
            }

            for (var c = k + 1; c < tourSize; ++c)
            {
                copy.UpdatePath(ant.GetNode(c));
            }

            return copy;
        }
    }
}