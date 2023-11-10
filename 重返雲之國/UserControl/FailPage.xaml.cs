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
    /// FailPage.xaml 的互動邏輯
    /// </summary>
    public partial class FailPage : UserControl
    {
        private MusicPlayer music = new MusicPlayer();
        private Data data;
        private PlayerStatus player;

        public Action Restart;
        public Action GameOver;
        
        private bool OnWork { get; set; } = false;

        public FailPage(PlayerStatus player)
        {
            InitializeComponent();

            Properties.Settings.Default.Ending01 = true;
            Properties.Settings.Default.Save();

            this.player = player;
            data = new Data();
            DataContext = data;
            Loaded += FailPage_Loaded;
        }

        private async void FailPage_Loaded(object sender, RoutedEventArgs e)
        {
            music.PlayMusic("失敗音樂");
            SetText();
            double d = 0;
            do
            {
                if (d == 0) { d = 1; } else { d = 0; }

                if (data.Continue)
                {
                    data.YesOpacity = d;
                    data.NoOpacity = 1;
                }
                else
                {
                    data.YesOpacity = 1;
                    data.NoOpacity = d;
                }

                await Task.Delay(410);
            }
            while (true);
        }

        private void SetText()
        {
            if(player.ContinueTime > 0)
            {
                data.YesText = "繼承能力，再試一次。剩餘次數: " + player.ContinueTime;
            }
            else if (player.ContinueTime == 0)
            {
                data.YesText = "沒機會了";
                data.YesColor = new SolidColorBrush(Colors.Gray);
            }
            else if (player.ContinueTime == -1)
            {
                data.YesText = "繼承能力，再試一次";
            }
        }

        public void KeyPress(Key key)
        {
            if(OnWork == false)
            {
                if (data.Continue)
                {
                    if (key == Key.Down)
                    {
                        data.Continue = false;
                        music.PlayShort("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        if (player.ContinueTime != 0)
                        {
                            StartContinue();
                        }
                    }
                }
                else
                {
                    if (key == Key.Up)
                    {
                        data.Continue = true;
                        music.PlayShort("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        EndContinue();
                    }
                }
            }      
        }

        private async void StartContinue()
        {
            OnWork = true;
            SelectionGrid.Visibility = Visibility.Hidden;

            if (player.difficulty == Entities.Difficulty.Middle) { player.ContinueTime--; }

            music.StopMusic();
            music.PlayLong("悠白復活音效");

            for (int i = 80; i >= 0; i--)
            {
                data.ULULUPosition = new Thickness(i * 5, 0, (i * -5), 0); 
                await Task.Delay(20);
            }

            for (int i = 0;i < 2; i++)
            {
                data.IMG = (BitmapImage)Application.Current.TryFindResource("ULULU02");
                await Task.Delay(1000);
                data.IMG = (BitmapImage)Application.Current.TryFindResource("ULULU01");
                await Task.Delay(1000);
            }
            Restart?.Invoke();
        }

        private async void EndContinue()
        {
            OnWork = true;

            music.StopMusic();
            music.PlayLong("家庭號修羅地獄");

            for (int i = 0; i < 20; i++)
            {
                MainGrid.Opacity -= 0.05;
                await Task.Delay(20);
            }
            await Task.Delay(5000);
            GameOver?.Invoke();
        }

        private class Data: MVVM
        {
            private BitmapImage _IMG = (BitmapImage) Application.Current.TryFindResource("ULULU01");
            public BitmapImage IMG
            {
                get => _IMG;
                set
                {
                    _IMG = value;
                    NotifyPropertyChanged("IMG");
                }
            }


            private bool _Continue = true;
            public bool Continue
            {
                get => _Continue;
                set
                {
                    _Continue = value;
                    if (_Continue)
                    {
                        YesVisibility = Visibility.Visible;
                        NoVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        YesVisibility = Visibility.Hidden;
                        NoVisibility = Visibility.Visible;
                    }
                }
            }


            private string _YesText = "";
            public string YesText
            {
                get => _YesText;
                set
                {
                    _YesText = value;
                    NotifyPropertyChanged("YesText");
                }
            }

            private Visibility _YesVisibility = Visibility.Visible;
            public Visibility YesVisibility
            {
                get => _YesVisibility;
                set
                {
                    _YesVisibility = value;
                    NotifyPropertyChanged("YesVisibility");
                }
            }

            private double _YesOpacity = 1;
            public double YesOpacity
            {
                get => _YesOpacity;
                set
                {
                    _YesOpacity = value;
                    NotifyPropertyChanged("YesOpacity");
                }
            }

            private SolidColorBrush _YesColor = new SolidColorBrush(Colors.Black);
            public SolidColorBrush YesColor
            {
                get => _YesColor;
                set
                {
                    _YesColor = value;
                    NotifyPropertyChanged("YesColor");
                }
            }



            private Visibility _NoVisibility = Visibility.Hidden;
            public Visibility NoVisibility
            {
                get => _NoVisibility;
                set
                {
                    _NoVisibility = value;
                    NotifyPropertyChanged("NoVisibility");
                }
            }

            private double _NoOpacity = 1;
            public double NoOpacity
            {
                get => _NoOpacity;
                set
                {
                    _NoOpacity = value;
                    NotifyPropertyChanged("NoOpacity");
                }
            }



            private Thickness _ULULUPosition = new Thickness(400, 0 , -400 , 0);
            public Thickness ULULUPosition
            {
                get => _ULULUPosition;
                set
                {
                    _ULULUPosition = value;
                    NotifyPropertyChanged("ULULUPosition");
                }
            }
        }
    }
}
