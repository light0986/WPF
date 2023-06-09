using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeuralNetwork
{
    /// <summary>
    /// LogView.xaml 的互動邏輯
    /// </summary>
    public partial class LogView : Window
    {
        private static List<ThisData> datas = new List<ThisData>();
        private List<ThisData> these = new List<ThisData>();

        private bool OnWork = false;
        private static bool Open = false;

        public LogView()
        {
            InitializeComponent();
            Loaded += LogView_Loaded;
        }

        private async void LogView_Loaded(object sender, RoutedEventArgs e)
        {
            Open = true;
            do
            {
                if (datas.Count != 0)
                {
                    Button button = new Button()
                    {
                        Content = datas[0].Name,
                        Tag = these.Count
                    };
                    button.Click += Button_Click;
                    _ = StepList.Children.Add(button);

                    datas[0].Tag = these.Count;
                    these.Add(datas[0]);
                    datas.RemoveAt(0);
                    await Task.Delay(1);
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
            while (Open);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                Button button = (Button)sender;
                int tag = (int)button.Tag;
                int index = these.FindIndex(x => x.Tag == tag);
                FirstMatrix.Children.Clear();
                SecondMatrix.Children.Clear();
                ThirdMatrix.Children.Clear();

                if (these[index].Matrix1 != null)
                {
                    for (int i = 0; i < these[index].Matrix1.GetLength(0); i++)
                    {
                        StackPanel stack = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };
                        for (int j = 0; j < these[index].Matrix1.GetLength(1); j++)
                        {
                            Label label = new Label()
                            {
                                Content = these[index].Matrix1[i, j],
                                Width = 50,
                                Height = 50
                            };
                            _ = stack.Children.Add(label);
                        }
                        _ = FirstMatrix.Children.Add(stack);
                        await Task.Delay(1);
                    }
                }

                if (these[index].Matrix2 != null)
                {
                    for (int i = 0; i < these[index].Matrix2.GetLength(0); i++)
                    {
                        StackPanel stack = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };
                        for (int j = 0; j < these[index].Matrix2.GetLength(1); j++)
                        {
                            Label label = new Label()
                            {
                                Content = these[index].Matrix2[i, j]
                            };
                            _ = stack.Children.Add(label);
                        }
                        _ = SecondMatrix.Children.Add(stack);
                        await Task.Delay(1);
                    }
                }

                if (these[index].Matrix3 != null)
                {
                    for (int i = 0; i < these[index].Matrix3.GetLength(0); i++)
                    {
                        StackPanel stack = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };
                        for (int j = 0; j < these[index].Matrix3.GetLength(1); j++)
                        {
                            Label label = new Label()
                            {
                                Content = these[index].Matrix3[i, j]
                            };
                            _ = stack.Children.Add(label);
                        }
                        _ = ThirdMatrix.Children.Add(stack);
                        await Task.Delay(1);
                    }
                }

                OnWork = false;
            }
        }

        public static void SetValue(string title, double[,] matrix1, double[,] matrix2, double[,] matrix3)
        {
            if (Open)
            {
                datas.Add(new ThisData() { Name = title, Matrix1 = matrix1, Matrix2 = matrix2, Matrix3 = matrix3 });
            }
        }

        private class ThisData
        {
            public string Name { get; set; }

            public int Tag { get; set; }

            public double[,] Matrix1 { get; set; }

            public double[,] Matrix2 { get; set; }

            public double[,] Matrix3 { get; set; }
        }
    }
}
