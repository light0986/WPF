using System;
using System.Windows;
using System.Windows.Controls;

namespace NeuralNetwork
{
    /// <summary>
    /// ResetPage.xaml 的互動邏輯
    /// </summary>
    public partial class ResetPage : UserControl
    {
        public delegate void Event();
        public event Event CloseClicked;

        public delegate void Event2(int type, bool one);
        public event Event2 TypeClicked;

        private bool OneNetwork;

        public ResetPage()
        {
            InitializeComponent();
            OneNetwork = true;
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


        private void Type_Click(object sender, RoutedEventArgs e)
        {
            int tag = Convert.ToInt32(((Button)sender).Tag);

            Visibility = Visibility.Hidden;
            TypeClicked?.Invoke(tag, OneNetwork);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            CloseClicked?.Invoke();
        }
    }
}
