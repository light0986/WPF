using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
using static 重返雲之國_外傳.Models.Entities;
using Label = System.Windows.Controls.Label;

namespace 重返雲之國_外傳.IMG.Boss
{
    /// <summary>
    /// BigCloud.xaml 的互動邏輯
    /// </summary>
    public partial class BigCloud : UserControl
    {
        private MusicPlayer music { get => data.music; set => data.music = value; }
        private readonly Data data;
        private readonly PlayerStatus player;

        public bool IsDead { get; set; } = false;
        public Action AttackSeccess;
        public Action CreateSeccess;

        public delegate void Event01(double TargetX, double TargetY, double StartX, double StartY);
        public event Event01 ShotBurstStream;
        public Action StopBurstStream;
        public Action PlayerWin;
        public Action CreateLittleCloud;

        public delegate void Event02(double StartX, double StartY);
        public Event02 CreateThunder;

        public double BossMaxHP
        {
            get => data.MaxHP;
            set => data.MaxHP = value;
        }

        public double BossHP
        {
            get => data.HP;
            set => data.HP = value;
        }

        public bool HitSuccess
        {
            get => data.HitSuccess;
            set => data.HitSuccess = value;
        }

        public double DamageToken { get => data.DamageToken; }

        public BigCloud(PlayerStatus player)
        {
            InitializeComponent();
            this.player = player;
            data = new Data(player);
            data.ShotBurstStream += (tx, ty, sx, sy) => { ShotBurstStream?.Invoke(tx, ty, sx, sy); };
            data.StopBurstStream += () => { StopBurstStream?.Invoke(); };
            data.CreateThunder += (sx, sy) => { CreateThunder?.Invoke(sx, sy); };
            data.CreateLittleCloud += () => { CreateLittleCloud?.Invoke(); };
            data.PositionX = player.CenterX - 50;
            data.PositionY = player.CenterY - 450;

            //能力
            if(player.difficulty == Difficulty.Middle)
            {
                data.MaxHP = 9000;
                data.MaxSheldHP = 10;
                data.SheldHP = 10;
                data.MonsterATK = 20;
            }
            else if (player.difficulty == Difficulty.Hard)
            {
                data.MaxHP = 10000;
                data.MaxSheldHP = 15;
                data.SheldHP = 15;
                data.MonsterATK = 25;
            }

            DataContext = data;
            Loaded += BigCloud_Loaded;
        }

        private async void BigCloud_Loaded(object sender, RoutedEventArgs e)
        {
            await SetStartPosition();
            data.OnAction = true;
            data.IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
        }

        private async Task SetStartPosition()
        {
            data.CenterX = player.CenterX;
            for (int i = 0; i < 250; i++)
            {
                data.CenterY = player.CenterY - 450 + i;
                await Task.Delay(player.DelayTime);
            }
            data.CenterY = player.CenterY - 200;
            await Task.Delay(1000);
            data.IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud03");
            double TeltaHP = data.MaxHP / 100;
            for(int i = 0; i < 100; i++)
            {
                data.HP += TeltaHP;
                await Task.Delay(player.DelayTime);
            }
            CreateSeccess?.Invoke();
        }

        public void MonsterMove()
        {
            if (data.OnAction)
            {
                CheckHit(); //怪物測試自己有沒有被打到
                CheckDead();
                MonsterAction(); //開始行動
            }
            data.RefreshView();
        }

