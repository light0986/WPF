using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeuralNetwork
{
    /// <summary>
    /// Look_BackQuery.xaml 的互動邏輯
    /// </summary>
    public partial class Look_BackQuery : UserControl
    {
        public delegate void Event();
        public event Event CloseClicked;

        private List<Network> networkList = new List<Network>();
        private bool OnWork = true;
        private int Outputnodes, Columns;

        public Look_BackQuery(int output_nodes, int col)
        {
            InitializeComponent();
            Outputnodes = output_nodes;
            Columns = col;
            Loaded += Look_BackQuery_Loaded;
        }

        private void Look_BackQuery_Loaded(object sender, RoutedEventArgs e)
        {
            OnWork = true;

            for (int i = 0; i < networkList.Count; i++)
            {
                _ = NeuralList.Items.Add(networkList[i].Name);
            }

            OnWork = false;
        }

        private void NeuralList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (NeuralList.SelectedIndex != -1)
                {
                    ImageView.Children.Clear();
                    AnswerView.Items.Clear();

                    if (networkList[0].Name == "Network")
                    {
                        for (int i = 0; i < Outputnodes; i++)
                        {
                            _ = AnswerView.Items.Add("Answer: " + networkList[0].neural.CheckIndexName(i) + " = 0.99");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < networkList.Count; i++)
                        {
                            _ = AnswerView.Items.Add("Answer: " + networkList[i].Name + " = 0.99");
                        }
                    }
                }

                OnWork = false;
            }
        }

        private async void AnswerView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (NeuralList.SelectedIndex != -1 && AnswerView.SelectedIndex != -1)
                {
                    ImageView.Children.Clear();
                    int index = NeuralList.SelectedIndex;
                    int ans = AnswerView.SelectedIndex;

                    //targets = numpy.zeros(output_nodes) + 0.01
                    //targets[label] = 0.99
                    //image_data = n.backquery(targets)
                    double[] targets = neuralNetwork.Purely_Array(0.01, Outputnodes);
                    targets[ans] = 0.99;
                    double[,] image_data = networkList[index].neural.Backquery(targets);

                    WorkProgress.Value = 0;
                    WorkProgress.Maximum = image_data.GetLength(0);

                    int i = 0;
                    do
                    {
                        if (i < image_data.GetLength(0))
                        {
                            StackPanel stack = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal
                            };

                            for (int j = 0; j < Columns; j++)
                            {
                                if (i < image_data.GetLength(0))
                                {
                                    Label label = new Label()
                                    {
                                        Width = 50,
                                        Height = 50,
                                        Content = image_data[i, 0]
                                    };
                                    SolidColorBrush solidColor = new SolidColorBrush
                                    {
                                        Color = Color.FromRgb(255, 255, 255),
                                        Opacity = image_data[i, 0]
                                    };
                                    label.Background = solidColor;

                                    _ = stack.Children.Add(label);
                                    i++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            WorkProgress.Value = i;
                            _ = ImageView.Children.Add(stack);
                        }
                        else
                        {
                            break;
                        }

                        await Task.Delay(16);
                    }
                    while (true);
                }

                OnWork = false;
            }
        }

        public void SetnetWorkList(string Name, neuralNetwork neural)
        {
            networkList.Add(new Network() { Name = Name, neural = neural });
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke();
            Visibility = Visibility.Hidden;
        }

        private class Network
        {
            public string Name { get; set; }

            public neuralNetwork neural { get; set; }
        }
    }
}
