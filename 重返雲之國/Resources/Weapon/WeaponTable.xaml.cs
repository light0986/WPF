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

namespace 重返雲之國_外傳.IMG.Weapon
{
    /// <summary>
    /// WeaponTable.xaml 的互動邏輯
    /// </summary>
    public partial class WeaponTable : UserControl
    {
        public Action WeaponShow;
        public Action WeaponStartHit;
        public Action WeaponStopHit;
        public Action WeaponHide;
        private PlayerStatus player;

        public bool OnPause { get; set; } = false;
        public bool OnStop { get; set; } = false;
        private bool Hit { get; set; } = true;
        private bool StillHit { get; set; } = false;
        private Key Dir;

        public WeaponTable(Key Dir, PlayerStatus player)
        {
            InitializeComponent();
            this.player = player;
            this.Dir = Dir;
            Loaded += WeaponTable_Loaded;
        }

        private async void WeaponTable_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTable();
            do
            {
                if (Hit && player.PlayerAP >= ((int)((player.PlayerATK * 2) - player.Amount))) { await One_Two_Three(); } else { break; }
                if (Hit && player.PlayerAP >= ((int)((player.PlayerATK * 2) - player.Amount))) { await Three_Four_One(); } else { break; }
                if (OnStop) { break; }
            }
            while (true);
            WeaponView.Opacity = 0;
            WeaponHide?.Invoke();
        }

        private void CreateTable()
        {
            if(Dir == Key.Up || Dir == Key.Down)
            {
                WeaponView.Width = 150;
                WeaponView.Height = 50;
                WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table01");
            }
            else
            {
                WeaponView.Width = 50;
                WeaponView.Height = 150;
                WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table11");
            }
            WeaponShow?.Invoke();
        }

        private async Task One_Two_Three()
        {
            WeaponView.Opacity = 1;
            double speed = player.MoveSpeed / 5;
            double first = 6 / speed;
            double second = 4 / speed;

            if (OnStop == false)
            {
                if (Dir == Key.Up || Dir == Key.Down) { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table01"); }
                else { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table11"); }
                for (int i = 0; i < first; i++)
                {
                    do
                    {
                        if (OnStop) { break; }
                        await Task.Delay(player.DelayTime);
                    }
                    while (OnPause);
                }
            }

            if (OnStop == false)
            {
                if (Dir == Key.Up || Dir == Key.Down) { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table02"); }
                else { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table12"); }
                WeaponStartHit?.Invoke();
                StillHit = false;
                for (int i = 0; i < second; i++)
                {
                    do
                    {
                        if (OnStop) { break; }
                        await Task.Delay(player.DelayTime);
                    }
                    while (OnPause);
                }
            }

            WeaponStopHit?.Invoke();

            if (OnStop == false)
            {
                if (Dir == Key.Up || Dir == Key.Down) { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table03"); }
                else { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table13"); }
                for (int i = 0; i < first; i++)
                {
                    do
                    {
                        if (OnStop) { break; }
                        await Task.Delay(player.DelayTime);
                    }
                    while (OnPause);
                }

                Hit = StillHit;
            }
        }

        private async Task Three_Four_One()
        {
            double speed = player.MoveSpeed / 5;
            double first = 6 / speed;
            double second = 4 / speed;

            if (OnStop == false)
            {
                if (Dir == Key.Up || Dir == Key.Down) { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table03"); }
                else { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table13"); }
                for (int i = 0; i < first; i++)
                {
                    do
                    {
                        if (OnStop) { break; }
                        await Task.Delay(player.DelayTime);
                    }
                    while (OnPause);
                }
            }

            if (OnStop == false)
            {
                if (Dir == Key.Up || Dir == Key.Down) { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table04"); }
                else { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table14"); }
                WeaponStartHit?.Invoke();
                StillHit = false;
                for (int i = 0; i < second; i++)
                {
                    do
                    {
                        if (OnStop) { break; }
                        await Task.Delay(player.DelayTime);
                    }
                    while (OnPause);
                }
            }

            WeaponStopHit?.Invoke();

            if (OnStop == false)
            {
                if (Dir == Key.Up || Dir == Key.Down) { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table01"); }
                else { WeaponView.Source = (BitmapImage)Application.Current.TryFindResource("Table11"); }
                for (int i = 0; i < first; i++)
                {
                    do
                    {
                        if (OnStop) { break; }
                        await Task.Delay(player.DelayTime);
                    }
                    while (OnPause);
                }

                Hit = StillHit;
            }
        }

        public void PlayerHit()
        {
            StillHit = true;
        }
    }
}
