using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using 重返雲之國_外傳.Models;
using static 重返雲之國_外傳.Models.Entities;

namespace 重返雲之國_外傳.UserControls
{
    /// <summary>
    /// OptionPage.xaml 的互動邏輯
    /// </summary>
    public partial class OptionPage : UserControl
    {
        private MusicPlayer music = new MusicPlayer();
        private Data data;
        private double ShiningToken { get; set; }
        private bool UnSelect { get; set; } = true;

        public delegate void Event(Difficulty difficulty);
        public event Event GameStart;
        public event Action History;
        public event Action Setting;
        public event Action Directions;
        public event Action OpeningStory;

        public OptionPage()
        {
            InitializeComponent();
            data = new Data();
            DataContext = data;
            data.SelectStep = 0;
            Loaded += OptionPage_Loaded;
        }

        private async void OptionPage_Loaded(object sender, RoutedEventArgs e)
        {
            music.PlayMusic("選單音樂");
            TimeDown();

            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(20);
                data.BigTitle += 34;
                data.SmallTitle -= 27.5;
            }

            int d = 1;
            do
            {
                if (UnSelect)
                {
                    if (data.SelectStep == 0) { data.EnterTextOpacity = ShiningToken; }
                    else { data.EnterTextOpacity = 1; }

                    if (data.SelectStep == 11) { data.StartGameOpacity = ShiningToken; }
                    else { data.StartGameOpacity = 1; }

                    if (data.SelectStep == 12) { data.HistoryOpacity = ShiningToken; }
                    else { data.HistoryOpacity = 1; }

                    if (data.SelectStep == 13) { data.SettingOpacity = ShiningToken; }
                    else { data.SettingOpacity = 1; }

                    if (data.SelectStep == 14) { data.DirectionsOpacity = ShiningToken; }
                    else { data.DirectionsOpacity = 1; }

                    if (data.SelectStep == 21) { data.EasyOpacity = ShiningToken; }
                    else { data.EasyOpacity = 1; }

                    if (data.SelectStep == 22) { data.NormalOpacity = ShiningToken; }
                    else
                    {
                        if (Properties.Settings.Default.Ending02) { data.NormalOpacity = 1; }
                        else { data.NormalOpacity = 0; }
                    }

                    if (data.SelectStep == 23) { data.HardOpacity = ShiningToken; }
                    else
                    {
                        if (Properties.Settings.Default.Ending03) { data.HardOpacity = 1; }
                        else { data.HardOpacity = 0; }
                    }
                }

                if (d == 34) { d = 0; }
                if (d == 0) { ShiningToken = 1; } else if (d == 17) { ShiningToken = 0; }
                d++;
                await Task.Delay(20);
            }
            while (true);
        }

