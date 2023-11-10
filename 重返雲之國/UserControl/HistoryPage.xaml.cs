using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using 重返雲之國_外傳.IMG.History;
using 重返雲之國_外傳.Models;

namespace 重返雲之國_外傳.UserControls
{
    /// <summary>
    /// HistoryPage.xaml 的互動邏輯
    /// </summary>
    public partial class HistoryPage : UserControl
    {
        private MusicPlayer music = new MusicPlayer();
        private Data data = new Data();
        public Action LookComplete;

        #region 需要用到的介面
        private HistoryIMG historyIMG;
        private HistoryAnimation animation;
        private LoadingPage loading;
        private OpeningStory opening; //0
        private FailPage fail; //3
        private FakeEnding fake; //4
        private GoodEnding good; //4
        //你以為彩蛋是可以免費看的嗎!?
        //才不會那麼容易就讓你們看到呢!
        #endregion

        private bool OnBusy { get; set; } = true;

        public HistoryPage()
        {
            InitializeComponent();
            DataContext = data;
            Loaded += HistoryPage_Loaded;
        }

        private async void HistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            PlayBackgroundMusic();
            await data.GoBright();
            OnBusy = false;
        }

        public async void KeyPress(Key key)
        {
            if (OnBusy == false)
            {
                if (key == Key.Up)
                {
                    CheckMoveable(-10, data.PositionShow - 10);
                    music.PlayShort("游標移動音效");
                }
                if (key == Key.Down)
                {
                    CheckMoveable(10, data.PositionShow + 10);
                    music.PlayShort("游標移動音效");
                }
                if (key == Key.Left)
                {
                    CheckMoveable(-1, data.PositionShow - 1);
                    music.PlayShort("游標移動音效");
                }
                if (key == Key.Right)
                {
                    CheckMoveable(1, data.PositionShow + 1);
                    music.PlayShort("游標移動音效");
                }
                if (key == Key.Enter)
                {
                    if (CheckIsIMG()) { ShowIMG(); }
                    else { ShowAnime(); }
                }
                if (key == Key.Back)
                {
                    OnBusy = true;
                    StopBackgroundMusic();
                    music.PlayShort("游標移動音效");
                    await data.GoDark();
                    LookComplete?.Invoke();
                }
            }
            else
            {
                if (key == Key.Up)
                {
                    fail?.KeyPress(key);
                }
                if (key == Key.Down)
                {
                    fail?.KeyPress(key);
                }
                if (key == Key.Back)
                {
                    music.PlayShort("游標移動音效");
                    historyIMG?.KeyPress(key);
                }
                if (key == Key.Enter)
                {
                    animation?.KeyPress(key);
                    opening?.KeyPress(key);
                    fail?.KeyPress(key);
                    fake?.KeyPress(key);
                    good?.KeyPress(key);
                }
                if (key == Key.Space)
                {
                    animation?.KeyPress(key);
                    opening?.KeyPress(key);
                    fail?.KeyPress(key);
                    fake?.KeyPress(key);
                    good?.KeyPress(key);
                }
            }
        }

