using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeuralNetwork
{
    /// <summary>
    /// StartSetting.xaml 的互動邏輯
    /// </summary>
    public partial class StartSetting : UserControl
    {
        public delegate void Event(int inputnodes, int hiddennodes, int outputnodes, double learningrate, bool onenetwork, int row, int col);
        public event Event OKClicked;

        private bool OneNetwork;

        public StartSetting()
        {
            InitializeComponent();
            OneNetwork = true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0123456789]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0123456789.]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if ((string)radioButton.Content == "One Object")
            {
                OneNetwork = true;
            }
            else
            {
                OneNetwork = false;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Rows.Foreground = Brushes.Black;
                Columns.Foreground = Brushes.Black;
                Hiddennodes.Foreground = Brushes.Black;
                Outputnodes.Foreground = Brushes.Black;

                int inputnodes = Convert.ToInt32(Inputnodes.Text);
                int hiddennodes = Convert.ToInt32(Hiddennodes.Text);
                int outputnodes = Convert.ToInt32(Outputnodes.Text);
                double learningrate = Convert.ToDouble(Learningrate.Text);

                int row = Convert.ToInt32(Rows.Text);
                int col = Convert.ToInt32(Columns.Text);

                if (row * col != inputnodes)
                {
                    Rows.Foreground = Brushes.Red;
                    Columns.Foreground = Brushes.Red;
                    for (int i = 0; i < 5; i++)
                    {
                        Rows.Margin = new Thickness(0, 5, 2, 5);
                        Columns.Margin = new Thickness(0, 5, 2, 5);
                        await Task.Delay(50);
                        Rows.Margin = new Thickness(2, 5, 0, 5);
                        Columns.Margin = new Thickness(2, 5, 0, 5);
                        await Task.Delay(50);
                    }
                    Rows.Margin = new Thickness(1, 5, 1, 5);
                    Columns.Margin = new Thickness(1, 5, 1, 5);
                }
                else if (hiddennodes < outputnodes)
                {
                    Hiddennodes.Foreground = Brushes.Red;
                    Outputnodes.Foreground = Brushes.Red;
                    for (int i = 0; i < 5; i++)
                    {
                        Hiddennodes.Margin = new Thickness(0, 5, 2, 5);
                        Outputnodes.Margin = new Thickness(0, 5, 2, 5);
                        await Task.Delay(50);
                        Hiddennodes.Margin = new Thickness(2, 5, 0, 5);
                        Outputnodes.Margin = new Thickness(2, 5, 0, 5);
                        await Task.Delay(50);
                    }
                    Hiddennodes.Margin = new Thickness(1, 5, 1, 5);
                    Outputnodes.Margin = new Thickness(1, 5, 1, 5);
                }
                else
                {
                    Visibility = Visibility.Hidden;
                    OKClicked?.Invoke(inputnodes, hiddennodes, outputnodes, learningrate, OneNetwork, row, col);
                }
            }
            catch { }
        }
    }
}
