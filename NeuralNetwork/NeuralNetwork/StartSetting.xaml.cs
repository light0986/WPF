using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeuralNetwork
{
    /// <summary>
    /// StartSetting.xaml 的互動邏輯
    /// </summary>
    public partial class StartSetting : UserControl
    {
        public delegate void Event(int inputnodes, int hiddennodes, int outputnodes, double learningrate, bool onenetwork);
        public event Event OKClicked;

        private bool OneNetwork;

        public StartSetting()
        {
            InitializeComponent();
            OneNetwork = true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0123456789.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0123456789]+");
            e.Handled = regex.IsMatch(e.Text);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int inputnodes = Convert.ToInt32(Inputnodes.Text);
                int hiddennodes = Convert.ToInt32(Hiddennodes.Text);
                int outputnodes = Convert.ToInt32(Outputnodes.Text);
                double learningrate = Convert.ToDouble(Learningrate.Text);

                Visibility = Visibility.Hidden;
                OKClicked?.Invoke(inputnodes, hiddennodes, outputnodes, learningrate, OneNetwork);
            }
            catch { }
        }
    }
}
