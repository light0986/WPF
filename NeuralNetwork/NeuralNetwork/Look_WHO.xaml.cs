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

namespace NeuralNetwork
{
    /// <summary>
    /// Look_WHO.xaml 的互動邏輯
    /// </summary>
    public partial class Look_WHO : UserControl
    {
        public delegate void Event();
        public event Event BackClicked;

        private readonly double[,] WHO;
        private bool OnWork = true;

        public Look_WHO(double[,] who)
        {
            InitializeComponent();
            WHO = who;
            Loaded += Look_WHO_Loaded;
        }

        private async void Look_WHO_Loaded(object sender, RoutedEventArgs e)
        {
            WorkProgress.Maximum = WHO.GetLength(0);
            WorkProgress.Value = 0;
            Style style = (Style)Resources["NoStyle"];

            for (int i = 0; i < WHO.GetLength(0); i++)
            {
                Grid grid = new Grid();
                Button button = new Button
                {
                    Tag = i,
                    Style = style,
                    Content = i
                };
                button.Click += Button_Click;
                _ = grid.Children.Add(button);
                _ = MatrixView.Children.Add(grid);
                WorkProgress.Value++;
                await Task.Delay(1);
            }

            OnWork = false;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                RowArray.Children.Clear();
                Button button = (Button)sender;
                int tag = (int)button.Tag;

                WorkProgress.Maximum = WHO.GetLength(1);
                WorkProgress.Value = 0;

                int i = 0;
                do
                {
                    if (i < WHO.GetLength(1))
                    {
                        StackPanel stack = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };

                        for (int j = 0; j < 10; j++)
                        {
                            if (i < WHO.GetLength(1))
                            {
                                Label label = new Label()
                                {
                                    Width = 40,
                                    Content = WHO[tag, i]
                                };

                                if (WHO[tag, i] > 0)
                                {
                                    label.Background = Brushes.LightGreen;
                                }
                                else
                                {
                                    label.Background = Brushes.LightPink;
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

                OnWork = false;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackClicked?.Invoke();
            Visibility = Visibility.Hidden;
        }
    }
}
