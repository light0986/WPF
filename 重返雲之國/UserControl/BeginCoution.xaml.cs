using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
    /// BeginCoution.xaml 的互動邏輯
    /// </summary>
    public partial class BeginCoution : UserControl
    {
        private MusicPlayer music = new MusicPlayer();
        public Action ReadComplete;
        private bool OnWork { get; set; } = true;

        public BeginCoution()
        {
            InitializeComponent();
            MainGrid.Opacity = 0;
            Loaded += BeginCoution_Loaded;
        }

        private async void BeginCoution_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i =0; i < 10; i++)
            {
                MainGrid.Opacity += 0.1;
                await Task.Delay(20);
            }
            OnWork = false;
            do
            {
                EnterGameText.Visibility = Visibility.Hidden;
                await Task.Delay(500);
                EnterGameText.Visibility = Visibility.Visible;
                await Task.Delay(500);
            }
            while (true);
        }

        public async void KeyPress(Key key)
        {
            if (key == Key.Space || key == Key.Enter)
            {
                if (OnWork == false)
                {
                    OnWork = true;
                    music.PlayShort("進入遊戲音效");

                    for (int i = 0; i < 10; i++)
                    {
                        MainGrid.Opacity -= 0.1;
                        await Task.Delay(20);
                    }
                    await Task.Delay(3000);
                    ReadComplete?.Invoke();
                }
            }
        }
    }
}
