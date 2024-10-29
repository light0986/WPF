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
using 撈資料.Models;
using 撈資料.UserControls;

namespace 撈資料
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data data = new Data();
        private CheckUserRole login;
        private ViewDataInterface ViewData;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = data;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartBusy();

            data.ServerCheck = "";
            SQL.SetString((string)Application.Current.TryFindResource("ConnectionString"));

            Task task = new Task(() =>
            {
                data.ServerCheck = SQL.PingServer();
                if (data.ServerCheck == "OK")
                {
                    Dispatcher.Invoke(() =>
                    {
                        login = new CheckUserRole();
                        login.LogSuccess += LogSuccess;
                        login.StartBusy += StartBusy;
                        login.FinishBusy += FinishBusy;
                        _ = MainGrid.Children.Add(login);
                        _ = MessageBox.Show("資料庫連線成功");
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        _ = MessageBox.Show("資料庫連線失敗");
                        Application.Current.Shutdown();
                    });
                }

                FinishBusy();
            });
            task.Start();
        }

        private void LogSuccess()
        {
            _ = MessageBox.Show("登入成功");
            MainGrid.Children.Remove(login);
            ViewData = new ViewDataInterface();
            ViewData.StartBusy += StartBusy;
            ViewData.FinishBusy += FinishBusy;
            _ = MainGrid.Children.Add(ViewData);
        }

        private void StartBusy()
        {
            data.OnBusy = Visibility.Visible;
        }

        private void FinishBusy()
        {
            data.OnBusy = Visibility.Hidden;
        }

        private class Data : MVVM
        {
            private Visibility _OnBusy = Visibility.Hidden;
            public Visibility OnBusy
            {
                get => _OnBusy;
                set
                {
                    _OnBusy = value;
                    NotifyPropertyChanged("OnBusy");
                }
            }


            public string ServerCheck { get; set; }
        }
    }
}
