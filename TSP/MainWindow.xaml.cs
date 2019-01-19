using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TSP
{
    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Stopwatch SW = new Stopwatch();
        private const int count = 64;
        private const double Alpha = 1.0;
        private const double Beta = 50.0;
        private const double Evaporation = 0.1;
        private const double InitPheromone = 1.0;
        private const double Q = 1.0;
        private readonly List<Point> pts = new List<Point>();
        private AntHandler _antHandler;
        private CoordMatrix _coordMatrix;
        private PheromoneMatrix _pheromoneMatrix;
        private Tour _Tour;
        private int h = 1000;
        private int w = 1000;

        public MainWindow()
        {
            InitializeComponent();
        }

        public (List<Point>, double) OrderByDistance(IEnumerable<Point> points, Point start)
        {
            Point[] current = { start };
            var remaining = points.ToList();
            var path = new List<Point>();

            var total_length = 0.0;

            while (remaining.Count != 0)
            {
                var (e1, e2) = remaining.Select(e => (e1: e, e2: (current[0] - e).Length)).OrderBy(ee => ee.e2).First();
                total_length += e2;
                path.Add(e1);
                remaining.Remove(e1);
                current[0] = e1;
            }

            return (path, total_length);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void CV_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            w = (int)CV.ActualWidth;
            h = (int)CV.ActualHeight;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bt1.IsEnabled = false;
            bt2.IsEnabled = false;

            pts.Clear();
            IC.ItemsSource = null;

            pts.Add(new Point());

            await Task.Factory.StartNew(() =>
                                        {
                                            for (var i = 0; i < count - 1; i++)
                                            {
                                                var rn = new Random((int)DateTime.Now.Ticks + i);
                                                pts.Add(new Point(rn.Next(w), rn.Next(h)));
                                            }

                                            var tourSize = pts.Count;

                                            _coordMatrix = new CoordMatrix(pts);
                                            _antHandler = new AntHandler(pts.Count);
                                            _pheromoneMatrix = new PheromoneMatrix();

                                            for (var i = 0; i < tourSize - 1; i++)
                                            {
                                                for (var j = i + 1; j < tourSize; j++)
                                                {
                                                    _pheromoneMatrix.SetMatrix(i, j, InitPheromone);
                                                }
                                            }

                                            var Stops = pts.Select(x => new Stop(new City(x.X, x.Y))).NearestNeighbors().ToList();
                                            Stops.Connect(true);
                                            _Tour = new Tour(Stops);
                                        },
                                        TaskCreationOptions.LongRunning);

            IC.ItemsSource = pts;
            bt1.IsEnabled = true;
            bt2.IsEnabled = true;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (pts == null || pts.Count == 0)
            {
                return;
            }

            TB.Text = "";
            TB2.Text = "";
            TB3.Text = "";
            TB_.Text = "";
            TB2_.Text = "";
            TB3_.Text = "";
            bt1.IsEnabled = false;
            bt2.IsEnabled = false;
            PS.Points = null;
            PS2.Points = null;
            PS3.Points = null;

            await Task.Delay(300);

            SW.Reset();
            SW.Start();

            var result = await _antHandler.Run(_pheromoneMatrix, _coordMatrix, Alpha, Beta, Evaporation, Q);

            SW.Stop();

            var tour_pts = new List<Point>();

            for (var i = 0; i < pts.Count; ++i)
            {
                var node = result.Item1.GetNode(i);
                tour_pts.Add(_coordMatrix.GetCoordinate(node));
            }

            PF.StartPoint = tour_pts.First();
            tour_pts.RemoveAt(0);

            var ps = new PointCollection();
            foreach (var el in tour_pts)
            {
                ps.Add(el);
            }

            ps.Freeze();
            PS.Points = ps;

            TB.Text = $"{result.Item2:f3}";
            TB_.Text = $"{SW.ElapsedMilliseconds} ms";

            SW.Reset();
            SW.Start();

            await Task.Factory.StartNew(() =>
                                        {
                                            var n = 0;
                                            var index = 0;
                                            while (true)
                                            {
                                                var newTour = _Tour.GenerateMutations().MinBy(tour => tour.Cost());
                                                if (newTour.Cost() < _Tour.Cost())
                                                {
                                                    _Tour = newTour;
                                                    index = n;
                                                }
                                                else if (n - index > 10)
                                                {
                                                    break;
                                                }

                                                n++;
                                            }
                                        });

            SW.Stop();

            var result2 = _Tour.GetResult();

            var pp2 = result2.Item1;
            var p1 = pp2.First();
            PF2.StartPoint = new Point(p1.X, p1.Y);

            var ps2 = new PointCollection();
            foreach (var el in pp2)
            {
                ps2.Add(new Point(el.X, el.Y));
            }

            ps2.Freeze();
            PS2.Points = ps2;

            TB2.Text = $"{result2.Item2:f3}";
            TB2_.Text = $"{SW.ElapsedMilliseconds} ms";

            //var result2 = OrderByDistance(pts, new Point(0, 0));

            //var ppp = result2.Item1;
            //PF2.StartPoint = ppp.First();
            //ppp.RemoveAt(0);

            //var ps2 = new PointCollection();
            //foreach (var el in ppp)
            //{
            //    ps2.Add(el);
            //}

            //ps2.Freeze();
            //PS2.Points = ps2;

            //TB2.Text = result2.Item2.ToString("0.000");

            SW.Reset();
            SW.Start();

            var result3 = await new SimulatedAnnealingOptimizer(pts, 0.98, true).Run();

            SW.Stop();

            var ppp = result3.Item1;
            
            PF3.StartPoint = ppp.First();
            ppp.RemoveAt(0);

            var ps3 = new PointCollection();
            foreach (var el in ppp)
            {
                ps3.Add(el);
            }

            ps3.Freeze();
            PS3.Points = ps3;

            TB3.Text = $"{result3.Item2:f3}";
            TB3_.Text = $"{SW.ElapsedMilliseconds} ms";

            bt1.IsEnabled = true;
            bt2.IsEnabled = true;
        }
    }

    public class City
    {
        public double X { get; }

        public double Y { get; }

        public City(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Stop
    {
        public Stop Next { get; set; }

        public City City { get; }

        public Stop(City city)
        {
            City = city;
        }


        public Stop Clone()
        {
            return new Stop(City);
        }


        public static double Distance(Stop first, Stop other)
        {
            return Math.Sqrt(Math.Pow(first.City.X - other.City.X, 2) + Math.Pow(first.City.Y - other.City.Y, 2));
        }


        //list of nodes, including this one, that we can get to
        public IEnumerable<Stop> CanGetTo()
        {
            var current = this;
            while (true)
            {
                yield return current;
                current = current.Next;
                if (Equals(current, this))
                {
                    break;
                }
            }
        }


        public override bool Equals(object obj)
        {
            return City == ((Stop)obj)?.City;
        }


        public override int GetHashCode()
        {
            return City.GetHashCode();
        }
    }


    public class Tour
    {
        public Stop Anchor { get; }

        public Tour(IEnumerable<Stop> stops)
        {
            Anchor = stops.First();
        }

        //the set of tours we can make with 2-opt out of this one
        public IEnumerable<Tour> GenerateMutations()
        {
            for (var stop = Anchor; !Equals(stop.Next, Anchor); stop = stop.Next)
            {
                //skip the next one, since you can't swap with that
                var current = stop.Next.Next;
                while (!Equals(current, Anchor))
                {
                    yield return CloneWithSwap(stop.City, current.City);
                    current = current.Next;
                }
            }
        }


        public Tour CloneWithSwap(City firstCity, City secondCity)
        {
            Stop firstFrom = null, secondFrom = null;
            var stops = UnconnectedClones();
            stops.Connect(true);

            foreach (var stop in stops)
            {
                if (stop.City == firstCity)
                {
                    firstFrom = stop;
                }

                if (stop.City == secondCity)
                {
                    secondFrom = stop;
                }
            }

            //the swap part
            if (firstFrom != null)
            {
                var firstTo = firstFrom.Next;
                if (secondFrom != null)
                {
                    var secondTo = secondFrom.Next;

                    //reverse all of the links between the swaps
                    firstTo.CanGetTo().TakeWhile(stop => !Equals(stop, secondTo)).Reverse().Connect(false);

                    firstTo.Next = secondTo;
                }
            }

            if (firstFrom != null)
            {
                firstFrom.Next = secondFrom;
            }

            var tour = new Tour(stops);
            return tour;
        }


        public IList<Stop> UnconnectedClones()
        {
            return Cycle().Select(stop => stop.Clone()).ToList();
        }


        public double Cost()
        {
            return Cycle().Aggregate(0.0, (sum, stop) => sum + Stop.Distance(stop, stop.Next));
        }


        public (City[], double) GetResult()
        {
            var list = Cycle().ToArray();
            var cost = list.Aggregate(0.0, (sum, stop) => sum + Stop.Distance(stop, stop.Next));

            var path = list.Select(stop => stop.City).ToArray();
            return (path, cost);
        }


        private IEnumerable<Stop> Cycle()
        {
            return Anchor.CanGetTo();
        }
    }

    public static class MyClass
    {
        //take an ordered list of nodes and set their next properties
        public static void Connect(this IEnumerable<Stop> stops, bool loop)
        {
            Stop prev = null, first = null;
            foreach (var stop in stops)
            {
                if (first == null)
                {
                    first = stop;
                }

                if (prev != null)
                {
                    prev.Next = stop;
                }

                prev = stop;
            }

            if (loop)
            {
                if (prev != null)
                {
                    prev.Next = first;
                }
            }
        }


        //T with the smallest func(T)
        public static T MinBy<T, TComparable>(this IEnumerable<T> xs, Func<T, TComparable> func) where TComparable : IComparable<TComparable>
        {
            return xs.DefaultIfEmpty().Aggregate((maxSoFar, elem) => func(elem).CompareTo(func(maxSoFar)) > 0 ? maxSoFar : elem);
        }


        //return an ordered nearest neighbor set
        public static IEnumerable<Stop> NearestNeighbors(this IEnumerable<Stop> stops)
        {
            var stopsLeft = stops.ToList();
            for (var stop = stopsLeft.First(); stop != null; stop = stopsLeft.MinBy(s => Stop.Distance(stop, s)))
            {
                stopsLeft.Remove(stop);
                yield return stop;
            }
        }
    }
}