        public void KeyPress(Key key)
        {
            if (UnSelect)
            {
                //正在等待按下Enter
                if (data.SelectStep == 0)
                {
                    if (key == Key.Enter)
                    {
                        data.SelectStep = 11;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                }
                //開始遊戲
                else if (data.SelectStep == 11)
                {
                    if (key == Key.Down)
                    {
                        data.SelectStep = 12;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 0;
                        ShiningToken = 1;
                        TimeDown();
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        data.SelectStep = 21;
                        ShiningToken = 1;
                        music.PlayShort("悠白能力提升音效");
                    } 
                }
                //歷史回顧
                else if (data.SelectStep == 12)
                {
                    if (key == Key.Down)
                    {
                        data.SelectStep = 13;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Up)
                    {
                        data.SelectStep = 11;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 0;
                        ShiningToken = 1;
                        TimeDown();
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        ShiningToken = 1;
                        music.PlayShort("悠白能力提升音效");
                        OtherSelected();
                    }
                }
                //系統調整
                else if (data.SelectStep == 13)
                {
                    if (key == Key.Down)
                    {
                        data.SelectStep = 14;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Up)
                    {
                        data.SelectStep = 12;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 0;
                        ShiningToken = 1;
                        TimeDown();
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        ShiningToken = 1;
                        music.PlayShort("悠白能力提升音效");
                        OtherSelected();
                    }
                }
                //操作說明
                else if (data.SelectStep == 14)
                {
                    if (key == Key.Up)
                    {
                        data.SelectStep = 13;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 0;
                        ShiningToken = 1;
                        TimeDown();
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        ShiningToken = 1;
                        music.PlayShort("悠白能力提升音效");
                        OtherSelected();
                    }
                }
                //開始遊戲 -> 選擇難度
                //難度簡單
                else if (data.SelectStep == 21) //簡單
                {
                    if (key == Key.Down)
                    {
                        if (Properties.Settings.Default.Ending02)
                        {
                            data.SelectStep = 22;
                            ShiningToken = 1;
                            SelectionMedia("游標移動音效");
                        }
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 11;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        ShiningToken = 1;
                        DifficultySelected(Difficulty.Easy);
                    }
                }
                //難度普通
                else if (data.SelectStep == 22) //普通
                {
                    if (key == Key.Up)
                    {
                        data.SelectStep = 21;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Down)
                    {
                        if (Properties.Settings.Default.Ending03)
                        {
                            data.SelectStep = 23;
                            ShiningToken = 1;
                            SelectionMedia("游標移動音效");
                        }
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 11;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        ShiningToken = 1;
                        DifficultySelected(Difficulty.Middle);
                    }
                }
                //難度困難
                else if (data.SelectStep == 23) //困難
                {
                    if (key == Key.Up)
                    {
                        data.SelectStep = 22;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Back)
                    {
                        data.SelectStep = 11;
                        ShiningToken = 1;
                        SelectionMedia("游標移動音效");
                    }
                    if (key == Key.Enter)
                    {
                        ShiningToken = 1;
                        DifficultySelected(Difficulty.Hard);
                    }
                }
            }
        }

        private async void DifficultySelected(Difficulty difficulty)
        {
            UnSelect = false;
            SelectionMedia("進入遊戲音效");
            await OpacityFlashing();
            await Task.Delay(3000);
            music.StopMusic();
            GameStart?.Invoke(difficulty);
            UnSelect = true;
        }

        private async void OtherSelected()
        {
            UnSelect = false;
            await OpacityFlashing();
            if (data.SelectStep == 12)
            {
                History?.Invoke();
            }
            else if (data.SelectStep == 13)
            {
                Setting?.Invoke();
            }
            else if (data.SelectStep == 14)
            {
                Directions?.Invoke();
            }

            music.StopMusic();
            UnSelect = true;
        }

        private async Task OpacityFlashing()
        {
            for (int i = 1; i < 10; i++)
            {
                if(data.SelectStep == 12) { data.HistoryOpacity = i % 2; }
                else if (data.SelectStep == 13) { data.SettingOpacity = i % 2; }
                else if (data.SelectStep == 14) { data.DirectionsOpacity = i % 2; }
                else if (data.SelectStep == 21) { data.EasyOpacity = i % 2; }
                else if (data.SelectStep == 22) { data.NormalOpacity = i % 2; }
                else if (data.SelectStep == 23) { data.HardOpacity = i % 2; }
                await Task.Delay(100);
            }
        }

        private async void TimeDown()
        {
            int d = 0;
            do
            {
                d++;
                await Task.Delay(20);
                if(d == 5150)
                {
                    music.StopMusic();
                    OpeningStory?.Invoke();
                }
            }
            while (data.SelectStep == 0);
        }

        private void SelectionMedia(string ResourcesName)
        {
            music.PlayShort(ResourcesName);
        }

        private class Data : MVVM
        {
            private int _SelectStep = -1;
            public int SelectStep
            {
                get => _SelectStep;
                set
                {
                    _SelectStep = value;
                    if (_SelectStep == 0) { EnterTextVisibility = Visibility.Visible; }
                    else { EnterTextVisibility = Visibility.Hidden; }

                    if (_SelectStep == 11 || _SelectStep == 12 || _SelectStep == 13 || _SelectStep == 14) { tenX_Visibility = Visibility.Visible; }
                    else { tenX_Visibility = Visibility.Hidden; }

                    if (_SelectStep == 11) { StartGameVisibility = Visibility.Visible; }
                    else { StartGameVisibility = Visibility.Hidden; }

                    if (_SelectStep == 12) { HistoryVisibility = Visibility.Visible; }
                    else { HistoryVisibility = Visibility.Hidden; }

                    if (_SelectStep == 13) { SettingVisibility = Visibility.Visible; }
                    else { SettingVisibility = Visibility.Hidden; }

                    if (_SelectStep == 14) { DirectionsVisibility = Visibility.Visible; }
                    else { DirectionsVisibility = Visibility.Hidden; }

                    if (_SelectStep == 21 || _SelectStep == 22 || _SelectStep == 23) { twentyX_Visibility = Visibility.Visible; }
                    else { twentyX_Visibility = Visibility.Hidden; }

                    if (_SelectStep == 21) { EasyVisibility = Visibility.Visible; }
                    else { EasyVisibility = Visibility.Hidden; }

                    if (_SelectStep == 22 && Properties.Settings.Default.Ending02) { NormlVisibility = Visibility.Visible; }
                    else { NormlVisibility = Visibility.Hidden; }

                    if (_SelectStep == 23 && Properties.Settings.Default.Ending03) { HardVisibility = Visibility.Visible; }
                    else { HardVisibility = Visibility.Hidden; }
                }
            }

            //-550 -> 130 差 680
            private double _BigTitle = -550;
            public double BigTitle
            {
                get => _BigTitle;
                set
                {
                    _BigTitle = value;
                    if (_BigTitle >= 130) { _BigTitle = 130; }
                    else if (_BigTitle <= -550) { _BigTitle = -550; }
                    NotifyPropertyChanged("BigTitle");
                }
            }


            //800 -> 250 差 550
            private double _SmallTitle = 800;
            public double SmallTitle
            {
                get => _SmallTitle;
                set
                {
                    _SmallTitle = value;
                    NotifyPropertyChanged("SmallTitle");
                }
            }

            #region SelectStep = 0
            private Visibility _EnterTextVisibility = Visibility.Hidden;
            public Visibility EnterTextVisibility
            {
                get => _EnterTextVisibility;
                set
                {
                    _EnterTextVisibility = value;
                    NotifyPropertyChanged("EnterTextVisibility");
                }
            }

            private double _EnterTextOpacity = 1;
            public double EnterTextOpacity
            {
                get => _EnterTextOpacity;
                set
                {
                    _EnterTextOpacity = value;
                    NotifyPropertyChanged("EnterTextOpacity");
                }
            }
            #endregion

            #region SelectStep = 1X
            private Visibility _tenX_Visibility = Visibility.Hidden;
            public Visibility tenX_Visibility
            {
                get => _tenX_Visibility;
                set
                {
                    _tenX_Visibility = value;
                    NotifyPropertyChanged("tenX_Visibility");
                }
            }
            #endregion

            #region SelectStep = 11
            private Visibility _StartGameVisibility = Visibility.Hidden;
            public Visibility StartGameVisibility
            {
                get => _StartGameVisibility;
                set
                {
                    _StartGameVisibility = value;
                    NotifyPropertyChanged("StartGameVisibility");
                }
            }

            private double _StartGameOpacity = 1;
            public double StartGameOpacity
            {
                get=> _StartGameOpacity;
                set
                {
                    _StartGameOpacity = value;
                    NotifyPropertyChanged("StartGameOpacity");
                }
            }
            #endregion

            #region SelectStep = 12
            private Visibility _HistoryVisibility = Visibility.Hidden;
            public Visibility HistoryVisibility
            {
                get => _HistoryVisibility;
                set
                {
                    _HistoryVisibility = value;
                    NotifyPropertyChanged("HistoryVisibility");
                }
            }

            private double _HistoryOpacity = 1;
            public double HistoryOpacity
            {
                get => _HistoryOpacity;
                set
                {
                    _HistoryOpacity = value;
                    NotifyPropertyChanged("HistoryOpacity");
                }
            }
            #endregion

            #region SelectStep = 13
            private Visibility _SettingVisibility = Visibility.Hidden;
            public Visibility SettingVisibility
            {
                get => _SettingVisibility;
                set
                {
                    _SettingVisibility = value;
                    NotifyPropertyChanged("SettingVisibility");
                }
            }

            private double _SettingOpacity = 1;
            public double SettingOpacity
            {
                get => _SettingOpacity;
                set
                {
                    _SettingOpacity = value;
                    NotifyPropertyChanged("SettingOpacity");
                }
            }
            #endregion

            #region SelectStep = 14
            private Visibility _DirectionsVisibility = Visibility.Hidden;
            public Visibility DirectionsVisibility
            {
                get => _DirectionsVisibility;
                set
                {
                    _DirectionsVisibility = value;
                    NotifyPropertyChanged("DirectionsVisibility");
                }
            }

            private double _DirectionsOpacity = 1;
            public double DirectionsOpacity
            {
                get => _DirectionsOpacity;
                set
                {
                    _DirectionsOpacity = value;
                    NotifyPropertyChanged("DirectionsOpacity");
                }
            }
            #endregion

            #region SelectStep = 2X
            private Visibility _twentyX_Visibility = Visibility.Hidden;
            public Visibility twentyX_Visibility
            {
                get => _twentyX_Visibility;
                set
                {
                    _twentyX_Visibility = value;
                    NotifyPropertyChanged("twentyX_Visibility");
                }
            }
            #endregion

            #region SelectStep = 21
            private Visibility _EasyVisibility = Visibility.Hidden;
            public Visibility EasyVisibility
            {
                get => _EasyVisibility;
                set
                {
                    _EasyVisibility = value;
                    NotifyPropertyChanged("EasyVisibility");
                }
            }

            private double _EasyOpacity = 1;
            public double EasyOpacity
            {
                get => _EasyOpacity;
                set
                {
                    _EasyOpacity = value;
                    NotifyPropertyChanged("EasyOpacity");
                }
            }
            #endregion

            #region SelectStep = 22
            private Visibility _NormlVisibility = Visibility.Hidden;
            public Visibility NormlVisibility
            {
                get => _NormlVisibility;
                set
                {
                    _NormlVisibility = value;
                    NotifyPropertyChanged("NormlVisibility");
                }
            }

            private double _NormalOpacity = 1;
            public double NormalOpacity
            {
                get => _NormalOpacity;
                set
                {
                    _NormalOpacity = value;
                    NotifyPropertyChanged("NormalOpacity");
                }
            }
            #endregion

            #region SelectStep = 23
            private Visibility _HardVisibility = Visibility.Hidden;
            public Visibility HardVisibility
            {
                get => _HardVisibility;
                set
                {
                    _HardVisibility = value;
                    NotifyPropertyChanged("HardVisibility");
                }
            }

            private double _HardOpacity = 1;
            public double HardOpacity
            {
                get => _HardOpacity;
                set
                {
                    _HardOpacity = value;
                    NotifyPropertyChanged("HardOpacity");
                }
            }
            #endregion
        }
    }
}
