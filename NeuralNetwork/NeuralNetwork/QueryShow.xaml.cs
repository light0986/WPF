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
using System.Windows.Shapes;

namespace NeuralNetwork
{
    /// <summary>
    /// QueryShow.xaml 的互動邏輯
    /// </summary>
    public partial class QueryShow : Window
    {
        public delegate void Event();
        public event Event CloseClicked;
        public event Event QueryRight;
        public event Event QueryWrong;

        private string _CAnswer;
        private string CAnswer
        {
            get => _CAnswer;
            set
            {
                _CAnswer = value;
                CorrectAnswer.Content = value;
            }
        }

        private string QAnswer
        {
            set => QueryAnswer.Content = value;
        }

        private List<double[,]> _QList = new List<double[,]>();
        private double[,] QList
        {
            set
            {
                double large = 0; int x = 0;
                for (int i = 0; i < value.GetLength(0); i++)
                {
                    for (int j = 0; j < value.GetLength(1); j++)
                    {
                        if (value[i, j] > large)
                        {
                            large = value[i, j];
                            x = i;
                        }
                    }
                }

                Button button = new Button()
                {
                    BorderThickness = new Thickness(0),
                    Height = 40,
                    Tag = _QList.Count,
                    Content = "Query: " + x + " Correct: " + CAnswer
                };
                button.Click += Button_Click;

                QAnswer = x.ToString();
                if (Convert.ToInt32(CAnswer) == x)
                {
                    QueryRight?.Invoke();
                    button.Background = Brushes.LightGreen;
                }
                else
                {
                    QueryWrong?.Invoke();
                    button.Background = Brushes.LightPink;
                }

                _ = ScoreList.Children.Add(button);
                _QList.Add(value);
            }
        }

        private bool OmWork = false;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OmWork == false)
            {
                OmWork = true;
                int tag = (int)((Button)sender).Tag;
                DescriptionList.Children.Clear();

                double[,] list = _QList[tag];
                for (int i = 0; i < list.GetLength(0); i++)
                {
                    Label label = new Label()
                    {
                        Content = i + ": " + list[i, 0]
                    };
                    DescriptionList.Children.Add(label);
                    await Task.Delay(1);
                }

                OmWork = false;
            }
        }

        public QueryShow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseClicked?.Invoke();
        }

        public void SetCorrectAnswer(string answer)
        {
            CAnswer = answer;
        }

        public void SetQueryAnswer(double[,] answer)
        {
            QList = answer;
        }

        public void SetMaxValue(int max)
        {
            ScoreMax.Content = max;
            ScoreList.Children.Clear();
            DescriptionList.Children.Clear();
            NowValue = 0;
        }

        private int _NowValue = 0;
        private int NowValue
        {
            get => _NowValue;
            set
            {
                ScoreValues.Content = value;
                _NowValue = value;
            }
        }

        public void SetNowValue()
        {
            NowValue++;
        }
    }
}
