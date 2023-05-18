using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace RenameFile
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data data = new Data();
        private bool OnWork
        {
            get => !data.Enable;
            set => data.Enable = !value;
        }

        private Task mainTask = null;

        private bool ShowOK
        {
            set
            {
                if (value)
                {
                    _ = MessageBox.Show("OK!");
                }
            }
        }

        private RenameType rename { get; set; } = RenameType.Sequence;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = data;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "讀取",
                    Filter = "All files (*.*)|*.*",
                    RestoreDirectory = true,
                    Multiselect = true
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    data.Files = openFileDialog.FileNames;
                    data.SafeFileNames = openFileDialog.SafeFileNames;
                    mainTask = new Task(() =>
                    {
                        data.MaxProgress = data.Files.GetLength(0);
                        string[] Filters = new string[data.MaxProgress];
                        string[] Paths = new string[data.MaxProgress];
                        foreach (string str in data.Files)
                        {
                            Filters[data.Progress] = str.Substring(str.LastIndexOf('.'));
                            Paths[data.Progress] = str.Replace(data.SafeFileNames[data.Progress], "");
                            data.Progress++;
                        }
                        data.FileFilters = Filters;
                        data.Paths = Paths;
                        OnWork = false;
                        ShowOK = true;
                    });
                    mainTask.Start();
                }
                else
                {
                    OnWork = false;
                }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                RadioButton radio = (RadioButton)e.OriginalSource;
                if (radio.Tag.ToString() == "Sequence")
                {
                    rename = RenameType.Sequence;
                }
                else if (radio.Tag.ToString() == "Random")
                {
                    rename = RenameType.Random;
                }

                OnWork = false;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                data.DataClear = true;

                OnWork = false;
                ShowOK = true;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (OnWork == false)
            {
                OnWork = true;

                mainTask = new Task(() =>
                {
                    data.MaxProgress = data.MaxProgress;
                    if (rename == RenameType.Sequence)
                    {
                        int count = data.MaxProgress.ToString().Length;
                        string newName1 = ""; string newName2 = "";
                        foreach (string path in data.Files)
                        {
                            newName1 = data.Paths[data.Progress] + Guid.NewGuid().ToString("N");
                            File.Move(path, newName1);
                            newName2 = data.Paths[data.Progress] + (data.Progress + 1).ToString("D" + count) + data.FileFilters[data.Progress];
                            File.Move(newName1, newName2);
                            data.Progress++;
                        }
                    }
                    else
                    {
                        string newName = "";
                        foreach (string path in data.Files)
                        {
                            newName = data.Paths[data.Progress] + Guid.NewGuid().ToString("N") + data.FileFilters[data.Progress];
                            File.Move(path, newName);
                            data.Progress++;
                        }
                    }
                    data.DataClear = true;
                    OnWork = false;
                    ShowOK = true;
                });
                mainTask.Start();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = OnWork;
        }

        private enum RenameType { Sequence, Random }

        private class Data : INotifyPropertyChanged
        {
            private string[] _Files = new string[0]; //全地址
            public string[] Files
            {
                get => _Files;
                set
                {
                    _Files = value;
                    NotifyPropertyChanged("TotalCount");
                }
            }

            public string[] SafeFileNames { get; set; } //檔案名稱

            public string[] FileFilters { get; set; } //檔案類型

            public string[] Paths { get; set; } //位置

            public bool DataClear
            {
                set
                {
                    if (value)
                    {
                        string[] list = new string[0];
                        Files = list;
                        SafeFileNames = list;
                        FileFilters = list;
                        Paths = list;
                    }
                }
            }

            #region UI
            public string TotalCount => "數量: " + _Files.GetLength(0);


            private int _MaxProgress = 0;
            public int MaxProgress
            {
                get => _MaxProgress;
                set
                {
                    Progress = 0;
                    _MaxProgress = value;
                    NotifyPropertyChanged("MaxProgress");
                }
            }


            private int _Progress = 0;
            public int Progress
            {
                get => _Progress;
                set
                {
                    _Progress = value;
                    NotifyPropertyChanged("Progress");
                }
            }


            private bool _Enable = true;
            public bool Enable
            {
                get => _Enable;
                set
                {
                    _Enable = value;
                    NotifyPropertyChanged("Enable");
                }
            }
            #endregion

            #region Public
            public event PropertyChangedEventHandler PropertyChanged;

            protected void NotifyPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}
