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

namespace 重返雲之國_外傳.UserControls
{
    /// <summary>
    /// OpeningStory.xaml 的互動邏輯
    /// </summary>
    public partial class OpeningStory : UserControl
    {
        private Data data;
        private MusicPlayer music = new MusicPlayer();
        public Action OpeningFinished;

        public OpeningStory()
        {
            InitializeComponent();
            data = new Data();
            DataContext = data;
            Loaded += OpeningStory_Loaded;
        }

        private async void OpeningStory_Loaded(object sender, RoutedEventArgs e)
        {
            music.PlayMusic("開頭音樂");

            for (int i = 0; i < 10; i++)
            {
                data.Mask += 0.1;
                await Task.Delay(20);
            }

            for (int i = 1; i < 19; i++)
            {
                char[] Narration = ((String)Application.Current.TryFindResource("S" + i) ?? "").ToArray();
                data.IMG = (BitmapImage)Application.Current.TryFindResource("Story" + i);
                data.StoryText = "";
                data.TextShow = Visibility.Visible;
                await Task.Delay(500);

                for (int j = 0; j < Narration.Length; j++)
                {
                    data.StoryText += Narration[j];
                    if (Narration[j] != '　')
                    {
                        await Task.Delay(100);
                    }
                }

                int count = 0;
                data.OnWork = false;
                do
                {
                    if ((count / 33) % 2 == 0) { data.NextShining = Visibility.Visible; }
                    else { data.NextShining = Visibility.Hidden; }
                    count++;
                    if (data.OnWork || count >= 300) { break; }
                    await Task.Delay(20);
                }
                while (true);

                data.NextShining = Visibility.Hidden;
                data.TextShow = Visibility.Hidden;
                await Task.Delay(1000);
            }

            StartStop();
        }

        public void KeyPress(Key key)
        {
            if (key == Key.Enter)
            {
                if (data.OnStop == false) { music.PlayShort("游標移動音效"); }
                StartStop();
            }
            if (key == Key.Space)
            {
                if (data.OnWork == false)
                {
                    data.OnWork = true;
                    music.PlayShort("游標移動音效");
                }
            }
        }

        private async void StartStop()
        {
            if (data.OnStop == false)
            {
                data.OnStop = true;

                for (int i = 0; i < 10; i++)
                {
                    data.Mask -= 0.1;
                    await Task.Delay(20);
                }

                music.StopMusic();
                OpeningFinished?.Invoke();
            }
        }

        private class Data : MVVM
        {
            public bool OnWork { get; set; } = true;

            public bool OnStop { get; set; } = false;

            #region 效果
            private BitmapImage _IMG = (BitmapImage)Application.Current.TryFindResource("Story1");
            public BitmapImage IMG
            {
                get => _IMG;
                set
                {
                    _IMG = value;
                    NotifyPropertyChanged("IMG");
                }
            }


            private Visibility _TextShow = Visibility.Hidden;
            public Visibility TextShow
            {
                get => _TextShow;
                set
                {
                    _TextShow = value;
                    NotifyPropertyChanged("TextShow");
                }
            }


            private Visibility _NextShining = Visibility.Hidden;
            public Visibility NextShining
            {
                get => _NextShining;
                set
                {
                    _NextShining = value;
                    NotifyPropertyChanged("NextShining");
                }
            }


            private string _StoryText = "";
            public string StoryText
            {
                get => _StoryText;
                set
                {
                    _StoryText = value;
                    NotifyPropertyChanged("StoryText");
                }
            }


            private double _Mask = 0;
            public double Mask
            {
                get => _Mask;
                set
                {
                    _Mask = value;
                    NotifyPropertyChanged("Mask");
                }
            }
            #endregion
        }
    }
}
