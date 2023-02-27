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
    /// NormalDistribution.xaml 的互動邏輯
    /// </summary>
    public partial class NormalDistribution : UserControl
    {
        public delegate void Event();
        public event Event BackClicked;

        public NormalDistribution()
        {
            InitializeComponent();
            Loaded += NormalDistribution_Loaded;
        }

        private void NormalDistribution_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas.Width="740" Height="490"
            //mu = 中心值, sigma = 單邊寬
            //中心值 = 370，單邊寬370

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            BackClicked?.Invoke();
        }
    }
}