        private void CheckMoveable(int P, int MoveTo)
        {
            //在第一層上方
            if (MoveTo == -10) { CheckMoveable(P, 30); } //外
            else if (MoveTo == -9) { CheckMoveable(P, 31); } //外
            else if (MoveTo == -8) { CheckMoveable(P, 32); } //外
            else if (MoveTo == -7) { CheckMoveable(P, 33); } //外
            else if (MoveTo == -6) { CheckMoveable(P, 34); } //外
            //往第一層
            else if (MoveTo == -1) { CheckMoveable(P, 4); } //外
            else if (MoveTo == 0) { data.PositionShow = 0; } //起點
            else if (MoveTo == 1) { if (data.Ending01) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 2) { if (data.Ending02) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 3) { if (data.Ending03) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 4) { if (data.Ending04) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 5) { CheckMoveable(P, 0); } //外
            //往第二層
            else if (MoveTo == 9) { CheckMoveable(P, 13); } //外
            else if (MoveTo == 10) { if (data.Ending02) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 11) { if (data.Ending02) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 12) { if (data.Ending02) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 13) { if (data.Ending02) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 14) { CheckMoveable(P, MoveTo + P); }
            else if (MoveTo == 15) { CheckMoveable(P, 13); } //外
            //往第三層
            else if (MoveTo == 19) { CheckMoveable(P, 24); } //外
            else if (MoveTo == 20) { if (data.Ending03) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 21) { if (data.Ending03) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 22) { if (data.Ending03) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 23) { if (data.Ending03) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 24) { if (data.Ending03) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 25) { CheckMoveable(P, 20); } //外
            //往第四層
            else if (MoveTo == 29) { CheckMoveable(P, 34); } //外
            else if (MoveTo == 30) { if (data.Ending04) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 31) { if (data.Ending04) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 32) { if (data.Ending04) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 33) { if (data.Ending04) { data.PositionShow = MoveTo; } else { CheckMoveable(P, MoveTo + P); } }
            else if (MoveTo == 34) { CheckMoveable(P, MoveTo + P); }
            else if (MoveTo == 35) { CheckMoveable(P, 30); } //外
            //往第四層外
            else if (MoveTo == 40) { CheckMoveable(P, 0); } //外
            else if (MoveTo == 41) { CheckMoveable(P, 1); } //外
            else if (MoveTo == 42) { CheckMoveable(P, 2); } //外
            else if (MoveTo == 43) { CheckMoveable(P, 3); } //外
            else if (MoveTo == 44) { CheckMoveable(P, 4); } //外
        }

