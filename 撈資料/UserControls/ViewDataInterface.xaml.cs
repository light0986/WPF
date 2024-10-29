using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    /// ViewDataInterface.xaml 的互動邏輯
    /// </summary>
    public partial class ViewDataInterface : UserControl
    {
        private Data data = new Data();
        public Action StartBusy;
        public Action FinishBusy;

        public ViewDataInterface()
        {
            InitializeComponent();
            DataContext = data;
            Loaded += ViewDataInterface_Loaded;
        }

        private void ViewDataInterface_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshTABLE_NAME();
        }

        private void RefreshTABLE_NAME()
        {
            StartBusy?.Invoke();
            string QueryString = (string)Application.Current.TryFindResource("SQL_TABLE_NAME");

            Task task = new Task(() =>
            {
                SQL.ConnectionOpen();
                DataTable names = SQL.Commend(QueryString);
                List<string> Table_name = new List<string>();
                for (int i = 0; i < names.Rows.Count; i++)
                {
                    Table_name.Add(names.Rows[i]["TABLE_NAME"].ToString());
                }
                SQL.ConnectionClose();

                Dispatcher.Invoke(() =>
                {
                    data.OriginTABLE_NAME = Table_name.OrderBy(q => q).ToList();
                    data.TABLE_NAME = data.OriginTABLE_NAME;
                    DataView.ItemsSource = data.VIEW_DATA.DefaultView;
                    FinishBusy?.Invoke();
                    StartCountDown();
                });
            });
            task.Start();
        }

        //避免DDOS
        private async void StartCountDown()
        {
            for (int i = 5; i >= 0; i--)
            {
                data.RefreshText = i.ToString();
                await Task.Delay(1000);
            }
            data.RefreshText = "重新整理";
        }

        private void TableNameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (data.TABLE_NAME.Count() > 0 && TableNameList.SelectedIndex != -1)
            {
                data.SelectedTable = data.TABLE_NAME[TableNameList.SelectedIndex];
            }
            else
            {
                data.SelectedTable = "";
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (TableNameList.SelectedIndex != -1)
            {
                //演算法方式:
                //1.刷新
                //2.創造TempTable,並且先知道有多少Row
                //3.逐次與SQL溝通要求資料(1次1000筆)
                //4.刪除掉TempTable

                string table_name = data.TABLE_NAME[TableNameList.SelectedIndex];
                Task task = new Task(() =>
                {
                    StartBusy?.Invoke();
                    SQL.ConnectionOpen();

                    #region 刷新並且先知道有多少Row
                    string sql01 = (string)Application.Current.TryFindResource("SQL_TABLE_REFRESH");
                    string QueryString01 = String.Format(sql01, table_name);
                    DataTable REQ = SQL.Commend(QueryString01);
                    Task.Delay(20);
                    #endregion

                    //新增成功
                    if (REQ != null && REQ.Rows.Count > 0 && (string)REQ.Rows[0]["CreateOK"] == "OK")
                    {
                        #region 新增一個Temp
                        string G_id = Guid.NewGuid().ToString("N");
                        string sql02 = (string)Application.Current.TryFindResource("SQL_CREATE_TEMP");
                        string WhereString = data.Where ?? "";
                        if(WhereString != "") { WhereString = "where " + WhereString; }

                        string QueryString02 = String.Format(sql02, G_id, table_name, WhereString, G_id);
                        DataTable count = SQL.Commend(QueryString02);
                        Task.Delay(20);
                        #endregion

                        if (count != null && count.Rows.Count > 0 && (int)count.Rows[0]["Count"] > 0)
                        {
                            #region 定義top數量
                            int top_num = 0;
                            if (data.TopX.Length > 0)
                            {
                                try
                                {
                                    top_num = Convert.ToInt32(data.TopX);
                                }
                                catch { }
                            }
                            #endregion

                            #region 逐次與SQL溝通要求資料(1次1000筆)
                            int TotalRow = (int)count.Rows[0]["Count"];
                            int Division = (TotalRow / 1000) + 1; //取總共要溝通幾次?
                            if (top_num != 0) { Division = (top_num / 1000) + 1; }
                            DataTable datas = new DataTable();

                            data.ProgressMax = Division;
                            for (int i = 0; i < Division; i++)
                            {
                                string sql03 = (string)Application.Current.TryFindResource("SQL_TABLE_DATA");
                                string TopNum = "";
                                if (data.TopX.Length > 0) { TopNum = "Top(" + data.TopX +")"; }

                                string QueryString03 = String.Format(sql03, TopNum, G_id, (i * 1000) + 1, ((i + 1) * 1000) + 1);
                                DataTable rows = SQL.Commend(QueryString03);

                                if (rows != null && rows.Rows.Count > 0)
                                {
                                    datas.Merge(rows);

                                    if (top_num != 0)
                                    {
                                        //只要top不是0且滿足top
                                        if (datas.Rows.Count >= top_num)
                                        {
                                            DataTable dt = datas.AsEnumerable().Take(top_num).CopyToDataTable();
                                            datas = dt;
                                            break;
                                        }
                                    }

                                }
                                else
                                {
                                    break;
                                }

                                data.ProgressValue = i + 1;
                                Task.Delay(20);
                            }
                            #endregion

                            #region 刪除掉TempTable
                            string sql04 = (string)Application.Current.TryFindResource("SQL_DROP_TEMP");
                            string QueryString04 = String.Format(sql04, G_id);
                            _ = SQL.Commend(QueryString04);
                            #endregion

                            Dispatcher.Invoke(() =>
                            {
                                if (datas == null) { datas = new DataTable(); }
                                data.VIEW_DATA = datas;

                                data.VIEW_DATA.DefaultView.Sort = "RowID";
                                DataTable_To_ListView(datas, DataView);

                                //完成, 撈出來的資料數量與實際數量比對，或是針對Top
                                if ((TotalRow == data.VIEW_DATA.Rows.Count) || (top_num == data.VIEW_DATA.Rows.Count))
                                {
                                    if (data.VIEW_DATA.Rows.Count > 0)
                                    {
                                        _ = MessageBox.Show("資料撈取成功");
                                    }
                                    else
                                    {
                                        _ = MessageBox.Show("目前沒有資料");
                                    }
                                }
                                else
                                {
                                    //途中失敗
                                    _ = MessageBox.Show("資料獲取中斷");
                                }
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _ = MessageBox.Show("表單沒有數量");
                            });
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _ = MessageBox.Show("錯誤，請稍後再重試一次");
                        });
                    }

                    SQL.ConnectionClose();
                    FinishBusy?.Invoke();
                });
                task.Start();
            }
            else
            {
                _ = MessageBox.Show("請選擇表單");
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTABLE_NAME();
        }

        private void DataTable_To_ListView(DataTable table, ListView view)
        {
            GridView gridView = new GridView();

            for (int i = 0; i < table.Columns.Count; i++)
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = table.Columns[i].ToString(),
                    DisplayMemberBinding = new Binding(table.Columns[i].ToString())
                });
            }

            view.View = gridView;
            view.ItemsSource = table.DefaultView;
        }

        private void ToCSV_Click(object sender, RoutedEventArgs e)
        {
            if (data.VIEW_DATA.Rows.Count > 0)
            {
                StartBusy?.Invoke();

                if (CSV_Style.SelectedIndex == 0) { CreateToTxt(); }
                else if (CSV_Style.SelectedIndex == 1) { CreateToCSV(); }
            }
            else
            {
                _ = MessageBox.Show("沒有資料");
            }
        }

        #region 輸入檢查
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            data.SelectedTable = FilterText.Text;
            if (data.SelectedTable != null && data.SelectedTable.Length > 0)
            {
                data.TABLE_NAME = data.OriginTABLE_NAME.Where(x => x.Contains(data.SelectedTable)).ToList();
            }
            else
            {
                data.TABLE_NAME = data.OriginTABLE_NAME;
            }
        }

        private void Where_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = Where_text.Text;
            Regex regex = new Regex(data.Pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(input))
            {
                Where_text.Text = "請勿進行SQL指令";
                Where_text.Focus();
                Where_text.SelectAll();
            }
        }
        #endregion

        //用空白鍵分隔
        private void CreateToTxt()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "TXT file (*.txt)|*.txt",
                Title = "將資料轉成txt檔",
                FileName = data.SelectedTable
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                List<string> row_data = new List<string>();
                string columnsName = "";
                for (int i = 0; i < data.VIEW_DATA.Columns.Count; i++)
                {
                    columnsName += data.VIEW_DATA.Columns[i].ToString();
                    if (i != data.VIEW_DATA.Columns.Count - 1)
                    {
                        columnsName += "	";
                    }
                }
                row_data.Add(columnsName);
                data.ProgressValue = 0;

                Task task = new Task(() =>
                {
                    foreach (DataRow dr in data.VIEW_DATA.Rows)
                    {
                        string row_text = "";
                        for (int i = 0; i < data.VIEW_DATA.Columns.Count; i++)
                        {
                            row_text += dr[i].ToString();
                            if (i != data.VIEW_DATA.Columns.Count - 1)
                            {
                                row_text += "	";
                            }
                        }
                        row_data.Add(row_text);
                        data.ProgressValue++;
                    }

                    using (var sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        for (int i = 0; i < row_data.Count; i++)
                        {
                            sw.WriteLine(row_data[i]);
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        FinishBusy?.Invoke();
                        _ = MessageBox.Show("完成");
                    });
                });
                task.Start();
            }
            else
            {
                FinishBusy?.Invoke();
            }
        }
        
        //用逗號分隔
        private void CreateToCSV()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "CSV file (*.csv)|*.csv",
                Title = "將資料轉成csv檔",
                FileName = data.SelectedTable
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                List<string> row_data = new List<string>();
                string columnsName = "";
                for (int i = 0; i < data.VIEW_DATA.Columns.Count; i++)
                {
                    columnsName += "\"";
                    columnsName += data.VIEW_DATA.Columns[i].ToString();
                    columnsName += "\"";

                    if (i != data.VIEW_DATA.Columns.Count - 1)
                    {
                        columnsName += ",";
                    }
                }
                row_data.Add(columnsName);
                data.ProgressValue = 0;

                Task task = new Task(() =>
                {
                    foreach (DataRow dr in data.VIEW_DATA.Rows)
                    {
                        string row_text = "";
                        for (int i = 0; i < data.VIEW_DATA.Columns.Count; i++)
                        {
                            row_text += "\"";
                            row_text += dr[i].ToString();
                            row_text += "\"";

                            if (i != data.VIEW_DATA.Columns.Count - 1)
                            {
                                row_text += ",";
                            }
                        }
                        row_data.Add(row_text);
                        data.ProgressValue++;
                    }

                    using (var sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        for (int i = 0; i < row_data.Count; i++)
                        {
                            sw.WriteLine(row_data[i]);
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        FinishBusy?.Invoke();
                        _ = MessageBox.Show("完成");
                    });
                });
                task.Start();
            }
            else
            {
                FinishBusy?.Invoke();
            }
        }

        private class Data : MVVM
        {
            public List<string> OriginTABLE_NAME { get; set; } = new List<string>();


            private List<string> _TABLE_NAME = new List<string>();
            public List<string> TABLE_NAME
            {
                get => _TABLE_NAME;
                set
                {
                    _TABLE_NAME = value;
                    NotifyPropertyChanged("TABLE_NAME");
                }
            }


            private DataTable _VIEW_DATA = new DataTable();
            public DataTable VIEW_DATA
            {
                get => _VIEW_DATA;
                set
                {
                    _VIEW_DATA = value;
                    ProgressMax = _VIEW_DATA.Rows.Count;
                    NotifyPropertyChanged("CanTransToCSV");
                }
            }


            public bool CanTransToCSV
            {
                get
                {
                    if (VIEW_DATA.Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }


            private string _TopX = "1";
            public string TopX
            {
                get => _TopX;
                set
                {
                    if (value.All(char.IsDigit))
                    {
                        _TopX = value;
                    }
                    else
                    {
                        _TopX = "";
                    }
                    NotifyPropertyChanged("TopX");
                }
            }


            public string Pattern { get; } = @"(select|insert|update|delete|drop|alter)\s";

            private string _Where = "";
            public string Where
            {
                get => _Where;
                set
                {
                    _Where = value;
                    NotifyPropertyChanged("Where");
                }
            }


            private string _SelectedTable = "";
            public string SelectedTable
            {
                get => _SelectedTable;
                set
                {
                    _SelectedTable = value;
                    NotifyPropertyChanged("SelectedTable");
                }
            }


            private string _RefreshText = "重新整理";
            public string RefreshText
            {
                get => _RefreshText;
                set
                {
                    _RefreshText = value;
                    NotifyPropertyChanged("RefreshText");
                    NotifyPropertyChanged("CanRefresh");
                }
            }


            public bool CanRefresh
            {
                get
                {
                    if (_RefreshText == "重新整理") { return true; }
                    else { return false; }
                }
            }

            #region 進度
            private int _ProgressMax = 0;
            public int ProgressMax
            {
                get => _ProgressMax;
                set
                {
                    _ProgressMax = value;
                    ProgressValue = 0;
                    NotifyPropertyChanged("ProgressMax");
                }
            }


            private int _ProgressValue = 0;
            public int ProgressValue
            {
                get => _ProgressValue;
                set
                {
                    _ProgressValue = value;
                    NotifyPropertyChanged("ProgressValue");
                    NotifyPropertyChanged("ProgressText");
                }
            }


            public string ProgressText
            {
                get => ProgressValue + " / " + ProgressMax;
            }
            #endregion
        }
    }
}
