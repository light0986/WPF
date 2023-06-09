using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NeuralNetwork
{
    /// <summary>
    /// SaveAndLoad.xaml 的互動邏輯
    /// </summary>
    public partial class SaveAndLoad : UserControl
    {
        public delegate void Event();
        public event Event CloseClicked;
        public event Event LoadClicked;

        public delegate void Event2(string name, List<string> nameList, int type, int inputnodes, int hiddennodes, int outputnodes, double learningrate, int row, int col, double[,] wih, double[,] who);
        public event Event2 DataLoading;

        private List<Data> DL = new List<Data>();

        public SaveAndLoad()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            CloseClicked?.Invoke();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "存檔",
                Filter = "txt File (.txt)|*.txt",
                FileName = "Type" + DL[0].Type,
                RestoreDirectory = true,
                InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string list = JsonConvert.SerializeObject(DL);
                File.WriteAllText(saveFileDialog.FileName, list);

                Visibility = Visibility.Hidden;
                CloseClicked?.Invoke();
                _ = MessageBox.Show("OK");
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "讀取",
                Filter = "txt File (.txt)|*.txt",
                RestoreDirectory = true,
                InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadClicked?.Invoke();
                string text = File.ReadAllText(openFileDialog.FileName);
                DL = JsonConvert.DeserializeObject<List<Data>>(text);

                for (int i = 0; i < DL.Count; i++)
                {
                    DataLoading(DL[i].Name, DL[i].NameList, DL[i].Type, DL[i].Inputnodes, DL[i].Hiddennodes, DL[i].Outputnodes, DL[i].Learningrate, DL[i].Rows, DL[i].Columns, DL[i].WIH, DL[i].WHO);
                }

                Visibility = Visibility.Hidden;
                CloseClicked?.Invoke();
                _ = MessageBox.Show("OK");
            }
        }

        public void SetData(string name, List<string> nameList, int type, int inputnodes, int hiddennodes, int outputnodes, double learningrate, int row, int col, double[,] wih, double[,] who)
        {
            DL.Add(new Data()
            {
                Name = name,
                NameList = nameList,
                Type = type,
                Rows = row,
                Columns = col,
                Inputnodes = inputnodes,
                Hiddennodes = hiddennodes,
                Outputnodes = outputnodes,
                Learningrate = learningrate,
                WIH = wih,
                WHO = who
            });
        }

        private class Data
        {
            public string Name { get; set; }

            public List<string> NameList { get; set; }

            public int Type { get; set; }

            public int Rows { get; set; }

            public int Columns { get; set; }

            public int Inputnodes { get; set; }

            public int Hiddennodes { get; set; }

            public int Outputnodes { get; set; }

            public double Learningrate { get; set; }

            public double[,] WHO { get; set; }

            public double[,] WIH { get; set; }
        }
    }
}