        private bool CheckIsIMG()
        {
            if(data.PositionShow == 11 || data.PositionShow == 12 || data.PositionShow == 13
            || data.PositionShow == 21 || data.PositionShow == 22 || data.PositionShow == 23
            || data.PositionShow == 31 || data.PositionShow == 32)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ShowIMG()
        {
            if (historyIMG == null)
            {
                OnBusy = true;
                data.MaskShow = Visibility.Visible;
                historyIMG = new HistoryIMG(data.PositionShow);
                historyIMG.LookComplete += () =>
                {
                    MainGrid.Children.Remove(historyIMG);
                    historyIMG = null;
                    data.MaskShow = Visibility.Hidden;
                    OnBusy = false;
                };
                MainGrid.Children.Add(historyIMG);
            }
        }

        private void ShowAnime()
        {
            OnBusy = true;
            StopBackgroundMusic();

            if (data.PositionShow == 10) //01開頭
            {
                animation = new HistoryAnimation("開頭音樂", 0, 0, 6);
                animation.OpeningFinished += () =>
                {
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(animation);
                    OnBusy = false;
                    animation = null;
                };
                MainGrid.Children.Add(animation);
            }
            else if (data.PositionShow == 20) //02開頭
            {
                animation = new HistoryAnimation("開頭音樂", 1, 0, 8);
                animation.OpeningFinished += () =>
                {
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(animation);
                    OnBusy = false;
                    animation = null;
                };
                MainGrid.Children.Add(animation);
            }
            else if (data.PositionShow == 24) //02結尾
            {
                animation = new HistoryAnimation("其他音樂", 1, 10, 16);
                animation.OpeningFinished += () =>
                {
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(animation);
                    OnBusy = false;
                    animation = null;
                };
                MainGrid.Children.Add(animation);
            }
            else if (data.PositionShow == 30) //03開頭
            {
                animation = new HistoryAnimation("開頭音樂", 2, 0, 12);
                animation.OpeningFinished += () =>
                {
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(animation);
                    OnBusy = false;
                    animation = null;
                };
                MainGrid.Children.Add(animation);
            }
            else if (data.PositionShow == 33) //03結尾
            {
                animation = new HistoryAnimation("其他音樂", 2, 20, 30);
                animation.OpeningFinished += () =>
                {
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(animation);
                    OnBusy = false;
                    animation = null;
                };
                MainGrid.Children.Add(animation);
            }
            //開頭動畫
            else if (data.PositionShow == 0)
            {
                CreateLoadingPage(1);
                loading.LoadingComplete += () =>
                {
                    MainGrid.Children.Remove(loading);
                    CreateOpeningStory();
                    loading = null;
                };
            }
            //失敗
            else if (data.PositionShow == 1)
            {
                CreateLoadingPage(3);
                loading.LoadingComplete += () =>
                {
                    MainGrid.Children.Remove(loading);
                    CreateFailPage();
                    loading = null;
                };
            }
            else if (data.PositionShow == 2) //假結局01
            {
                CreateLoadingPage(4);
                loading.LoadingComplete += () =>
                {
                    MainGrid.Children.Remove(loading);
                    CreateFakeEnding(Entities.Difficulty.Easy);
                    loading = null;
                };
            }
            else if (data.PositionShow == 3) //假結局02
            {
                CreateLoadingPage(4);
                loading.LoadingComplete += () =>
                {
                    MainGrid.Children.Remove(loading);
                    CreateFakeEnding(Entities.Difficulty.Middle);
                    loading = null;
                };
            }
            else if (data.PositionShow == 4) //真結局
            {
                CreateLoadingPage(5);
                loading.LoadingComplete += () =>
                {
                    CreateGoodEnding();
                    MainGrid.Children.Remove(loading);
                    loading = null;
                };
            }
        }

        private void CreateLoadingPage(int GameStep)
        {
            data.MainOpacity = 0;
            loading = new LoadingPage(GameStep);
            MainGrid.Children.Add(loading);
        }

        //1.開頭故事
        private async void CreateOpeningStory()
        {
            if (opening == null)
            {
                await Task.Delay(1000);
                opening = new OpeningStory();
                opening.OpeningFinished += () =>
                {
                    data.MainOpacity = 1;
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(opening);
                    OnBusy = false;
                    opening = null;
                };
                MainGrid.Children.Add(opening);
            }
        }

        //3.遊戲失敗
        private async void CreateFailPage()
        {
            if (fail == null)
            {
                await Task.Delay(1000);
                fail = new FailPage(new PlayerStatus());
                fail.Restart += () =>
                {
                    data.MainOpacity = 1;
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(fail);
                    OnBusy = false;
                    fail = null;
                };
                fail.GameOver += () =>
                {
                    MainGrid.Children.Remove(fail);
                    PlayBackgroundMusic();
                    fail = null;
                    OnBusy = false;
                };
                MainGrid.Children.Add(fail);
            }
        }

        //4.假結局
        private async void CreateFakeEnding(Entities.Difficulty difficulty)
        {
            if (fake == null)
            {
                await Task.Delay(1000);
                fake = new FakeEnding(difficulty);
                fake.AnimationFinished += () =>
                {
                    data.MainOpacity = 1;
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(fake);
                    OnBusy = false;
                    fake = null;
                };
                MainGrid.Children.Add(fake);
            }
        }

        //5.真結局
        private async void CreateGoodEnding()
        {
            if (good == null)
            {
                await Task.Delay(1000);
                good = new GoodEnding();
                good.AnimationFinished += () =>
                {
                    data.MainOpacity = 1;
                    PlayBackgroundMusic();
                    MainGrid.Children.Remove(good);
                    OnBusy = false;
                    good = null;
                };
                MainGrid.Children.Add(good);
            }
        }

        private void PlayBackgroundMusic()
        {
            music.PlayMusic("其他音樂");
        }

        private void StopBackgroundMusic()
        {
            music?.StopMusic();
        }

        private class Data : MVVM
        {
            #region 讀取存檔紀錄
            public Data()
            {
                Ending01 = Properties.Settings.Default.Ending01;
                Ending02 = Properties.Settings.Default.Ending02;
                Ending03 = Properties.Settings.Default.Ending03;
                Ending04 = Properties.Settings.Default.Ending04;
            }

            private bool _Ending01 = false;
            public bool Ending01
            {
                get => _Ending01;
                set
                {
                    _Ending01 = value;
                    if (_Ending01) { Story01Show = Visibility.Visible; }
                }
            }

            private bool _Ending02 = false;
            public bool Ending02
            {
                get => _Ending02;
                set
                {
                    _Ending02 = value;
                    if (_Ending02) { Story02Show = Visibility.Visible; }
                }
            }

            private bool _Ending03 = false;
            public bool Ending03
            {
                get => _Ending03;
                set
                {
                    _Ending03 = value;
                    if (_Ending03) { Story03Show = Visibility.Visible; }
                }
            }

            private bool _Ending04 = false;
            public bool Ending04
            {
                get => _Ending04;
                set
                {
                    _Ending04 = value;
                    if (_Ending04) { Story04Show = Visibility.Visible; }
                }
            }
            #endregion

            #region 游標位子
            private int _PositionShow = 0;
            public int PositionShow
            {
                get => _PositionShow;
                set
                {
                    _PositionShow = value;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            NotifyPropertyChanged("Position" + i + j);
                        }
                    }
                }
            }

