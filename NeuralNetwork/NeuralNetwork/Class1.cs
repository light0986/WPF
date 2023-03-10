using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeuralNetwork
{
    public class neuralNetwork
    {
        public delegate void Event(string name, double[,] output, string description);
        public event Event OutputMatrix;

        private readonly MatrixComputations MC = new MatrixComputations();
        private readonly int inodes, hnodes, onodes;
        private readonly double lr;
        public double[,] wih;
        public double[,] who;

        public neuralNetwork(int inputnodes, int hiddennodes, int outputnodes, double learningrate)
        {
            //self.inodes = inputnodes
            inodes = inputnodes;
            //self.hnodes = hiddennodes
            hnodes = hiddennodes;
            //self.onodes = outputnodes
            onodes = outputnodes;
            //self.wih = numpy.random.normal(0.0, pow(self.inodes, -0.5), (self.hnodes, self.inodes))
            wih = MC.Normal_Matrix(0, Math.Pow(inodes, -0.5), hnodes, inodes); //200 * 784
            //wih = Purely_Matrix(0, hnodes, inodes);
            //self.who = numpy.random.normal(0.0, pow(self.hnodes, -0.5), (self.onodes, self.hnodes))
            who = MC.Normal_Matrix(0, Math.Pow(hnodes, -0.5), onodes, hnodes); //10 * 200
            //who = Purely_Matrix(0, onodes, hnodes);
            //self.lr = learningrate
            lr = learningrate;
        }

        //training會加深印象，然後把之前的映象誤差扣掉，持續累積訓練數量，就會只剩特徵值。
        public async Task Train(double[] input_list, double[] targets_list)
        {
            //inputs = numpy.array(inputs_list, ndmin=2).T
            double[,] inputs1 = DoubleArrayToDoubleMatrix(input_list, 1, input_list.GetLength(0));
            double[,] inputs2 = MC.Matrix_Transpose(inputs1);
            OutputMatrix?.Invoke("inputs2", inputs2, "inputs = numpy.array(inputs_list, ndmin=2).T");

            //targets = numpy.array(targets_list, ndmin = 2).T
            double[,] targets1 = DoubleArrayToDoubleMatrix(targets_list, 1, targets_list.GetLength(0));
            double[,] targets2 = MC.Matrix_Transpose(targets1);
            OutputMatrix?.Invoke("targets2", targets2, "targets = numpy.array(targets_list, ndmin = 2).T");

            //hidden_inputs = numpy.dot(self.wih, inputs)
            double[,] hidden_inputs = MC.Matrix_Dot(wih, inputs2);
            OutputMatrix?.Invoke("hidden_inputs", hidden_inputs, "hidden_inputs = numpy.dot(self.wih, inputs)");

            //hidden_outputs = self.activation_function(hidden_inputs)
            double[,] hidden_outputs = MC.Activation_Function(hidden_inputs);
            OutputMatrix?.Invoke("hidden_outputs", hidden_outputs, "hidden_outputs = self.activation_function(hidden_inputs)");

            //final_inputs = numpy.dot(self.who, hidden_outputs)
            double[,] final_inputs = MC.Matrix_Dot(who, hidden_outputs);
            OutputMatrix?.Invoke("final_inputs", final_inputs, "final_inputs = numpy.dot(self.who, hidden_outputs)");

            //final_outputs = self.activation_function(final_inputs)
            double[,] final_outputs = MC.Activation_Function(final_inputs);
            OutputMatrix?.Invoke("final_outputs", final_outputs, "final_outputs = self.activation_function(final_inputs)");

            //output_errors = targets - final_outputs
            double[,] output_errors = MC.Matrix_Subtract(targets2, final_outputs);
            OutputMatrix?.Invoke("output_errors", output_errors, "output_errors = targets - final_outputs");

            //hidden_errors = numpy.dot(self.who.T, output_errors)
            double[,] hidden_errors = MC.Matrix_Dot(MC.Matrix_Transpose(who), output_errors);
            OutputMatrix?.Invoke("hidden_errors", hidden_errors, "hidden_errors = numpy.dot(self.who.T, output_errors)");

            //self.who += self.lr * numpy.dot((output_errors * final_outputs * (1 - final_outputs)), numpy.transpose(hidden_outputs))
            //(1 - final_outputs)
            double[,] who1 = MC.Matrix_Subtract(MC.Purely_Matrix(1, final_outputs.GetLength(0), final_outputs.GetLength(1)), final_outputs);
            OutputMatrix?.Invoke("who1", who1, "(1 - final_outputs)");

            //output_errors * final_outputs
            double[,] who2 = MC.Matrix_Multiplied(output_errors, final_outputs);
            OutputMatrix?.Invoke("who2", who2, "output_errors * final_outputs");

            //(output_errors * final_outputs * (1 - final_outputs)
            double[,] who3 = MC.Matrix_Multiplied(who2, who1);
            OutputMatrix?.Invoke("who3", who3, "(output_errors * final_outputs * (1 - final_outputs)");

            //numpy.transpose(hidden_outputs)
            double[,] who4 = MC.Matrix_Transpose(hidden_outputs);
            OutputMatrix?.Invoke("who4", who4, "numpy.transpose(hidden_outputs)");

            //numpy.dot((output_errors * final_outputs * (1 - final_outputs)), numpy.transpose(hidden_outputs)
            double[,] who5 = MC.Matrix_Dot(who3, who4);
            OutputMatrix?.Invoke("who5", who5, "numpy.dot((output_errors * final_outputs * (1 - final_outputs)), numpy.transpose(hidden_outputs)");

            //self.lr * numpy.dot
            double[,] who6 = MC.Matrix_Multiplied(MC.Purely_Matrix(lr, who5.GetLength(0), who5.GetLength(1)), who5);
            OutputMatrix?.Invoke("who6", who6, "self.lr * numpy.dot");

            //self.who +=
            double[,] who7 = who;
            OutputMatrix?.Invoke("who7", who7, "self.who");

            who = MC.Matrix_Add(who7, who6);
            OutputMatrix?.Invoke("who2", who, "self.who +=");

            //self.wih += self.lr * numpy.dot((hidden_errors * hidden_outputs * (1 - hidden_outputs)), numpy.transpose(inputs))
            //(1 - hidden_outputs)
            double[,] wih1 = MC.Matrix_Subtract(MC.Purely_Matrix(1, hidden_outputs.GetLength(0), hidden_outputs.GetLength(1)), hidden_outputs);
            OutputMatrix?.Invoke("wih1", wih1, "(1 - hidden_outputs)");

            //hidden_errors * hidden_outputs
            double[,] wih2 = MC.Matrix_Multiplied(hidden_errors, hidden_outputs);
            OutputMatrix?.Invoke("wih2", wih2, "hidden_errors * hidden_outputs");

            //(hidden_errors * hidden_outputs * (1 - hidden_outputs)
            double[,] wih3 = MC.Matrix_Multiplied(wih2, wih1);
            OutputMatrix?.Invoke("wih3", wih3, "(hidden_errors * hidden_outputs * (1 - hidden_outputs)");

            //numpy.transpose(inputs)
            double[,] wih4 = MC.Matrix_Transpose(inputs2);
            OutputMatrix?.Invoke("wih4", wih4, "numpy.transpose(inputs)");

            //numpy.dot((hidden_errors * hidden_outputs * (1 - hidden_outputs)), numpy.transpose(inputs))
            double[,] wih5 = MC.Matrix_Dot(wih3, wih4);
            OutputMatrix?.Invoke("wih5", wih5, "numpy.dot((hidden_errors * hidden_outputs * (1 - hidden_outputs)), numpy.transpose(inputs))");

            //self.lr * numpy.dot
            double[,] wih6 = MC.Matrix_Multiplied(MC.Purely_Matrix(lr, wih5.GetLength(0), wih5.GetLength(1)), wih5);
            OutputMatrix?.Invoke("wih6", wih6, "self.lr * numpy.dot");

            //self.wih +=
            double[,] wih7 = wih;
            OutputMatrix?.Invoke("wih7", wih7, "self.wih");

            wih = MC.Matrix_Add(wih7, wih6);
            OutputMatrix?.Invoke("wih", wih, "self.wih +=");

            await Task.Delay(1);
        }

        public async Task<double[,]> Query(double[] input_list)
        {
            //inputs = numpy.array(inputs_list, ndmin=2).T
            double[,] input = DoubleArrayToDoubleMatrix(input_list, 1, input_list.GetLength(0));
            double[,] inputs = MC.Matrix_Transpose(input);
            OutputMatrix?.Invoke("inputs", inputs, "inputs = numpy.array(inputs_list, ndmin=2).T");

            //hidden_inputs = numpy.dot(self.wih, inputs)
            double[,] hidden_inputs = MC.Matrix_Dot(wih, inputs);
            OutputMatrix?.Invoke("hidden_inputs", hidden_inputs, "hidden_inputs = numpy.dot(self.wih, inputs)");

            //hidden_outputs = self.activation_function(hidden_inputs)
            double[,] hidden_outputs = MC.Activation_Function(hidden_inputs);
            OutputMatrix?.Invoke("hidden_outputs", hidden_outputs, "hidden_outputs = self.activation_function(hidden_inputs)");

            //final_inputs = numpy.dot(self.who, hidden_outputs)
            double[,] final_inputs = MC.Matrix_Dot(who, hidden_outputs);
            OutputMatrix?.Invoke("final_inputs", final_inputs, "final_inputs = numpy.dot(self.who, hidden_outputs)");

            //final_outputs = self.activation_function(final_inputs)
            double[,] final_outputs = MC.Activation_Function(final_inputs);
            OutputMatrix?.Invoke("final_outputs", final_outputs, "final_outputs = self.activation_function(final_inputs)");

            await Task.Delay(1);
            return final_outputs;
        }

        //查詢觸發後的形狀
        public async Task<double[,]> Backquery(double[] targets_list)
        {
            //final_outputs = numpy.array(targets_list, ndmin=2).T
            double[,] outputs = DoubleArrayToDoubleMatrix(targets_list, 1, targets_list.GetLength(0));
            double[,] final_outputs = MC.Matrix_Transpose(outputs);

            //final_inputs = self.inverse_activation_function(final_outputs)
            double[,] final_inputs = MC.Inverse_Activation_Function(final_outputs);

            //hidden_outputs = numpy.dot(self.who.T, final_inputs)
            double[,] who_T = MC.Matrix_Transpose(who);
            double[,] hidden_outputs = MC.Matrix_Dot(who_T, final_inputs);

            //hidden_outputs -= numpy.min(hidden_outputs)
            double[,] hidden_min = MC.Purely_Min(hidden_outputs);
            double[,] hidden_outputs2 = MC.Matrix_Subtract(hidden_outputs, hidden_min);

            //hidden_outputs /= numpy.max(hidden_outputs)
            double[,] hidden_max = MC.Purely_Max(hidden_outputs2);
            double[,] hidden_outputs3 = MC.Matrix_Divide(hidden_outputs2, hidden_max);

            //hidden_outputs *= 0.98
            double[,] outputs_098 = MC.Purely_Matrix(0.98, hidden_outputs3.GetLength(0), hidden_outputs3.GetLength(1));
            double[,] hidden_outputs4 = MC.Matrix_Multiplied(hidden_outputs3, outputs_098);

            //hidden_outputs += 0.01
            double[,] outputs_001 = MC.Purely_Matrix(0.01, hidden_outputs4.GetLength(0), hidden_outputs4.GetLength(1));
            double[,] hidden_outputs5 = MC.Matrix_Add(hidden_outputs4, outputs_001);

            //hidden_inputs = self.inverse_activation_function(hidden_outputs)
            double[,] hidden_inputs = MC.Inverse_Activation_Function(hidden_outputs5);

            //inputs = numpy.dot(self.wih.T, hidden_inputs)
            double[,] wih_T = MC.Matrix_Transpose(wih);
            double[,] inputs = MC.Matrix_Dot(wih_T, hidden_inputs);

            //inputs -= numpy.min(inputs)
            double[,] inputs_min = MC.Purely_Min(inputs);
            double[,] inputs2 = MC.Matrix_Subtract(inputs, inputs_min);

            //inputs /= numpy.max(inputs)
            double[,] inputs_max = MC.Purely_Max(inputs2);
            double[,] inputs3 = MC.Matrix_Divide(inputs2, inputs_max);

            //inputs *= 0.98
            double[,] inputs_098 = MC.Purely_Matrix(0.98, inputs3.GetLength(0), inputs3.GetLength(1));
            double[,] inputs4 = MC.Matrix_Multiplied(inputs3, inputs_098);

            //inputs += 0.01
            double[,] inputs_001 = MC.Purely_Matrix(0.01, inputs4.GetLength(0), inputs4.GetLength(1));
            double[,] inputs5 = MC.Matrix_Add(inputs4, inputs_001);

            await Task.Delay(1);
            return inputs5;
        }

        public static double[] Purely_Array(double num, int columns)
        {
            double[] output = new double[columns];
            for (int i = 0; i < columns; i++)
            {
                output[i] = num;
            }

            return output;
        }

        public static double[] StringArrayToDoubleArray(string[] row)
        {
            //asfarray
            string[] input = row;
            double[] output = new double[784];

            if (input.GetLength(0) > 784)
            {
                for (int i = 0; i < 784; i++)
                {
                    double alpha = Convert.ToDouble(input[i + 1]);
                    output[i] = alpha;
                }
            }
            else
            {
                for (int i = 0; i < 784; i++)
                {
                    double alpha = Convert.ToDouble(input[i]);
                    output[i] = alpha;
                }
            }

            return output;
        }

        public static string[] DoubleArrayToStringArray(double[] row)
        {
            string[] output = new string[784];

            for (int i = 0; i < 784; i++)
            {
                output[i] = row[i].ToString();
            }

            return output;
        }

        public static double[] ArrayWeighted(double[] row)
        {
            double[] output = new double[row.GetLength(0)];
            for(int i = 0; i < row.GetLength(0); i++)
            {
                output[i] = (row[i] / 255.0 * 0.99) + 0.01;
            }

            return output;
        }

        public static double[,] DoubleArrayToDoubleMatrix(double[] array, int rows, int columns)
        {
            double[,] outputs = new double[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    outputs[i, j] = array[(i * rows) + j];
                }
            }

            return outputs;
        }
        //公開的工具
    }

    public class MatrixComputations
    {
        private readonly Random _random = new Random();

        public MatrixComputations()
        {

        }

        public double[,] Matrix_Transpose(double[,] input)
        {
            double[,] output = new double[input.GetLength(1), input.GetLength(0)];
            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    output[i, j] = input[j, i];
                }
            }
            return output;
        }

        //https://www.codeproject.com/Articles/5298657/Matrix-Multiplication-in-Csharp-Applying-Transform
        //py = numpy.dot
        public double[,] Matrix_Dot(double[,] matrix1, double[,] matrix2)
        {
            int matrix1Rows = matrix1.GetLength(0);
            int matrix1Cols = matrix1.GetLength(1);
            int matrix2Rows = matrix2.GetLength(0);
            int matrix2Cols = matrix2.GetLength(1);

            // checking if product is defined  
            if (matrix1Cols != matrix2Rows)
            {
                throw new InvalidOperationException("第一個矩陣的 n 列，必須等於第二個矩陣的 n 行");
            }

            double[,] product = new double[matrix1Rows, matrix2Cols];

            for (int matrix1_row = 0; matrix1_row < matrix1Rows; matrix1_row++)
            {
                for (int matrix2_col = 0; matrix2_col < matrix2Cols; matrix2_col++)
                {
                    for (int matrix1_col = 0; matrix1_col < matrix1Cols; matrix1_col++)
                    {
                        product[matrix1_row, matrix2_col] += matrix1[matrix1_row, matrix1_col] * matrix2[matrix1_col, matrix2_col];
                    }
                    product[matrix1_row, matrix2_col] = Math.Round(product[matrix1_row, matrix2_col], 8);
                }
            }

            return product;
        }

        //Array相加，用於 數字 + array
        public double[,] Matrix_Add(double[,] matrix1, double[,] matrix2)
        {
            int matrix1Rows = matrix1.GetLength(0);
            int matrix1Cols = matrix1.GetLength(1);
            int matrix2Rows = matrix2.GetLength(0);
            int matrix2Cols = matrix2.GetLength(1);

            if (matrix1Rows != matrix2Rows || matrix1Cols != matrix2Cols)
            {
                throw new InvalidOperationException("第一個矩陣的行列大小，必須等於第二個矩陣的行列大小一致");
            }

            double[,] product = new double[matrix1Rows, matrix1Cols];

            for (int matrix_row = 0; matrix_row < matrix1Rows; matrix_row++)
            {
                for (int matrix_col = 0; matrix_col < matrix1Cols; matrix_col++)
                {
                    product[matrix_row, matrix_col] = Math.Round(matrix1[matrix_row, matrix_col] + matrix2[matrix_row, matrix_col], 8);
                }
            }

            return product;
        }

        //Array相減，用於 數字 - array
        public double[,] Matrix_Subtract(double[,] matrix1, double[,] matrix2)
        {
            int matrix1Rows = matrix1.GetLength(0);
            int matrix1Cols = matrix1.GetLength(1);
            int matrix2Rows = matrix2.GetLength(0);
            int matrix2Cols = matrix2.GetLength(1);

            if (matrix1Rows != matrix2Rows || matrix1Cols != matrix2Cols)
            {
                throw new InvalidOperationException("第一個矩陣的行列大小，必須等於第二個矩陣的行列大小一致");
            }

            double[,] product = new double[matrix1Rows, matrix1Cols];

            for (int matrix_row = 0; matrix_row < matrix1Rows; matrix_row++)
            {
                for (int matrix_col = 0; matrix_col < matrix1Cols; matrix_col++)
                {
                    product[matrix_row, matrix_col] = Math.Round(matrix1[matrix_row, matrix_col] - matrix2[matrix_row, matrix_col], 8);
                }
            }

            return product;
        }

        //Array相乘，用於 數字 * array
        public double[,] Matrix_Multiplied(double[,] matrix1, double[,] matrix2)
        {
            int matrix1Rows = matrix1.GetLength(0);
            int matrix1Cols = matrix1.GetLength(1);
            int matrix2Rows = matrix2.GetLength(0);
            int matrix2Cols = matrix2.GetLength(1);

            if (matrix1Rows != matrix2Rows || matrix1Cols != matrix2Cols)
            {
                throw new InvalidOperationException("第一個矩陣的行列大小，必須等於第二個矩陣的行列大小一致");
            }

            double[,] product = new double[matrix1Rows, matrix1Cols];

            for (int matrix_row = 0; matrix_row < matrix1Rows; matrix_row++)
            {
                for (int matrix_col = 0; matrix_col < matrix1Cols; matrix_col++)
                {
                    product[matrix_row, matrix_col] = Math.Round(matrix1[matrix_row, matrix_col] * matrix2[matrix_row, matrix_col], 8);
                }
            }

            return product;
        }

        //Array相除，用於 數字 / array
        public double[,] Matrix_Divide(double[,] matrix1, double[,] matrix2)
        {
            int matrix1Rows = matrix1.GetLength(0);
            int matrix1Cols = matrix1.GetLength(1);
            int matrix2Rows = matrix2.GetLength(0);
            int matrix2Cols = matrix2.GetLength(1);

            if (matrix1Rows != matrix2Rows || matrix1Cols != matrix2Cols)
            {
                throw new InvalidOperationException("第一個矩陣的行列大小，必須等於第二個矩陣的行列大小一致");
            }

            double[,] product = new double[matrix1Rows, matrix1Cols];

            for (int matrix_row = 0; matrix_row < matrix1Rows; matrix_row++)
            {
                for (int matrix_col = 0; matrix_col < matrix1Cols; matrix_col++)
                {
                    product[matrix_row, matrix_col] = Math.Round(matrix1[matrix_row, matrix_col] / matrix2[matrix_row, matrix_col], 8);
                }
            }

            return product;
        }

        //給一個數字，設定大小，給出一個全是該數字的Array
        public double[,] Purely_Matrix(double num, int rows, int columns)
        {
            double[,] output = new double[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    output[i, j] = num;
                }
            }

            return output;
        }

        //py = numpy.random.normal
        public double[,] Normal_Matrix(double mu, double sigma, int rows, int columns)
        {
            double[,] matrix = new double[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = Normal(mu, sigma);
                }
            }

            return matrix;
        }

        //py = numpy.min
        public double[,] Purely_Min(double[,] matrix)
        {
            double min = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] <= min)
                    {
                        min = matrix[i, j];
                    }
                }
            }

            return Purely_Matrix(min, matrix.GetLength(0), matrix.GetLength(1));
        }

        //py = numpy.max
        public double[,] Purely_Max(double[,] matrix)
        {
            double max = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] >= max)
                    {
                        max = matrix[i, j];
                    }
                }
            }

            return Purely_Matrix(max, matrix.GetLength(0), matrix.GetLength(1));
        }

        //常態分配隨機數
        public double Normal(double mu, double sigma)
        {
            //mu = 中心值, sigma = 單邊寬
            double u1 = _random.NextDouble();
            double u2 = _random.NextDouble();

            double rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double rand_normal = mu + (sigma * rand_std_normal);

            return Math.Round(rand_normal, 8);
        }

        //py = lanbda x: scipy.special.expit(x)
        public double[,] Activation_Function(double[,] matrix)
        {
            double[,] output = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    output[i, j] = Sigmoid(matrix[i, j]);
                }
            }
            return output;
        }

        //py = lambda x: scipy.special.logit(x)
        public double[,] Inverse_Activation_Function(double[,] matrix)
        {
            double[,] output = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    output[i, j] = Logit(matrix[i, j]);
                }
            }
            return output;
        }

        //它是 logit 函數的反函數。expit(x) = 1/(1+exp(-x))
        public double Sigmoid(double x)
        {
            //https://docs.scipy.org/doc/scipy/reference/generated/scipy.special.expit.html
            double z = Math.Exp(-x);
            return Math.Round(1 / (1 + z), 8);
        }

        //logit 函數定義為 logit(p) = log(p/(1-p))
        public double Logit(double x)
        {
            //https://docs.scipy.org/doc/scipy/reference/generated/scipy.special.logit.html#scipy.special.logit
            return Math.Log(x / (1 - x));
        }
    }

    public class CSV
    {
        public CSV()
        {

        }

        public Bitmap ArrayToBitmpa(string[] array)
        {
            Bitmap bmp = new Bitmap(28, 28);
            string[] outputs = new string[784];
            for (int i = 0; i < 784; i++)
            {
                outputs[i] = array[i];
            }

            try
            {
                for (int i = 0; i < 784; i++)
                {
                    int Row = i / 28;
                    int Col = i % 28;
                    int alpha = Convert.ToInt32(outputs[i]);
                    alpha = 255 - alpha;
                    Color color = Color.FromArgb(255, alpha, alpha, alpha);
                    bmp.SetPixel(Col, Row, color);
                }
            }
            catch { }

            return bmp;
        }

        public BitmapImage BitmapToBitmapImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            _ = ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public string[] BitmapToArray(Bitmap src)
        {
            string[] array = new string[784];

            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    Color c = src.GetPixel(j, i);
                    double cs = c.R + c.G + c.B;
                    double rc = Math.Round(cs / 3, 0);
                    array[(i * 28) + j] = Math.Round(255 - rc, 0).ToString();
                }
            }

            return array;
        }
    }
}
