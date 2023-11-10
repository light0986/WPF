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

namespace 重返雲之國_外傳.IMG.Status
{
    /// <summary>
    /// StatusUp.xaml 的互動邏輯
    /// </summary>
    public partial class StatusUp : UserControl
    {
        private MusicPlayer music = new MusicPlayer();
        private Data data;
        public Action Complete;
        public Action IsFull;
        private bool OnWork = true;

        public StatusUp(PlayerStatus player)
        {
            InitializeComponent();
            data = new Data(player);
            DataContext = data;
            Loaded += StatusUp_Loaded;
        }

        private async void StatusUp_Loaded(object sender, RoutedEventArgs e)
        {
            data.Margin = new Thickness(700, 0, -700, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(600, 0, -600, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(500, 0, -500, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(400, 0, -400, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(300, 0, -300, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(200, 0, -200, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(100, 0, -100, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(0, 0, 0, 0);
            OnWork = false;
            StatusFull();
        }

        public void KeyPress(Key key)
        {
            if (key == Key.Up)
            {
                music.PlayShort("游標移動音效");
                data.PressUp();
            }
            if (key == Key.Down)
            {
                music.PlayShort("游標移動音效");
                data.PressDown();
            }
            if (key == Key.Left)
            {
                music.PlayShort("游標移動音效");
                data.PressLeft();
            }
            if (key == Key.Right)
            {
                music.PlayShort("游標移動音效");
                data.PressRight();
            }
            if (key == Key.Enter)
            {
                if (data.SelectIndex == 1) { SAN_Click(); }
                else if (data.SelectIndex == 2) { AP_Click(); }
                else if (data.SelectIndex == 3) { Speed_Click(); }
                else if (data.SelectIndex == 4) { ATK_Click(); }
                else if (data.SelectIndex == 5) { Cure_Click(); }
                else if (data.SelectIndex == 6) { CD_Click(); }
            }
        }

        private void SAN_Click()
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.player.PlayerMaxHP *= 1.2;
                GoOut();
            }
        }

        private void AP_Click()
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.player.PlayerMaxAP *= 1.2;
                data.player.PlayerAP = data.player.PlayerMaxAP;
                GoOut();
            }
        }

        private void Speed_Click()
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.player.MoveSpeed += 0.5;
                GoOut();
            }
        }

        private void ATK_Click()
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.player.PlayerATK += 1;
                GoOut();
            }
        }