        private void CheckHit()
        {
            if (player.OnHit)
            {
                //怪獸的位子
                double startX = data.RelativeX;
                double startY = data.RelativeY;
                double endX = data.RelativeX + 150;
                double endY = data.RelativeY + 150;

                //武器的位子
                double WstartX = player.HitX;
                double WstartY = player.HitY;
                double WendX = player.HitX + player.HitWidth;
                double WendY = player.HitY + player.HitHeight;

                //玩家的終點位子
                //起點X與終點X，有一個在武器的起點X與終點X之間
                if ((WstartX >= startX && WstartX <= endX) || (WendX >= startX && WendX <= endX) || (startX >= WstartX && startX <= WendX) || (endX >= WstartX && endX <= WendX))
                {
                    //起點Y與終點Y，有一個在武器的起點Y與終點Y之間
                    if ((WstartY >= startY && WstartY <= endY) || (WendY >= startY && WendY <= endY) || (startY >= WstartY && startY <= WendY) || (endY >= WstartY && endY <= WendY))
                    {
                        //被打到一次不會再被打到第二次
                        if (data.CanHit)
                        {
                            double damage = Math.Round(player.PlayerATK + (player.PlayerATK * player.AttackCombo / 10), 1);

                            if (player.difficulty == Difficulty.Middle || player.difficulty == Difficulty.Hard)
                            {
                                if (data.KnockDown == false)
                                {
                                    if (data.SheldStillShow == false) { ShowSheld(); }
                                    damage *= 0.5;
                                    data.SheldHP -= 1;
                                    data.HP -= damage;
                                    if (data.SheldHP == 0) { data.KnockDown = true; }
                                }
                                else
                                {
                                    if(data.ActionType == 6)
                                    {
                                        data.HP -= damage;
                                    }
                                    else
                                    {
                                        damage *= 0.5;
                                        data.HP -= 1;
                                    }
                                }
                            }
                            else
                            {
                                damage = Math.Round(player.PlayerATK + (player.PlayerATK * player.AttackCombo / 10), 1);
                                data.HP -= damage;
                            }

                            data.CanHit = false;
                            player.AttackCombo++;
                            music.PlayShort("浮雲撞人音效");
                            MonsterShake(damage);
                        }
                    }
                }
            }
            //玩家攻擊結束，所有怪獸都能再次被打
            else
            {
                data.CanHit = true;
            }
        }

        private void CheckDead()
        {
            if (BossHP <= 0)
            {
                IsDead = true;
                music.StopLong();
                music.StopMusic();
                StopBurstStream?.Invoke();
                data.DeadAnimation();
            }
        }

        private async void ShowSheld()
        {
            data.SheldStillShow = true;
            data.SheldShow = Visibility.Visible;
            await Task.Delay(1000);
            data.SheldStillShow = false;
            data.SheldShow = Visibility.Hidden;
        }

        private async void MonsterShake(double damage)
        {
            Label label = new Label
            {
                Foreground = new SolidColorBrush(Colors.Red),
                FontSize = 20,
                Content = "-" + Math.Round(damage, 1),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            };
            MainGrid.Children.Add(label);

            data.Shake = new Thickness(0, 0, 5, 0);
            label.Margin = new Thickness(0, 0, 0, 14);
            await Task.Delay(player.DelayTime);
            data.Shake = new Thickness(5, 0, 0, 0);
            label.Margin = new Thickness(0, 0, 0, 16);
            await Task.Delay(player.DelayTime);
            data.Shake = new Thickness(0, 0, 5, 0);
            label.Margin = new Thickness(0, 0, 0, 18);
            await Task.Delay(player.DelayTime);
            data.Shake = new Thickness(5, 0, 0, 0);
            label.Margin = new Thickness(0, 0, 0, 20);
            await Task.Delay(player.DelayTime);
            data.Shake = new Thickness(2.5, 0, 2.5, 0);
            MainGrid.Children.Remove(label);
        }

        private void MonsterAction()
        {
            if(IsDead == false)
            {
                //發呆
                if (data.ActionType == 0)
                {
                    data.Relax();
                }
                //跟隨玩家
                else if (data.ActionType == 1)
                {
                    data.FollowPlayer();
                }
                //攻擊玩家01 (衝刺撞擊)
                else if (data.ActionType == 2)
                {
                    data.Sprint();
                }
                //攻擊玩家02 (毀滅噴射白光)
                else if (data.ActionType == 3)
                {
                    data.BurstStream();
                }
                //攻擊玩家03 (閃電彈幕)
                else if (data.ActionType == 4)
                {
                    data.Thunder();
                }
                //攻擊玩家04 (詐欺)
                else if (data.ActionType == 5)
                {
                    data.Cheating();
                }
                //被擊倒
                else if (data.ActionType == 6)
                {
                    data.KO();
                }
            }
        }

        private class Data : MVVM
        {
            #region 主要
            public MusicPlayer music = new MusicPlayer();
            public Data(PlayerStatus player) { this.player = player; }

