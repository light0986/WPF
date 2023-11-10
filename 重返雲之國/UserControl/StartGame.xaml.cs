using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using 重返雲之國_外傳.IMG.Boss;
using 重返雲之國_外傳.IMG.Enemies;
using 重返雲之國_外傳.IMG.MedicalKit;
using 重返雲之國_外傳.IMG.Weapon;
using 重返雲之國_外傳.Models;
using static 重返雲之國_外傳.Models.Entities;

namespace 重返雲之國_外傳.UserControls
{
    /// <summary>
    /// StartGame.xaml 的互動邏輯
    /// 15分鐘的時間瘋狂殺
    /// 殺越多，場上的浮雲有機會變更多
    /// 殺越快，場上的浮雲也會越強
    /// 15分鐘清空後出現Boss
    /// 
    /// </summary>
    public partial class StartGame : UserControl
    {
        public Action StatusUpShow;
        public Action PlayerDead;

        public delegate void Event2(bool TrueOrFalse);
        public Event2 UsePause;
        public Event2 GameWin;

        private MusicPlayer music { get => thisData.music; set => thisData.music = value; }
        private ThisData thisData { get; set; } = new ThisData();
        private WeaponTable weapon { get; set; }
        private WhitePowder white { get; set; }
        private BrustStream brust { get; set; }
        private List<LittleCloud> monsterList { get; set; } = new List<LittleCloud>();
        private List<ThunderBall> thunderBalls { get; set; } = new List<ThunderBall>();
        private int MonsterLevel { get; set; } = 0;
        private bool GamePause { get; set; } = false;
        public bool statusUp
        {
            get => thisData.playerStatus.StatusUp;
            set
            {
                thisData.playerStatus.StatusUp = value;
                if (thisData.playerStatus.StatusUp)
                {
                    white.StopProgress();
                    thisData.AllPressfalse();
                    thisData.TimeStop = true;
                    if (thunderBalls.Count > 0)
                    {
                        foreach (var thunderBall in thunderBalls)
                        {
                            if (thunderBall != null)
                            {
                                thunderBall.OnPause = true;
                            }
                        }
                    }
                    if (weapon != null) { weapon.OnPause = true; }
                }
                else
                {
                    white.StartProgress();
                    thisData.RefreshStatus();
                    thisData.TimeStop = false;
                    if (thunderBalls.Count > 0)
                    {
                        foreach (var thunderBall in thunderBalls)
                        {
                            if (thunderBall != null)
                            {
                                thunderBall.OnPause = false;
                            }
                        }
                    }
                    if (weapon != null) { weapon.OnPause = false; }
                }
            }
        }
        private bool IsBusy { get; set; } = false; //過場動畫用
        private bool CanCreateWeapon { get; set; } = true;

        public StartGame(PlayerStatus player)
        {
            InitializeComponent();

            Properties.Settings.Default.Ending00 = true;
            Properties.Settings.Default.Save();

            Secret(player);

            thisData.playerStatus = player;
            thisData.PureStop += () =>
            {
                thisData.OnCure = false;
                if (thisData.OnDamage == false)
                {
                    CheckMove();
                }
            };
            thisData.AvoidStop += () =>
            {
                thisData.OnAvoid = false;
                CheckMove();
            };

            DataContext = thisData;
            Loaded += StartGame_Loaded; ;
        }

        private void StartGame_Loaded(object sender, RoutedEventArgs e)
        {
            music.PlayMusic("遊戲中音樂");

            thisData.IsDead += () =>
            {
                music.StopMusic();
                GameWin?.Invoke(false);
            };
            thisData.IsWin += () =>
            {
                music.StopMusic();
                GameWin?.Invoke(true);
            };

            white = new WhitePowder(thisData.playerStatus);
            PowderPosition.Children.Add(white);

            thisData.CloudChange();
            thisData.TimeStart();

            Restart();
            Start();
        }

        public void KeyPress(Key key)
        {
            if (IsBusy == false && thisData.IsAlive)
            {
                if (GamePause == false && statusUp == false)
                {
                    if (key == Key.Up)
                    {
                        thisData.PressUp = true;
                        Move(key);
                    }
                    if (key == Key.Down)
                    {
                        thisData.PressDown = true;
                        Move(key);
                    }
                    if (key == Key.Left)
                    {
                        thisData.PressLeft = true;
                        if (thisData.OnAvoid == false) { thisData.LeftOrRight = key; }
                        Move(key);
                    }
                    if (key == Key.Right)
                    {
                        thisData.PressRight = true;
                        if (thisData.OnAvoid == false) { thisData.LeftOrRight = key; }
                        Move(key);
                    }
                    if (key == Key.Z)
                    {
                        thisData.PressZ = true;
                        if (thisData.OnAvoid == false && thisData.OnCure == false && thisData.OnAttack == false)
                        {
                            if (thisData.PlayerAP >= 25)
                            {
                                thisData.OnAvoid = true;
                                thisData.OnMove = true;
                                thisData.Roll = thisData.Look;
                            }
                        }
                    }
                    if (key == Key.X)
                    {
                        thisData.PressX = true;
                        if (thisData.OnAvoid == false && thisData.OnCure == false && thisData.OnDamage == false)
                        {
                            if (thisData.PlayerAP >= (thisData.playerStatus.PlayerATK * 2))
                            {
                                thisData.AttackDir = thisData.Look;
                                Attack(thisData.AttackDir);
                            }
                        }
                    }
                    if (key == Key.A)
                    {
                        if (thisData.OnAvoid == false && thisData.OnAttack == false && thisData.OnCure == false)
                        {
                            Cure();
                        }
                    }
                }
                if (statusUp == false)
                {
                    if (key == Key.Enter)
                    {
                        if (GamePause)
                        {
                            music.PlayShort("游標移動音效");
                            Restart();
                        }
                        else
                        {
                            music.PlayShort("游標移動音效");
                            Pause();
                        }
                    }
                }
            }
        }

