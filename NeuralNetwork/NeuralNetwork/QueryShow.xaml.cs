using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private double Rate
        {
            set => CorrectRate.Content = value + "%";
        }

        private int _MaxValue = 0;
        private int MaxValue
        {
            get => _MaxValue;
            set
            {
                ScoreMax.Content = value;
                _MaxValue = value;
            }
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

        private List<Answer> QList = new List<Answer>();
        private List<string> WrongAnswer = new List<string>();

        private bool OmWork = false;
        private bool Complete = false;

        public QueryShow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OmWork == false)
            {
                OmWork = true;
                int tag = (int)((Button)sender).Tag;
                DescriptionList.Children.Clear();

                double[] list = QList[tag].Output;
                string[] name = QList[tag].Name;
                for (int i = 0; i < list.GetLength(0); i++)
                {
                    Label label = new Label()
                    {
                        Content = name[i] + ": " + list[i]
                    };
                    _ = DescriptionList.Children.Add(label);
                    await Task.Delay(1);
                }

                OmWork = false;
            }
        }

        public bool SetQueryAnswer(string qanswer, string canswer, double[] output, string[] names)
        {
            Button button = new Button()
            {
                BorderThickness = new Thickness(0),
                Height = 40,
                Tag = QList.Count,
                Content = "Query: " + qanswer + " Correct: " + canswer
            };
            button.Click += Button_Click;

            QAnswer = qanswer;
            CAnswer = canswer;

            bool result = true;
            if (qanswer == canswer)
            {
                QueryRight?.Invoke();
                button.Background = Brushes.LightGreen;
                result = true;
            }
            else
            {
                QueryWrong?.Invoke();
                button.Background = Brushes.LightPink;
                WrongAnswer.Add(CAnswer);
                result = false;
            }

            QList.Add(new Answer() { Canswer = canswer, Qanswer = qanswer, Output = output, Name = names });
            _ = ScoreList.Children.Add(button);
            return result;
        }

        public void Reset()
        {
            ScoreList.Children.Clear();
            DescriptionList.Children.Clear();
            ResultsList.Children.Clear();
            MaxValue = 0;
            NowValue = 0;
            //QList = new List<double[,]>();
            WrongAnswer = new List<string>();
            Complete = false;
        }

        public void SetComplete()
        {
            Complete = true;
        }

        public void AddMax()
        {
            MaxValue++;
            Rate = Math.Round((double)NowValue / (double)MaxValue, 2) * 100;
        }

        public void AddValue()
        {
            NowValue++;
        }

        public async Task SetResult()
        {
            for (int i = 0; i < 10; i++)
            {
                Label label = new Label()
                {
                    Content = i + ": " + WrongAnswer.Count(x => x == i.ToString()) + " times"
                };

                _ = ResultsList.Children.Add(label);
            }
            await Task.Delay(1);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Complete)
            {
                CloseClicked?.Invoke();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private class Answer
        {
            public string Qanswer { get; set; }

            public string Canswer { get; set; }

            public double[] Output { get; set; }

            public string[] Name { get; set; }
        }
    }
}
