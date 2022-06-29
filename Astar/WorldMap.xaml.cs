using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GamblingHero.Page
{
    /// <summary>
    /// WorldMap.xaml 的互動邏輯
    /// </summary>
    public partial class WorldMap : UserControl
    {
        private ImageSource MB_03, MB_04;
        private ImageBrush M1_1, M1_2, M2_1;
        private Style WorldMapButton;
        private bool OnMove = false;

        private double startX = 0, startY = 0;

        public WorldMap()
        {
            InitializeComponent();
            Loaded += WorldMap_Loaded;
        }

        private void WorldMap_Loaded(object sender, RoutedEventArgs e)
        {
            SetImageBrush();
            CreateMap();
            MapAnimation();
        }

        private void SetImageBrush()
        {
            MB_03 = Application.Current.FindResource("Water01") as ImageSource;
            MB_04 = Application.Current.FindResource("Water02") as ImageSource;

            M1_1 = this.Resources["M1_1"] as ImageBrush;
            M1_2 = this.Resources["M1_2"] as ImageBrush;
            M2_1 = this.Resources["M2_1"] as ImageBrush;

            WorldMapButton = Application.Current.FindResource("WorldMapButton") as Style;
        }

        private void CreateMap()
        {
            MapBackground.Width = 1000;
            MapBackground.Height = 1000;
            MapObjects.Width = 1000;
            MapObjects.Height = 1000;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for(int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i,j] == "W") 
                    {
                        Label img = new Label();
                        img.Height = 100;
                        img.Width = 100;
                        img.Background = M2_1;
                        MapBackground.Children.Add(img);
                    }
                    else
                    {
                        Button button = new Button();
                        button.Height = 100;
                        button.Width = 100;
                        button.Style = WorldMapButton;
                        button.Click += MapButton_Click;

                        if (j % 2 == 0) { button.Background = M1_1; }
                        else { button.Background = M1_2; }

                        MapBackground.Children.Add(button);
                    }
                }
            }
        }

        private async void MapButton_Click(object sender, RoutedEventArgs e)
        {
            if(OnMove == false)
            {
                OnMove = true;

                Button button = (Button)e.Source;
                Point point = button.TransformToAncestor(MapBackground).Transform(new Point(0, 0));
                if(await AStar(point) == false) { await FindPathMove(point); }

                OnMove = false;
            }
        }

        private async Task FindPathMove(Point point)
        {
            double newX = 0; double newY = 0; double newX2 = 0; double newY2 = 0;
            string orientation = "";
            if (point.X >= startX && point.Y >= startY) //目標在原點的右下
            {
                newX = startX; newY = startY;
                newX2 = (int)point.X; newY2 = (int)point.Y;
                orientation = "右下";
            }
            else if (point.X >= startX && point.Y < startY) //目標在原點的右上
            {
                newX = startX; newY = (int)point.Y;
                newX2 = (int)point.X; newY2 = startY;
                orientation = "右上";
            }
            else if (point.X < startX && point.Y >= startY) //目標在原點的左下
            {
                newX = (int)point.X; newY = startY;
                newX2 = startX; newY2 = (int)point.Y;
                orientation = "左下";
            }
            else if (point.X < startX && point.Y < startY) //目標在原點的左上
            {
                newX = (int)point.X; newY = (int)point.Y;
                newX2 = startX; newY2 = startY;
                orientation = "左上";
            }

            newX /= 100; newY /= 100; newX2 /= 100; newY2 /= 100;
            double XLength = Math.Abs(newX - newX2) + 1;
            double YLength = Math.Abs(newY - newY2) + 1;

            string[,] newmap = new string[(int)YLength, (int)XLength]; //繪製新區塊
            for (int i = 0; i < YLength; i++)
            {
                for (int j = 0; j < XLength; j++)
                {
                    newmap[i, j] = map[(int)newY + i, (int)newX + j];
                }
            }

            double[,] mappoint = new double[(int)YLength, (int)XLength]; //新區塊評分代價，無限阻力為65535
            for (int i = 0; i < newmap.GetLength(0); i++)
            {
                for (int j = 0; j < newmap.GetLength(1); j++)
                {
                    if (newmap[i, j] == "")
                    {
                        double EndX = newX + j; double EndY = newY + i;
                        double Distance = Math.Abs(EndX - (startX / 100)) + Math.Abs(EndY - (startY / 100));
                        mappoint[i, j] = Distance;
                    }
                    else
                    {
                        mappoint[i, j] = 65535;
                    }
                }
            }

            MapObjects.Children.Clear();

            List<PointList> pointLists = new List<PointList>();
            if (orientation == "右下")
            {
                List<PointList> Path01 = new List<PointList>();
                List<PointList> Path02 = new List<PointList>();

                int index1 = 0; int count1 = 0; int score1 = 0;
                for (int i = 0; i < mappoint.GetLength(0); i++)
                {
                    for (int j = 0; j < mappoint.GetLength(1); j++)
                    {
                        if (mappoint[i, j] == index1)
                        {
                            Path01.Add(new PointList()
                            {
                                score = index1,
                                X = startX + (j * 100),
                                Y = startY + (i * 100)
                            });
                            index1++;
                            count1 = 0;
                            score1 += (i * j);
                        }
                        else { count1++; }
                    }
                    if (count1 >= mappoint.GetLength(1)) { break; }
                }

                int index2 = 0; int count2 = 0; int score2 = 0;
                for (int j = 0; j < mappoint.GetLength(1); j++)
                {
                    for (int i = 0; i < mappoint.GetLength(0); i++)
                    {
                        if (mappoint[i, j] == index2)
                        {
                            Path02.Add(new PointList()
                            {
                                score = index2,
                                X = startX + (j * 100),
                                Y = startY + (i * 100)
                            });
                            index2++;
                            count2 = 0;
                            score2 += (i * j);
                        }
                        else { count2++; }
                    }
                    if (count2 >= mappoint.GetLength(0)) { break; }
                }

                if (Path01.Count > Path02.Count) { pointLists = Path01; }
                else if (Path01.Count < Path02.Count) { pointLists = Path02; }
                else
                {
                    if (score1 > score2) { pointLists = Path01; }
                    else { pointLists = Path02; }
                }
            }
            else if (orientation == "右上")
            {
                List<PointList> Path01 = new List<PointList>();
                List<PointList> Path02 = new List<PointList>();

                int index1 = 0; int count1 = 0; int score1 = 0;
                for (int j = 0; j < mappoint.GetLength(1); j++)
                {
                    for (int i = mappoint.GetLength(0) - 1; i >= 0; i--)
                    {
                        if (mappoint[i, j] == index1)
                        {
                            Path01.Add(new PointList()
                            {
                                score = index1,
                                X = startX + (j * 100),
                                Y = startY + ((i - mappoint.GetLength(0) + 1) * 100)
                            });
                            index1++;
                            count1 = 0;
                            score1 += (i * j);
                        }
                        else { count1++; }
                    }
                    if (count1 >= mappoint.GetLength(0)) { break; }
                }

                int index2 = 0; int count2 = 0; int score2 = 0;
                for (int i = mappoint.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = 0; j < mappoint.GetLength(1); j++)
                    {
                        if (mappoint[i, j] == index2)
                        {
                            Path02.Add(new PointList()
                            {
                                score = index2,
                                X = startX + (j * 100),
                                Y = startY + ((i - mappoint.GetLength(0) + 1) * 100)
                            });
                            index2++;
                            count2 = 0;
                            score2 += (i * j);
                        }
                        else { count2++; }
                    }
                    if (count2 >= mappoint.GetLength(1)) { break; }
                }

                if (Path01.Count > Path02.Count) { pointLists = Path01; }
                else if (Path01.Count < Path02.Count) { pointLists = Path02; }
                else
                {
                    if (score1 > score2) { pointLists = Path01; }
                    else { pointLists = Path02; }
                }
            }
            else if (orientation == "左下")
            {
                List<PointList> Path01 = new List<PointList>();
                List<PointList> Path02 = new List<PointList>();

                int index1 = 0; int count1 = 0; int score1 = 0;
                for (int j = mappoint.GetLength(1) - 1; j >= 0; j--)
                {
                    for (int i = 0; i < mappoint.GetLength(0); i++)
                    {
                        if (mappoint[i, j] == index1)
                        {
                            Path01.Add(new PointList()
                            {
                                score = index1,
                                X = startX + ((j - mappoint.GetLength(1) + 1) * 100),
                                Y = startY + (i * 100)
                            });

                            index1++;
                            count1 = 0;
                            score1 += (i * j);
                        }
                        else { count1++; }
                    }
                    if (count1 >= mappoint.GetLength(0)) { break; }
                }

                int index2 = 0; int count2 = 0; int score2 = 0;
                for (int i = 0; i < mappoint.GetLength(0); i++)
                {
                    for (int j = mappoint.GetLength(1) - 1; j >= 0; j--)
                    {
                        if (mappoint[i, j] == index2)
                        {
                            Path02.Add(new PointList()
                            {
                                score = index2,
                                X = startX + ((j - mappoint.GetLength(1) + 1) * 100),
                                Y = startY + (i * 100)
                            });

                            index2++;
                            count2 = 0;
                            score2 += (i * j);
                        }
                        else { count2++; }
                    }
                    if (count2 >= mappoint.GetLength(1)) { break; }
                }

                if (Path01.Count > Path02.Count) { pointLists = Path01; }
                else if (Path01.Count < Path02.Count) { pointLists = Path02; }
                else
                {
                    if (score1 > score2) { pointLists = Path01; }
                    else { pointLists = Path02; }
                }
            }
            else if (orientation == "左上")
            {
                List<PointList> Path01 = new List<PointList>();
                List<PointList> Path02 = new List<PointList>();

                int index1 = 0; int count1 = 0; int score1 = 0;
                for (int i = mappoint.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = mappoint.GetLength(1) - 1; j >= 0; j--)
                    {
                        if (mappoint[i, j] == index1)
                        {
                            Path01.Add(new PointList()
                            {
                                score = index1,
                                X = startX + ((j - mappoint.GetLength(1) + 1) * 100),
                                Y = startY + ((i - mappoint.GetLength(0) + 1) * 100)
                            });

                            index1++;
                            count1 = 0;
                            score1 += (i * j);
                        }
                        else { count1++; }
                    }
                    if (count1 >= mappoint.GetLength(1)) { break; }
                }

                int index2 = 0; int count2 = 0; int score2 = 0;
                for (int j = mappoint.GetLength(1) - 1; j >= 0; j--)
                {
                    for (int i = mappoint.GetLength(0) - 1; i >= 0; i--)
                    {
                        if (mappoint[i, j] == index2)
                        {
                            Path02.Add(new PointList()
                            {
                                score = index2,
                                X = startX + ((j - mappoint.GetLength(1) + 1) * 100),
                                Y = startY + ((i - mappoint.GetLength(0) + 1) * 100)
                            });

                            index2++;
                            count2 = 0;
                            score2 += (i * j);
                        }
                        else { count2++; }
                    }
                    if (count2 >= mappoint.GetLength(0)) { break; }
                }

                if (Path01.Count > Path02.Count) { pointLists = Path01; }
                else if (Path01.Count < Path02.Count) { pointLists = Path02; }
                else
                {
                    if (score1 > score2) { pointLists = Path01; }
                    else { pointLists = Path02; }
                }
            }

            for (int i = 0; i < pointLists.Count; i++)
            {
                Border border = new Border();
                border.Width = 100;
                border.Height = 100;
                border.Background = Brushes.Blue;
                MapObjects.Children.Add(border);
                Canvas.SetLeft(border, pointLists[i].X);
                Canvas.SetTop(border, pointLists[i].Y);

                await Task.Delay(100);

                if (i != pointLists.Count - 1) { MapObjects.Children.Remove(border); }
            }
            startX = pointLists[pointLists.Count - 1].X;
            startY = pointLists[pointLists.Count - 1].Y;
        }

        private async Task<bool> AStar(Point point)
        {
            int[,] mappoint = new int[map.GetLength(0), map.GetLength(1)]; //新區塊評分代價，無限阻力為65535
            for (int i = 0; i < mappoint.GetLength(0); i++)
            {
                for (int j = 0; j < mappoint.GetLength(1); j++)
                {
                    if (map[i, j] == "")
                    {
                        double EndX = point.X; double EndY = point.Y;
                        double Distance1 = Math.Abs(j - (startX / 100)) + Math.Abs(i - (startY / 100));
                        double Distance2 = Math.Abs(j - (EndX / 100)) + Math.Abs(i - (EndY / 100));
                        mappoint[i, j] = (int)Distance1 + (int)Distance2;
                    }
                    else
                    {
                        mappoint[i, j] = 65535;
                    }
                }
            }

            bool[,] maplock = new bool[mappoint.GetLength(0), mappoint.GetLength(1)];

            List<PointList> pl = new List<PointList>();
            List<PointList> openpl = new List<PointList>();
            Point NowPoint = new Point(startX / 100, startY / 100);
            int NowScore = mappoint[(int)NowPoint.Y, (int)NowPoint.X];
            string NowDirection = "使";
            bool final = false; bool CanGo = true;
            do
            {
                //上
                if (NowPoint.Y > 0)
                {
                    if (mappoint[(int)NowPoint.Y - 1, (int)NowPoint.X] <= NowScore + 2 &&
                        pl.FindIndex(x => x.X == (int)NowPoint.X && x.Y == (int)NowPoint.Y - 1) == -1 &&
                        maplock[(int)NowPoint.Y - 1, (int)NowPoint.X] != true)
                    {
                        pl.Add(new PointList()
                        {
                            direction = "下",
                            score = mappoint[(int)NowPoint.Y - 1, (int)NowPoint.X],
                            X = (int)NowPoint.X,
                            Y = (int)NowPoint.Y - 1
                        });
                    }
                }

                //下
                if (NowPoint.Y < mappoint.GetLength(0) - 1)
                {
                    if (mappoint[(int)NowPoint.Y + 1, (int)NowPoint.X] <= NowScore + 2 &&
                        pl.FindIndex(x => x.X == (int)NowPoint.X && x.Y == (int)NowPoint.Y + 1) == -1 &&
                        maplock[(int)NowPoint.Y + 1, (int)NowPoint.X] != true)
                    {
                        pl.Add(new PointList()
                        {
                            direction = "上",
                            score = mappoint[(int)NowPoint.Y + 1, (int)NowPoint.X],
                            X = (int)NowPoint.X,
                            Y = (int)NowPoint.Y + 1
                        });
                    }
                }

                //左
                if (NowPoint.X > 0)
                {
                    if (mappoint[(int)NowPoint.Y, (int)NowPoint.X - 1] <= NowScore + 2 &&
                        pl.FindIndex(x => x.X == (int)NowPoint.X - 1 && x.Y == (int)NowPoint.Y) == -1 &&
                        maplock[(int)NowPoint.Y, (int)NowPoint.X - 1] != true)
                    {
                        pl.Add(new PointList()
                        {
                            direction = "右",
                            score = mappoint[(int)NowPoint.Y, (int)NowPoint.X - 1],
                            X = (int)NowPoint.X - 1,
                            Y = (int)NowPoint.Y
                        });
                    }
                }

                //右
                if (NowPoint.X < mappoint.GetLength(1) - 1)
                {
                    if (mappoint[(int)NowPoint.Y, (int)NowPoint.X + 1] <= NowScore + 2 &&
                        pl.FindIndex(x => x.X == (int)NowPoint.X + 1 && x.Y == (int)NowPoint.Y) == -1 &&
                        maplock[(int)NowPoint.Y, (int)NowPoint.X + 1] != true)
                    {
                        pl.Add(new PointList()
                        {
                            direction = "左",
                            score = mappoint[(int)NowPoint.Y, (int)NowPoint.X + 1],
                            X = (int)NowPoint.X + 1,
                            Y = (int)NowPoint.Y
                        });
                    }
                }

                maplock[(int)NowPoint.Y, (int)NowPoint.X] = true;

                int plindex = pl.FindIndex(x => x.X == NowPoint.X && x.Y == NowPoint.Y);
                if (plindex != -1) { openpl.Add(pl[plindex]); pl.RemoveAt(plindex); }
                if (final) { break; }
                if (pl.Count == 0) { CanGo = false; break; }

                NowPoint = new Point(pl[0].X, pl[0].Y);
                NowDirection = pl[0].direction;
                NowScore = pl[0].score;

                if (NowPoint.X * 100 == point.X && NowPoint.Y * 100 == point.Y) { final = true; }
            }
            while (true);

            if (CanGo)
            {
                List<PointList> pointLists = new List<PointList>();
                pointLists.Add(new PointList() { X = point.X, Y = point.Y });
                int NowX = (int)(point.X * 0.01);
                int NowY = (int)(point.Y * 0.01);
                do
                {
                    if (openpl[openpl.FindIndex(x => x.X == NowX && x.Y == NowY)].direction == "上") { NowY--; }
                    else if (openpl[openpl.FindIndex(x => x.X == NowX && x.Y == NowY)].direction == "下") { NowY++; }
                    else if (openpl[openpl.FindIndex(x => x.X == NowX && x.Y == NowY)].direction == "左") { NowX--; }
                    else if (openpl[openpl.FindIndex(x => x.X == NowX && x.Y == NowY)].direction == "右") { NowX++; }
                    pointLists.Add(new PointList() { X = NowX * 100, Y = NowY * 100 });

                    if (NowX * 100 == startX && NowY * 100 == startY) { break; }
                }
                while (true);

                for (int i = pointLists.Count - 1; i >= 0; i--)
                {
                    MapObjects.Children.Clear();
                    Border border = new Border();
                    border.Width = 100;
                    border.Height = 100;
                    border.Background = Brushes.Blue;
                    MapObjects.Children.Add(border);
                    Canvas.SetLeft(border, pointLists[i].X);
                    Canvas.SetTop(border, pointLists[i].Y);

                    await Task.Delay(100);
                }
                startX = pointLists[0].X;
                startY = pointLists[0].Y;
            }

            return CanGo;
        }

        private async void MapAnimation()
        {
            do
            {
                M2_1.ImageSource = MB_03;
                await Task.Delay(1000);

                M2_1.ImageSource = MB_04;
                await Task.Delay(1000);
            } 
            while (true);
        }

        private class PointList
        {
            public string direction { get; set; }

            public int score { get; set; }

            public double X { get; set; }

            public double Y { get; set; }
        }

        private string[,] map = new string[10, 10] 
        { 
            { "", "W", "W", "W", "", "", "", "", "", "" },
            { "", "W", "", "", "", "", "", "", "", "" },
            { "", "W", "", "W", "", "", "", "", "", "" },
            { "", "W", "W", "W", "W", "", "", "", "", "" },
            { "", "", "W", "W", "W", "W", "", "", "", "" },
            { "", "", "", "", "", "W", "", "", "", "" },
            { "", "", "", "W", "", "", "", "", "", "" },
            { "", "", "", "W", "W", "W", "W", "", "", "" },
            { "", "", "", "W", "", "", "W", "W", "", "" },
            { "", "", "", "W", "", "", "W", "", "", "" },
        };
    }
}
