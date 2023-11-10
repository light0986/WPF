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
using 重返雲之國_外傳.Models;

namespace 重返雲之國_外傳.IMG.Boss
{
    /// <summary>
    /// BrustStream.xaml 的互動邏輯
    /// </summary>
    public partial class BrustStream : UserControl
    {
        private Data data;

        public BrustStream(PlayerStatus player)
        {
            InitializeComponent();
            data = new Data(player);
            DataContext = data;
            Loaded += BrustStream_Loaded;
        }

        private async void BrustStream_Loaded(object sender, RoutedEventArgs e)
        {
            do
            {
                data.BrustMargin = new Thickness(0);
                for(int i = 0; i < 5; i++)
                {
                    data.RefreshView();
                    await Task.Delay(data.player.DelayTime);
                }
                
                data.BrustMargin = new Thickness(0, 5, 0, 5);
                for (int i = 0; i < 5; i++)
                {
                    data.RefreshView();
                    await Task.Delay(data.player.DelayTime);
                }
            }
            while (true);
        }

        public void SetPosition(double TargetX, double TargetY, double StartX, double StartY)
        {
            data.PositionX = StartX;
            data.PositionY = StartY;
            data.TargetX = TargetX;
            data.TargetY = TargetY;
        }

        private class Data : MVVM
        {
            public Data(PlayerStatus player) { this.player = player; }

            public readonly PlayerStatus player;


            private Thickness _BrustMargin = new Thickness(0, 5, 0, 5);
            public Thickness BrustMargin
            {
                get => _BrustMargin;
                set
                {
                    _BrustMargin = value;
                    NotifyPropertyChanged("BrustMargin");
                }
            }


            public double RelativeX
            {
                get => PositionX - player.CenterX + 375;
            }

            public double RelativeY
            {
                get => PositionY - player.CenterY + 275;
            }


            private double _PositionX = 0;
            public double PositionX
            {
                get => _PositionX;
                set
                {
                    _PositionX = value;
                    NotifyPropertyChanged("RelativeX");
                }
            }


            private double _PositionY = 0;
            public double PositionY
            {
                get => _PositionY;
                set
                {
                    _PositionY = value;
                    NotifyPropertyChanged("RelativeY");
                }
            }


            public double TargetX { get; set; } = 0;

            public double TargetY { get; set; } = 0;

            public void RefreshView()
            {
                NotifyPropertyChanged("RelativeX");
                NotifyPropertyChanged("RelativeY");
            }
        }
    }
}
