using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static 重返雲之國_外傳.Models.Entities;

namespace 重返雲之國_外傳.Models
{
    /// <summary>
    /// 可改變的能力
    /// 1.MaxHP
    /// 2.MaxAP
    /// 3.PlayerATK
    /// 4.MoveSpeed
    /// 5.回血量
    /// </summary>

    public class PlayerStatus
    {
        public int PlayerHeight { get; set; } = 100;

        public int PlayerWidth { get; set; } = 50;

        #region 可改變狀態
        //等級
        public int Level { get; set; } = 1;

        //敏捷最快10，起始5，攻擊速度 + 移動速度
        private double _MoveSpeed = 5;
        public double MoveSpeed
        {
            get => _MoveSpeed;
            set
            {
                _MoveSpeed = value;
                if (_MoveSpeed >= 10) { _MoveSpeed = 10; }
                else if (_MoveSpeed <= 0) { _MoveSpeed = 1; }
            }
        }

        //HP最多1000，起始100
        private double _PlayerMaxHP = 100;
        public double PlayerMaxHP
        {
            get => _PlayerMaxHP;
            set
            {
                _PlayerMaxHP = value;
                if (PlayerMaxHP > 1000) { PlayerMaxHP = 1000; }
                if (PlayerMaxHP < 0) { PlayerMaxHP = 1; }
            }
        }

        private double _PlayerHP = 100;
        public double PlayerHP
        {
            get => _PlayerHP;
            set
            {
                _PlayerHP = value;
                if (PlayerHP > PlayerMaxHP) { PlayerHP = PlayerMaxHP; }
                if (PlayerHP < 0) { PlayerHP = 0; }
            }
        }

        //AP最多1000，起始100
        private double _PlayerMaxAP = 100;
        public double PlayerMaxAP
        {
            get => _PlayerMaxAP;
            set
            {
                _PlayerMaxAP = value;
                if (_PlayerMaxAP > 1000) { _PlayerMaxAP = 1000; }
                if (_PlayerMaxAP < 0) { _PlayerMaxAP = 1; }
            }
        }

        private double _PlayerAP = 100;
        public double PlayerAP
        {
            get => _PlayerAP;
            set
            {
                _PlayerAP = value;
                if (_PlayerAP > PlayerMaxAP) { _PlayerAP = PlayerMaxAP; }
                if (_PlayerAP < 0) { _PlayerAP = 0; }
            }
        }

        //EXP，起始100
        private double _PlayerMaxExp = 100;
        public double PlayerMaxExp
        {
            get => _PlayerMaxExp;
            set => _PlayerMaxExp = value;
        }

        private double _PlayerExp = 0;
        public double PlayerExp
        {
            get=> _PlayerExp;
            set => _PlayerExp = value;
        }

        //連擊數量
        private double _AttackCombo = 0;
        public double AttackCombo
        {
            get => _AttackCombo;
            set
            {
                _AttackCombo = value;
                if (_AttackCombo > 10) { _AttackCombo = 10; }
            }
        }

        //傷害，起始10
        private double _PlayerATK = 10;
        public double PlayerATK
        {
            get => _PlayerATK;
            set
            {
                _PlayerATK = value;
                if (_PlayerATK > 500) { _PlayerATK = 500; }
            }
        }

        //回血量，起始0.5
        private double _Recover = 0.5;
        public double Recover
        {
            get => _Recover;
            set
            {
                _Recover = value;
                if(_Recover > 8) { _Recover = 8; }
            }
        }

        //持有量，起始5
        private double _Amount = 5;
        public double Amount
        {
            get => _Amount;
            set
            {
                _Amount = value;
                if(_Amount > 20) { _Amount = 20; }
            }
        }

        //白粉CD，起始1
        private double _PowderCD = 1;
        public double PowderCD
        {
            get => _PowderCD;
            set
            {
                _PowderCD = value;
                if (_PowderCD > 10) { _PowderCD = 10; }
            }
        }
        #endregion

        #region 玩家實際位子
        private double _CenterX = 10000;
        public double CenterX
        {
            get => _CenterX;
            set => _CenterX = value;
        }

        private double _CenterY = 10000;
        public double CenterY
        {
            get => _CenterY;
            set => _CenterY = value;
        }

        public double StartX { get => CenterX - (PlayerHeight / 2); }

        public double StartY { get => CenterY - (PlayerWidth / 2); }

        public double EndX { get => CenterX + (PlayerHeight / 2); }

        public double EndY { get => CenterY + (PlayerWidth / 2); }
        #endregion

        #region 玩家攻擊位置
        public bool OnHit { get; set; } = false;

        public bool OnAttack { get; set; } = false;

        public double HitWidth { get; set; } = 150;

        public double HitHeight { get; set; } = 50;


        private double _HitX = 325;
        public double HitX
        {
            get => _HitX;
            set
            {
                _HitX = value;
            }
        }

        private double _HitY = 350;
        public double HitY
        {
            get => _HitY;
            set
            {
                _HitY = value;
            }
        }
        #endregion

        #region 玩家其他狀態
        public bool OnAvoid { get; set; } = false;

        public bool OnDamage { get; set; } = false;

        public bool StatusUp { get; set; } = false;

        public bool IsAlive { get; set; } = true;


        private Difficulty _difficulty = Difficulty.Easy;
        public Difficulty difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                if(_difficulty == Difficulty.Easy)
                {
                    ContinueTime = -1;
                }
                else if (_difficulty == Difficulty.Middle)
                {
                    ContinueTime = 3;
                }
                else if (_difficulty == Difficulty.Hard)
                {
                    ContinueTime = 0;
                }
            }
        }

        public int ContinueTime { get; set; } = -1;

        public int DelayTime { get; set; } = 20;
        #endregion
    }
}