            private readonly PlayerStatus player;

            public BitmapImage IMG { get; set; } = (BitmapImage)Application.Current.TryFindResource("BigCloud01");

            private Random random = new Random();

            private Thickness _Shake = new Thickness(2.5, 0, 2.5, 0);
            public Thickness Shake
            {
                get => _Shake;
                set
                {
                    _Shake = value;
                    NotifyPropertyChanged("Shake");
                }
            }

            public void RefreshView()
            {
                NotifyPropertyChanged("RelativeX");
                NotifyPropertyChanged("RelativeY");
                NotifyPropertyChanged("IMG");
            }

            public bool OnAction { get; set; } = false; //可以開始被打

            private double TargetX, TargetY, StartX, StartY;

            public bool AttackRelex { get; set; } = false;

            public double DamageToken { get; set; } = 0;

            public double MonsterATK { get; set; } = 15;

            public bool HitSuccess { get; set; } = false;

            private int _ActionType = 0;
            public int ActionType
            {
                get => _ActionType;
                set
                {
                    _ActionType = value;
                    //0:休息
                    if (_ActionType == 0)
                    {
                        RelaxToken = 0;
                    }
                    //1:跟隨玩家
                    else if (_ActionType == 1)
                    {
                        MoveTime = 300;
                    }
                    //2:衝刺撞擊
                    else if (_ActionType == 2)
                    {
                        LengthToken = 0;
                        //SprintToken = 0;
                        if (player.difficulty == Difficulty.Middle)
                        {
                            if (HP / MaxHP >= 0.5) { SprintToken = 1; }
                            else { SprintToken = random.Next(2, 4); }
                        }
                        else if (player.difficulty == Difficulty.Hard)
                        {
                            if (HP / MaxHP >= 0.7) { SprintToken = 1; }
                            else if (HP / MaxHP >= 0.3 && HP / MaxHP < 0.7) { SprintToken = random.Next(2, 4); }
                            else { SprintToken = random.Next(3, 5); }
                        }
                        AttackRelex = true;
                    }
                    //3:毀滅噴射白光
                    else if (_ActionType == 3)
                    {
                        RelaxToken = 0;
                        GatherHide = Visibility.Visible;
                        IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud03");
                    }
                    //4:閃電彈幕
                    else if (_ActionType == 4)
                    {
                        CreateThunder?.Invoke(PositionX + 75, PositionY + 50);
                        RelaxToken = 0;
                        IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud03");
                    }
                    //5:詐欺
                    else if (_ActionType == 5)
                    {
                        CheatingToken = 0;
                        double XorY = random.Next(0, 3);
                        //橫移
                        if (XorY == 0)
                        {
                            TargetX = PositionY * -1;
                            TargetY = PositionX;
                        }
                        else if (XorY == 1)
                        {
                            TargetX = PositionY;
                            TargetY = PositionX * -1;
                        }
                        //後退
                        else
                        {
                            TargetX = PositionY * -1;
                            TargetY = PositionX * -1;
                        }
                        StartX = player.CenterX;
                        StartY = player.CenterY;
                    }
                    //6:被擊倒
                    else if (_ActionType == 6)
                    {
                        SheldShow = Visibility.Hidden;
                        RelaxToken = 0;
                        IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud04");
                    }
                }
            }
            #endregion

            #region 狀態
            public bool CanHit { get; set; } = false;

            public double MaxHP { get; set; } = 8000;

            public double HP { get; set; } = 0;
            #endregion

            #region 0:休息
            private int RelaxToken { get; set; } = 0;

