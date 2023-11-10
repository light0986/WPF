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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 重返雲之國_外傳.Models;

namespace 重返雲之國_外傳.IMG.MedicalKit
{
    /// <summary>
    /// WhitePowder.xaml 的互動邏輯
    /// </summary>
    public partial class WhitePowder : UserControl
    {
        private Data data;
        private PlayerStatus player;
        private bool OnWork = false;

        public WhitePowder(PlayerStatus player)
        {
            InitializeComponent();
            data = new Data(player);
            DataContext = data;
            this.player = player;
        }

        public async void StartProgress()
        {
            if (OnWork == false)
            {
                OnWork = true;
                do
                {
                    data.Progress -= player.PowderCD;
                    await Task.Delay(1000);
                }
                while (OnWork);
            }
        }

        public void AddProgress(int i)
        {
            data.Progress -= i;
        }

        public void AddMax()
        {
            data.Max += 1;
        }

        public void StopProgress()
        {
            OnWork = false;
        }

        public bool UsePowder()
        {
            if (data.Now > 0)
            {
                data.Now--;
                return true;
            }
            else
            {
                return false;
            }
        }

        private class Data : MVVM
        {
            public Data(PlayerStatus player)
            {
                this.player = player;
            }

            private readonly PlayerStatus player;


            private double _Now = 3;
            public double Now
            {
                get => _Now;
                set
                {
                    _Now = value;
                    if (_Now > Max) { _Now = Max; }
                    NotifyPropertyChanged("Now");
                }
            }


            public double Max
            {
                get => player.Amount;
                set => player.Amount = value;
            }


            private double _Progress = 100;
            public double Progress
            {
                get => _Progress;
                set
                {
                    _Progress = value;
                    if (_Progress <= 0)
                    {
                        _Progress += 100;
                        Now++;
                    }
                    NotifyPropertyChanged("Progress");
                }
            }
        }
    }
}