        public void KeyRelese(Key key)
        {
            if (key == Key.Up)
            {
                thisData.PressUp = false;
            }
            if (key == Key.Down)
            {
                thisData.PressDown = false;
            }
            if (key == Key.Left)
            {
                thisData.PressLeft = false;
            }
            if (key == Key.Right)
            {
                thisData.PressRight = false;
            }
            if (key == Key.Z)
            {
                thisData.PressZ = false;
            }
            if (key == Key.X)
            {
                thisData.PressX = false;
            }
            if (thisData.OnAttack == false && thisData.OnCure == false && IsBusy == false) { CheckMove(); }
        }

        //遊戲主要畫面刷新迴圈
        private async void Start()
        {
            KeyPress(Key.Enter);
            do
            {
                //遊戲沒有暫停，且玩家沒有死亡
                if (GamePause == false && statusUp == false && thisData.IsAlive)
                {
                    //怪物沒了製造怪獸
                    if (monsterList.Count == 0)
                    {
                        //小於10分鐘創造小怪
                        if (thisData.Seconds > 0) { await CreateMonster(); }
                        //倒數結束時招喚Boss
                        else
                        {
                            if(thisData.bigCloud == null)
                            {
                                await CreateBoss();
                            }
                        }
                    }
                    //怪物移動。怪物優先度高
                    MonsterAction();
                    if (thisData.WinAnimation) { break; }
                    //判斷有沒有被打到
                    if (thisData.OnDamage == true)
                    {
                        //中斷攻擊
                        if (weapon != null)
                        {
                            weapon.OnStop = true;
                            weapon = null;
                            WeaponGrid.Children.Remove(weapon);
                        }
                        //判斷有無死亡
                        if (thisData.PlayerHP <= 0)
                        {
                            thisData.IsAlive = false;
                            break;
                        }
                    }
                    //沒有被打到，可以繼續輸出
                    else
                    {
                        //持續攻擊的刷新
                        if (thisData.PressX == true && thisData.OnAttack == true)
                        {
                            if (thisData.PlayerAP >= (thisData.playerStatus.PlayerATK * 2))
                            {
                                Attack(thisData.Look);
                                thisData.PressX = false;
                            }
                        }
                    }
                    //動畫的刷新
                    thisData.PlayerAction();
                }

                await Task.Delay(thisData.playerStatus.DelayTime);
            }
            while (true);
        }

        private void Pause()
        {
            GamePause = true;
            UsePause?.Invoke(true);
            thisData.TimeStop = true;
            white.StopProgress();
            if(thunderBalls.Count > 0)
            {
                foreach(var thunderBall in thunderBalls)
                {
                    if (thunderBall != null)
                    {
                        thunderBall.OnPause = true;
                    }
                }
            }
            if(weapon != null) { weapon.OnPause = true; }
        }

        private void Restart()
        {
            GamePause = false;
            UsePause?.Invoke(false);
            thisData.TimeStop = false;
            white.StartProgress();
            if (thunderBalls.Count > 0)
            {
                foreach (var thunderBall in thunderBalls)
                {
                    if (thunderBall != null)
                    {
                        thunderBall.OnPause = false;
                    }
                }
            }
            if (weapon != null) { weapon.OnPause = false; }
        }

        private void Move(Key key)
        {
            if (thisData.OnAttack == false && thisData.OnCure == false && thisData.OnAvoid == false && thisData.PressZ == false)
            {
                thisData.Look = key;

                if (thisData.PressDown == true && thisData.PressLeft == true && 
                    thisData.PressUp == true && thisData.PressRight == true)
                {
                    thisData.OnMove = false;
                }
                else
                {
                    thisData.OnMove = true;
                }
            }
        }

        private void CheckMove()
        {
            if (thisData.PressDown == true && thisData.PressLeft == false
                && thisData.PressUp == false && thisData.PressRight == false)
            {
                thisData.Look = Key.Down;
                thisData.OnMove = true;
            }
            else if (thisData.PressDown == false && thisData.PressLeft == true && 
                    thisData.PressUp == false && thisData.PressRight == false)
            {
                thisData.Look = Key.Left;
                if (thisData.OnAvoid == false) { thisData.LeftOrRight = Key.Left; }
                thisData.OnMove = true;
            }
            else if (thisData.PressDown == false && thisData.PressLeft == false &&
                    thisData.PressUp == true && thisData.PressRight == false)
            {
                thisData.Look = Key.Up;
                thisData.OnMove = true;
            }
            else if (thisData.PressDown == false && thisData.PressLeft == false &&
                    thisData.PressUp == false && thisData.PressRight == true)
            {
                thisData.Look = Key.Right;
                if (thisData.OnAvoid == false) { thisData.LeftOrRight = Key.Right; }
                thisData.OnMove = true;
            }
        }