            public void Relax() //0:發呆
            {
                PositionX = PositionX;
                PositionY = PositionY;
                RelaxToken++;
                if (RelaxToken >= 60)
                {
                    RelaxToken = 0;

                    double X = Math.Abs(player.CenterX - PositionX);
                    double Y = Math.Abs(player.CenterY - PositionY);
                    double C = Math.Sqrt((X * X) + (Y * Y));

                    int rd = random.Next(1, 101);

                    //距離太遠
                    if (C > 200)
                    {
                        if (rd >= 1 && rd <= 60) { ActionType = 1; }
                        else if (rd > 60 && rd <= 80) { ActionType = 2; }
                        else if (rd > 80 && rd <= 90) { ActionType = 3; }
                        else if (rd > 90 && rd <= 100) { ActionType = 4; }
                        //ActionType = 4;
                    }
                    //距離剛好
                    else
                    {
                        if (rd >= 1 && rd <= 20) { ActionType = 2; }
                        else if (rd > 20 && rd <= 50) { ActionType = 3; }
                        else if (rd > 50 && rd <= 60) { ActionType = 4; }
                        else if (rd > 60 && rd <= 100) { ActionType = 5; }
                        //ActionType = 4;
                    }
                }
                if (KnockDown) { ActionType = 6; }
            }
            #endregion

            #region 1:位子相關
            public double CenterX
            {
                get => PositionX + 75;
                set => PositionX = value - 75;
            }

            public double CenterY
            {
                get => PositionY + 75;
                set => PositionY = value - 75;
            }

            public double RelativeX
            {
                get => PositionX - player.StartX + 350;
            }

            public double RelativeY
            {
                get => PositionY - player.StartY + 250;
            }


            private double _PositionX = 0;
            public double PositionX
            {
                get => _PositionX;
                set
                {
                    _PositionX = value;
                    if (ActionType == 0 || ActionType == 1 || ActionType == 6)
                    {
                        if (_PositionX - player.StartX > 600)
                        {
                            _PositionX = player.StartX - 600;
                        }
                        else if (_PositionX - player.StartX < -600)
                        {
                            _PositionX = player.StartX + 600;
                        }
                    }
                }
            }


            private double _PositionY = 0;
            public double PositionY
            {
                get => _PositionY;
                set
                {
                    _PositionY = value;
                    if (ActionType == 0 || ActionType == 1 || ActionType == 6)
                    {
                        if (_PositionY - player.StartY > 600)
                        {
                            _PositionY = player.StartY - 600;
                        }
                        else if (PositionY - player.StartY < -600)
                        {
                            _PositionY = player.StartY + 600;
                        }
                    }
                }
            }

            private int MoveTime { get; set; } = 300;

