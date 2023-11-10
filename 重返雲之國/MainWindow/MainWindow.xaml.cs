using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using 重返雲之國_外傳.Models;
using 重返雲之國_外傳.UserControls;
using 重返雲之國_外傳.IMG.Status;
using static 重返雲之國_外傳.Models.Entities;
using System.Runtime.CompilerServices;
using System;
using 重返雲之國_外傳.Properties;
using System.IO;
using System.Windows.Markup;

namespace 重返雲之國_外傳
{
    /// <summary>
    /// 
    /// 0,開頭提要
    /// 1.開頭故事/選項介面
    /// 2.開始遊戲/重新開始
    /// 3.遊戲失敗
    /// 4.假結尾
    /// 5.真結尾
    /// 6.其他介面
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        private int GameStep { get; set; } = 0;
        private bool OnPause { get; set; } = false;

        private List<Key> OnPress = new List<Key>(); //過濾鍵盤訊號避免連發

        #region 所有介面
        private PlayerStatus player;
        private LoadingPage loading;
        private BeginCoution begin; //0
        private OpeningStory opening; //0
        private OptionPage option; //1
        private StartGame startGame; //2
        private StatusUp statusUp; //2
        private FailPage fail; //3
        private FakeEnding fake; //4
        private GoodEnding good; //4
        private HistoryPage history; //6
        private SettingPage setting; //6
        private DirectionsPage direction; //6
        private EasterEgg easter;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateNewGame();
            //CreateClearGame();

            if (Settings.Default.Ending00) { CreateBeginCoution(); }
            else
            {
                GameStep = 6;
                CreateLoadingPage();
                loading.LoadingComplete += () =>
                {
                    MainGrid.Children.Remove(opening);
                    direction = new DirectionsPage();
                    direction.LookComplete += () =>
                    {
                        GameStep = 0;
                        CreateBeginCoution();
                        MainGrid.Children.Remove(direction);
                        direction = null;
                    };
                    MainGrid.Children.Add(direction);
                    opening = null;
                };
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (GameStep == 0)
            {
                if (!OnPress.Contains(e.Key))
                {
                    OnPress.Add(e.Key);
                    begin?.KeyPress(e.Key);
                }
            }
            else if (GameStep == 1)
            {
                if (!OnPress.Contains(e.Key))
                {
                    OnPress.Add(e.Key);
                    opening?.KeyPress(e.Key);
                    option?.KeyPress(e.Key);
                }
            }
            else if (GameStep == 2)
            {
                if(e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Down)
                {
                    startGame?.KeyPress(e.Key);
                    statusUp?.KeyPress(e.Key);
                }
                else
                {
                    if (!OnPress.Contains(e.Key))
                    {
                        OnPress.Add(e.Key);
                        startGame?.KeyPress(e.Key);
                        statusUp?.KeyPress(e.Key);
                    }
                }
            }
            else if (GameStep == 3)
            {
                if (!OnPress.Contains(e.Key))
                {
                    OnPress.Add(e.Key);
                    fail?.KeyPress(e.Key);
                }
            }
            else if (GameStep == 4)
            {
                if (!OnPress.Contains(e.Key))
                {
                    OnPress.Add(e.Key);
                    fake?.KeyPress(e.Key);
                }
            }
            else if (GameStep == 5)
            {
                if (!OnPress.Contains(e.Key))
                {
                    OnPress.Add(e.Key);
                    easter?.KeyPress(e.Key);
                    good?.KeyPress(e.Key);
                }
            }
            else if (GameStep == 6)
            {
                if (!OnPress.Contains(e.Key))
                {
                    OnPress.Add(e.Key);
                    history?.KeyPress(e.Key);
                    setting?.KeyPress(e.Key);
                    direction?.KeyPress(e.Key);
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (OnPress.Contains(e.Key)) { OnPress.Remove(e.Key); }

            if (GameStep == 2)
            {
                startGame?.KeyRelese(e.Key);
            }
        }

        //0.開頭提要
        private void CreateBeginCoution()
        {
            CreateLoadingPage();
            loading.LoadingComplete += () =>
            {
                begin = new BeginCoution();
                begin.ReadComplete += () =>
                {
                    GameStep = 1;
                    CreateOpeningStory();
                    MainGrid.Children.Remove(begin);
                    begin = null;
                };
                MainGrid.Children.Add(begin);
                MainGrid.Children.Remove(loading);
                loading = null;
            };
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
                    CreateOptionPage();
                    MainGrid.Children.Remove(opening);
                    opening = null;
                };
                MainGrid.Children.Add(opening);
            }
        }

