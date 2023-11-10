using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

namespace 重返雲之國_外傳.UserControls
{
    /// <summary>
    /// DirectionsPage.xaml 的互動邏輯
    /// </summary>
    public partial class DirectionsPage : UserControl
    {
        private MusicPlayer music = new MusicPlayer();
        private Data data = new Data();
        public Action LookComplete;
        private bool OnBusy { get; set; } = false;

        public DirectionsPage()
        {
            InitializeComponent();
            DataContext = data;
            Loaded += DirectionsPage_Loaded;
        }

        private async void DirectionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            music.PlayMusic("其他音樂");
            data.GoBright();

            do
            {
                data.AvoidToken -= 9.4;
                await Task.Delay(20);
            }
            while (true);
        }

        public async void KeyPress(Key key)
        {
            if (OnBusy == false)
            {
                if (key == Key.Enter || key == Key.Back)
                {
                    OnBusy = true;
                    music.PlayShort("游標移動音效");
                    await data.GoDark();
                    music.StopMusic();
                    LookComplete?.Invoke();
                }
            }
        }

        private class Data : MVVM
        {
            private double _AvoidToken = 360;
            public double AvoidToken
            {
                get => _AvoidToken;
                set
                {
                    _AvoidToken = value;
                    if(_AvoidToken <= 0)
                    {
                        _AvoidToken += 360;
                        RollCount++;
                    }
                    NotifyPropertyChanged("AvoidToken");
                }
            }


            private double _MainOpacity = 0;
            public double MainOpacity
            {
                get => _MainOpacity;
                set
                {
                    _MainOpacity = value;
                    NotifyPropertyChanged("MainOpacity");
                }
            }

            //0~750
            private int _PositionX = -50;
            public int PositionX
            {
                get => _PositionX;
                set
                {
                    _PositionX = value;
                    NotifyPropertyChanged("PositionX");
                }
            }

            //0~550
            private int _PositionY = -50;
            public int PositionY
            {
                get => _PositionY;
                set
                {
                    _PositionY = value;
                    NotifyPropertyChanged("PositionY");
                }
            }


            private int _IMG_Size = 50;
            public int IMG_Size
            {
                get => _IMG_Size;
                set
                {
                    _IMG_Size = value;
                    NotifyPropertyChanged("IMG_Size");
                }
            }


            private int _RollCount = 0;
            public int RollCount
            {
                get => _RollCount;
                set
                {
                    _RollCount = value;
                    if(_RollCount >= 4)
                    {
                        _RollCount = 0;
                        ChangePosition();
                    }
                }
            }


            private Random random = new Random();
            private void ChangePosition()
            {
                IMG_Size = random.Next(50, 201);
                PositionX = random.Next(0, 751 - IMG_Size);
                PositionY = random.Next(0, 551 - IMG_Size);
            }

            public async void GoBright()
            {
                for (int i = 0; i < 10; i++)
                {
                    MainOpacity += 0.1;
                    await Task.Delay(20);
                }
                ChangePosition();
            }

            public async Task GoDark()
            {
                for (int i = 0; i < 10; i++)
                {
                    MainOpacity -= 0.1;
                    await Task.Delay(20);
                }
            }
        }
    }
}