        private void Cure_Click()
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.player.Recover *= 1.1;
                data.player.Amount += 1;
                GoOut();
            }
        }

        private void CD_Click()
        {
            if (OnWork == false)
            {
                OnWork = true;
                data.player.PowderCD += 0.5;
                GoOut();
            }
        }

        private void StatusFull()
        {
            //用到機率極低，但不是不可能
            if (data.SANEnable == false && data.APEnable == false && data.SpeedEnable == false
            && data.ATKEnable == false && data.CureEnable == false && data.CDEnable == false)
            {
                IsFull?.Invoke();
                GoOut();
            }
        }

        private async void GoOut()
        {
            music.PlayShort("悠白能力提升音效");
            data.Margin = new Thickness(-100, 0, 100, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-200, 0, 200, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-300, 0, 300, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-400, 0, 400, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-500, 0, 500, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-600, 0, 600, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-700, 0, 700, 0);
            await Task.Delay(20);
            data.Margin = new Thickness(-800, 0, 800, 0);
            await Task.Delay(20);
            Complete?.Invoke();
        }

        private class Data : MVVM
        {
            public PlayerStatus player;

            public Data(PlayerStatus player)
            {
                this.player = player;
                if (SANEnable) { SelectIndex = 1; }
                else
                {
                    if (APEnable) { SelectIndex = 2; }
                    else
                    {
                        if (SpeedEnable) { SelectIndex = 3; }
                        else
                        {
                            if (ATKEnable) { SelectIndex = 4; }
                            else
                            {
                                if (CureEnable) { SelectIndex = 5; }
                                else
                                {
                                    SelectIndex = 6;
                                }
                            }
                        }
                    }
                }
            }


            private Thickness _Margin = new Thickness(800, 0, 0, 0);
            public Thickness Margin
            {
                get => _Margin;
                set
                {
                    _Margin = value;
                    NotifyPropertyChanged("Margin");
                }
            }


            private int _SelectIndex = 0;
            public int SelectIndex
            {
                get => _SelectIndex;
                set
                {
                    _SelectIndex = value;
                    AllUnSelect();
                    if (_SelectIndex == 1) { SANThickness = new Thickness(2); }
                    else if (_SelectIndex == 2) { APThickness = new Thickness(2); }
                    else if (_SelectIndex == 3) { SpeedThickness = new Thickness(2); }
                    else if (_SelectIndex == 4) { ATKThickness = new Thickness(2); }
                    else if (_SelectIndex == 5) { CureThickness = new Thickness(2); }
                    else if (_SelectIndex == 6) { CDThickness = new Thickness(2); }
                }
            }

            private void AllUnSelect()
            {
                SANThickness = new Thickness(0);
                APThickness = new Thickness(0);
                SpeedThickness = new Thickness(0);
                ATKThickness = new Thickness(0);
                CureThickness = new Thickness(0);
                CDThickness = new Thickness(0);
            }

            #region 1 Max:1000
            public bool SANEnable
            {
                get
                {
                    if (player.PlayerMaxHP >= 1000)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            private Thickness _SANThickness = new Thickness(0);
            public Thickness SANThickness
            {
                get => _SANThickness;
                set
                {
                    _SANThickness = value;
                    NotifyPropertyChanged("SANThickness");
                }
            }
            #endregion

            #region 2 Max:1000
            public bool APEnable
            {
                get
                {
                    if (player.PlayerMaxAP >= 1000)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            private Thickness _APThickness = new Thickness(0);
            public Thickness APThickness
            {
                get => _APThickness;
                set
                {
                    _APThickness = value;
                    NotifyPropertyChanged("APThickness");
                }
            }
            #endregion

            #region 3 Max:10
            public bool SpeedEnable
            {
                get
                {
                    if (player.MoveSpeed >= 10)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            private Thickness _SpeedThickness = new Thickness(0);
            public Thickness SpeedThickness
            {
                get => _SpeedThickness;
                set
                {
                    _SpeedThickness = value;
                    NotifyPropertyChanged("SpeedThickness");
                }
            }
            #endregion

            #region 4 ATK Max:必須低於AP÷2
            public bool ATKEnable
            {
                get
                {
                    if((player.PlayerATK * 2) >= player.PlayerMaxAP)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            private Thickness _ATKThickness = new Thickness(0);
            public Thickness ATKThickness
            {
                get => _ATKThickness;
                set
                {
                    _ATKThickness = value;
                    NotifyPropertyChanged("ATKThickness");
                }
            }
            #endregion

            #region 5 Cure Max:8
            public bool CureEnable
            {
                get
                {
                    if (player.Recover >= 8)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            private Thickness _CureThickness = new Thickness(0);
            public Thickness CureThickness
            {
                get => _CureThickness;
                set
                {
                    _CureThickness = value;
                    NotifyPropertyChanged("CureThickness");
                }
            }
            #endregion

            #region 6 CD Max:10
            public bool CDEnable
            {
                get
                {
                    if (player.PowderCD >= 10)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            private Thickness _CDThickness = new Thickness(0);
            public Thickness CDThickness
            {
                get => _CDThickness;
                set
                {
                    _CDThickness = value;
                    NotifyPropertyChanged("CDThickness");
                }
            }
            #endregion

            public void PressUp()
            {
                if (SelectIndex == 1 && ATKEnable) { SelectIndex = 4; }
                else if (SelectIndex == 2 && CureEnable) { SelectIndex = 5; }
                else if (SelectIndex == 3 && CDEnable) { SelectIndex = 6; }
                else if (SelectIndex == 4 && SANEnable) { SelectIndex = 1; }
                else if (SelectIndex == 5 && APEnable) { SelectIndex = 2; }
                else if (SelectIndex == 6 && SpeedEnable) { SelectIndex = 3; }
            }

            public void PressDown()
            {
                if (SelectIndex == 1 && ATKEnable) { SelectIndex = 4; }
                else if (SelectIndex == 2 && CureEnable) { SelectIndex = 5; }
                else if (SelectIndex == 3 && CDEnable) { SelectIndex = 6; }
                else if (SelectIndex == 4 && SANEnable) { SelectIndex = 1; }
                else if (SelectIndex == 5 && APEnable) { SelectIndex = 2; }
                else if (SelectIndex == 6 && SpeedEnable) { SelectIndex = 3; }
            }

            public void PressLeft()
            {
                for (int i = SelectIndex; i >= -1; i--)
                {
                    if (i == 2 && SANEnable) { SelectIndex = 1; break; }
                    else if (i == 3 && APEnable) { SelectIndex = 2; break; }
                    else if (i == 4 && SpeedEnable) { SelectIndex = 3; break; }
                    else if (i == 5 && ATKEnable) { SelectIndex = 4; break; }
                    else if (i == 6 && CureEnable) { SelectIndex = 5; break; }
                    else if (i == 1 && CDEnable) { SelectIndex = 6; break; }
                    if (i == 0) { i = 7; }
                }
            }

            public void PressRight()
            {
                for (int i = SelectIndex; i < 7; i++)
                {
                    if (i == 6 && SANEnable) { SelectIndex = 1; break; }
                    else if (i == 1 && APEnable) { SelectIndex = 2; break; }
                    else if (i == 2 && SpeedEnable) { SelectIndex = 3; break; }
                    else if (i == 3 && ATKEnable) { SelectIndex = 4; break; }
                    else if (i == 4 && CureEnable) { SelectIndex = 5; break; }
                    else if (i == 5 && CDEnable) { SelectIndex = 6; break; }
                    if (i == 6) { i = 0; }
                }
            }
        }
    }
}
