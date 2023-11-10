using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
using System.Windows.Shell;
using 重返雲之國_外傳.Models;

namespace 重返雲之國_外傳.IMG.Enemies
{
    /// <summary>
    /// LittleCloud.xaml 的互動邏輯
    /// 玩家太遠時，追逐玩家
    /// 玩家在攻擊範圍內時，攻擊玩家，或發呆3秒鐘。
    /// 攻擊玩家有兩種模式:
    /// 1、往前跳，撞到玩家會反彈，共撞3次。
    ///    第2次與第3次會刻意等待玩家閃躲完畢。
    /// 2、續力，然後衝向玩家，然後僵直。
    /// 
    /// 血量低於一半時，玩家太遠會補血25%
    /// 玩家在攻擊範圍時，有機率補血25%
    /// 
    /// </summary>
    public partial class LittleCloud : UserControl
    {
        private MusicPlayer music { get => data.music; set => data.music = value; }
        private readonly Data data;
        private readonly PlayerStatus player;
        public bool IsDead = false;
        public Action AttackSeccess;

        public bool HitSuccess
        {
            get => data.HitSuccess;
            set => data.HitSuccess = value;
        }

        public double DamageToken { get => data.DamageToken; }

        public LittleCloud(PlayerStatus player)
        {
            InitializeComponent();
            this.player = player;
            data = new Data(player);
            DataContext = data;
            SetStartPosition();
            Loaded += LittleCloud_Loaded;
        }

        private void LittleCloud_Loaded(object sender, RoutedEventArgs e)
        {
            if (player.difficulty == Entities.Difficulty.Middle)
            {
                data.MaxHP = 90;
                data.HP = 90;
            }
            else if(player.difficulty == Entities.Difficulty.Hard)
            {
                data.MaxHP = 100;
                data.HP = 100;
            }
            data.Alive = true;            
        }

        private void SetStartPosition()
        {
            Random rd = new Random();
            int i = rd.Next(0, 251); //0~250
            int j = 250 - i; //補
            int k = (rd.Next() & 2) - 1;
            int l = (rd.Next() & 2) - 1;
            data.PositionX = player.CenterX + (i * k);
            data.PositionY = player.CenterY - (j * l);
        }

        public void MonsterMove()
        {
            //開始行動
            if (data.OnAction)
            {
                CheckHit(); //怪物測試自己有沒有被打到
                MonsterAction(); //開始行動
            }
            else
            {
                if (data.Alive == false)
                {
                    IsDead = true;
                    music?.StopMusic();
                    music?.StopLong();
                    music = null;
                    Visibility = Visibility.Hidden;
                }
            }
            data.RefreshView();
        }

