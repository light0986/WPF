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

namespace 重返雲之國_外傳.IMG.History
{
    /// <summary>
    /// HistoryIMG.xaml 的互動邏輯
    /// </summary>
    public partial class HistoryIMG : UserControl
    {
        private Data data = new Data();
        public Action LookComplete;

        public HistoryIMG(int PositionShow)
        {
            InitializeComponent();
            DataContext = data;
            data.PositionShow = PositionShow;
        }

        public void KeyPress(Key key)
        {
            if (key == Key.Back || key == Key.Enter)
            {
                LookComplete?.Invoke();
            }
        }

        private class Data : MVVM
        {
            private BitmapImage _IMG;
            public BitmapImage IMG
            {
                get => _IMG;
                set
                {
                    _IMG = value;
                    NotifyPropertyChanged("IMG");
                }
            }

            public int PositionShow
            {
                set
                {
                    if(value == 11)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory09");
                    }
                    else if (value == 12)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory010");
                    }
                    else if (value == 13)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory011");
                    }
                    else if (value == 21)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory120");
                    }
                    else if (value == 22)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory121");
                    }
                    else if (value == 23)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory122");
                    }
                    else if (value == 31)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory240");
                    }
                    else if (value == 32)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("HisStory241");
                    }
                }
            }
        }
    }
}