        //1.選項介面
        private async void CreateOptionPage()
        {
            if (option == null)
            {
                await Task.Delay(1000);
                option = new OptionPage();
                option.GameStart += (difficulty) =>
                {
                    GameStep = 2;
                    player = new PlayerStatus
                    {
                        difficulty = difficulty
                    };
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(opening);
                        CreateStartGame();
                        opening = null;
                    };
                    MainGrid.Children.Remove(option);
                    option = null;
                };
                option.History += () =>
                {
                    GameStep = 6;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(opening);
                        CreateHistoryPage();
                        opening = null;
                    };
                    MainGrid.Children.Remove(option);
                    option = null;
                };
                option.Setting += () =>
                {
                    GameStep = 6;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(opening);
                        CreateSettingPage();
                        opening = null;
                    };
                    MainGrid.Children.Remove(option);
                    option = null;
                };
                option.Directions += () =>
                {
                    GameStep = 6;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(opening);
                        CreateDirectionsPage();
                        opening = null;
                    };
                    MainGrid.Children.Remove(option);
                    option = null;
                };
                option.OpeningStory += () =>
                {
                    CreateOpeningStory();
                    MainGrid.Children.Remove(option);
                    option = null;
                };
                MainGrid.Children.Add(option);
            }
        }

        //2.開始遊戲
        private async void CreateStartGame()
        {
            if (startGame == null)
            {
                await Task.Delay(1000);
                startGame = new StartGame(player);
                startGame.StatusUpShow += () =>
                {
                    GrayMask.Visibility = Visibility.Visible;
                    CreateStatusUp();
                };
                startGame.UsePause += (bool isPause) =>
                {
                    OnPause = isPause;
                    if (OnPause)
                    {
                        if (player.ContinueTime == -1) { RemainLife.Text = "剩餘接關數量: 無限次"; }
                        else { RemainLife.Text = "剩餘接關數量: " + player.ContinueTime + "次"; }

                        GameRules.Visibility = Visibility.Visible;
                        PressEnter.Visibility = Visibility.Visible;
                        GrayMask.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GameRules.Visibility = Visibility.Hidden;
                        PressEnter.Visibility = Visibility.Hidden;
                        GrayMask.Visibility = Visibility.Hidden;
                    }
                };
                startGame.GameWin += (bool isWin) =>
                {
                    //true = 贏了
                    if (isWin)
                    {
                        //假劇情
                        if (player.difficulty == Difficulty.Easy || player.difficulty == Difficulty.Middle)
                        {
                            GameStep = 4;
                            CreateLoadingPage();
                            loading.LoadingComplete += () =>
                            {
                                MainGrid.Children.Remove(loading);
                                CreateFakeEnding();
                                loading = null;
                            };
                        }
                        //真劇情
                        else
                        {
                            GameStep = 5;
                            CreateLoadingPage();
                            loading.LoadingComplete += () =>
                            {
                                MainGrid.Children.Remove(loading);
                                CreateGoodEnding();
                                loading = null;
                            };
                        }
                    }
                    //false = 輸了
                    else
                    {
                        GameStep = 3;
                        CreateLoadingPage();
                        loading.LoadingComplete += () =>
                        {
                            MainGrid.Children.Remove(loading);
                            CreateFailPage();
                            loading = null;
                        };
                    }
                    MainGrid.Children.Remove(startGame);
                    startGame = null;
                };
                MainGrid.Children.Add(startGame);
            }         
        }

        //2.遊戲升等能力提升
        private void CreateStatusUp()
        {
            startGame.statusUp = true;
            statusUp = new StatusUp(player);
            statusUp.Complete += () =>
            {
                if(player.PlayerExp >= player.PlayerMaxExp)
                {
                    player.Level++;
                    player.PlayerExp -= player.PlayerMaxExp;
                    player.PlayerMaxExp *= 1.1;
                }
                OtherGrid.Children.Remove(statusUp);
                statusUp = null;
                if (player.PlayerExp >= player.PlayerMaxExp)
                {
                    CreateStatusUp();
                }
                else
                {
                    startGame.statusUp = false;
                    GrayMask.Visibility = Visibility.Hidden;
                }
            };
            statusUp.IsFull += () =>
            {
                while (player.PlayerExp >= player.PlayerMaxExp)
                {
                    player.Level++;
                    player.PlayerExp -= player.PlayerMaxExp;
                    player.PlayerMaxExp *= 1.1;
                    if (player.ContinueTime != -1)
                    {
                        player.ContinueTime++;
                    }
                }
            };
            OtherGrid.Children.Add(statusUp);
        }

        //3.遊戲失敗
        private async void CreateFailPage()
        {
            if (fail == null)
            {
                await Task.Delay(1000);
                player.PlayerHP = player.PlayerMaxHP;
                player.PlayerAP = player.PlayerMaxAP;
                player.IsAlive = true;
                player.OnAvoid = false;
                player.OnDamage = false;
                player.StatusUp = false;

                fail = new FailPage(player);
                fail.Restart += () =>
                {
                    GameStep = 2;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateStartGame();
                        loading = null;
                    };
                    MainGrid.Children.Remove(fail);
                    fail = null;
                };
                fail.GameOver += () =>
                {
                    player = null;
                    GameStep = 1;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateOptionPage();
                        loading = null;
                    };
                    MainGrid.Children.Remove(fail);
                    fail = null;
                };
                MainGrid.Children.Add(fail);
            }
        }

        //4.假結局
        private async void CreateFakeEnding()
        {
            if (fake == null)
            {
                await Task.Delay(1000);
                fake = new FakeEnding(player.difficulty);
                fake.AnimationFinished += () =>
                {
                    GameStep = 1;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateOptionPage();
                        loading = null;
                    };
                    MainGrid.Children.Remove(fake);
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
                //DebugLog(player.difficulty + "破關");
                await Task.Delay(1000);
                good = new GoodEnding();
                good.AnimationFinished += () =>
                {
                    CreateEasterEgg();
                    MainGrid.Children.Remove(good);
                    good = null;
                };
                MainGrid.Children.Add(good);
            }
        }
        
        //5.隱藏彩蛋
        private async void CreateEasterEgg()
        {
            if (easter == null)
            {
                await Task.Delay(2000);
                easter = new EasterEgg();
                easter.AnimationFinished += () =>
                {
                    GameStep = 1;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateOptionPage();
                        loading = null;
                    };
                    MainGrid.Children.Remove(easter);
                    easter = null;
                };
                MainGrid.Children.Add(easter);
            }
        }

        //6.歷史紀錄
        private async void CreateHistoryPage()
        {
            if (history == null)
            {
                await Task.Delay(2000);
                history = new HistoryPage();
                history.LookComplete += () =>
                {
                    GameStep = 1;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateOptionPage();
                        loading = null;
                    };
                    MainGrid.Children.Remove(history);
                    history = null;
                };
                MainGrid.Children.Add(history);
            }
        }

        //6.設定介面
        private async void CreateSettingPage()
        {
            if (setting == null)
            {
                await Task.Delay(2000);
                setting = new SettingPage();
                setting.Closed += () =>
                {
                    GameStep = 1;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateOptionPage();
                        loading = null;
                    };
                    MainGrid.Children.Remove(setting);
                    setting = null;
                };
                setting.UsePause += (bool isPause) =>
                {
                    OnPause = isPause;
                    if (OnPause)
                    {
                        PressEnter.Visibility = Visibility.Visible;
                        GrayMask.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PressEnter.Visibility = Visibility.Hidden;
                        GrayMask.Visibility = Visibility.Hidden;
                    }
                };
                MainGrid.Children.Add(setting);
            }
        }

        //說明介面
        private async void CreateDirectionsPage()
        {
            if (direction == null)
            {
                await Task.Delay(2000);
                direction = new DirectionsPage();
                direction.LookComplete += () =>
                {
                    GameStep = 1;
                    CreateLoadingPage();
                    loading.LoadingComplete += () =>
                    {
                        MainGrid.Children.Remove(loading);
                        CreateOptionPage();
                        loading = null;
                    };
                    MainGrid.Children.Remove(direction);
                    direction = null;
                };
                MainGrid.Children.Add(direction);
            }
        }

        //LoadingPage
        private void CreateLoadingPage()
        {
            loading = new LoadingPage(GameStep);
            MainGrid.Children.Add(loading);
        }

        private void CreateNewGame()
        {
            Settings.Default.Ending00 = false;
            Settings.Default.Ending01 = false;
            Settings.Default.Ending02 = false;
            Settings.Default.Ending03 = false;
            Settings.Default.Ending04 = false;
            Settings.Default.MusicVolume = 0.5;
            Settings.Default.EffectVolume = 0.5;
            Settings.Default.Save();
        }

        private void CreateClearGame()
        {
            Settings.Default.Ending00 = true;
            Settings.Default.Ending01 = true;
            Settings.Default.Ending02 = true;
            Settings.Default.Ending03 = true;
            Settings.Default.Ending04 = true;
            Settings.Default.Save();
        }
    }
}
