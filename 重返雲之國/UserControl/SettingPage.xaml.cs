using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace 重返雲之國_外傳.UserControls
{
    /// <summary>
    /// SettingPage.xaml 的互動邏輯
    /// </summary>
    public partial class SettingPage : UserControl
    {
        private Data data = new Data();
        private MusicPlayer music { get => data.music; set => data.music = value; }
        public Action Closed;

        private bool OnBusy { get; set; } = false;
        private bool OnPause { get; set; } = false;

        public delegate void Event2(bool TrueOrFalse);
        public Event2 UsePause;

        public SettingPage()
        {
            InitializeComponent();
            data = new Data();
            DataContext = data;
            Loaded += SettingPage_Loaded;
        }

        private async void SettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            music.PlayMusic("其他音樂");
            CreateStackPanelItems();
            await data.GoBright();

            int d = 0;
            do
            {
                if(d == 50) { d = 0; }
                if(d == 0) { data.IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01"); }
                else if (d == 25) { data.IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud06"); }

                d++;
                await Task.Delay(20);
            }
            while(true);
        }

        public void KeyPress(Key key)
        {
            if (OnBusy == false)
            {
                if (OnPause)
                {
                    if (key == Key.Enter)
                    {
                        music.PlayMusic("其他音樂");
                        OnPause = false;
                        UsePause?.Invoke(false);
                    }
                }
                else
                {
                    if (data.SelectIndex == 0)
                    {
                        if (key == Key.Down)
                        {
                            data.SelectIndex = 1;
                            music.PlayShort("游標移動音效");
                        }
                        else if (key == Key.Right)
                        {
                            data.MusicVolume += 0.1;
                            music.PlayShort("游標移動音效");
                        }
                        else if (key == Key.Left)
                        {
                            data.MusicVolume -= 0.1;
                            music.PlayShort("游標移動音效");
                        }
                    }
                    else if (data.SelectIndex == 1)
                    {
                        if (key == Key.Up)
                        {
                            data.SelectIndex = 0;
                            music.PlayShort("游標移動音效");
                        }
                        else if (key == Key.Down)
                        {
                            data.SelectIndex = 2;
                            music.PlayShort("游標移動音效");
                        }
                        else if (key == Key.Right)
                        {
                            data.EffectVolume += 0.1;
                            music.PlayShort("游標移動音效");
                        }
                        else if (key == Key.Left)
                        {
                            data.EffectVolume -= 0.1;
                            music.PlayShort("游標移動音效");
                        }
                    }
                    else if (data.SelectIndex == 2)
                    {
                        if (key == Key.Up)
                        {
                            data.SelectIndex = 1;
                            music.PlayShort("游標移動音效");
                        }
                        if (key == Key.Enter)
                        {
                            music.PlayShort("悠白能力提升音效");
                            OnBusy = true;
                            Save();
                        }
                    }

                    if (key == Key.Back)
                    {
                        music.PlayShort("游標移動音效");
                        OnBusy = true;
                        Close();
                    }
                }
            }
        }

        private void Save()
        {
            Properties.Settings.Default.MusicVolume = data.MusicVolume;
            Properties.Settings.Default.EffectVolume = data.EffectVolume;
            Properties.Settings.Default.Save();
            Close();
        }

        private async void Close()
        {
            await data.GoDark();
            music.StopMusic();
            Closed?.Invoke();
        }

        private void CreateStackPanelItems()
        {
            //開頭基本
            for (int i = 0; i < 5; i++)
            {
                Button button = new Button()
                {
                    Content = (string)Application.Current.TryFindResource("Name1" + i),
                    Tag = (string)Application.Current.TryFindResource("URL1" + i)
                };
                button.MouseEnter += (s, e) => { music.PlayShort("游標移動音效"); };
                button.Click += (s, e) => { OpenBrowser((string)button.Tag); };
                SelectionView.Children.Add(button);
            }

            //有進過遊戲
            if (Properties.Settings.Default.Ending00)
            {
                for (int i = 0; i < 4; i++)
                {
                    Button button = new Button()
                    {
                        Content = (string)Application.Current.TryFindResource("Name2" + i),
                        Tag = (string)Application.Current.TryFindResource("URL2" + i)
                    };
                    button.MouseEnter += (s, e) => { music.PlayShort("游標移動音效"); };
                    button.Click += (s, e) => { OpenBrowser((string)button.Tag); };
                    SelectionView.Children.Add(button);
                }
            }

            //有失敗過
            if (Properties.Settings.Default.Ending01)
            {
                for (int i = 0; i < 3; i++)
                {
                    Button button = new Button()
                    {
                        Content = (string)Application.Current.TryFindResource("Name3" + i),
                        Tag = (string)Application.Current.TryFindResource("URL3" + i)
                    };
                    button.MouseEnter += (s, e) => { music.PlayShort("游標移動音效"); };
                    button.Click += (s, e) => { OpenBrowser((string)button.Tag); };
                    SelectionView.Children.Add(button);
                }
            }

            //簡單破關過 || 中等破關過
            if (Properties.Settings.Default.Ending02 || Properties.Settings.Default.Ending03)
            {
                for (int i = 0; i < 2; i++)
                {
                    Button button = new Button()
                    {
                        Content = (string)Application.Current.TryFindResource("Name4" + i),
                        Tag = (string)Application.Current.TryFindResource("URL4" + i)
                    };
                    button.MouseEnter += (s, e) => { music.PlayShort("游標移動音效"); };
                    button.Click += (s, e) => { OpenBrowser((string)button.Tag); };
                    SelectionView.Children.Add(button);
                }
            }

            //最難破關過
            if (Properties.Settings.Default.Ending04)
            {
                for (int i = 0; i < 3; i++)
                {
                    Button button = new Button()
                    {
                        Content = (string)Application.Current.TryFindResource("Name5" + i),
                        Tag = (string)Application.Current.TryFindResource("URL5" + i)
                    };
                    button.MouseEnter += (s, e) => { music.PlayShort("游標移動音效"); };
                    button.Click += (s, e) => { OpenBrowser((string)button.Tag); };
                    SelectionView.Children.Add(button);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;
            string b_tag = (string)button.Tag;
            string path = "";
            if (b_tag == "01")
            {
                path = (string)Application.Current.TryFindResource("BackTo01");
            }
            else if (b_tag == "02")
            {
                path = (string)Application.Current.TryFindResource("BackTo02");
            }
            else if (b_tag == "03")
            {
                path = (string)Application.Current.TryFindResource("BackTo03");
            }
            else if (b_tag == "04")
            {
                path = (string)Application.Current.TryFindResource("Github");
            }
            else if (b_tag == "05")
            {
                path = (string)Application.Current.TryFindResource("TonyCWater");
            }
            else if (b_tag == "06")
            {
                path = (string)Application.Current.TryFindResource("cloudhorizon");
            }
            OpenBrowser(path);
        }

        private void OpenBrowser(string URL)
        {
            OnPause = true;
            UsePause?.Invoke(true);
            music?.StopMusic();

            Process proc = new Process();
            proc.StartInfo.FileName = URL;
            proc.Start();
        }

        private class Data : MVVM
        {
            public Data()
            {
                MusicVolume = Properties.Settings.Default.MusicVolume;
                EffectVolume = Properties.Settings.Default.EffectVolume;
            }

            public MusicPlayer music { get; set; } = new MusicPlayer();

            #region 顯示控制
            private BitmapImage _IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01");
            public BitmapImage IMG
            {
                get => _IMG;
                set
                {
                    _IMG = value;
                    NotifyPropertyChanged("IMG");
                }
            }

            private int _SelectIndex = 0;
            public int SelectIndex
            {
                get => _SelectIndex;
                set
                {
                    _SelectIndex = value;
                    NotifyPropertyChanged("MusicShow");
                    NotifyPropertyChanged("EffectShow");
                    NotifyPropertyChanged("SaveShow");
                }
            }

            public Visibility MusicShow
            {
                get
                {
                    if (SelectIndex == 0) { return Visibility.Visible; }
                    else { return Visibility.Hidden; }
                } 
            }

            public Visibility EffectShow
            {
                get
                {
                    if (SelectIndex == 1) { return Visibility.Visible; }
                    else { return Visibility.Hidden; }
                }
            }

            public Visibility SaveShow
            {
                get
                {
                    if (SelectIndex == 2) { return Visibility.Visible; }
                    else { return Visibility.Hidden; }
                }
            }
            #endregion

            #region 音量控制
            private double _MusicVolume = 1;
            public double MusicVolume
            {
                get => _MusicVolume;
                set
                {
                    _MusicVolume = value;
                    if (_MusicVolume > 1) { _MusicVolume = 1; }
                    else if (_MusicVolume < 0) { _MusicVolume = 0; }
                    music.SetMusicVolume(value);
                    NotifyPropertyChanged("MV");
                }
            }
            public int MV
            {
                get => (int)(250 * MusicVolume);
            }


            private double _EffectVolume = 1;
            public double EffectVolume
            {
                get => _EffectVolume;
                set
                {
                    _EffectVolume = value;
                    if (_EffectVolume > 1) { _EffectVolume = 1; }
                    else if (_EffectVolume < 0) { _EffectVolume = 0; }
                    music.SetEffectVolume(value);
                    NotifyPropertyChanged("EV");
                }
            }
            public int EV
            {
                get => (int)(250 * EffectVolume);
            }
            #endregion

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

            public async Task GoBright()
            {
                for (int i = 0; i < 10; i++)
                {
                    MainOpacity += 0.1;
                    await Task.Delay(20);
                }
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