        private void Attack(Key key)
        {
            if (CanCreateWeapon)
            {
                if (weapon == null)
                {
                    thisData.OnMove = false;
                    thisData.PlayerNotMove(key);
                    weapon = new WeaponTable(key, thisData.playerStatus);
                    if (key == Key.Down)
                    {
                        thisData.HitX = 325;
                        thisData.HitY = 350;
                        thisData.HitWidth = 150;
                        thisData.HitHeight = 50;
                        thisData.WeaponGridAngle = 0;
                    }
                    else if (key == Key.Left)
                    {
                        thisData.HitX = 300;
                        thisData.HitY = 225;
                        thisData.HitWidth = 50;
                        thisData.HitHeight = 150;
                        thisData.WeaponGridAngle = 0;
                    }
                    else if (key == Key.Right)
                    {
                        thisData.HitX = 450;
                        thisData.HitY = 225;
                        thisData.HitWidth = 50;
                        thisData.HitHeight = 150;
                        thisData.WeaponGridAngle = 180;
                    }
                    else if (key == Key.Up)
                    {
                        thisData.HitX = 325;
                        thisData.HitY = 200;
                        thisData.HitWidth = 150;
                        thisData.HitHeight = 50;
                        thisData.WeaponGridAngle = 180;
                    }
                    WeaponGrid.Children.Add(weapon);

                    weapon.WeaponShow += () =>
                    {
                        thisData.OnAttack = true;
                        thisData.AttackCombo = 0;
                        thisData.RefreshCombo();
                    };
                    weapon.WeaponStartHit += () =>
                    {
                        if (CanCreateWeapon)
                        {
                            music.PlayShort("悠白攻擊音效");
                            thisData.OnHit = true;
                            thisData.UseAP((int)((thisData.playerStatus.PlayerATK * 2) - thisData.playerStatus.Amount));
                            thisData.RefreshCombo();
                        }
                    };
                    weapon.WeaponStopHit += () =>
                    {
                        thisData.RefreshCombo();
                        thisData.OnHit = false;
                    };
                    weapon.WeaponHide += () =>
                    {
                        //連擊數10，可以回SAN值
                        if (thisData.AttackCombo == 10) { thisData.PlayerHP += 10; }
                        thisData.AttackCombo = 0;

                        thisData.OnAttack = false;
                        thisData.RefreshCombo();
                        CheckMove();
                        CreateWeaponCD();
                        WeaponGrid.Children.Remove(weapon);
                        weapon = null;
                    };
                }
                else
                {
                    if (thisData.PlayerAP >= 20)
                    {
                        weapon?.PlayerHit();
                    }
                }
            }
        }

        private void Cure()
        {
            if (white.UsePowder())
            {
                music.PlayLong("悠白回血音效");
                thisData.OnMove = false;
                thisData.OnCure = true;
                thisData.CureToken = 0;
                thisData.PureStop += () =>
                {
                    thisData.OnCure = false;
                    if (thisData.OnDamage == false)
                    {
                        CheckMove();
                    }
                };
            }            
        }

        private async Task CreateMonster()
        {
            Random rd = new Random();
            for (int i = 0; i < rd.Next(1, MonsterLevel + 1); i++)
            {
                LittleCloud littleCloud = new LittleCloud(thisData.playerStatus);
                littleCloud.AddLevel(MonsterLevel);

                Binding X = new Binding("RelativeX")
                {
                    Source = littleCloud.DataContext
                };
                littleCloud.SetBinding(Canvas.LeftProperty, X);

                Binding Y = new Binding("RelativeY")
                {
                    Source = littleCloud.DataContext
                };
                littleCloud.SetBinding(Canvas.TopProperty, Y);

                monsterList.Add(littleCloud);
                ObjectGrid.Children.Add(littleCloud);
                await Task.Delay(thisData.playerStatus.DelayTime);
            }
        }

        private async Task CreateBoss()
        {
            IsBusy = true;
            //Warning閃耀
            if (thisData.playerStatus.difficulty == Difficulty.Hard)
            {
                music.PlayMusic("最難Boss");
            }
            else if (thisData.playerStatus.difficulty == Difficulty.Easy || thisData.playerStatus.difficulty == Difficulty.Middle)
            {
                music.PlayMusic("中等以下Boss");
            }
            
            WarningText.Visibility = Visibility.Visible;
            for(int i = 0; i < 3; i++)
            {
                WarningText.Opacity = 0.9;
                await Task.Delay(100);
                WarningText.Opacity = 0.8;
                await Task.Delay(100);
                WarningText.Opacity = 0.7;
                await Task.Delay(100);
                WarningText.Opacity = 0.6;
                await Task.Delay(100);
                WarningText.Opacity = 0.5;
                await Task.Delay(100);
                WarningText.Opacity = 0.6;
                await Task.Delay(100);
                WarningText.Opacity = 0.7;
                await Task.Delay(100);
                WarningText.Opacity = 0.8;
                await Task.Delay(100);
                WarningText.Opacity = 0.9;
                await Task.Delay(100);
                WarningText.Opacity = 1.0;
                await Task.Delay(100);
            }
            WarningText.Visibility = Visibility.Hidden;

            BigCloud bigCloud = new BigCloud(thisData.playerStatus);
            bigCloud.ShotBurstStream += (tx, ty, sx, sy) =>
            {
                if (brust == null)
                {
                    brust = new BrustStream(thisData.playerStatus)
                    {
                        RenderTransformOrigin = new Point(0, 0.5)
                    };
                    brust.SetPosition(tx, ty, sx, sy);
                    Binding BX = new Binding("RelativeX")
                    {
                        Source = brust.DataContext
                    };
                    brust.SetBinding(Canvas.LeftProperty, BX);

                    Binding BY = new Binding("RelativeY")
                    {
                        Source = brust.DataContext
                    };
                    brust.SetBinding(Canvas.TopProperty, BY);

                    double angleOfLine = Math.Atan2((ty - sy), (tx - sx)) * 180 / Math.PI;
                    RotateTransform rotate = new RotateTransform(angleOfLine)
                    {
                        CenterX = 25,
                        CenterY = 0
                    };
                    brust.RenderTransform = rotate;

                    ObjectGrid.Children.Add(brust);
                }
            };
            bigCloud.StopBurstStream += () =>
            {
                if (brust != null && ObjectGrid.Children.Contains(brust))
                {
                    ObjectGrid.Children.Remove(brust);
                    brust = null;
                }
            };
            bigCloud.CreateThunder += (sx, sy) =>{ RefreshBallList(); ShootThunder(sx, sy); };
            bigCloud.CreateSeccess += () => { IsBusy = false; };
            bigCloud.PlayerWin += () => { IsBusy = true; };
            bigCloud.CreateLittleCloud += () => { BossCreateMonster(); };

            Binding X = new Binding("RelativeX")
            {
                Source = bigCloud.DataContext
            };
            bigCloud.SetBinding(Canvas.LeftProperty, X);

            Binding Y = new Binding("RelativeY")
            {
                Source = bigCloud.DataContext
            };
            bigCloud.SetBinding(Canvas.TopProperty, Y);

            thisData.bigCloud = bigCloud;
            ObjectGrid.Children.Add(thisData.bigCloud);

            for (int i = 0; i < 64; i++)
            {
                ThunderBall ball = null;
                thunderBalls.Add(ball);
            }
        }