            public void FollowPlayer() //1:跟隨玩家
            {
                double X = player.StartX - PositionX;
                double Y = player.StartY - PositionY;
                double absX = Math.Abs(X);
                double absY = Math.Abs(Y);
                double C = Math.Sqrt((absX * absX) + (absY * absY));
                if (C > 200)
                {
                    double XY = absX + absY;
                    if (XY != 0)
                    {
                        double LX = 3 * (X / XY);
                        PositionX += LX;

                        double LY = 3 * (Y / XY);
                        PositionY += LY;
                    }

                    MoveTime--;
                    if ((MoveTime / 20) % 2 == 1) { IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01"); }
                    else { IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud02"); }

                    if (MoveTime == 0) { ActionType = 0; }
                }
                else
                {
                    ActionType = 5;
                }

                if (KnockDown) { ActionType = 6; }
            }
            #endregion

            #region 2:衝刺撞擊
            private int LengthToken { get; set; } = 0;

            private int SprintToken { get; set; } = 0;

            public void Sprint() //2
            {
                if (AttackRelex)
                {
                    if (RelaxToken == 0)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud02");
                        HalfSecondStop();
                    }
                    if (RelaxToken > 25)
                    {
                        RelaxToken = 0;
                        AttackRelex = false;
                        IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud03");
                        Shake = new Thickness(2.5, 0, 2.5, 0);
                        TargetX = player.StartX - 25;
                        TargetY = player.StartY - 25;
                        StartX = PositionX;
                        StartY = PositionY;
                        music.StopLong();
                    }
                    else if (RelaxToken == 25)
                    {
                        if (player.difficulty == Difficulty.Hard && player.OnAvoid)
                        {
                            RelaxToken--;
                        }
                    }
                    else
                    {
                        if ((RelaxToken / 5) % 2 == 1)
                        {
                            Shake = new Thickness(0, 0, 5, 0);
                        }
                        else
                        {
                            Shake = new Thickness(5, 0, 0, 0);
                        }
                    }

                    RelaxToken++;
                }
                else
                {
                    double X = Math.Abs(StartX - TargetX);
                    double Y = Math.Abs(StartY - TargetY);

                    double XY = X + Y;
                    if (XY == 0) { XY = 1; }

                    double LX = 60 * X / XY;
                    double LY = 60 * Y / XY;

                    //衝出去
                    if (StartX >= TargetX) { PositionX -= LX; }
                    else { PositionX += LX; }

                    if (StartY >= TargetY) { PositionY -= LY; }
                    else { PositionY += LY; }
                    LengthToken++;

                    //確認有沒有打到
                    if ((375 >= RelativeX && 375 <= RelativeX + 150) || (425 >= RelativeX && 425 <= RelativeX + 150) || (RelativeX >= 375 && RelativeX + 150 <= 375) || (RelativeX >= 425 && RelativeX + 150 <= 425)) //X
                    {
                        if ((275 >= RelativeY && 275 <= RelativeY + 150) || (325 >= RelativeY && 325 <= RelativeY + 150) || (RelativeY >= 275 && RelativeY + 150 <= 275) || (RelativeY >= 325 && RelativeY + 150 <= 325)) //Y
                        {
                            if (player.OnAvoid == false && player.OnDamage == false)
                            {
                                DamageToken = MonsterATK * 2;
                                HitSuccess = true;
                                music.PlayShort("浮雲撞人音效");
                            }
                        }
                    }

                    if (LengthToken >= 8)
                    {
                        if (player.difficulty == Difficulty.Easy)
                        {
                            IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
                            HitSuccess = false;
                            ActionType = 0;
                            if (KnockDown) { ActionType = 6; }
                        }
                        else if (player.difficulty == Difficulty.Middle)
                        {
                            if (SprintToken == 0)
                            {
                                IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
                                HitSuccess = false;
                                ActionType = 0;
                                if (KnockDown) { ActionType = 6; }
                            }
                            else
                            {
                                SprintToken--;
                                LengthToken = 0;
                                AttackRelex = true;
                                RelaxToken = 0;
                            }
                        }
                        else if (player.difficulty == Difficulty.Hard)
                        {
                            if (SprintToken == 0)
                            {
                                IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
                                HitSuccess = false;
                                ActionType = 0;
                                if (KnockDown) { ActionType = 6; }
                            }
                            else
                            {
                                SprintToken--;
                                LengthToken = 0;
                                AttackRelex = true;
                                RelaxToken = 0;
                            }
                        }
                    }
                }
            }

            private async void HalfSecondStop()
            {
                try
                {
                    music.PlayLong("浮雲衝撞前音效");
                    await Task.Delay(500);
                    music.StopLong();
                }
                catch { }
            }
            #endregion

            #region 3:毀滅噴射白光
            public delegate void Event01(double TargetX, double TargetY, double StartX, double StartY);
            public event Event01 ShotBurstStream;
            public Action StopBurstStream;

            private Visibility _GatherHide = Visibility.Hidden;
            public Visibility GatherHide
            {
                get => _GatherHide;
                set
                {
                    _GatherHide = value;
                    NotifyPropertyChanged("GatherHide");
                }
            }

            private int _Gather = 0;
            public int Gather
            {
                get => _Gather;
                set
                {
                    _Gather = value;
                    NotifyPropertyChanged("Gather");
                }
            }

            public void BurstStream()
            {
                //集氣
                if (RelaxToken < 40)
                {
                    if(RelaxToken == 0) { music.PlayLong("浮雲續氣音效"); }

                    if ((RelaxToken / 5) % 2 == 1)
                    {
                        Shake = new Thickness(0, 0, 5, 0);
                    }
                    else
                    {
                        Shake = new Thickness(5, 0, 0, 0);
                    }
                    Gather += 10;
                    if(Gather == 360) { Gather = 0; }
                }
                //開始攻擊
                else if (RelaxToken == 40)
                {
                    Shake = new Thickness(2.5, 0, 2.5, 0);
                    GatherHide = Visibility.Hidden;

                    StartX = CenterX; //X中間值
                    StartY = CenterY; //Y中間值

                    double slopeX = Math.Abs(StartX - player.CenterX), slopeY = Math.Abs(StartY - player.CenterY);
                    double Lx, Ly;
                    if (slopeX != 0 && slopeY != 0)
                    {
                        double XY = slopeX + slopeY;
                        if (XY == 0) { XY = 1; }

                        Lx = Math.Round(800 / XY * slopeX , 4);
                        Ly = Math.Round(800 / XY * slopeY , 4);
                    }
                    else
                    {
                        Lx = 0;
                        Ly = 800;
                    }

                    if (player.CenterX < StartX) { TargetX = StartX - Lx; } else { TargetX = StartX + Lx; }
                    if (player.CenterY < StartY) { TargetY = StartY - Ly; } else { TargetY = StartY + Ly; }

                    music.StopLong();
                    music.PlayShort("浮雲噴射白光音效");
                    ShotBurstStream?.Invoke(TargetX, TargetY, StartX, StartY - 20);
                }
                //開始計算傷害
                if (RelaxToken >= 40 && RelaxToken < 150)
                {
                    double x1 = TargetX, x2 = StartX, x3 = player.CenterX;
                    double y1 = TargetY, y2 = StartY, y3 = player.CenterY;
                    double L = Math.Sqrt((x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3));

                    //以Boss為中心800距離內有效
                    if (L <= 800)
                    {
                        //玩家與白光的夾角必須小於90
                        if (GetAngle(x2, y2, x3, y3, x1, y1) <= 90)
                        {
                            /// 1.要計算與噴射白光的距離，距離小於等於10則判定受傷
                            /// 2.求(玩家實際位子),(白光起點),(白光終點)三點中面積，並求(玩家實際位子)到光的垂直距離?

                            //尋找總面積
                            double S = Math.Abs(((x1 - x3) * (y2 - y3)) - ((x2 - x3) * (y1 - y3))) * 0.5;

                            //玩家與brust的距離
                            double playerLen = Math.Round(S) * 2 / 800;
                            if (playerLen <= 25)
                            {
                                if (player.OnAvoid == false && player.OnDamage == false)
                                {
                                    DamageToken = MonsterATK;
                                    HitSuccess = true;
                                }
                                else
                                {
                                    DamageToken = 0;
                                    HitSuccess = false;
                                }
                            }
                        }
                    }
                }
                //攻擊結束
                else if (RelaxToken >= 150)
                {
                    StopBurstStream?.Invoke();
                    RelaxToken = 0;
                    IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
                    ActionType = 0;
                    if (KnockDown) { ActionType = 6; }
                }

                if (RelaxToken < 150)
                {
                    if (player.difficulty == Difficulty.Hard || player.difficulty == Difficulty.Middle) { RelaxToken += 2; }
                    else { RelaxToken++; }
                }
            }

            private double GetAngle(double ax, double ay, double bx, double by, double cx, double cy)
            {
                double ma_x = bx - ax;
                double ma_y = by - ay;
                double mb_x = cx - ax;
                double mb_y = cy - ay;
                double v1 = (ma_x * mb_x) + (ma_y * mb_y);
                double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
                double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
                double cosM = v1 / (ma_val * mb_val);
                double angleAMB = Math.Acos(cosM) * 180 / Math.PI;
                return angleAMB;
            }
            #endregion

            #region 4:閃電彈幕
            public delegate void Event02(double StartX, double StartY);
            public Event02 CreateThunder;

            public void Thunder()
            {
                if (RelaxToken == 0) { music.PlayShort("浮雲彈幕攻擊"); }
                RelaxToken++;
                if(RelaxToken >= 200)
                {
                    IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
                    RelaxToken = 0;
                    ActionType = 0;
                    if (KnockDown) { ActionType = 6; }
                }
            }
            #endregion

            #region 5:詐欺
            private int _AvoidToken = 360;
            public int AvoidToken
            {
                get => _AvoidToken;
                set
                {
                    _AvoidToken = value;
                    NotifyPropertyChanged("AvoidToken");
                }
            }

            private int CheatingToken = 0;
            private bool StartAvoid = false;

            public void Cheating()
            {
                //現在開始無法被攻擊
                if (CanHit) { CanHit = false; }

                //被打的時候會迴避
                if (player.OnAttack && StartAvoid == false)
                {
                    StartAvoid = true;
                    music.PlayShort("悠白迴避音效");
                    if (TargetX >= StartX) { AvoidToken = 0; }
                    else { AvoidToken = 720; }
                }

                //開始迴避: Target是玩家, Start是Boss
                double X = Math.Abs(StartX - TargetX);
                double Y = Math.Abs(StartY - TargetY);
                double absX = Math.Abs(X);
                double absY = Math.Abs(Y);
                double XY = absX + absY;
                if (XY == 0) { XY = 1; }

                if (StartAvoid)
                {
                    double LY = 4 * (Y / XY);
                    if (TargetY >= StartY) { PositionY += LY; }
                    else { PositionY -= LY; }

                    double LX = 4 * (X / XY);
                    if (TargetX >= StartX)
                    {
                        PositionX += LX;
                        AvoidToken += 20;
                    }
                    else
                    {
                        PositionX -= LX;
                        AvoidToken -= 20;
                    }

                    //滾動中且360
                    if (StartAvoid && AvoidToken == 360)
                    {
                        //ActionType = random.Next(2, 5);
                        StartAvoid = false;
                        CheatingToken = 0;
                        CanHit = true;
                        ActionType = random.Next(2,4);
                    }
                }
                //沒有攻擊時會垂直移動
                else
                {
                    double LY = 2 * (Y / XY);
                    if (TargetY >= StartY) { PositionY += LY; }
                    else { PositionY -= LY; }

                    double LX = 2 * (X / XY);
                    if (TargetX >= StartX) { PositionX += LX; }
                    else { PositionX -= LX; }

                    //增加時間
                    CheatingToken++;
                    //時間到會發動攻擊
                    if (CheatingToken >= 150)
                    {
                        CheatingToken = 0;
                        //ActionType = random.Next(2, 5);
                        ActionType = 0;
                        CanHit = true;
                        if (KnockDown) { ActionType = 6; }
                    }
                }
            }
            #endregion

            #region 6:被擊倒
            private Visibility _SheldShow = Visibility.Hidden;
            public Visibility SheldShow
            {
                get => _SheldShow;
                set
                {
                    _SheldShow = value;
                    NotifyPropertyChanged("SheldShow");
                }
            }

            private int _MaxSheldHP = 0;
            public int MaxSheldHP
            {
                get => _MaxSheldHP;
                set
                {
                    _MaxSheldHP = value;
                    if(_MaxSheldHP <= 0) { _MaxSheldHP = 0; }
                    NotifyPropertyChanged("MaxSheldHP");
                }
            }

            private int _SheldHP = 0;
            public int SheldHP
            {
                get => _SheldHP;
                set
                {
                    _SheldHP = value;
                    if (_SheldHP > MaxSheldHP) { _SheldHP = MaxSheldHP; }
                    if (_SheldHP <= 0) { _SheldHP = 0; }
                    NotifyPropertyChanged("SheldHP");
                }
            }

            public bool KnockDown { get; set; } = false;

            public bool SheldStillShow = false;

            public Action CreateLittleCloud;

            public void KO()
            {
                PositionX = PositionX;
                PositionY = PositionY;
                RelaxToken++;
                if (RelaxToken >= 250)
                {
                    CreateLittle();
                    IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud01");
                    RelaxToken = 0;
                    ActionType = 2;
                    KnockDown = false;
                    SheldHP = MaxSheldHP;
                }
            }

            private void CreateLittle()
            {
                if (player.difficulty == Difficulty.Hard)
                {
                    CreateLittleCloud?.Invoke();
                }
            }

            public async void DeadAnimation()
            {
                do
                {
                    IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud05");
                    NotifyPropertyChanged("IMG");
                    await Task.Delay(500);
                    IMG = (BitmapImage)Application.Current.TryFindResource("BigCloud04");
                    NotifyPropertyChanged("IMG");
                    await Task.Delay(500);
                }
                while (true);
            }
            #endregion

            private string _TestText = "";
            public string TestText
            {
                get => _TestText;
                set
                {
                    _TestText = value;
                    NotifyPropertyChanged("TestText");
                }
            }
        }
    }
}