            public Thickness Position00
            {
                get
                {
                    if(PositionShow == 0) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position01
            {
                get
                {
                    if (PositionShow == 1) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position02
            {
                get
                {
                    if (PositionShow == 2) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position03
            {
                get
                {
                    if (PositionShow == 3) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position04
            {
                get
                {
                    if (PositionShow == 4) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position10
            {
                get
                {
                    if (PositionShow == 10) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position11
            {
                get
                {
                    if (PositionShow == 11) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position12
            {
                get
                {
                    if (PositionShow == 12) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position13
            {
                get
                {
                    if (PositionShow == 13) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position20
            {
                get
                {
                    if (PositionShow == 20) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position21
            {
                get
                {
                    if (PositionShow == 21) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position22
            {
                get
                {
                    if (PositionShow == 22) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position23
            {
                get
                {
                    if (PositionShow == 23) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position24
            {
                get
                {
                    if (PositionShow == 24) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position30
            {
                get
                {
                    if (PositionShow == 30) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position31
            {
                get
                {
                    if (PositionShow == 31) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position32
            {
                get
                {
                    if (PositionShow == 32) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }

            public Thickness Position33
            {
                get
                {
                    if (PositionShow == 33) { return new Thickness(2); }
                    else { return new Thickness(0); }
                }
            }
            #endregion

            #region 故事顯示
            private Visibility _Story01Show = Visibility.Hidden;
            public Visibility Story01Show
            {
                get => _Story01Show;
                set
                {
                    _Story01Show = value;
                    NotifyPropertyChanged("Story01Show");
                }
            }

            private Visibility _Story02Show = Visibility.Hidden;
            public Visibility Story02Show
            {
                get => _Story02Show;
                set
                {
                    _Story02Show = value;
                    NotifyPropertyChanged("Story02Show");
                }
            }

            private Visibility _Story03Show = Visibility.Hidden;
            public Visibility Story03Show
            {
                get => _Story03Show;
                set
                {
                    _Story03Show = value;
                    NotifyPropertyChanged("Story03Show");
                }
            }

            private Visibility _Story04Show = Visibility.Hidden;
            public Visibility Story04Show
            {
                get => _Story04Show;
                set
                {
                    _Story04Show = value;
                    NotifyPropertyChanged("Story04Show");
                }
            }
            #endregion

            private Visibility _MaskShow = Visibility.Hidden;
            public Visibility MaskShow
            {
                get => _MaskShow;
                set
                {
                    _MaskShow = value;
                    NotifyPropertyChanged("MaskShow");
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

            public async Task GoBright()
            {
                for (int i = 0;i < 10; i++)
                {
                    MainOpacity += 0.1;
                    await Task.Delay(50);
                }
            }

            public async Task GoDark()
            {
                for (int i = 0; i < 10; i++)
                {
                    MainOpacity -= 0.1;
                    await Task.Delay(50);
                }
            }
        }
    }
}