        private async void BossCreateMonster()
        {
            if (monsterList.Count <= 0)
            {
                //固定出五隻
                for (int i = 0; i < 5; i++)
                {
                    LittleCloud littleCloud = new LittleCloud(thisData.playerStatus);
                    littleCloud.AddLevel(0);

                    Binding X = new Binding("RelativeX")
                    {
                        Source = littleCloud.DataContext
                    };
                    littleCloud.SetBinding(Canvas.LeftProperty, X);

                    Binding Y = new Binding("RelativeY")
                    {
                        Source = littleCloud.DataContext
                    };
                    littleCloud.SetBinding(Canvas.TopProperty, Y);

                    monsterList.Add(littleCloud);
                    ObjectGrid.Children.Add(littleCloud);
                    await Task.Delay(thisData.playerStatus.DelayTime);
                }
            }
        }

        private async void ShootThunder(double StartX, double StartY)
        {
            Random rd = new Random();
            int i = rd.Next(0, 721);
            bool clockwise = true;
            if (i >= 360) { clockwise = false; }
            foreach (ThunderBall ball in thunderBalls)
            {
                ball.Destroyed += () =>
                {
                    ObjectGrid.Children.Remove(ball);
                    if (thunderBalls.Contains(ball))
                    {
                        int index = thunderBalls.IndexOf(ball);
                        thunderBalls[index] = null;
                    }
                };
                await Task.Delay(50);
                ball.ShootDir(i, StartX, StartY);
                if (!ObjectGrid.Children.Contains(ball))
                {
                    ObjectGrid.Children.Add(ball);
                }
                if (clockwise) { i += 13; }
                else { i -= 13; }
            }
        }

        private void RefreshBallList()
        {
            for (int i = 0; i < 64; i++)
            {
                thunderBalls[i] = CreateBall();
            }
        }

        private ThunderBall CreateBall()
        {
            ThunderBall ball = new ThunderBall(thisData.playerStatus);
            Binding TX = new Binding("RelativeX")
            {
                Source = ball.DataContext
            };
            ball.SetBinding(Canvas.LeftProperty, TX);
            Binding TY = new Binding("RelativeY")
            {
                Source = ball.DataContext
            };
            ball.SetBinding(Canvas.TopProperty, TY);
            return ball;
        }

        private void MonsterAction()
        {
            //小怪用
            if (monsterList.Count > 0)
            {
                List<LittleCloud> DeadList = new List<LittleCloud>();
                foreach (var i in monsterList)
                {
                    if (i.IsDead == false)
                    {
                        i.MonsterMove();
                        if (i.HitSuccess && thisData.OnDamage == false && thisData.OnAvoid == false)
                        {
                            thisData.DamageToken += i.DamageToken;
                            i.HitSuccess = false;
                        }
                    }
                    else
                    {
                        DeadList.Add(i);
                    }
                }

                if (DeadList.Count > 0)
                {
                    foreach (var i in DeadList)
                    {
                        if (monsterList.Contains(i)) { monsterList.Remove(i); }
                        if (ObjectGrid.Children.Contains(i)) { ObjectGrid.Children.Remove(i); }
                    }

                    thisData.PlayerExp += (DeadList.Count * 100) + (MonsterLevel * 5) + (thisData.PlayerHP * 0.1);
                    white.AddProgress(DeadList.Count * 20);
                }
                //大浮雲召喚的小浮雲，不會變強
                if (monsterList.Count == 0 && thisData.bigCloud == null) { MonsterLevel++; }
                if (thisData.DamageToken != 0 && thisData.OnDamage == false && thisData.OnAvoid == false) { thisData.OnDamage = true; }
            }

            //大浮雲用
            if (thisData.bigCloud != null)
            {
                if(thisData.bigCloud.IsDead == false)
                {
                    //Boss行動一次
                    thisData.bigCloud.MonsterMove();
                    if (thisData.OnDamage == false && thisData.OnAvoid == false)
                    {
                        //玩家被撞到
                        if (thisData.bigCloud.HitSuccess)
                        {
                            thisData.DamageToken += thisData.bigCloud.DamageToken;
                            thisData.bigCloud.HitSuccess = false;
                        }

                        //玩家被雷電打到
                        foreach (var ball in thunderBalls)
                        {
                            if (ball != null && ball.HitSuccess)
                            {
                                thisData.DamageToken += ball.DamageToken;
                                ball.DamageToken = 0;
                                ball.HitSuccess = false;
                            }
                        }

                        //玩家當下受到總傷害
                        if (thisData.DamageToken != 0)
                        {
                            thisData.OnDamage = true;
                        }
                    }
                    //更新HP的Bar顯示
                    thisData.RefreshBoss();
                }
                //打敗Boss
                else
                {
                    thisData.GoWin();
                }
            }

            //確認經驗值
            if (CheckExp())
            {
                statusUp = true;
                StatusUpShow?.Invoke();
            }
        }

