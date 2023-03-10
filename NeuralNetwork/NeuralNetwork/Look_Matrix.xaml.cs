using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeuralNetwork
{
    /// <summary>
    /// Look_Matrix.xaml 的互動邏輯
    /// </summary>
    public partial class Look_Matrix : UserControl
    {
        public delegate void Event();
        public event Event CloseClicked;
        public event Event NextClicked;
        public event Event SkipClicked;

        private List<Data> dataList = new List<Data>();
        private bool OnWork = false;

        public Look_Matrix()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (listView.SelectedIndex != -1)
                {
                    double[,] output = dataList[listView.SelectedIndex].OutputMatrix;
                    Description.Content = dataList[listView.SelectedIndex].Description;

                    FirstColumn.Items.Clear();
                    RowArray.Children.Clear();

                    WorkProgress.Maximum = output.GetLength(0);
                    WorkProgress.Value = 0;

                    for (int i = 0; i < output.GetLength(0); i++)
                    {
                        _ = FirstColumn.Items.Add(output[i, 0]);
                    }
                }

                OnWork = false;
            }
        }

        private async void FirstColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                if (listView.SelectedIndex != -1 && FirstColumn.SelectedIndex != -1)
                {
                    double[,] output = dataList[listView.SelectedIndex].OutputMatrix;
                    int col = FirstColumn.SelectedIndex;
                    int width = (int)Math.Sqrt(output.GetLength(1));

                    RowArray.Children.Clear();

                    WorkProgress.Maximum = output.GetLength(1);
                    WorkProgress.Value = 0;

                    for (int i = 0; i < output.GetLength(1); i++)
                    {
                        StackPanel stack = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };

                        for (int j = 0; j < width; j++)
                        {
                            if (i < output.GetLength(1))
                            {
                                Label label = new Label()
                                {
                                    Height = 40,
                                    Width = 50,
                                    Content = output[col, i]
                                };

                                if (output[col, i] < 0)
                                {
                                    SolidColorBrush solidColor = new SolidColorBrush
                                    {
                                        Color = Color.FromRgb(255, 0, 0),
                                        Opacity = output[col, i] * -1
                                    };
                                    label.Background = solidColor;
                                }
                                else
                                {
                                    SolidColorBrush solidColor = new SolidColorBrush
                                    {
                                        Color = Color.FromRgb(0, 255, 0),
                                        Opacity = output[col, i]
                                    };
                                    label.Background = solidColor;
                                }

                                _ = stack.Children.Add(label);
                                WorkProgress.Value++;

                                if (j < width - 1)
                                {
                                    i++;
                                }
                            }
                        }

                        _ = RowArray.Children.Add(stack);
                        await Task.Delay(1);
                    }
                }

                OnWork = false;
            }
        }

        public void InputMatrix(string name, double[,] output, string description)
        {
            dataList.Add(new Data() { Name = name, OutputMatrix = output, Description = description });
            _ = listView.Items.Add(name);
        }

        public void SetNeuralName(string name)
        {
            NeuralName.Content = name;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                Visibility = Visibility.Hidden;
                CloseClicked?.Invoke();

                OnWork = false;
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                dataList = new List<Data>();
                listView.Items.Clear();
                RowArray.Children.Clear();
                NextClicked?.Invoke();

                OnWork = false;
            }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                Visibility = Visibility.Hidden;
                SkipClicked?.Invoke();

                OnWork = false;
            }
        }

        private class Data
        {
            public string Name { get; set; }

            public double[,] OutputMatrix { get; set; }

            public string Description { get; set; }
        }
    }
}
