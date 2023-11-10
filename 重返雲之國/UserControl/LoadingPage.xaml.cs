using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
    /// 因為SoundPlayer無法多聲音執行，必須要用MediaPlayer。
    /// 但MediaPlayer卻無法讀取Properties.Resources內的wav檔。
    /// 這也許跟Audio演算法的限制有關，
    /// 但我不清楚為什麼微軟到現在都不改善就是了。
    /// 
    /// 因此需要使用這個Page來把Properties.Resources內的wav檔，轉成Stream，
    /// 接著把Stream寫入玩家電腦內，儲存成wav檔讓MediaPlayer可以讀取，作為必要檔案。
    /// 也就是這個遊戲檔有兼任安裝檔的功能。
    /// 
    /// 於此同時，此頁面也要用來當作轉場時的Loading畫面，
    /// 此Loading的過程，也是要確保需要的檔案，是完整存在的。
    /// </summary>
    public partial class LoadingPage : UserControl
    {
        public Action LoadingComplete;

        private readonly string path;
        private readonly string FolderName = "BGM";
        private Data data;
        private int GameStep { get; set; }

        public LoadingPage(int GameStep)
        {
            InitializeComponent();
            this.GameStep = GameStep;
            data = new Data();
            DataContext = data;
            path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            Loaded += LoadingPage_Loaded;
        }

        private void LoadingPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (GameStep == 1 || GameStep == 0) { ProcessOne(); }
            else if (GameStep == 2) { ProcessTwo(); }
            else if (GameStep == 3) { ProcessThree(); }
            else if (GameStep == 4) { ProcessFour(); }
            else if (GameStep == 5) { ProcessFive(); }
            else if (GameStep == 6) { ProcessSix(); }
            else if (GameStep == 7) { ProcessSeven(); }
        }

        private async void CheckFolder()
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
                await Task.Delay(500);
            }
        }

        #region GameStep = 開頭動畫與選擇介面
        private async void ProcessOne()
        {
            data.TotalFile = 6;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckStartMusic(); //確認開頭音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckOptionMusic(); //確認選單音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckEnterGameEffect(); //確認進入遊戲音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckUbyeLevelUp(); //悠白能力提升音效
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }

        private async void CheckStartMusic()
        {
            string music = System.IO.Path.Combine(path, "開頭音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.開頭音樂);
                await Task.Delay(500);
            }
        }

        private async void CheckOptionMusic()
        {
            string music = System.IO.Path.Combine(path, "選單音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.選單音樂);
                await Task.Delay(500);
            }
        }

        private async void CheckEnterGameEffect()
        {
            string music = System.IO.Path.Combine(path, "進入遊戲音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.進入遊戲音效);
                await Task.Delay(500);
            }
        }

        private async void CheckCursorEffect()
        {
            string music = System.IO.Path.Combine(path, "游標移動音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.游標移動音效);
                await Task.Delay(500);
            }
        }
        #endregion

        #region GameStep = 遊戲開始
        private async void ProcessTwo()
        {
            data.TotalFile = 16;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckGameMusic(); //遊戲中音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckNormalBoss(); //中等以下Boss音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckHardBoss(); //最強Boss音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCloudDisappear(); //浮雲消失音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCloudBrustStream(); //浮雲噴射白光音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCloudSprint(); //浮雲撞人音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCloudBeforeSprint(); //浮雲衝撞前音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCloudAccumulate(); //浮雲續氣音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCloudThunder(); //浮雲彈幕攻擊
            await Task.Delay(100);

            data.CompleteFile++;
            CheckUbyeCure(); //悠白回血音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckUbyeAttack(); //悠白攻擊音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckUbyeGetHitting(); //悠白被打到音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckUbyeAvoid(); //悠白迴避音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckUbyeLevelUp(); //悠白能力提升音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }

        private async void CheckGameMusic()
        {
            string music = System.IO.Path.Combine(path, "遊戲中音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.遊戲中音樂);
                await Task.Delay(500);
            }
        }

        private async void CheckNormalBoss()
        {
            string music = System.IO.Path.Combine(path, "中等以下Boss.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.中等以下Boss);
                await Task.Delay(500);
            }
        }

        private async void CheckHardBoss()
        {
            string music = System.IO.Path.Combine(path, "最難Boss.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.最難Boss);
                await Task.Delay(500);
            }
        }

        private async void CheckCloudDisappear()
        {
            string music = System.IO.Path.Combine(path, "最難Boss.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.浮雲消失音效);
                await Task.Delay(500);
            }
        }

        private async void CheckCloudBrustStream()
        {
            string music = System.IO.Path.Combine(path, "浮雲噴射白光音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.浮雲噴射白光音效);
                await Task.Delay(500);
            }
        }

        private async void CheckCloudSprint()
        {
            string music = System.IO.Path.Combine(path, "浮雲撞人音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.浮雲撞人音效);
                await Task.Delay(500);
            }
        }

        private async void CheckCloudBeforeSprint()
        {
            string music = System.IO.Path.Combine(path, "浮雲衝撞前音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.浮雲衝撞前音效);
                await Task.Delay(500);
            }
        }

        private async void CheckCloudAccumulate()
        {
            string music = System.IO.Path.Combine(path, "浮雲續氣音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.浮雲續氣音效);
                await Task.Delay(500);
            }
        }

        private async void CheckCloudThunder()
        {
            string music = System.IO.Path.Combine(path, "浮雲彈幕攻擊.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.浮雲彈幕攻擊);
                await Task.Delay(500);
            }
        }

        private async void CheckUbyeCure()
        {
            string music = System.IO.Path.Combine(path, "悠白回血音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.悠白回血音效);
                await Task.Delay(500);
            }
        }

        private async void CheckUbyeAttack()
        {
            string music = System.IO.Path.Combine(path, "悠白攻擊音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.悠白攻擊音效);
                await Task.Delay(500);
            }
        }

        private async void CheckUbyeGetHitting()
        {
            string music = System.IO.Path.Combine(path, "悠白被打到音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.悠白被打到音效);
                await Task.Delay(500);
            }
        }

        private async void CheckUbyeAvoid()
        {
            string music = System.IO.Path.Combine(path, "悠白迴避音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.悠白迴避音效);
                await Task.Delay(500);
            }
        }

        private async void CheckUbyeLevelUp()
        {
            string music = System.IO.Path.Combine(path, "悠白能力提升音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.悠白能力提升音效);
                await Task.Delay(500);
            }
        }
        #endregion

        #region GameStep = 遊戲失敗
        private async void ProcessThree()
        {
            data.TotalFile = 5;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckFail(); //失敗音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckULULU(); //悠白復活音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckGameOver(); //家庭號修羅地獄
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }

        private async void CheckFail()
        {
            string music = System.IO.Path.Combine(path, "失敗音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.失敗音樂);
                await Task.Delay(500);
            }
        }

        private async void CheckULULU()
        {
            string music = System.IO.Path.Combine(path, "悠白復活音效.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.悠白復活音效);
                await Task.Delay(500);
            }
        }

        private async void CheckGameOver()
        {
            string music = System.IO.Path.Combine(path, "家庭號修羅地獄.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.家庭號修羅地獄);
                await Task.Delay(500);
            }
        }
        #endregion

        #region GameStep = 偽結局
        private async void ProcessFour()
        {
            data.TotalFile = 3;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckFake(); //假結尾音樂
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }

        private async void CheckFake()
        {
            string music = System.IO.Path.Combine(path, "假結尾音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.假結尾音樂);
                await Task.Delay(500);
            }
        }
        #endregion

        #region GameStep = 真結局
        private async void ProcessFive()
        {
            data.TotalFile = 4;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckReal(); //真結尾音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckEasterEdd(); //彩蛋音樂
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }

        private async void CheckReal()
        {
            string music = System.IO.Path.Combine(path, "真結尾音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.真結尾音樂);
                await Task.Delay(500);
            }
        }

        private async void CheckEasterEdd()
        {
            string music = System.IO.Path.Combine(path, "彩蛋音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.彩蛋音樂);
                await Task.Delay(500);
            }
        }
        #endregion

        #region GameStep = 其他
        private async void ProcessSix()
        {
            data.TotalFile = 3;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckOtherMusic(); //確認其他音樂
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }

        private async void CheckOtherMusic()
        {
            string music = System.IO.Path.Combine(path, "其他音樂.wav");
            if (!File.Exists(music))
            {
                data.CreateFile(music, Properties.Resources.其他音樂);
                await Task.Delay(500);
            }
        }
        #endregion

        #region GameStep = 歷史紀錄
        private async void ProcessSeven()
        {
            data.TotalFile = 5;
            CheckFolder(); //確認資料夾是否存在
            await Task.Delay(100);

            data.CompleteFile++;
            CheckCursorEffect(); //確認游標移動音效
            await Task.Delay(100);

            data.CompleteFile++;
            CheckOtherMusic(); //確認其他音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckStartMusic(); //確認開頭音樂
            await Task.Delay(100);

            data.CompleteFile++;
            CheckFail(); //失敗音樂
            await Task.Delay(100);

            data.CompleteFile++;
            await data.GoDart();
            LoadingComplete?.Invoke();
        }
        #endregion

        private class Data : MVVM
        {
            //Prgress總共650
            private double _Progress = 0;
            public double Progress
            {
                get => _Progress;
                set
                {
                    _Progress = value;
                    if (_Progress > 650) { _Progress = 650; }
                    if (_Progress < 0) { _Progress = 0; }
                    NotifyPropertyChanged("Progress");
                }
            }


            private double _TotalFile = 1;
            public double TotalFile
            {
                get => _TotalFile;
                set
                {
                    _TotalFile = value;
                    NotifyPropertyChanged("TotalFile");
                }
            }


            private double _CompleteFile = 0;
            public double CompleteFile
            {
                get => _CompleteFile;
                set
                {
                    _CompleteFile = value;
                    Progress = 650 * (CompleteFile / TotalFile);
                    NotifyPropertyChanged("CompleteFile");
                }
            }


            private double _MainOpacity = 1;
            public double MainOpacity
            {
                get => _MainOpacity;
                set
                {
                    _MainOpacity = value;
                    NotifyPropertyChanged("MainOpacity");
                }
            }

            public void CreateFile(string music, Stream stream)
            {
                using (FileStream outputFileStream = new FileStream(music, FileMode.Create))
                {
                    stream.CopyTo(outputFileStream);
                }
            }

            public async Task GoDart()
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
