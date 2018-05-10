using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    public class PheromoneMatrix
    {
        private readonly Dictionary<string, double> _pheromoneMatrix = new Dictionary<string, double>();

        public void Evaporate(double evaporationRate, int x, int y)
        {
            var pheromone = Pheromone(x, y);
            pheromone = (1.0 - evaporationRate) * pheromone;

            if (x < y)
            {
                SetMatrix(x, y, pheromone);
            }
            else
            {
                SetMatrix(y, x, pheromone);
            }
        }

        public void Contribute(int x, int y, double delta)
        {
            var pheromone = Pheromone(x, y);

            pheromone += delta;

            if (x < y)
            {
                SetMatrix(x, y, pheromone);
            }
            else
            {
                SetMatrix(y, x, pheromone);
            }
        }

        public void SetMatrix(int i, int j, double value)
        {
            var s = i + "," + j;
            _pheromoneMatrix[s] = value;
        }

        public double Pheromone(int i, int j)
        {
            if (i < j)
            {
                var s = i + "," + j;
                var dist = _pheromoneMatrix[s];
                return dist;
            }
            else
            {
                var s = j + "," + i;
                var dist = _pheromoneMatrix[s];
                return dist;
            }
        }
    }
}
