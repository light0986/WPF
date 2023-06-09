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
using System.Windows.Threading;

namespace NeuralNetwork
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// 
    /// 此程式為練習用，設計上並未符合商業化需求。
    /// 其目的為求了解過程，而非為求效率與速度。
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 變數
        public int Inputnodes { get; set; } = 784;
        public int Hiddennodes { get; set; } = 100;
        public int Outputnodes { get; set; } = 10; // = 10;
        public double Learningrate { get; set; } = 0.1;
        private int Rows { get; set; } = 28;
        private int Columns { get; set; } = 28;
        private int Type { get; set; }

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
        private List<Items> itemList = new List<Items>();
        #endregion

        #region 主程式開關
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OnWork = true;
            StartSetting startSetting = new StartSetting();
            startSetting.OKClicked += (inputnodes, hiddennodes, outputnodes, learningrate, onenetwork, row, col) =>
            {
                Inputnodes = inputnodes;
                Hiddennodes = hiddennodes;
                Outputnodes = outputnodes;
                Learningrate = learningrate;
                Rows = row;
                Columns = col;

                if (onenetwork)
                {
                    networkList.Add(new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = "Network" });
                }
                else
                {
                    for (int i = 0; i < Outputnodes; i++)
                    {
                        networkList.Add(new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = "" });
                    }
                }
                PopupGrid.Children.Clear();
                OnWork = false;
            };
            _ = PopupGrid.Children.Add(startSetting);
            Type = 1;
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
        #endregion

        #region 常用物件
        private Look_Matrix look_Matrix;
        private DispatcherTimer timer;
        private CSV cSV;
        #endregion

        #region 訓練用元件
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
                    Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;

                    look_Matrix = null; timer = null;
                    bool OnPause = false; bool Skip = WillShowProcess;
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
                        StartTraining.Content = "停止訓練";

                        if (Skip == true)
                        {
                            int Count = 0;
                            void dispatcherTimer_Tick(object obj, EventArgs args)
                            {
                                if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                                {
                                    int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                                    int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                                    EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                                }
                                Count = (int)WorkProgress.Value;
                            }
                            timer = new DispatcherTimer
                            {
                                Interval = TimeSpan.FromSeconds(1),
                            };
                            timer.Tick += new EventHandler(dispatcherTimer_Tick);
                            timer.Start();
                        }
                    }

                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        if (lines.GetLength(0) <= 1000)
                        {
                            await Task.Run(() =>
                            {
                                string[] row = lines[i].Split(','); //列，這裡有785個字節
                                Dispatcher.Invoke(() => { ImageViewAddImage(row); });
                            });
                        }
                        else
                        {
                            if (look_Matrix == null && Skip == false)
                            {
                                look_Matrix = new Look_Matrix(Inputnodes, Columns);
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

                                    int Count = 0;
                                    void dispatcherTimer_Tick(object obj, EventArgs args)
                                    {
                                        if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                                        {
                                            int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                                            int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                                            EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                                        }
                                        Count = (int)WorkProgress.Value;
                                    }
                                    timer = new DispatcherTimer
                                    {
                                        Interval = TimeSpan.FromSeconds(1),
                                    };
                                    timer.Tick += new EventHandler(dispatcherTimer_Tick);
                                    timer.Start();
                                };
                            }
                            int j = -1;

                            await Task.Run(() =>
                            {                            //訓練原始圖片
                                string[] row = lines[i].Split(','); //列，這裡有785個字節
                                double[] targets_double = neuralNetwork.StringArrayToDoubleArray(row);
                                double[] inputs = neuralNetwork.ArrayWeighted(targets_double);
                                double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);

                                if (networkList[0].Name == "Network")
                                {
                                    j = networkList[0].neural.CheckNameIndex(row[0]);
                                    if (j == -1)
                                    {
                                        for (int c = 0; c < Inputnodes; c++)
                                        {
                                            if (networkList[0].neural.CheckIndexName(c) == "")
                                            {
                                                networkList[0].neural.SetIndexName(c, row[0]);
                                                j = c;
                                                break;
                                            }
                                        }
                                    }

                                    if (j != -1)
                                    {
                                        if (Skip == false)
                                        {
                                            OnPause = true;
                                            look_Matrix.SetNeuralName(networkList[0].Name);
                                            networkList[0].neural.OutputMatrix += OutputMatrix;
                                        }

                                        targets[j] = 0.99;
                                        networkList[0].neural.Train(inputs, targets);
                                    }
                                }
                                else
                                {
                                    j = networkList.FindIndex(x => x.Name == row[0]);
                                    if (j == -1)
                                    {
                                        for (int c = 0; c < networkList.Count; c++)
                                        {
                                            if (networkList[c].Name == "")
                                            {
                                                networkList[c].Name = row[0];
                                                j = c;
                                                break;
                                            }
                                        }
                                    }

                                    if (j != -1)
                                    {
                                        if (Skip == false)
                                        {
                                            OnPause = true;
                                            look_Matrix.SetNeuralName(networkList[i].Name);
                                            networkList[j].neural.OutputMatrix += OutputMatrix;
                                        }

                                        targets[j] = 0.99;
                                        networkList[j].neural.Train(inputs, targets);
                                    }
                                }
                            });

                            if (Skip == false)
                            {
                                do
                                {
                                    await Task.Delay(16);
                                }
                                while (OnPause);
                                networkList[j].neural.OutputMatrix -= OutputMatrix;
                            }
                            if (OnWork == false) { break; }
                        }

                        WorkProgress.Value++;
                        Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;
                        await Task.Delay(1);
                    }

                    if (timer != null)
                    {
                        timer.Stop();
                        timer = null;
                    }
                    if (lines.GetLength(0) > 1000 && OnWork)
                    {
                        EstimatedTime.Content = "";
                        StartTraining.Content = "開始訓練";
                        _ = MessageBox.Show("完成");
                    }
                }

                OnWork = false;
            }
            else
            {
                if ((string)StartTraining.Content == "停止訓練")
                {
                    if (MessageBox.Show("停止訓練", "注意:", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        OnWork = false;
                        StartTraining.Content = "開始訓練";
                        ImageView.Children.Clear();
                        _ = MessageBox.Show("已停止");
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
        }

        private async void StartTraining_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (itemList.Count > 0)
                {
                    StartTraining.Content = "停止訓練";
                    look_Matrix = null; timer = null;
                    bool OnPause = false; bool Skip = WillShowProcess;
                    void OutputMatrix(string name, double[,] matrix, string description)
                    {
                        look_Matrix.InputMatrix(name, matrix, description);
                    };

                    WorkProgress.Maximum = itemList.Count;
                    WorkProgress.Value = 0;
                    Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;

                    if (Skip == true)
                    {
                        int Count = 0;
                        void dispatcherTimer_Tick(object obj, EventArgs args)
                        {
                            if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                            {
                                int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                                int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                                EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                            }
                            Count = (int)WorkProgress.Value;
                        }
                        timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1),
                        };
                        timer.Tick += new EventHandler(dispatcherTimer_Tick);
                        timer.Start();
                    }

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        if (look_Matrix == null && Skip == false)
                        {
                            look_Matrix = new Look_Matrix(Inputnodes, Columns);
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

                                int Count = 0;
                                void dispatcherTimer_Tick(object obj, EventArgs args)
                                {
                                    if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                                    {
                                        int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                                        int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                                        EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                                    }
                                    Count = (int)WorkProgress.Value;
                                }
                                timer = new DispatcherTimer
                                {
                                    Interval = TimeSpan.FromSeconds(1),
                                };
                                timer.Tick += new EventHandler(dispatcherTimer_Tick);
                                timer.Start();
                            };
                        }

                        int j = -1;

                        //訓練原始圖片
                        await Task.Run(() =>
                        {
                            double[] inputs = itemList[i].DoubleArray;
                            double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);

                            if (networkList[0].Name == "Network")
                            {
                                j = networkList[0].neural.CheckNameIndex(itemList[i].Name);
                                if (j == -1)
                                {
                                    for (int c = 0; c < Inputnodes; c++)
                                    {
                                        if (networkList[0].neural.CheckIndexName(c) == "")
                                        {
                                            networkList[0].neural.SetIndexName(c, itemList[i].Name);
                                            j = c;
                                            break;
                                        }
                                    }
                                }

                                if (j != -1)
                                {
                                    if (Skip == false)
                                    {
                                        OnPause = true;
                                        look_Matrix.SetNeuralName(networkList[i].Name);
                                        networkList[j].neural.OutputMatrix += OutputMatrix;
                                    }

                                    targets[j] = 0.99;
                                    networkList[0].neural.Train(inputs, targets);
                                }
                            }
                            else
                            {
                                j = networkList.FindIndex(x => x.Name == itemList[i].Name);
                                if (j == -1)
                                {
                                    for (int c = 0; c < networkList.Count; c++)
                                    {
                                        if (networkList[c].Name == "")
                                        {
                                            networkList[c].Name = itemList[i].Name;
                                            j = c;
                                            break;
                                        }
                                    }
                                }

                                if (j != -1)
                                {
                                    if (Skip == false)
                                    {
                                        OnPause = true;
                                        look_Matrix.SetNeuralName(networkList[i].Name);
                                        networkList[j].neural.OutputMatrix += OutputMatrix;
                                    }

                                    targets[j] = 0.99;
                                    networkList[j].neural.Train(inputs, targets);
                                }
                            }
                        });

                        if (Skip == false && j != -1)
                        {
                            do
                            {
                                await Task.Delay(16);
                            }
                            while (OnPause);
                            networkList[j].neural.OutputMatrix -= OutputMatrix;
                        }

                        Grid grid = itemList[i].Grid;
                        ImageView.Children.Remove(grid);

                        WorkProgress.Value++;
                        Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;
                        if (OnWork == false) { break; }
                        await Task.Delay(1);
                    }

                    itemList = new List<Items>();
                    if (timer != null)
                    {
                        timer.Stop();
                        timer = null;
                    }
                    if ((string)StartTraining.Content == "停止訓練")
                    {
                        EstimatedTime.Content = "";
                        StartTraining.Content = "開始訓練";
                        _ = MessageBox.Show("完成");
                    }
                }
                else
                {
                    _ = MessageBox.Show("請輸入資料");
                }

                OnWork = false;
            }
            else
            {
                if ((string)StartTraining.Content == "停止訓練")
                {
                    if (MessageBox.Show("停止訓練", "注意:", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        OnWork = false;
                        StartTraining.Content = "開始訓練";
                        ImageView.Children.Clear();
                        _ = MessageBox.Show("已停止");
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
        }

        private void ImageViewAddImage(string[] row)
        {
            double[] input = neuralNetwork.StringArrayToDoubleArray(row);
            string[] input_str = neuralNetwork.DoubleArrayToStringArray(input);
            double[] input_double = neuralNetwork.ArrayWeighted(input);

            cSV = new CSV(Inputnodes, Rows, Columns);
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
                Grid = grid,
                Button = button
            };
            itemList.Add(items);
        }

        private Button OnSelected = null;

        private async void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (OnSelected != null)
                {
                    OnSelected.Background = System.Windows.Media.Brushes.White;
                }

                OnSelected = (Button)sender;
                OnSelected.Background = System.Windows.Media.Brushes.LightBlue;
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        CreateTrainingArray(itemList.FindIndex(x => x.Button == OnSelected));
                    });
                });

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

        private void CreateTrainingArray(int index)
        {
            string[] row = itemList[index].StringArray;
            double[] dRow = itemList[index].DoubleArray;
            TrainingArray.Children.Clear();

            for (int i = 0; i < Rows; i++)
            {
                StackPanel stack = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                for (int j = 0; j < Columns; j++)
                {
                    if ((bool)DoubleArrayCheck.IsChecked)
                    {
                        Label label = new Label()
                        {
                            Content = dRow[(i * Rows) + j],
                            Width = 50,
                            Height = 50
                        };
                        _ = stack.Children.Add(label);
                    }
                    else
                    {
                        Label label = new Label()
                        {
                            Content = row[(i * Rows) + j],
                            Width = 50,
                            Height = 50
                        };
                        _ = stack.Children.Add(label);
                    }
                }

                _ = TrainingArray.Children.Add(stack);
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
        #endregion

        #region 識別用
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
                    Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;

                    if (lines.GetLength(0) <= 1000)
                    {
                        for (int i = 0; i < lines.GetLength(0); i++)
                        {
                            await Task.Run(() =>
                            {
                                string[] all_values = lines[i].Split(',');
                                Dispatcher.Invoke(() =>
                                {
                                    QueryImageAddImage(all_values);
                                });
                            });

                            WorkProgress.Value++;
                            Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;
                            await Task.Delay(1);
                        }
                    }
                    else
                    {
                        if (queryShow == null)
                        {
                            queryShow = new QueryShow(Rows, Columns);
                            queryShow.Show();
                            queryShow.CloseClicked += () =>
                            {
                                queryShow = null;
                            };
                            queryShow.QueryRight += QueryShow_QueryRight;
                            queryShow.QueryWrong += QueryShow_QueryWrong;
                        }

                        StartQuery.Content = "停止識別";
                        queryShow.Reset();
                        await AutoRunQuary(lines);
                        queryShow.SetComplete();
                    }

                    if (lines.GetLength(0) > 1000)
                    {
                        queryShow.SetResult();
                        StartQuery.Content = "開始識別";
                    }
                }

                OnWork = false;
            }
            else
            {
                if ((string)StartQuery.Content == "停止識別")
                {
                    if (MessageBox.Show("停止識別", "注意:", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        OnWork = false;
                        StartQuery.Content = "開始識別";
                        _ = MessageBox.Show("已停止");
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
        }

        private async Task AutoRunQuary(string[] lines)
        {
            PopupGrid.Children.Clear();
            look_Matrix = null; timer = null;
            bool OnPause = false; bool Skip = WillShowProcess;
            void OutputMatrix(string name, double[,] matrix, string description)
            {
                look_Matrix.InputMatrix(name, matrix, description);
            };

            if (Skip == true)
            {
                int Count = 0;
                void dispatcherTimer_Tick(object obj, EventArgs args)
                {
                    if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                    {
                        int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                        int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                        EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                    }
                    Count = (int)WorkProgress.Value;
                }
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                timer.Tick += new EventHandler(dispatcherTimer_Tick);
                timer.Start();
            }

            for (int i = 0; i < lines.GetLength(0); i++) //每行字跑一遍
            {
                string[] row = lines[i].Split(',');
                double[] output = new double[networkList.Count];
                string[] names = new string[networkList.Count];
                double[] inputs = neuralNetwork.StringArrayToDoubleArray(row);

                for (int j = 0; j < networkList.Count; j++) //每個
                {
                    if (look_Matrix == null && Skip == false)
                    {
                        look_Matrix = new Look_Matrix(Inputnodes, Columns);
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

                            int Count = 0;
                            void dispatcherTimer_Tick(object obj, EventArgs args)
                            {
                                if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                                {
                                    int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                                    int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                                    EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                                }
                                Count = (int)WorkProgress.Value;
                            }
                            timer = new DispatcherTimer
                            {
                                Interval = TimeSpan.FromSeconds(1),
                            };
                            timer.Tick += new EventHandler(dispatcherTimer_Tick);
                            timer.Start();
                        };
                    }
                    if (Skip == false)
                    {
                        OnPause = true;
                        look_Matrix.SetNeuralName(networkList[j].Name);
                        networkList[j].neural.OutputMatrix += OutputMatrix;
                    }

                    await Task.Run(() =>
                    {
                        double[,] outputs = networkList[j].neural.Query(inputs);
                        if (networkList[0].Name == "Network")
                        {
                            names = new string[Outputnodes];
                            output = new double[Outputnodes];

                            for (int n = 0; n < Outputnodes; n++)
                            {
                                names[n] = networkList[0].neural.CheckIndexName(n);
                                output[n] = outputs[n, 0];
                            }
                        }
                        else
                        {
                            int index = networkList.FindIndex(x => x.Name == networkList[j].Name);
                            output[j] = outputs[index, 0];
                            names[j] = networkList[j].Name;
                        }
                    });

                    if (Skip == false)
                    {
                        do
                        {
                            await Task.Delay(16);
                        }
                        while (OnPause);
                        networkList[j].neural.OutputMatrix -= OutputMatrix;
                    }
                }

                string answer = names[output.ToList().IndexOf(output.Max())];
                bool result = queryShow.SetQueryAnswer(answer, row[0], output, names, inputs);

                if (AutoTrain && !result)
                {
                    await Task.Run(() =>
                    {
                        double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);
                        int j;
                        if (networkList[0].Name == "Network")
                        {
                            j = networkList[0].neural.CheckNameIndex(row[0]);
                            if (j == -1)
                            {
                                for (int c = 0; c < Inputnodes; c++)
                                {
                                    if (networkList[0].neural.CheckIndexName(c) == "")
                                    {
                                        networkList[0].neural.SetIndexName(c, row[0]);
                                        j = c;
                                        break;
                                    }
                                }
                            }

                            if (j != -1)
                            {
                                targets[j] = 0.99;
                                networkList[0].neural.Train(inputs, targets);
                            }
                        }
                        else
                        {
                            j = networkList.FindIndex(x => x.Name == row[0]);
                            if (j == -1)
                            {
                                for (int c = 0; c < networkList.Count; c++)
                                {
                                    if (networkList[c].Name == "")
                                    {
                                        networkList[c].Name = row[0];
                                        j = c;
                                        break;
                                    }
                                }
                            }

                            if (j != -1)
                            {
                                targets[j] = 0.99;
                                networkList[j].neural.Train(inputs, targets);
                            }
                        }
                    });
                }

                WorkProgress.Value++;
                Taskbar.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;
                if (OnWork == false) { break; }

                await Task.Delay(1);
            }

            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void QueryImageAddImage(string[] row)
        {
            double[] input = neuralNetwork.StringArrayToDoubleArray(row);
            string[] input_str = neuralNetwork.DoubleArrayToStringArray(input);
            double[] input_double = neuralNetwork.ArrayWeighted(input);

            cSV = new CSV(Inputnodes, Rows, Columns);
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

            button.Click += QListImage_Click; ;
            _ = grid.Children.Add(img);
            _ = grid.Children.Add(label);
            _ = grid.Children.Add(button);
            _ = QueryImage.Children.Add(grid);

            Items items = new Items()
            {
                Name = row[0],
                StringArray = input_str,
                DoubleArray = input_double,
                Grid = grid,
                Button = button
            };
            QuaryList.Add(items);
        }

        private async void QListImage_Click(object sender, RoutedEventArgs e)
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
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        CreateQuaryArray(QuaryList.FindIndex(x => x.Button == OnSelectQuary));
                    });
                });

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

        private void CreateQuaryArray(int index)
        {
            if (QuaryList.Count > 0)
            {
                string[] row = QuaryList[index].StringArray;
                double[] dRow = QuaryList[index].DoubleArray;
                ImageArray.Children.Clear();

                for (int i = 0; i < Rows; i++)
                {
                    StackPanel stack = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    for (int j = 0; j < Columns; j++)
                    {
                        if ((bool)DoubleArrayCheck.IsChecked)
                        {
                            Label label = new Label()
                            {
                                Content = dRow[(i * Rows) + j],
                                Width = 50,
                                Height = 50
                            };
                            _ = stack.Children.Add(label);
                        }
                        else
                        {
                            Label label = new Label()
                            {
                                Content = row[(i * Rows) + j],
                                Width = 50,
                                Height = 50
                            };
                            _ = stack.Children.Add(label);
                        }
                    }

                    _ = ImageArray.Children.Add(stack);
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
                    StartQuery.Content = "停止識別";
                    if (queryShow == null)
                    {
                        queryShow = new QueryShow(Rows, Columns);
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
                    StartQuery.Content = "開始識別";
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
                if ((string)StartQuery.Content == "停止識別")
                {
                    if (MessageBox.Show("停止識別", "注意:", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        OnWork = false;
                        _ = MessageBox.Show("已停止");
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
        }

        private async Task RunQuary()
        {
            PopupGrid.Children.Clear();
            look_Matrix = null; timer = null;

            bool OnPause = false; bool Skip = WillShowProcess;
            void OutputMatrix(string name, double[,] matrix, string description)
            {
                look_Matrix.InputMatrix(name, matrix, description);
            };

            if (Skip == true)
            {
                int Count = 0;
                void dispatcherTimer_Tick(object obj, EventArgs args)
                {
                    if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                    {
                        int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                        int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                        EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                    }
                    Count = (int)WorkProgress.Value;
                }
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                timer.Tick += new EventHandler(dispatcherTimer_Tick);
                timer.Start();
            }

            for (int i = 0; i < QuaryList.Count; i++) //所有圖片跑一遍
            {
                double[] inputs = QuaryList[i].DoubleArray;
                double[] output = new double[networkList.Count];
                string[] names = new string[networkList.Count];

                for (int j = 0; j < networkList.Count; j++) //所有類神經跑一遍
                {
                    if (look_Matrix == null && Skip == false)
                    {
                        look_Matrix = new Look_Matrix(Inputnodes, Columns);
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

                        int Count = 0;
                        void dispatcherTimer_Tick(object obj, EventArgs args)
                        {
                            if (Count != 0 && WorkProgress.Maximum != WorkProgress.Value)
                            {
                                int dt = (int)WorkProgress.Value - Count; //每秒鐘完成
                                int TotalTime = (int)(WorkProgress.Maximum - WorkProgress.Value) / dt; // 剩餘距離/每秒完成時間
                                EstimatedTime.Content = (TotalTime % 86400 / 3600) + "時 " + (TotalTime % 86400 % 3600 / 60) + "分 " + (TotalTime % 86400 % 3600 % 60) + "秒";
                            }
                            Count = (int)WorkProgress.Value;
                        }
                        timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1),
                        };
                        timer.Tick += new EventHandler(dispatcherTimer_Tick);
                        timer.Start();
                    }

                    await Task.Run(() =>
                    {
                        double[,] outputs = networkList[j].neural.Query(inputs);

                        if (networkList[0].Name == "Network")
                        {
                            names = new string[Outputnodes];
                            output = new double[Outputnodes];

                            for (int n = 0; n < Outputnodes; n++)
                            {
                                names[n] = networkList[0].neural.CheckIndexName(n);
                                output[n] = outputs[n, 0];
                            }
                        }
                        else
                        {
                            int index = networkList.FindIndex(x => x.Name == networkList[j].Name);
                            names[j] = networkList[j].Name;
                            output[j] = outputs[index, 0];
                        }
                    });

                    if (Skip == false)
                    {
                        do
                        {
                            await Task.Delay(16);
                        }
                        while (OnPause);
                        networkList[j].neural.OutputMatrix -= OutputMatrix;
                    }
                }

                string answer = names[output.ToList().IndexOf(output.Max())];
                bool result = queryShow.SetQueryAnswer(answer, QuaryList[i].Name, output, names, QuaryList[i].DoubleArray);

                if (AutoTrain && !result)
                {
                    double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);
                    int j;
                    if (networkList[0].Name == "Network")
                    {
                        j = networkList[0].neural.CheckNameIndex(QuaryList[i].Name);
                        if (j == -1)
                        {
                            for (int c = 0; c < Inputnodes; c++)
                            {
                                if (networkList[0].neural.CheckIndexName(c) == "")
                                {
                                    networkList[0].neural.SetIndexName(c, QuaryList[i].Name);
                                    j = c;
                                    break;
                                }
                            }
                        }

                        if (j != -1)
                        {
                            targets[j] = 0.99;
                            networkList[0].neural.Train(inputs, targets);
                        }
                    }
                    else
                    {
                        j = networkList.FindIndex(x => x.Name == QuaryList[i].Name);
                        if (j == -1)
                        {
                            for (int c = 0; c < networkList.Count; c++)
                            {
                                if (networkList[c].Name == "")
                                {
                                    networkList[c].Name = QuaryList[i].Name;
                                    j = c;
                                    break;
                                }
                            }
                        }

                        if (j != -1)
                        {
                            targets[j] = 0.99;
                            networkList[j].neural.Train(inputs, targets);
                        }
                    }
                }

                //double[,] outputs = await networkList[index].neural.Query(inputs);
                QueryImage.Children.Remove(QuaryList[i].Grid);

                if (OnWork == false) { break; }
                await Task.Delay(1);
            }

            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            queryShow.SetResult();
        }
        #endregion

        #region Events
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
        #endregion

        #region 其他功能
        private bool AutoTrain = false;
        private bool WillShowProcess = true;

        private async void DoubleArrayCheck_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (OnSelectQuary != null)
                {
                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CreateQuaryArray(QuaryList.FindIndex(x => x.Button == OnSelectQuary));
                        });
                    });
                }

                if (OnSelected != null)
                {
                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CreateTrainingArray(itemList.FindIndex(x => x.Button == OnSelected));
                        });
                    });
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

        private async void ShowProcess_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                WillShowProcess = !(bool)ShowProcess.IsChecked;

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
                Look_BackQuery look_ = new Look_BackQuery(Outputnodes, Columns);
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
                Look_WIH look_ = new Look_WIH(Columns);
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
                saveAndLoad.CloseClicked += () =>
                {
                    PopupGrid.Children.Clear();
                    OnWork = false;
                };
                saveAndLoad.LoadClicked += () => { networkList = new List<Network>(); };
                saveAndLoad.DataLoading += (name, namelist, type, inputnodes, hiddennodes, outputnodes, learningrate, row, col, wih, who) =>
                {
                    Network network = new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = name, };
                    network.neural.NameList = namelist;
                    network.neural.wih = wih;
                    network.neural.who = who;
                    Type = type;
                    Rows = row;
                    Columns = col;
                    networkList.Add(network);
                };

                for (int i = 0; i < networkList.Count; i++)
                {
                    saveAndLoad.SetData(networkList[i].Name, networkList[i].neural.NameList, Type, Inputnodes, Hiddennodes, Outputnodes, Learningrate, Rows, Columns, networkList[i].neural.wih, networkList[i].neural.who);
                }

                _ = PopupGrid.Children.Add(saveAndLoad);
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

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;
                PopupGrid.Children.Clear();
                ResetPage resetPage = new ResetPage();
                resetPage.CloseClicked += () =>
                {
                    PopupGrid.Children.Clear();
                    OnWork = false;
                    _ = MessageBox.Show("完成");
                };
                resetPage.TypeClicked += (type, one) =>
                {
                    Type = type;
                    networkList = new List<Network>();
                    if (one)
                    {
                        networkList.Add(new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = "Network" });
                        networkList[0].neural.Reset(type);
                    }
                    else
                    {
                        for (int i = 0; i < Outputnodes; i++)
                        {
                            networkList.Add(new Network(Inputnodes, Hiddennodes, Outputnodes, Learningrate) { Name = "" });
                            networkList[i].neural.Reset(type);
                        }
                    }
                    PopupGrid.Children.Clear();
                    OnWork = false;
                    _ = MessageBox.Show("完成");
                };

                _ = PopupGrid.Children.Add(resetPage);
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
        #endregion

        private class Items
        {
            public string Name { get; set; } //答案

            public Grid Grid { get; set; }

            public Button Button { get; set; }

            public double[] DoubleArray { get; set; } //

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