        private bool CheckExp()
        {
            if (thisData.PlayerExp >= thisData.PlayerMaxExp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void CreateWeaponCD()
        {
            CanCreateWeapon = false;
            await Task.Delay(thisData.playerStatus.DelayTime * 10);
            CanCreateWeapon = true;
        }

        private void Secret(PlayerStatus player)
        {
            //阿Light好帥，悠白最可愛
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BGM", "密碼.txt");
            if (System.IO.File.Exists(path))
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                if (lines.Length >= 1)
                {
                    string password01 = (string)Application.Current.TryFindResource(lines[0]) ?? "";
                    if (password01 != "")
                    {
                        if (password01 == "經驗值") { player.PlayerExp += 100000; }
                        else if (password01 == "生命") { player.ContinueTime = -1; }
                    }
                }
                if (lines.Length >= 2)
                {
                    string password02 = (string)Application.Current.TryFindResource(lines[1]) ?? "";
                    if (password02 != "")
                    {
                        if (password02 == "經驗值") { player.PlayerExp += 100000; }
                        else if (password02 == "生命") { player.ContinueTime = -1; }
                    }
                }
            }
        }

        private class ThisData : MVVM
        {
            //遊戲主要畫面刷新:
            //補AP、開始移動、吸白粉、甚麼都沒做
            public void PlayerAction()
            {
                if (OnDamage == false)
                {
                    //開始移動
                    if (OnMove == true && OnAttack == false && OnCure == false)
                    {
                        if (OnAvoid == false && AvoidToken == 0)
                        {
                            if (PressDown == false && PressUp == false && PressLeft == false && PressRight == false)
                            {
                                OnMove = false;
                            }
                            else
                            {
                                PlayerMove();
                                CloudMove(playerStatus.MoveSpeed);
                            }
                        }
                        else
                        {
                            PlayerAvoid();
                            CloudMove(playerStatus.MoveSpeed * 2);
                        }
                    }
                    //吸白粉
                    else if (OnMove == false && OnAvoid == false && OnAttack == false && OnCure == true)
                    {
                        PlayerCure();
                    }
                    //甚麼都沒做
                    else if (OnMove == false && OnAvoid == false && OnAttack == false && OnCure == false)
                    {
                        PlayerNotMove(Look);
                    }
                }
                //固定回體力
                if (OnAttack == false && OnAvoid == false) { PlayerAP += (1 + (playerStatus.PowderCD / 10)); }

            }

            public MusicPlayer music = new MusicPlayer();

            #region 按鍵
            public bool PressUp { get; set; } = false;

            public bool PressDown { get; set;} = false;

            public bool PressLeft { get; set; } = false;

            public bool PressRight { get; set;} = false;

            public bool PressZ { get; set; } = false;

            public bool PressX { get; set; } = false;

            public Key Look { get; set; } = Key.Down;

            public void AllPressfalse()
            {
                PressUp = false;
                PressDown = false;
                PressLeft = false;
                PressRight = false;
                PressZ = false;
                PressX = false;
            }
            #endregion

            #region 地圖
            private BitmapImage _Cloud = (BitmapImage)Application.Current.TryFindResource("Cloud01");
            public BitmapImage Cloud
            {
                get => _Cloud;
                set
                {
                    _Cloud = value;
                    NotifyPropertyChanged("Cloud");
                }
            }

            private double _CloudX1 = -1199;
            public double CloudX1
            {
                get => _CloudX1;
                set
                {
                    _CloudX1 = value;
                    if(_CloudX1 < -1199) { _CloudX1 += 2399; }
                    else if (_CloudX1 > 1200) { _CloudX1 -= 2399; }
                    NotifyPropertyChanged("CloudX1");
                }
            }

            private double _CloudX2 = -400;
            public double CloudX2
            {
                get => _CloudX2;
                set
                {
                    _CloudX2 = value;
                    if (_CloudX2 < -1199) { _CloudX2 += 2399; }
                    else if (_CloudX2 > 1200) { _CloudX2 -= 2399; }
                    NotifyPropertyChanged("CloudX2");
                }
            }

            private double _CloudX3 = 400;
            public double CloudX3
            {
                get => _CloudX3;
                set
                {
                    _CloudX3 = value;
                    if (_CloudX3 < -1199) { _CloudX3 += 2399; }
                    else if (_CloudX3 > 1200) { _CloudX3 -= 2399; }
                    NotifyPropertyChanged("CloudX3");
                }
            }


            private double _CloudY1 = -1299;
            public double CloudY1
            {
                get => _CloudY1;
                set
                {
                    _CloudY1 = value;
                    if (_CloudY1 < -1299) { _CloudY1 += 2399; }
                    else if (_CloudY1 > 1100) { _CloudY1 -= 2399; }
                    NotifyPropertyChanged("CloudY1");
                }
            }

            private double _CloudY2 = -500;
            public double CloudY2
            {
                get => _CloudY2;
                set
                {
                    _CloudY2 = value;
                    if (_CloudY2 < -1299) { _CloudY2 += 2399; }
                    else if (_CloudY2 > 1100) { _CloudY2 -= 2399; }
                    NotifyPropertyChanged("CloudY2");
                }
            }

            private double _CloudY3 = 300;
            public double CloudY3
            {
                get => _CloudY3;
                set
                {
                    _CloudY3 = value;
                    if (_CloudY3 < -1299) { _CloudY3 += 2399; }
                    else if (_CloudY3 > 1100) { _CloudY3 -= 2399; }
                    NotifyPropertyChanged("CloudY3");
                }
            }

            public async void CloudChange()
            {
                do
                {
                    Cloud = (BitmapImage)Application.Current.TryFindResource("Cloud01");
                    await Task.Delay(1000);
                    Cloud = (BitmapImage)Application.Current.TryFindResource("Cloud02");
                    await Task.Delay(1000);
                }
                while (true);
            }

            private void CloudMove(double speed)
            {
                if (OnAvoid == false)
                {
                    if (Look == Key.Up)
                    {
                        CloudY1 += speed;
                        CloudY2 += speed;
                        CloudY3 += speed;
                        playerStatus.CenterY -= speed;
                    }
                    else if (Look == Key.Down)
                    {
                        CloudY1 -= speed;
                        CloudY2 -= speed;
                        CloudY3 -= speed;
                        playerStatus.CenterY += speed;
                    }
                    else if (Look == Key.Left)
                    {
                        CloudX1 += speed;
                        CloudX2 += speed;
                        CloudX3 += speed;
                        playerStatus.CenterX -= speed;
                    }
                    else if (Look == Key.Right)
                    {
                        CloudX1 -= speed;
                        CloudX2 -= speed;
                        CloudX3 -= speed;
                        playerStatus.CenterX += speed;
                    }
                }
                else
                {
                    if (Roll == Key.Up)
                    {
                        CloudY1 += speed;
                        CloudY2 += speed;
                        CloudY3 += speed;
                        playerStatus.CenterY -= speed;
                    }
                    else if (Roll == Key.Down)
                    {
                        CloudY1 -= speed;
                        CloudY2 -= speed;
                        CloudY3 -= speed;
                        playerStatus.CenterY += speed;
                    }
                    else if (Roll == Key.Left)
                    {
                        CloudX1 += speed;
                        CloudX2 += speed;
                        CloudX3 += speed;
                        playerStatus.CenterX -= speed;
                    }
                    else if (Roll == Key.Right)
                    {
                        CloudX1 -= speed;
                        CloudX2 -= speed;
                        CloudX3 -= speed;
                        playerStatus.CenterX += speed;
                    }
                }
            }
            #endregion

            #region 玩家
            private BitmapImage _PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player01");
            public BitmapImage PlayerIMG
            {
                get => _PlayerIMG;
                set
                {
                    _PlayerIMG = value;
                    NotifyPropertyChanged("PlayerIMG");
                }
            }

            public PlayerStatus playerStatus { get; set; } = new PlayerStatus();

            private Thickness _PlayerMargin = new Thickness(0, 5, 0, 35);
            public Thickness PlayerMargin
            {
                get => _PlayerMargin;
                set
                {
                    _PlayerMargin = value;
                    NotifyPropertyChanged("PlayerMargin");
                }
            }

            public int PlayerWidth
            {
                get => playerStatus.PlayerWidth;
                set
                {
                    playerStatus.PlayerWidth = value;
                    NotifyPropertyChanged("PlayerWidth");
                }
            }

            public int PlayerHeight
            {
                get => playerStatus.PlayerHeight;
                set
                {
                    playerStatus.PlayerHeight = value;
                    NotifyPropertyChanged("PlayerHeight");
                }
            }

            public double PlayerMaxHP
            {
                get => playerStatus.PlayerMaxHP;
                set
                {
                    playerStatus.PlayerMaxHP = value;

                    NotifyPropertyChanged("PlayerMaxHP");
                }
            }

            public double PlayerHP
            {
                get => playerStatus.PlayerHP;
                set
                {
                    playerStatus.PlayerHP = value;
                    NotifyPropertyChanged("PlayerHP");
                }
            }

            public double PlayerMaxAP
            {
                get => playerStatus.PlayerMaxAP;
                set
                {
                    playerStatus.PlayerMaxAP = value;
                    NotifyPropertyChanged("PlayerMaxAP");
                }
            }

            public double PlayerAP
            {
                get => playerStatus.PlayerAP;
                set
                {
                    playerStatus.PlayerAP = value;
                    NotifyPropertyChanged("PlayerAP");
                }
            }

            public double PlayerMaxExp
            {
                get => playerStatus.PlayerMaxExp;
                set
                {
                    playerStatus.PlayerMaxExp = value;
                    NotifyPropertyChanged("PlayerMaxExp");
                }
            }

            public double PlayerExp
            {
                get => playerStatus.PlayerExp;
                set
                {
                    playerStatus.PlayerExp = value;
                    NotifyPropertyChanged("PlayerExp");
                }
            }

            public int PlayerLV
            {
                get => playerStatus.Level;
                set
                {
                    playerStatus.Level = value;
                    NotifyPropertyChanged("PlayerLV");
                }
            }


            private Thickness _ApMargin = new Thickness(5);
            public Thickness ApMargin
            {
                get => _ApMargin;
                set
                {
                    _ApMargin = value;
                    NotifyPropertyChanged("ApMargin");
                }
            }

            public void RefreshStatus()
            {
                NotifyPropertyChanged("PlayerMaxHP");
                NotifyPropertyChanged("PlayerHP");
                NotifyPropertyChanged("PlayerMaxAP");
                NotifyPropertyChanged("PlayerAP");
                NotifyPropertyChanged("PlayerMaxExp");
                NotifyPropertyChanged("PlayerExp");
                NotifyPropertyChanged("PlayerLV");
            }

            public void PlayerNotMove(Key key)
            {
                if (key == Key.Down) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player01"); }
                else if (key == Key.Up) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player11"); }
                else if (key == Key.Left) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player21"); }
                else if (key == Key.Right) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player31"); }
            }

            public bool UseAP(int consumption)
            {
                if(PlayerAP >= consumption)
                {
                    PlayerAP -= consumption;
                    if (PlayerAP / (double)PlayerMaxAP <= 0.3) { APShake(); }
                    return true;
                }
                else
                {
                    APShake();
                    return false;
                }
            }

            private async void APShake()
            {
                ApMargin = new Thickness(0, 5, 10, 5);
                await Task.Delay(playerStatus.DelayTime);
                ApMargin = new Thickness(10, 5, 0, 5);
                await Task.Delay(playerStatus.DelayTime);
                ApMargin = new Thickness(5);
            }
            #endregion

            #region 移動
            private int MoveToken { get; set; } = 0;

            public bool OnMove { get; set; } = false;

            private int FootAhead { get; set; } = 0;

            private void PlayerMove()
            {
                if (OnMove == true && OnAttack == false && OnCure == false && OnAvoid == false)
                {
                    MoveToken++;
                    FootAhead = MoveToken / 10;
                    if (FootAhead >= 4)
                    {
                        FootAhead = 0;
                        MoveToken = 0;
                    }

                    if (FootAhead == 0)
                    {
                        if (Look == Key.Down) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player02"); }
                        else if (Look == Key.Up) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player12"); }
                        else if (Look == Key.Left) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player22"); }
                        else if (Look == Key.Right) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player32"); }
                    }
                    else if (FootAhead == 1)
                    {
                        if (Look == Key.Down) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player01"); }
                        else if (Look == Key.Up) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player11"); }
                        else if (Look == Key.Left) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player21"); }
                        else if (Look == Key.Right) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player31"); }
                    }
                    else if (FootAhead == 2)
                    {
                        if (Look == Key.Down) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player03"); }
                        else if (Look == Key.Up) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player13"); }
                        else if (Look == Key.Left) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player23"); }
                        else if (Look == Key.Right) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player33"); }
                    }
                    else if (FootAhead == 3)
                    {
                        if (Look == Key.Down) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player01"); }
                        else if (Look == Key.Up) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player11"); }
                        else if (Look == Key.Left) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player21"); }
                        else if (Look == Key.Right) { PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player31"); }
                    }
                }
            }
            #endregion

            #region 迴避
            public Action AvoidStop;

            public bool OnAvoid
            {
                get => playerStatus.OnAvoid;
                set => playerStatus.OnAvoid = value;
            }

            private int _AvoidToken = 0;
            public int AvoidToken
            {
                get => _AvoidToken;
                set
                {
                    _AvoidToken = value;
                    NotifyPropertyChanged("AvoidToken");
                }
            }

            public Key LeftOrRight { get; set; } = Key.Right;

            public Key Roll { get; set; } = Key.Down;

            private void PlayerAvoid()
            {
                if (LeftOrRight == Key.Right)
                {
                    if (AvoidToken == 0)
                    {
                        if (UseAP(25))
                        {
                            music.PlayLong("悠白迴避音效");
                            PlayerIMG = (BitmapImage)Application.Current.TryFindResource("PlayerAvoid");
                            AvoidToken = 360;
                        }
                        else { AvoidToken = 720; }
                    }

                    if (AvoidToken < 720)
                    {
                        AvoidToken += (int)playerStatus.MoveSpeed * 3;
                    }

                    if (AvoidToken >= 720)
                    {
                        AvoidToken = 0;
                        Roll = Look;
                        AvoidStop?.Invoke();
                    }
                }
                else if (LeftOrRight == Key.Left)
                {
                    if (AvoidToken == 0)
                    {
                        if (UseAP(25))
                        {
                            music.PlayLong("悠白迴避音效");
                            PlayerIMG = (BitmapImage)Application.Current.TryFindResource("PlayerAvoid");
                            AvoidToken = 720;
                        }
                        else { AvoidToken = 360; }
                    }

                    if (AvoidToken > 360)
                    {
                        AvoidToken -= (int)playerStatus.MoveSpeed * 3;
                    }
                    
                    if (AvoidToken <= 360)
                    {
                        AvoidToken = 0;
                        Roll = Look;
                        AvoidStop?.Invoke();
                    }
                }
            }
            #endregion

            #region 攻擊
            //連擊數
            public double AttackCombo
            {
                get => playerStatus.AttackCombo;
                set
                {
                    playerStatus.AttackCombo = value;
                    NotifyPropertyChanged("AttackCombo");
                }
            }

            public double ComboShow
            {
                get
                {
                    if (AttackCombo >= 2) { return 1; }
                    else { return 0; }
                }
            }

            public Thickness ComboMargin { get; set; } = new Thickness(10);

            public async void RefreshCombo()
            {
                NotifyPropertyChanged("ComboShow");
                NotifyPropertyChanged("AttackCombo");
                ComboMargin = new Thickness(11);
                NotifyPropertyChanged("ComboMargin");
                await Task.Delay(playerStatus.DelayTime);
                ComboMargin = new Thickness(9);
                NotifyPropertyChanged("ComboMargin");
                await Task.Delay(playerStatus.DelayTime);
                ComboMargin = new Thickness(10);
                NotifyPropertyChanged("ComboMargin");
                await Task.Delay(playerStatus.DelayTime);
            }

            //開始攻擊
            public bool OnAttack
            {
                get => playerStatus.OnAttack;
                set
                {
                    playerStatus.OnAttack = value;
                }
            }

            //進入傷害階段
            public bool OnHit
            {
                get => playerStatus.OnHit;
                set
                {
                    playerStatus.OnHit = value;
                }
            }

            //傷害起點
            public double HitX
            {
                get => playerStatus.HitX;
                set
                {
                    playerStatus.HitX = value;
                    NotifyPropertyChanged("HitX");
                }
            }

            public double HitY
            {
                get => playerStatus.HitY;
                set
                {
                    playerStatus.HitY = value;
                    NotifyPropertyChanged("HitY");
                }
            }

            //傷害區域的長與寬
            public double HitWidth
            {
                get => playerStatus.HitWidth;
                set
                {
                    playerStatus.HitWidth = value;
                    NotifyPropertyChanged("HitWidth");
                }
            }

            public double HitHeight
            {
                get => playerStatus.HitHeight;
                set
                {
                    playerStatus.HitHeight = value;
                    NotifyPropertyChanged("HitHeight");
                }
            }

            //轉角度
            private int _WeaponGridAngle = 0;
            public int WeaponGridAngle
            {
                get => _WeaponGridAngle;
                set
                {
                    _WeaponGridAngle = value;
                    NotifyPropertyChanged("WeaponGridAngle");
                }
            }

            public Key AttackDir { get; set; } = Key.Down;
            #endregion

            #region 補血
            public Action PureStop;


            public bool _OnCure = false;
            public bool OnCure
            {
                get => _OnCure;
                set
                {
                    _OnCure = value;
                    if (_OnCure) { CureProgress = Visibility.Visible; }
                    else { CureProgress = Visibility.Hidden; }
                }
            }


            private Visibility _CureProgress = Visibility.Hidden;
            public Visibility CureProgress
            {
                get => _CureProgress;
                set
                {
                    _CureProgress = value;
                    NotifyPropertyChanged("CureProgress");
                }
            }


            private int _CureToken = 0; //最大120
            public int CureToken
            {
                get => _CureToken;
                set
                {
                    _CureToken = value;
                    NotifyPropertyChanged("CureToken");
                }
            }

            private void PlayerCure()
            {
                if (CureToken / 10 == 0 || CureToken / 10 == 2 || CureToken / 10 == 6 || CureToken / 10 == 8)
                {
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player41");
                }
                else if (CureToken / 10 == 1 || CureToken / 10 == 3 || CureToken / 10 == 7 || CureToken / 10 == 9)
                {
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player42");
                }
                else if(CureToken / 10 == 4 || CureToken / 10 == 5 || CureToken / 10 == 11)
                {
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player43");
                }
                else if (CureToken / 10 == 12)
                {
                    PureStop?.Invoke();
                }

                CureToken++;
                PlayerHP += playerStatus.Recover;
            }
            #endregion

            #region 受傷
            //受傷時，停止一切動作。
            public bool OnDamage
            {
                get => playerStatus.OnDamage;
                set
                {
                    playerStatus.OnDamage = value;
                    if (playerStatus.OnDamage)
                    {
                        OnAvoid = false;
                        OnCure = false;
                        PlayerDamage();
                    }
                }
            }

            public double DamageToken = 0; //累積傷害

            private async void PlayerDamage()
            {
                PlayerIMG = (BitmapImage)Application.Current.TryFindResource("PlayerDamage");
                PlayerHP -= DamageToken;
                DamageToken = 0;
                PureStop?.Invoke();
                music.PlayLong("悠白被打到音效");
                await Task.Delay(300);
                OnDamage = false;
            }
            #endregion

            #region 時間
            //這裡改變時間 => 900秒 = 15分鐘
            private int _Seconds = 900;
            public int Seconds
            {
                get => _Seconds;
                set
                {
                    _Seconds = value;
                    if (_Seconds < 0) { _Seconds = 0; }
                }
            }


            public string TimeText
            {
                get => (Seconds / 60) + ":" + (Seconds % 60).ToString("D2");
            }


            public bool TimeStop { get; set; } = false;
            public async void TimeStart()
            {
                do
                {
                    if (TimeStop)
                    {
                        await Task.Delay(playerStatus.DelayTime);
                    }
                    else
                    {
                        Seconds--;
                        NotifyPropertyChanged("TimeText");
                        if (Seconds <= 0) { break; }
                        await Task.Delay(1000);
                    }
                }
                while (IsAlive);
            }
            #endregion

            #region 死亡
            public Action IsDead;

            public Action IsWin;

            public bool WinAnimation { get; set; } = false;

            public bool IsAlive
            {
                get => playerStatus.IsAlive;
                set
                {
                    playerStatus.IsAlive = value;
                    if (value == false)
                    {
                        GoDie();
                    }
                }
            }

            private async void GoDie()
            {
                for (int i = 0; i < 3; i++)
                {
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player01");
                    await Task.Delay(100);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player21");
                    await Task.Delay(100);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player11");
                    await Task.Delay(100);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player31");
                    await Task.Delay(100);
                }

                for (int i = 0; i < 3; i++)
                {
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("PlayerCry01");
                    await Task.Delay(500);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("PlayerCry02");
                    await Task.Delay(500);
                }
                
                IsDead?.Invoke();
            }

            public async void GoWin()
            {
                WinAnimation = true;
                for (int i = 0; i < 3; i++)
                {
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player01");
                    await Task.Delay(100);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player21");
                    await Task.Delay(100);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player11");
                    await Task.Delay(100);
                    PlayerIMG = (BitmapImage)Application.Current.TryFindResource("Player31");
                    await Task.Delay(100);
                }

                PlayerIMG = (BitmapImage)Application.Current.TryFindResource("PlayerWin");
                for (int i = 0; i < 3; i++)
                {
                    PlayerMargin = new Thickness(0, 0, 0, 40);
                    await Task.Delay(500);
                    PlayerMargin = new Thickness(0, 5, 0, 35);
                    await Task.Delay(500);
                }

                IsWin?.Invoke();
            }
            #endregion

            #region Boss
            private BigCloud _bigCloud = null;
            public BigCloud bigCloud
            {
                get => _bigCloud;
                set
                {
                    _bigCloud = value;
                    if(value != null)
                    {
                        BossShow = Visibility.Visible;
                    }
                    else
                    {
                        BossShow = Visibility.Hidden;
                    }
                }
            }


            private Visibility _BossShow = Visibility.Hidden;
            public Visibility BossShow
            {
                get => _BossShow;
                set
                {
                    _BossShow = value;
                    RefreshBoss();
                }
            }

            public double BossMaxHP
            {
                get => bigCloud?.BossMaxHP ?? 0;
                set
                {
                    if (bigCloud != null) { bigCloud.BossMaxHP = value; }
                }
            }

            public double BossHP
            {
                get => bigCloud?.BossHP ?? 0;
                set
                {
                    if (bigCloud != null) { bigCloud.BossHP = value; }
                }
            }

            public void RefreshBoss()
            {
                NotifyPropertyChanged("BossShow");
                NotifyPropertyChanged("BossMaxHP");
                NotifyPropertyChanged("BossHP");
            }
            #endregion
        }
    }
}
