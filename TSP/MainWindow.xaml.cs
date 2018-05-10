using System;
using System.Collections.Generic;
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
        private int h = 1000;
        private int w = 1000;

        public MainWindow()
        {
            InitializeComponent();
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
                                            for (var i = 0; i < count; i++)
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
                                        },
                                        TaskCreationOptions.LongRunning);

            IC.ItemsSource = pts;
            bt1.IsEnabled = true;
            bt2.IsEnabled = true;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TB.Text = "";
            TB2.Text = "";
            bt1.IsEnabled = false;
            bt2.IsEnabled = false;

            var result = await _antHandler.Run(_pheromoneMatrix, _coordMatrix, Alpha, Beta, Evaporation, Q);

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

            TB.Text = result.Item2.ToString("0.000");

            var result2 = OrderByDistance(pts, new Point(0, 0));

            //var pp = result2.Item1;
            //PF2.StartPoint = pp.First();
            //pp.RemoveAt(0);

            //var ps2 = new PointCollection();
            //foreach (var el in pp)
            //{
            //    ps2.Add(el);
            //}

            //ps2.Freeze();
            //PS2.Points = ps2;

            TB2.Text = result2.Item2.ToString("0.000");

            bt1.IsEnabled = true;
            bt2.IsEnabled = true;
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
    }
}