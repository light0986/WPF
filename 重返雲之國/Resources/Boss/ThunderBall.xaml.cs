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
    /// ThunderBall.xaml 的互動邏輯
    /// </summary>
    public partial class ThunderBall : UserControl
    {
        private Data data;
        public bool OnWork { get => data.OnWork; set => data.OnWork = value; }

        public bool OnPause { get; set; } = false;

        public double DamageToken { get => data.DamageToken; set => data.DamageToken = value; }

        public bool HitSuccess
        {
            get => data.HitSuccess;
            set => data.HitSuccess = value;
        }

        public Action Destroyed;

        public ThunderBall(PlayerStatus player)
        {
            InitializeComponent();
            data = new Data(player);
            DataContext = data;
            data.PositionX = player.StartX;
            data.PositionY = player.StartY - 250;
        }

        public async void ShootDir(double dir, double StartX, double StartY)
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.ItemVisibility = 1;
                data.PositionX = StartX;
                data.PositionY = StartY;

                data.MoveX = Math.Cos(dir) * 4;
                data.MoveY = Math.Sin(dir) * 4;
                for(int j = 0; j < 20; j++)
                {
                    data.IMG = (BitmapImage)Application.Current.TryFindResource("Thunder0" + ((j % 2) + 1));
                    for (int i = 0; i < 10; i++)
                    {
                        data.PositionX += data.MoveX;
                        data.PositionY += data.MoveY;
                        data.CheckHit();

                        do
                        {
                            await Task.Delay(data.player.DelayTime);
                        }
                        while (OnPause);
                    }
                }

                data.ItemVisibility = 0;
                Destroyed?.Invoke();
            }
        }

        private class Data : MVVM
        {
            public Data(PlayerStatus player)
            {
                this.player = player;
                if (player.difficulty == Entities.Difficulty.Easy) { Attack = 6; }
                else if (player.difficulty == Entities.Difficulty.Middle) { Attack = 8; }
            }

            public readonly PlayerStatus player;

            private BitmapImage _IMG = (BitmapImage)Application.Current.TryFindResource("Thunder01");
            public BitmapImage IMG
            {
                get => _IMG;
                set
                {
                    _IMG = value;
                    NotifyPropertyChanged("IMG");
                }
            }


            private bool _OnWork = false;
            public bool OnWork
            {
                get => _OnWork;
                set => _OnWork = value;
            }


            private double _ItemVisibility = 0;
            public double ItemVisibility
            {
                get => _ItemVisibility;
                set
                {
                    _ItemVisibility = value;
                    NotifyPropertyChanged("ItemVisibility");
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

            public double MoveX { get; set; }

            public double MoveY { get; set; }

            public double DamageToken { get; set; }

            public bool HitSuccess { get; set; } = false;

            public double Attack { get; set; } = 10;

            public void CheckHit()
            {
                //確認有沒有打到
                if (ItemVisibility == 1)
                {
                    if ((375 >= RelativeX && 375 <= RelativeX + 50) || (425 >= RelativeX && 425 <= RelativeX + 50) || (RelativeX >= 375 && RelativeX + 50 <= 375) || (RelativeX >= 425 && RelativeX + 50 <= 425)) //X
                    {
                        if ((275 >= RelativeY && 275 <= RelativeY + 50) || (325 >= RelativeY && 325 <= RelativeY + 50) || (RelativeY >= 275 && RelativeY + 50 <= 275) || (RelativeY >= 325 && RelativeY + 50 <= 325)) //Y
                        {
                            if (player.OnAvoid == false && player.OnDamage == false)
                            {
                                ItemVisibility = 0;
                                DamageToken = Attack;
                                HitSuccess = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
