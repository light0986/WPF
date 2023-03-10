using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NeuralNetwork
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public int Inputnodes { get; set; } = 784;
        public int Hiddennodes { get; set; } = 100;
        public int Outputnodes { get; set; } = 10; // = 10;
        public double Learningrate { get; set; } = 0.1;

        private bool _OnWork = false;
        public bool OnWork
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

        private List<Network> networkList = new List<Network>();
        private CSV cSV = new CSV();
        private List<Items> itemList = new List<Items>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OnWork = true;
            StartSetting startSetting = new StartSetting();
            startSetting.OKClicked += (inputnodes, hiddennodes, outputnodes, learningrate, onenetwork) =>
            {
                Inputnodes = inputnodes;
                Hiddennodes = hiddennodes;
                Outputnodes = outputnodes;
                Learningrate = learningrate;

                if (onenetwork)
                {
                    networkList.Add(new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = "Network" });
                }
                else
                {
                    for (int i = 0; i < Outputnodes; i++)
                    {
                        networkList.Add(new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = i.ToString() });
                    }
                }
                PopupGrid.Children.Clear();
                OnWork = false;
            };
            _ = PopupGrid.Children.Add(startSetting);
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = OnWork;
            for (int i = 0; i < 5; i++)
            {

                MainGrid.Margin = new Thickness(1, 0, -1, 0);
                await Task.Delay(50);
                MainGrid.Margin = new Thickness(-1, 0, 1, 0);
                await Task.Delay(50);
            }
            MainGrid.Margin = new Thickness(0, 0, 0, 0);
        }

        //訓練用元件
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

                    Look_Matrix look_Matrix = null;
                    bool OnPause = false; bool Skip = false;
                    void OutputMatrix(string name, double[,] matrix, string description)
                    {
                        look_Matrix.InputMatrix(name, matrix, description);
                    };

                    if (lines.GetLength(0) > 1000)
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

                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        if (lines.GetLength(0) <= 1000)
                        {
                            string[] row = lines[i].Split(','); //列，這裡有785個字節
                            await ImageViewAddImage(row);
                        }
                        else
                        {
                            int j;
                            if (networkList[0].Name == "Network")
                            {
                                j = 0;
                            }
                            else
                            {
                                j = networkList.FindIndex(x => x.Name == itemList[i].Name);
                            }

                            if (look_Matrix == null && Skip == false)
                            {
                                look_Matrix = new Look_Matrix();
                                _ = PopupGrid.Children.Add(look_Matrix);
                                look_Matrix.CloseClicked += () =>
                                {
                                    look_Matrix = null;
                                    OnPause = false;
                                    PopupGrid.Children.Clear();
                                };
                                look_Matrix.NextClicked += () =>
                                {
                                    OnPause = false;
                                };
                                look_Matrix.SkipClicked += () =>
                                {
                                    Skip = true;
                                    OnPause = false;
                                    PopupGrid.Children.Clear();
                                };
                            }
                            if (Skip == false)
                            {
                                OnPause = true;
                                look_Matrix.SetNeuralName(networkList[i].Name);
                                networkList[j].neural.OutputMatrix += OutputMatrix;
                            }

                            //訓練原始圖片
                            string[] row = lines[i].Split(','); //列，這裡有785個字節
                            double[] targets_double = neuralNetwork.StringArrayToDoubleArray(row);
                            double[] inputs = neuralNetwork.ArrayWeighted(targets_double);
                            double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);

                            if(networkList[0].Name == "Network")
                            {
                                targets[Convert.ToInt32(row[0])] = 0.99;
                                await Task.WhenAll(networkList[0].neural.Train(inputs, targets));
                            }
                            else
                            {
                                int index = networkList.FindIndex(x => x.Name == row[0]);
                                targets[i % Outputnodes] = 0.99;
                                await Task.WhenAll(networkList[index].neural.Train(inputs, targets));
                            }

                            if (Skip == false)
                            {
                                do
                                {
                                    await Task.Delay(10);
                                }
                                while (OnPause);
                                networkList[j].neural.OutputMatrix -= OutputMatrix;
                            }
                        }

                        WorkProgress.Value++;
                    }
                }

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void StartTraining_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (itemList.Count > 0)
                {
                    Look_Matrix look_Matrix = null;
                    bool OnPause = false; bool Skip = false;
                    void OutputMatrix(string name, double[,] matrix, string description)
                    {
                        look_Matrix.InputMatrix(name, matrix, description);
                    };

                    WorkProgress.Maximum = itemList.Count;
                    WorkProgress.Value = 0;

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        int j;
                        if (networkList[0].Name == "Network")
                        {
                            j = 0;
                        }
                        else
                        {
                            j = networkList.FindIndex(x => x.Name == itemList[i].Name);
                        }

                        if (look_Matrix == null && Skip == false)
                        {
                            look_Matrix = new Look_Matrix();
                            _ = PopupGrid.Children.Add(look_Matrix);
                            look_Matrix.CloseClicked += () =>
                            {
                                look_Matrix = null;
                                OnPause = false;
                                PopupGrid.Children.Clear();
                            };
                            look_Matrix.NextClicked += () =>
                            {
                                OnPause = false;
                            };
                            look_Matrix.SkipClicked += () =>
                            {
                                Skip = true;
                                OnPause = false;
                                PopupGrid.Children.Clear();
                            };
                        }
                        if (Skip == false)
                        {
                            OnPause = true;
                            look_Matrix.SetNeuralName(networkList[i].Name);
                            networkList[j].neural.OutputMatrix += OutputMatrix;
                        }

                        //訓練原始圖片
                        double[] inputs = itemList[i].DoubleArray;
                        double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);

                        if(networkList[0].Name == "Network")
                        {
                            targets[Convert.ToInt32(itemList[i].Name)] = 0.99;
                            await Task.WhenAll(networkList[j].neural.Train(inputs, targets));
                        }
                        else
                        {
                            int index = networkList.FindIndex(x => x.Name == itemList[i].Name);
                            targets[i % Outputnodes] = 0.99;
                            await Task.WhenAll(networkList[j].neural.Train(inputs, targets));
                        }

                        if (Skip == false)
                        {
                            do
                            {
                                await Task.Delay(10);
                            }
                            while (OnPause);
                            networkList[j].neural.OutputMatrix -= OutputMatrix;
                        }

                        Grid grid = itemList[i].Grid;
                        ImageView.Children.Remove(grid);

                        WorkProgress.Value++;
                    }

                    itemList = new List<Items>();
                }
                else
                {
                    _ = MessageBox.Show("請輸入資料");
                }

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async Task ImageViewAddImage(string[] row)
        {
            double[] input = neuralNetwork.StringArrayToDoubleArray(row);
            string[] input_str = neuralNetwork.DoubleArrayToStringArray(input);
            double[] input_double = neuralNetwork.ArrayWeighted(input);

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
                DoubleArrayToString = neuralNetwork.DoubleArrayToStringArray(input_double),
                Grid = grid,
                Button = button
            };
            itemList.Add(items);

            await Task.Delay(1);
        }

        private Button OnSelected = null;

        private async void ImageButton_Click(object sender, RoutedEventArgs e)
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
                await CreateTrainingArray(itemList.FindIndex(x => x.Button == OnSelected));

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async Task CreateTrainingArray(int index)
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
                            Width = 50
                        };
                        stack.Children.Add(label);
                    }
                    else
                    {
                        Label label = new Label()
                        {
                            Content = row[(i * 28) + j],
                            Width = 50
                        };
                        stack.Children.Add(label);
                    }
                }

                _ = TrainingArray.Children.Add(stack);
                await Task.Delay(1);
            }
        }

        private async void DelectChoosed_Click(object sender, RoutedEventArgs e)
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
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void AllClear_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                OnSelected = null;
                itemList = new List<Items>();
                ImageView.Children.Clear();

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }


        //測試用
        private QueryShow queryShow = null;

        private List<Items> QuaryList = new List<Items>();

        private Button OnSelectQuary = null;

        private bool AutoTrain = false;

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

                    if (lines.GetLength(0) <= 1000)
                    {
                        for (int i = 0; i < lines.GetLength(0); i++)
                        {
                            string[] all_values = lines[i].Split(',');
                            await QueryImageAddImage(all_values);

                            WorkProgress.Value++;
                        }
                    }
                    else
                    {
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

                        queryShow.Reset();
                        await AutoRunQuary(lines);
                        queryShow.SetComplete();
                    }

                    if (lines.GetLength(0) > 1000)
                    {
                        await queryShow.SetResult();
                    }
                }

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async Task AutoRunQuary(string[] lines)
        {
            PopupGrid.Children.Clear();
            Look_Matrix look_Matrix = null;
            bool OnPause = false; bool Skip = false;
            void OutputMatrix(string name, double[,] matrix, string description)
            {
                look_Matrix.InputMatrix(name, matrix, description);
            };

            for (int i = 0; i < lines.GetLength(0); i++) //每行字跑一遍
            {
                string[] row = lines[i].Split(',');
                double[] output = new double[networkList.Count];
                string[] names = new string[networkList.Count];

                for (int j = 0; j < networkList.Count; j++) //每個
                {
                    if (look_Matrix == null && Skip == false)
                    {
                        look_Matrix = new Look_Matrix();
                        _ = PopupGrid.Children.Add(look_Matrix);
                        look_Matrix.CloseClicked += () =>
                        {
                            look_Matrix = null;
                            OnPause = false;
                            PopupGrid.Children.Clear();
                        };
                        look_Matrix.NextClicked += () =>
                        {
                            OnPause = false;
                        };
                        look_Matrix.SkipClicked += () =>
                        {
                            Skip = true;
                            OnPause = false;
                            PopupGrid.Children.Clear();
                        };
                    }
                    if (Skip == false)
                    {
                        OnPause = true;
                        look_Matrix.SetNeuralName(networkList[j].Name);
                        networkList[j].neural.OutputMatrix += OutputMatrix;
                    }

                    double[] inputs = neuralNetwork.StringArrayToDoubleArray(row);
                    double[,] outputs = await networkList[j].neural.Query(inputs);

                    if (networkList[0].Name == "Network")
                    {
                        names = new string[10];
                        output = new double[10];

                        for (int n = 0; n < 10; n++)
                        {
                            names[n] = n.ToString();
                            output[n] = outputs[n, 0];
                        }
                    }
                    else
                    {
                        int index = networkList.FindIndex(x => x.Name == networkList[j].Name);
                        output[j] = outputs[index, 0];
                        names[j] = networkList[j].Name;
                    }

                    if (Skip == false)
                    {
                        do
                        {
                            await Task.Delay(10);
                        }
                        while (OnPause);
                        networkList[j].neural.OutputMatrix -= OutputMatrix;
                    }
                }

                List<double> list = new List<double>();
                for (int n = 0; n < output.GetLength(0); n++)
                {
                    list.Add(output[n]);
                }

                string answer = names[output.ToList().IndexOf(output.Max())];
                bool result = queryShow.SetQueryAnswer(answer, row[0], output, names);

                if (AutoTrain && !result)
                {
                    double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);
                    double[] inputs = neuralNetwork.StringArrayToDoubleArray(row);
                    targets[Convert.ToInt32(row[0])] = 0.99;

                    int index = networkList.FindIndex(x => x.Name == row[0]);
                    await networkList[index].neural.Train(inputs, targets);
                }

                WorkProgress.Value++;
            }

        }

        private async Task QueryImageAddImage(string[] row)
        {
            double[] input = neuralNetwork.StringArrayToDoubleArray(row);
            string[] input_str = neuralNetwork.DoubleArrayToStringArray(input);
            double[] input_double = neuralNetwork.ArrayWeighted(input);

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
                DoubleArrayToString = neuralNetwork.DoubleArrayToStringArray(input_double),
                Grid = grid,
                Button = button
            };
            QuaryList.Add(items);

            await Task.Delay(1);
        }

        private async void QueryImage_Click(object sender, RoutedEventArgs e)
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
                await CreateQuaryArray(QuaryList.FindIndex(x => x.Button == OnSelectQuary));

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async Task CreateQuaryArray(int index)
        {
            if (QuaryList.Count > 0)
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
                                Width = 50
                            };
                            stack.Children.Add(label);
                        }
                        else
                        {
                            Label label = new Label()
                            {
                                Content = row[(i * 28) + j],
                                Width = 50
                            };
                            stack.Children.Add(label);
                        }
                    }

                    _ = ImageArray.Children.Add(stack);
                    await Task.Delay(1);
                }
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

                    queryShow.Reset();
                    await RunQuary();
                    queryShow.SetComplete();

                    QuaryList = new List<Items>();
                    OnWork = false;
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
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async Task RunQuary()
        {
            PopupGrid.Children.Clear();
            Look_Matrix look_Matrix = null;
            bool OnPause = false; bool Skip = false;
            void OutputMatrix(string name, double[,] matrix, string description)
            {
                look_Matrix.InputMatrix(name, matrix, description);
            };

            for (int i = 0; i < QuaryList.Count; i++) //所有圖片跑一遍
            {
                double[] inputs = QuaryList[i].DoubleArray;
                double[] output = new double[networkList.Count];
                string[] names = new string[networkList.Count];

                for (int j = 0; j < networkList.Count; j++) //所有類神經跑一遍
                {
                    if (look_Matrix == null && Skip == false)
                    {
                        look_Matrix = new Look_Matrix();
                        _ = PopupGrid.Children.Add(look_Matrix);
                        look_Matrix.CloseClicked += () =>
                        {
                            look_Matrix = null;
                            OnPause = false;
                            PopupGrid.Children.Clear();
                        };
                        look_Matrix.NextClicked += () =>
                        {
                            OnPause = false;
                        };
                        look_Matrix.SkipClicked += () =>
                        {
                            Skip = true;
                            OnPause = false;
                            PopupGrid.Children.Clear();
                        };
                    }
                    if (Skip == false)
                    {
                        OnPause = true;
                        look_Matrix.SetNeuralName(networkList[j].Name);
                        networkList[j].neural.OutputMatrix += OutputMatrix;
                    }

                    double[,] outputs = await networkList[j].neural.Query(inputs);

                    if (networkList[0].Name == "Network")
                    {
                        names = new string[10];
                        output = new double[10];

                        for (int n = 0; n < 10; n++)
                        {
                            names[n] = n.ToString();
                            output[n] = outputs[n, 0];
                        }
                    }
                    else
                    {
                        int index = networkList.FindIndex(x => x.Name == networkList[j].Name);
                        names[j] = networkList[j].Name;
                        output[j] = outputs[index, 0];
                    }

                    if (Skip == false)
                    {
                        do
                        {
                            await Task.Delay(10);
                        }
                        while (OnPause);
                        networkList[j].neural.OutputMatrix -= OutputMatrix;
                    }
                }

                string answer = names[output.ToList().IndexOf(output.Max())];
                bool result = queryShow.SetQueryAnswer(answer, QuaryList[i].Name, output, names);

                if (AutoTrain && !result)
                {
                    double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);
                    targets[Convert.ToInt32(itemList[i].Name)] = 0.99;

                    int index = networkList.FindIndex(x => x.Name == QuaryList[i].Name);
                    await networkList[index].neural.Train(inputs, targets);
                }

                //double[,] outputs = await networkList[index].neural.Query(inputs);
                QueryImage.Children.Remove(QuaryList[i].Grid);
            }

            await queryShow.SetResult();
        }

        //Events
        private void QueryShow_QueryRight()
        {
            QueryError.Content = "Correct!";
            queryShow.AddValue();
            queryShow.AddMax();
        }

        private void QueryShow_QueryWrong()
        {
            QueryError.Content = "Wrong!";
            queryShow.AddMax();
        }


        //其他功能
        private async void DoubleArrayCheck_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (OnSelectQuary != null)
                {
                    await CreateQuaryArray(QuaryList.FindIndex(x => x.Button == OnSelectQuary));
                }

                if (OnSelected != null)
                {
                    await CreateTrainingArray(itemList.FindIndex(x => x.Button == OnSelected));
                }

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void AutoTrainCheck_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                AutoTrain = (bool)AutoTrainCheck.IsChecked;

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void Backquery_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                PopupGrid.Children.Clear();
                Look_BackQuery look_ = new Look_BackQuery(Outputnodes);
                look_.CloseClicked += () => { PopupGrid.Children.Clear(); };

                for (int i = 0; i < networkList.Count; i++)
                {
                    look_.SetnetWorkList(networkList[i].Name, networkList[i].neural);
                }

                _ = PopupGrid.Children.Add(look_);

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void WHO_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                PopupGrid.Children.Clear();
                Look_WHO look_ = new Look_WHO();
                look_.CloseClicked += () => { PopupGrid.Children.Clear(); };

                for (int i = 0; i < networkList.Count; i++)
                {
                    look_.SetnetWorkList(networkList[i].Name, networkList[i].neural);
                }

                _ = PopupGrid.Children.Add(look_);

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void WIH_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                PopupGrid.Children.Clear();
                Look_WIH look_ = new Look_WIH();
                look_.CloseClicked += () => { PopupGrid.Children.Clear(); };

                for (int i = 0; i < networkList.Count; i++)
                {
                    look_.SetnetWorkList(networkList[i].Name, networkList[i].neural);
                }

                _ = PopupGrid.Children.Add(look_);

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
        }

        private async void SL_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                PopupGrid.Children.Clear();
                SaveAndLoad saveAndLoad = new SaveAndLoad();
                saveAndLoad.CloseClicked += () => { PopupGrid.Children.Clear(); };
                saveAndLoad.LoadClicked += () => { networkList = new List<Network>(); };
                saveAndLoad.DataLoading += (name, inputnodes, hiddennodes, outputnodes, learningrate, wih, who) =>
                {
                    Network network = new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = name, };
                    network.neural.wih = wih;
                    network.neural.who = who;
                    networkList.Add(network);
                };

                for (int i = 0; i < networkList.Count; i++)
                {
                    saveAndLoad.SetData(networkList[i].Name, Inputnodes, Hiddennodes, Outputnodes, Learningrate, networkList[i].neural.wih, networkList[i].neural.who);
                }

                _ = PopupGrid.Children.Add(saveAndLoad);

                OnWork = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AppStatus.Margin = new Thickness(2, 0, 0, 0);
                    await Task.Delay(50);
                    AppStatus.Margin = new Thickness(0, 0, 0, 0);
                    await Task.Delay(50);
                }
            }
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

        private class Network
        {
            public Network(int inputnodes, int hiddennodes, int outputnodes, double learningrate)
            {
                neural = new neuralNetwork(inputnodes, hiddennodes, outputnodes, learningrate);
            }

            public string Name { get; set; }

            public neuralNetwork neural { get; set; }
        }
    }
}
