using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace NeuralNetwork
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public int Inputnodes { get; set; } = 784;
        public int Hiddennodes { get; set; } = 300;
        public int Outputnodes { get; set; } = 10;
        public double Learningrate { get; set; } = 0.1;

        private bool _OnWork = false;
        private bool OnWork
        {
            get => _OnWork;
            set
            {
                _OnWork = value;
                if (_OnWork)
                {
                    AppStatus.Content = "Busy..";
                    AppStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    AppStatus.Content = "Waiting..";
                    AppStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
        }

        private neuralNetwork neural;
        private CSV cSV = new CSV();
        private List<Items> itemList = new List<Items>();
        //private LogView log = new LogView();

        public MainWindow()
        {
            InitializeComponent();
            neural = new neuralNetwork(Inputnodes, Hiddennodes, Outputnodes, Learningrate);

            //log.Show();
        }

        //訓練用元件
        private async void StartTraining_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (itemList.Count > 0)
                {
                    WorkProgress.Maximum = itemList.Count;
                    WorkProgress.Value = 0;

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        double[] inputs = itemList[i].DoubleArray;
                        double[] targets = neural.Purely_Array(0.01, Outputnodes);
                        targets[Convert.ToInt32(itemList[i].Name)] = 0.99;
                        await neural.Train(inputs, targets);
                        WorkProgress.Value++;
                    }
                }
                else
                {
                    _ = MessageBox.Show("請輸入資料");
                }

                OnWork = false;
            }
        }

        private async void GetCSV_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "讀取",
                    Filter = "csv File (.csv)|*.csv",
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string[] lines = System.IO.File.ReadAllLines(openFileDialog.FileName); //行
                    WorkProgress.Maximum = lines.GetLength(0);
                    WorkProgress.Value = 0;

                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        if (lines.GetLength(0) <= 100)
                        {
                            string[] row = lines[i].Split(','); //列，這裡有785個字節
                            await ImageViewAddImage(row);
                        }
                        else
                        {
                            string[] row = lines[i].Split(','); //列，這裡有785個字節
                            double[] targets_double = neural.StringArrayToDoubleArray(row);
                            double[] inputs = neural.ArrayWeighted(targets_double);
                            double[] targets = neural.Purely_Array(0.01, Outputnodes);
                            targets[Convert.ToInt32(row[0])] = 0.99;
                            await neural.Train(inputs, targets);
                        }

                        WorkProgress.Value++;
                    }

                    if (lines.GetLength(0) > 100)
                    {
                        Label label = new Label()
                        {
                            Content = "Total: " + lines.GetLength(0),
                            Height = 40,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        _ = ImageView.Children.Add(label);
                    }
                }

                OnWork = false;
            }
        }

        private async Task ImageViewAddImage(string[] row)
        {
            double[] input = neural.StringArrayToDoubleArray(row);
            string[] input_str = neural.DoubleArrayToStringArray(input);
            double[] input_double = neural.ArrayWeighted(input);

            Bitmap bmp = cSV.ArrayToBitmpa(input_str); //轉成28*28的圖片
            BitmapImage image = cSV.BitmapToBitmapImage(bmp); //Bitmap轉成BitmapImage

            Grid grid = new Grid()
            {
                Width = 28,
                Height = 28,
                Margin = new Thickness(5, 5, 5, 5)
            };
            Label label = new Label()
            {
                Content = row[0],
                FontSize = 8,
                Padding = new Thickness(0),
                Foreground = System.Windows.Media.Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            System.Windows.Controls.Image img = new System.Windows.Controls.Image
            {
                Width = 28,
                Height = 28,
                Source = image
            };
            Button button = new Button
            {
                Width = 28,
                Height = 28,
                Background = System.Windows.Media.Brushes.Transparent,
                Opacity = 0.3,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1, 1, 1, 1)
            };

            button.Click += ImageButton_Click;
            _ = grid.Children.Add(img);
            _ = grid.Children.Add(label);
            _ = grid.Children.Add(button);
            _ = ImageView.Children.Add(grid);

            Items items = new Items()
            {
                Name = row[0],
                StringArray = input_str,
                DoubleArray = input_double,
                DoubleArrayToString = neural.DoubleArrayToStringArray(input_double),
                Grid = grid,
                Button = button
            };
            itemList.Add(items);

            await Task.Delay(1);
        }

        private Button OnSelected = null;

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if(OnSelected != null)
                {
                    OnSelected.Background = System.Windows.Media.Brushes.White;
                }

                OnSelected = (Button)sender;
                OnSelected.Background = System.Windows.Media.Brushes.LightBlue;
                CreateTrainingArray(itemList.FindIndex(x => x.Button == OnSelected));

                OnWork = false;
            }
        }

        private async void CreateTrainingArray(int index)
        {
            string[] row = itemList[index].StringArray;
            string[] dRow = itemList[index].DoubleArrayToString;
            TrainingArray.Children.Clear();

            for (int i = 0; i < 28; i++)
            {
                StackPanel stack = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                for (int j = 0; j < 28; j++)
                {
                    if ((bool)DoubleArrayCheck.IsChecked)
                    {
                        Label label = new Label()
                        {
                            Content = dRow[(i * 28) + j],
                            Width = 40
                        };
                        stack.Children.Add(label);
                    }
                    else
                    {
                        Label label = new Label()
                        {
                            Content = row[(i * 28) + j],
                            Width = 40
                        };
                        stack.Children.Add(label);
                    }
                }

                _ = TrainingArray.Children.Add(stack);
                await Task.Delay(1);
            }
        }

        private void DelectChoosed_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (OnSelected != null)
                {
                    OnSelected.Background = System.Windows.Media.Brushes.White;
                    int index = itemList.FindIndex(x => x.Button == OnSelected);
                    Grid grid = (Grid)OnSelected.Parent;
                    itemList.RemoveAt(index);
                    ImageView.Children.Remove(grid);
                    OnSelected = null;
                }

                OnWork = false;
            }
        }

        private void AllClear_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                OnSelected = null;
                itemList.Clear();
                ImageView.Children.Clear();
                neural = new neuralNetwork(Inputnodes, Hiddennodes, Outputnodes, Learningrate);

                OnWork = false;
            }
        }


        //測試用
        private QueryShow queryShow = null;

        private List<Items> QuaryList = new List<Items>();

        private Button OnSelectQuary = null;

        private async void QuaryImage_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "讀取",
                    Filter = "csv File (.csv)|*.csv",
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string[] lines = System.IO.File.ReadAllLines(openFileDialog.FileName);
                    WorkProgress.Maximum = lines.GetLength(0);
                    WorkProgress.Value = 0;

                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        string[] all_values = lines[i].Split(',');
                        await QueryImageAddImage(all_values);
                        WorkProgress.Value++;
                    }
                }

                OnWork = false;
            }
        }

        private async Task QueryImageAddImage(string[] row)
        {
            double[] input = neural.StringArrayToDoubleArray(row);
            string[] input_str = neural.DoubleArrayToStringArray(input);
            double[] input_double = neural.ArrayWeighted(input);

            Bitmap bmp = cSV.ArrayToBitmpa(input_str); //轉成28*28的圖片
            BitmapImage image = cSV.BitmapToBitmapImage(bmp); //Bitmap轉成BitmapImage

            Grid grid = new Grid()
            {
                Width = 28,
                Height = 28,
                Margin = new Thickness(5, 5, 5, 5)
            };
            Label label = new Label()
            {
                Content = row[0],
                FontSize = 8,
                Padding = new Thickness(0),
                Foreground = System.Windows.Media.Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            System.Windows.Controls.Image img = new System.Windows.Controls.Image
            {
                Width = 28,
                Height = 28,
                Source = image
            };
            Button button = new Button
            {
                Width = 28,
                Height = 28,
                Background = System.Windows.Media.Brushes.Transparent,
                Opacity = 0.3,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1, 1, 1, 1)
            };

            button.Click += QueryImage_Click; ;
            _ = grid.Children.Add(img);
            _ = grid.Children.Add(label);
            _ = grid.Children.Add(button);
            _ = QueryImage.Children.Add(grid);

            Items items = new Items()
            {
                Name = row[0],
                StringArray = input_str,
                DoubleArray = input_double,
                DoubleArrayToString = neural.DoubleArrayToStringArray(input_double),
                Grid = grid,
                Button = button
            };
            QuaryList.Add(items);

            await Task.Delay(1);
        }

        private void QueryImage_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (OnSelectQuary != null)
                {
                    OnSelectQuary.Background = System.Windows.Media.Brushes.White;
                }

                OnSelectQuary = (Button)sender;
                OnSelectQuary.Background = System.Windows.Media.Brushes.LightBlue;
                CreateQuaryArray(QuaryList.FindIndex(x => x.Button == OnSelectQuary));

                OnWork = false;
            }
        }

        private async void CreateQuaryArray(int index)
        {
            string[] row = QuaryList[index].StringArray;
            string[] dRow = QuaryList[index].DoubleArrayToString;
            ImageArray.Children.Clear();

            for (int i = 0; i < 28; i++)
            {
                StackPanel stack = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                for (int j = 0; j < 28; j++)
                {
                    if ((bool)DoubleArrayCheck.IsChecked)
                    {
                        Label label = new Label()
                        {
                            Content = dRow[(i * 28) + j],
                            Width = 40
                        };
                        stack.Children.Add(label);
                    }
                    else
                    {
                        Label label = new Label()
                        {
                            Content = row[(i * 28) + j],
                            Width = 40
                        };
                        stack.Children.Add(label);
                    }
                }

                _ = ImageArray.Children.Add(stack);
                await Task.Delay(1);
            }
        }

        private async void StartQuery_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (QuaryList.Count > 0)
                {
                    QueryError.Content = "";
                    if (queryShow == null)
                    {
                        queryShow = new QueryShow();
                        queryShow.Show();
                        queryShow.CloseClicked += () =>
                        {
                            queryShow = null;
                        };
                        queryShow.QueryRight += QueryShow_QueryRight;
                        queryShow.QueryWrong += QueryShow_QueryWrong;
                    }

                    WorkProgress.Maximum = QuaryList.Count;
                    WorkProgress.Value = 0;
                    queryShow.SetMaxValue(QuaryList.Count);
                    RunQuary();
                }
                else
                {
                    QueryError.Content = "請讀取圖片";
                    for (int i = 0; i < 4; i++)
                    {
                        QueryError.Margin = new Thickness(12, 10, 12, 10);
                        await Task.Delay(50);
                        QueryError.Margin = new Thickness(10, 10, 10, 10);
                        await Task.Delay(50);
                    }

                    OnWork = false;
                }
            }
        }

        private async void RunQuary()
        {
            for (int i = 0; i < QuaryList.Count; i++)
            {
                double[] inputs = QuaryList[i].DoubleArray;
                queryShow.SetCorrectAnswer(QuaryList[i].Name);
                double[,] outputs = await neural.Query(inputs);
                queryShow.SetQueryAnswer(outputs);
                QueryImage.Children.Remove(QuaryList[i].Grid);
            }

            QuaryList = new List<Items>();
            OnWork = false;
        }

        private void QueryShow_QueryRight()
        {
            QueryError.Content = "Correct!";
            queryShow.SetNowValue();
        }

        private void QueryShow_QueryWrong()
        {
            QueryError.Content = "Wrong!";
        }


        //其他功能
        private void DoubleArrayCheck_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (OnSelectQuary != null)
                {
                    CreateQuaryArray(QuaryList.FindIndex(x => x.Button == OnSelectQuary));
                }

                if (OnSelected != null)
                {
                    CreateTrainingArray(itemList.FindIndex(x => x.Button == OnSelected));
                }

                OnWork = false;
            }
        }

        private void WHO_Click(object sender, RoutedEventArgs e)
        {
            Look_WHO look_ = new Look_WHO(neuralNetwork.who);
            look_.BackClicked += () => { PopupGrid.Children.Clear(); };
            _ = PopupGrid.Children.Add(look_);
        }

        private void WIH_Click(object sender, RoutedEventArgs e)
        {
            Look_WIH look_ = new Look_WIH(neuralNetwork.wih);
            look_.BackClicked += () => { PopupGrid.Children.Clear(); };
            _ = PopupGrid.Children.Add(look_);
        }

        private void Normal_Click(object sender, RoutedEventArgs e)
        {
            NormalDistribution normalDistribution = new NormalDistribution();
            normalDistribution.BackClicked += () => { PopupGrid.Children.Clear(); };
            _ = PopupGrid.Children.Add(normalDistribution);
        }

        private class Items
        {
            public string Name { get; set; } //答案

            public Grid Grid { get; set; }

            public Button Button { get; set; }

            public double[] DoubleArray { get; set; } //

            public string[] DoubleArrayToString { get; set; } //

            public string[] StringArray { get; set; } //
        }
    }
}
