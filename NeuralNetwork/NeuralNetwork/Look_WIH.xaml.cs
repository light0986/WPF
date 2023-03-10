using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeuralNetwork
{
    /// <summary>
    /// Look_WIH.xaml 的互動邏輯
    /// </summary>
    public partial class Look_WIH : UserControl
    {
        public delegate void Event();
        public event Event CloseClicked;

        private List<Network> networkList = new List<Network>();
        private bool OnWork = true;

        public Look_WIH()
        {
            InitializeComponent();
            Loaded += Look_WIH_Loaded;
        }

        private async void Look_WIH_Loaded(object sender, RoutedEventArgs e)
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

                    for (int i = 0; i < networkList[NeuralList.SelectedIndex].neural.wih.GetLength(0); i++)
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
                    WorkProgress.Maximum = networkList[index].neural.wih.GetLength(1);
                    WorkProgress.Value = 0;

                    int i = 0;
                    do
                    {
                        if (i < networkList[index].neural.wih.GetLength(1))
                        {
                            StackPanel stack = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal
                            };

                            for (int j = 0; j < 28; j++)
                            {
                                if (i < networkList[index].neural.wih.GetLength(1))
                                {
                                    Label label = new Label()
                                    {
                                        Width = 50,
                                        Content = networkList[index].neural.wih[row, i]
                                    };

                                    if (networkList[index].neural.wih[row, i] > 0)
                                    {
                                        SolidColorBrush solidColor = new SolidColorBrush
                                        {
                                            Color = Color.FromRgb(0, 255, 0),
                                            Opacity = networkList[index].neural.wih[row, i]
                                        };
                                        label.Background = solidColor;
                                    }
                                    else
                                    {
                                        SolidColorBrush solidColor = new SolidColorBrush
                                        {
                                            Color = Color.FromRgb(255, 0, 0),
                                            Opacity = networkList[index].neural.wih[row, i]
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

        private async void Activation_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (MatrixView.SelectedIndex != -1 && NeuralList.SelectedIndex != -1)
                {
                    RowArray.Children.Clear();
                    MatrixComputations MC = new MatrixComputations();
                    int index = NeuralList.SelectedIndex;
                    int row = MatrixView.SelectedIndex;
                    double[,] newWIH = MC.Activation_Function(networkList[index].neural.wih);
                    WorkProgress.Maximum = newWIH.GetLength(1);
                    WorkProgress.Value = 0;

                    int i = 0;
                    do
                    {
                        if (i < newWIH.GetLength(1))
                        {
                            StackPanel stack = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal
                            };

                            for (int j = 0; j < 28; j++)
                            {
                                if (i < newWIH.GetLength(1))
                                {
                                    Label label = new Label()
                                    {
                                        Width = 50,
                                        Content = newWIH[row, i]
                                    };

                                    if (newWIH[row, i] > 0)
                                    {
                                        SolidColorBrush solidColor = new SolidColorBrush
                                        {
                                            Color = Color.FromRgb(0, 0, 0),
                                            Opacity = newWIH[row, i]
                                        };
                                        label.Background = solidColor;
                                    }
                                    else
                                    {
                                        SolidColorBrush solidColor = new SolidColorBrush
                                        {
                                            Color = Color.FromRgb(0, 0, 0),
                                            Opacity = newWIH[row, i]
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

        private class Network
        {
            public string Name { get; set; }

            public neuralNetwork neural { get; set; }
        }
    }
}