        public void CheckHit()
        {
            if (player.OnHit)
            {
                //怪獸的位子
                double startX = data.RelativeX;
                double startY = data.RelativeY;
                double endX = data.RelativeX + 50;
                double endY = data.RelativeY + 50;

                double WstartX = player.HitX;
                double WstartY = player.HitY;
                double WendX = player.HitX + player.HitWidth;
                double WendY = player.HitY + player.HitHeight;

                //玩家的終點位子
                //起點X與終點X，有一個在武器的起點X與終點X之間
                if ((startX >= WstartX && startX <= WendX) || (endX >= WstartX && endX <= WendX) || (WstartX >= startX && WstartX <= endX) || (WendX >= startX && WendX <= endX))
                {
                    //起點Y與終點Y，有一個在武器的起點Y與終點Y之間
                    if ((startY >= WstartY && startY <= WendY) || (endY >= WstartY && endY <= WendY) || (WstartY >= startY && WstartY <= endY) || (WendY >= startY && WendY <= endY))
                    {
                        //被打到一次不會再被打到第二次
                        if (data.CanHit)
                        {
                            double damage = Math.Round(player.PlayerATK + (player.PlayerATK * player.AttackCombo / 10), 1);
                            data.HP -= damage;
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

        public void AddLevel(int i)
        {
            //怪物每階等級增加2血
            data.MaxHP += (i * 2);
            data.HP += (i * 2);
        }

        private async void MonsterShake(double damage)
        {
            Label label = new Label
            {
                Foreground = new SolidColorBrush(Colors.Red),
                FontSize = 12,
                Content = "-" + Math.Round(damage, 1),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10 )
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
            //攻擊玩家01
            else if (data.ActionType == 2)
            {
                if (player.OnAvoid == false || data.OnAttack)
                {
                    data.OnAttack = true;
                    data.Attack01();
                }
                else { data.ActionType = 2; }
            }
            //攻擊玩家02
            else if (data.ActionType == 3)
            {
                data.OnAttack = true;
                data.Attack02();
            }
            //補血
            else if (data.ActionType == 4)
            {
                data.Cure();
            }
        }

        private class Data : MVVM
        {
            #region 主要
            public Data(PlayerStatus player) { this.player = player; }

            public MusicPlayer music { get; set; } = new MusicPlayer();

            private readonly PlayerStatus player = new PlayerStatus();

            public BitmapImage IMG { get; set; } = (BitmapImage)Application.Current.TryFindResource("LittleCloud03");

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

            private int _ActionType = 0;
            public int ActionType
            {
                get => _ActionType;
                set
                {
                    _ActionType = value;
                    if (_ActionType == 0)
                    {
                        RelaxToken = 0;
                    }
                    else if (_ActionType == 1)
                    {
                        MoveTime = 300;
                    }
                    else if (_ActionType == 2)
                    {
                        AttackToken = 0;
                        LengthToken = 0;
                    }
                    else if (_ActionType == 3) { }
                }
            }
            #endregion

            #region 血
            public bool CanHit { get; set; } = false;

            private double _MaxHP = 80;
            public double MaxHP
            {
                get => _MaxHP;
                set
                {
                    _MaxHP = value;
                    if (_MaxHP <= 0) { _MaxHP = 1; }
                    NotifyPropertyChanged("MaxHP");
                }
            }


            private double _HP = 80;
            public double HP
            {
                get => _HP;
                set
                {
                    _HP = value;
                    if (_HP > MaxHP) { _HP = MaxHP; }
                    if (_HP <= 0) { _HP = 0; GoDie(); }
                    NotifyPropertyChanged("HP");
                }
            }


            private Visibility _Show = Visibility.Hidden;
            public Visibility Show
            {
                get => _Show;
                set
                {
                    _Show = value;
                    NotifyPropertyChanged("Show");
                }
            }
            #endregion

            #region 生存狀態
            private bool _Alive = true;
            public bool Alive
            {
                get => _Alive;
                set
                {
                    _Alive = value;
                    if (_Alive)
                    {
                        GoALive();
                    }
                }
            }


            private double _Transparency = 1.0;
            public double Transparency
            {
                get => _Transparency;
                set
                {
                    _Transparency = value;
                    NotifyPropertyChanged("Transparency");
                }
            }

            private async void GoALive()
            {
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud03");
                await Task.Delay(500);
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud04");
                await Task.Delay(500);
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud03");
                await Task.Delay(500);
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud04");
                await Task.Delay(500);
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud03");
                await Task.Delay(500);
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud04");
                await Task.Delay(500);
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01");
                Show = Visibility.Visible;
                OnAction = true;
                CanHit = true;
            }

            private async void GoDie()
            {
                music.StopLong();
                music.PlayShort("浮雲消失音效");
                IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud05");
                OnAction = false;
                CanHit = false;
                Show = Visibility.Hidden;
                for (double i = 1.0; i >= 0; i -= 0.1)
                {
                    Transparency = i;
                    await Task.Delay(32);
                }
                music = null;
                Alive = false;
            }
            #endregion

            #region 位子相關
            public double RelativeY
            {
                get => PositionY - player.StartY + 275;
            }
            public double RelativeX
            {
                get => PositionX - player.StartX + 375;
            }



            private double _PositionX = 0;
            public double PositionX
            {
                get => _PositionX;
                set
                {
                    _PositionX = value;
                    if (ActionType == 1 || ActionType == 4)
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
                    if (ActionType == 1 || ActionType == 4)
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

            public void FollowPlayer() //1
            {
                double X = player.StartX - PositionX;
                double Y = player.StartY - PositionY;
                double absX = Math.Abs(X);
                double absY = Math.Abs(Y);
                double C = Math.Sqrt((absX * absX) + (absY * absY));
                if (C > 150)
                {
                    double XY = absX + absY;
                    if(XY != 0)
                    {
                        double LX = 3 * (X / XY);
                        PositionX += LX;

                        double LY = 3 * (Y / XY);
                        PositionY += LY;
                    }

                    MoveTime--;
                    if ((MoveTime / 20) % 2 == 1) { IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01"); }
                    else { IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud02"); }

                    if (MoveTime == 0) { ActionType = 3; }
                }
                else if (C <= 150)
                {
                    ActionType = 0;
                }
            }
            #endregion

            #region 攻擊
            private double TargetX, TargetY, StartX, StartY;

            private int AttackToken { get; set; } = 0;

            public double DamageToken { get; set; } = 0;

            private int LengthToken { get; set; } = 0;

            public bool HitSuccess { get; set; } = false;

            public bool JumpBack { get; set; } = false;

            public bool OnAttack { get; set; } = false;

            public bool AttackRelex { get; set; } = false;

            public double MonsterATK { get; set; } = 5;

            public void Attack01() //2:撞三次
            {
                if (AttackRelex) //休息一下
                {
                    if (RelaxToken >= 25)
                    {
                        //找尋新的位子
                        JumpBack = false;
                        LengthToken = 0;
                        TargetX = player.StartX;
                        TargetY = player.StartY;
                        StartX = PositionX;
                        StartY = PositionY;

                        AttackRelex = false;
                        RelaxToken = 0;
                        IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud05");
                    }
                    RelaxToken++;
                }
                else
                {
                    double X = Math.Abs(StartX - TargetX);
                    double Y = Math.Abs(StartY - TargetY);

                    double XY = X + Y;
                    if (XY == 0) { XY = 1; }

                    double LX = 25 * X / XY;
                    double LY = 25 * Y / XY;

                    //衝出去或衝回來
                    if (JumpBack == false)
                    {
                        //衝出去
                        if (StartX >= TargetX) { PositionX -= LX; }
                        else { PositionX += LX; }

                        if (StartY >= TargetY) { PositionY -= LY; }
                        else { PositionY += LY; }
                        LengthToken++;

                        //確認有沒有打到，有打到會退5個距離
                        if ((375 >= RelativeX && 375 <= RelativeX + 50) || (425 >= RelativeX && 425 <= RelativeX + 50) || (RelativeX >= 375 && RelativeX + 50 <= 375) || (RelativeX >= 425 && RelativeX + 50 <= 425)) //X
                        {
                            if ((275 >= RelativeY && 275 <= RelativeY + 50) || (325 >= RelativeY && 325 <= RelativeY + 50) || (RelativeY >= 275 && RelativeY + 50 <= 275) || (RelativeY >= 325 && RelativeY + 50 <= 325)) //Y
                            {
                                if (player.OnAvoid == false && player.OnDamage == false)
                                {
                                    DamageToken = MonsterATK + AttackToken;
                                    HitSuccess = true;
                                    music.PlayShort("浮雲撞人音效");
                                }
                            }
                        }

                        //有打到會反彈
                        if (HitSuccess == true)
                        {
                            JumpBack = true;
                        }
                        //沒打到回到位子上休息
                        else
                        {
                            if (LengthToken >= 5)
                            {
                                LengthToken = 0;
                            }
                        }
                    }
                    else
                    {
                        //返回
                        if (StartX >= TargetX) { PositionX += LX; }
                        else { PositionX -= LX; }

                        if (StartY >= TargetY) { PositionY += LY; }
                        else { PositionY -= LY; }
                        LengthToken--;

                        //確認回來了沒?
                        if (LengthToken == 0)
                        {
                            //休息一下
                            HitSuccess = false;
                            JumpBack = false;
                        }
                    }

                    //計算攻擊三次 或 沒打到?
                    if (LengthToken == 0)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01");
                        if (AttackToken >= 2)
                        {
                            AttackToken = 0;
                            ActionType = 0;
                            OnAttack = false;
                        }
                        else
                        {
                            AttackRelex = true;
                            AttackToken++;
                        }
                    }
                }
            }

            public void Attack02() //3:衝出去
            {
                if (AttackRelex)
                {
                    if (RelaxToken == 0)
                    {
                        IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01");
                        HalfSecondStop();
                    }
                    if (RelaxToken >= 25)
                    {
                        RelaxToken = 0;
                        LengthToken = 0;
                        AttackRelex = false;
                        IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud05");
                        Shake = new Thickness(2.5, 0,2.5, 0);
                        TargetX = player.StartX;
                        TargetY = player.StartY;
                        StartX = PositionX;
                        StartY = PositionY;
                        music.StopLong();
                    }
                    else
                    {
                        if((RelaxToken / 5) % 2 == 1)
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
                    IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud05");

                    double X = Math.Abs(StartX - TargetX);
                    double Y = Math.Abs(StartY - TargetY);

                    double XY = X + Y;
                    if (XY == 0) { XY = 1; }

                    double LX = 50 * X / XY;
                    double LY = 50 * Y / XY;

                    //衝出去
                    if (StartX >= TargetX) { PositionX -= LX; }
                    else { PositionX += LX; }

                    if (StartY >= TargetY) { PositionY -= LY; }
                    else { PositionY += LY; }
                    LengthToken++;

                    //確認有沒有打到: 數字為玩家
                    if ((375 >= RelativeX && 375 <= RelativeX + 50) || (425 >= RelativeX && 425 <= RelativeX + 50) || (RelativeX >= 375 && RelativeX + 50 <= 375) || (RelativeX >= 425 && RelativeX + 50 <= 425)) //X
                    {
                        if ((275 >= RelativeY && 275 <= RelativeY + 50) || (325 >= RelativeY && 325 <= RelativeY + 50) || (RelativeY >= 275 && RelativeY + 50 <= 275) || (RelativeY >= 325 && RelativeY + 50 <= 325)) //Y
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
                        IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud02");
                        HitSuccess = false;
                        ActionType = 0;
                        OnAttack = false;
                    }
                }
            }

            private async void HalfSecondStop()
            {
                try
                {
                    music.PlayLong("浮雲衝撞前音效");
                    await Task.Delay(500);
                    music?.StopLong();
                }
                catch { }
            }
            #endregion

            #region 回血
            private double CureHP { get; set; } = 0;

            public void Cure()
            {
                PositionX = PositionX;
                PositionY = PositionY;
                if (CureHP > 0)
                {
                    HP ++;
                    CureHP --;
                    if (HP == MaxHP) { CureHP = 0; }
                }
                else
                {
                    ActionType = 0;
                    IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud01");
                }
            }
            #endregion

            #region 休息
            private int RelaxToken { get; set; } = 0;

            public void Relax() //4
            {
                PositionX = PositionX;
                PositionY = PositionY;
                RelaxToken++;
                if (RelaxToken >= 20)
                {
                    RelaxToken = 0;

                    double X = Math.Abs(player.StartX - PositionX);
                    double Y = Math.Abs(player.StartY - PositionY);
                    double absX = Math.Abs(X);
                    double absY = Math.Abs(Y);
                    double C = Math.Sqrt((absX * absX) + (absY * absY));

                    //距離太遠
                    if (C > 150)
                    {
                        //血量低於，回半血
                        if (HP / MaxHP <= 0.25 && random.Next(0, 4) == 0)
                        {
                            IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud06");
                            CureHP = MaxHP / 4;
                            ActionType = 4;
                        }
                        //血量充足找玩家
                        else
                        {
                            int rd = random.Next(0, 5);
                            if(rd == 0)
                            {
                                ActionType = 3;
                                AttackRelex = true;
                            }
                            else
                            {
                                ActionType = 1;
                            }
                        }
                    }
                    //距離剛好
                    else
                    {
                        //用特定方法打玩家或是補血
                        if (HP != MaxHP) { ActionType = random.Next(2, 5); }
                        else { ActionType = random.Next(2, 4); }

                        if (ActionType == 4)
                        {
                            IMG = (BitmapImage)Application.Current.TryFindResource("LittleCloud06");
                            CureHP = MaxHP / 10;
                        }
                        else if (ActionType == 3 || ActionType == 2) { AttackRelex = true; }
                    }
                }
            }
            #endregion
        }
    }
}
