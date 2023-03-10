using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeuralNetwork
{
    /// <summary>
    /// Look_WHO.xaml 的互動邏輯
    /// </summary>
    public partial class Look_WHO : UserControl
    {
        public delegate void Event();
        public event Event CloseClicked;

        private List<Network> networkList = new List<Network>();
        private bool OnWork = true;

        public Look_WHO()
        {
            InitializeComponent();
            Loaded += Look_WHO_Loaded;
        }

        private async void Look_WHO_Loaded(object sender, RoutedEventArgs e)
        {
            WorkProgress.Maximum = networkList.Count;
            WorkProgress.Value = 0;

            for (int i = 0; i < networkList.Count; i++)
            {
                _ = NeuralList.Items.Add(networkList[i].Name);
                WorkProgress.Value++;
                await Task.Delay(1);
            }

            OnWork = false;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;
                RowArray.Children.Clear();

                if (NeuralList.SelectedIndex != -1)
                {
                    MatrixView.Items.Clear();
                    WorkProgress.Maximum = networkList.Count;
                    WorkProgress.Value = 0;

                    for (int i = 0; i < networkList[NeuralList.SelectedIndex].neural.who.GetLength(0); i++)
                    {
                        _ = MatrixView.Items.Add(i);
                        WorkProgress.Value++;
                    }
                }

                OnWork = false;
            }
        }

        private async void MatrixView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;
                RowArray.Children.Clear();

                if (MatrixView.SelectedIndex != -1)
                {
                    int index = NeuralList.SelectedIndex;
                    int row = MatrixView.SelectedIndex;
                    WorkProgress.Maximum = networkList[index].neural.who.GetLength(1);
                    WorkProgress.Value = 0;

                    int sqr = (int)Math.Sqrt(networkList[index].neural.who.GetLength(1));

                    int i = 0;
                    do
                    {
                        if (i < networkList[index].neural.who.GetLength(1))
                        {
                            StackPanel stack = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal
                            };

                            for (int j = 0; j < sqr; j++)
                            {
                                if (i < networkList[index].neural.who.GetLength(1))
                                {
                                    Label label = new Label()
                                    {
                                        Width = 50,
                                        Content = networkList[index].neural.who[row, i]
                                    };

                                    if (networkList[index].neural.who[row, i] > 0)
                                    {
                                        SolidColorBrush solidColor = new SolidColorBrush
                                        {
                                            Color = Color.FromRgb(255, 0, 0),
                                            Opacity = networkList[index].neural.who[row, i]
                                        };
                                        label.Background = solidColor;
                                    }
                                    else
                                    {
                                        SolidColorBrush solidColor = new SolidColorBrush
                                        {
                                            Color = Color.FromRgb(0, 255, 0),
                                            Opacity = networkList[index].neural.who[row, i] * -1
                                        };
                                        label.Background = solidColor;
                                    }

                                    _ = stack.Children.Add(label);
                                    i++;
                                }
                                else
                                {
                                    break;
                                }

                            }

                            WorkProgress.Value = i;
                            _ = RowArray.Children.Add(stack);
                        }
                        else
                        {
                            break;
                        }

                        await Task.Delay(1);
                    }
                    while (true);
                }

                OnWork = false;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke();
            Visibility = Visibility.Hidden;
        }

        public void SetnetWorkList(string Name, neuralNetwork neural)
        {
            networkList.Add(new Network() { Name = Name, neural = neural });
        }

        private class Network
        {
            public string Name { get; set; }

            public neuralNetwork neural { get; set; }
        }
    }
}
