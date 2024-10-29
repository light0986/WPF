using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace 撈資料.UserControls
{
    /// <summary>
    /// CheckUserRole.xaml 的互動邏輯
    /// </summary>
    public partial class CheckUserRole : UserControl
    {
        private Data data = new Data();
        public Action StartBusy;
        public Action FinishBusy;
        public Action LogSuccess;

        public CheckUserRole()
        {
            InitializeComponent();
            DataContext = data;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            StartBusy?.Invoke();
            Task task = new Task(() =>
            {
                SQL.ConnectionOpen();
                string req = SQL.CheckRoles(data.Account, data.Password);
                if (req == "身分驗證正確")
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogSuccess?.Invoke();
                        FinishBusy?.Invoke();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        _ = MessageBox.Show(req);
                        FinishBusy?.Invoke();
                    });
                }
                SQL.ConnectionClose();
            });
            task.Start();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            data.Account = "";
            data.Password = "";
        }

        //避免SQL injection
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)e.Source;
            string input = textBox.Text;
            Regex regex = new Regex(data.pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(input))
            {
                textBox.Text = "請勿進行SQL指令";
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        private class Data : MVVM
        {
            private string _Account = "";
            public string Account
            {
                get => _Account;
                set
                {
                    _Account = value;
                    NotifyPropertyChanged("Account");
                }
            }


            private string _Password = "";
            public string Password
            {
                get => _Password;
                set
                {
                    _Password = value;
                    NotifyPropertyChanged("Password");
                }
            }

            public string pattern { get; } = @"(select|insert|update|delete|drop|alter)\s";
        }
    }
}
