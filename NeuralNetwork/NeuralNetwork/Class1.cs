using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeuralNetwork
{
    public class neuralNetwork
    {
        private readonly int inodes, hnodes, onodes;
        private readonly double lr;
        private readonly Random _random = new Random();
        public static double[,] wih;
        public static double[,] who;

        public neuralNetwork(int inputnodes, int hiddennodes, int outputnodes, double learningrate)
        {
            _ = _random.Next();
            //self.inodes = inputnodes
            inodes = inputnodes;
            //self.hnodes = hiddennodes
            hnodes = hiddennodes;
            //self.onodes = outputnodes
            onodes = outputnodes;
            //self.wih = numpy.random.normal(0.0, pow(self.inodes, -0.5), (self.hnodes, self.inodes))
            wih = Normal_Matrix(0, Math.Pow(inodes, -0.5), hnodes, inodes); //200 * 784
            //self.who = numpy.random.normal(0.0, pow(self.hnodes, -0.5), (self.onodes, self.hnodes))
            who = Normal_Matrix(0, Math.Pow(hnodes, -0.5), onodes, hnodes); //10 * 200
            //self.lr = learningrate
            lr = learningrate;
        }

        public async Task Train(double[] input_list, double[] targets_list)
        {
            //inputs = numpy.array(inputs_list, ndmin=2).T
            double[,] inputs1 = DoubleArrayToDoubleMatrix(input_list, 1, input_list.GetLength(0));
            double[,] inputs2 = Matrix_Transpose(inputs1);

            //targets = numpy.array(targets_list, ndmin = 2).T
            double[,] targets1 = DoubleArrayToDoubleMatrix(targets_list, 1, targets_list.GetLength(0));
            double[,] targets2 = Matrix_Transpose(targets1);

            //hidden_inputs = numpy.dot(self.wih, inputs)
            double[,] hidden_inputs = Matrix_Dot(wih, inputs2);

            //hidden_outputs = self.activation_function(hidden_inputs)
            double[,] hidden_outputs = Activation_Function(hidden_inputs);

            //final_inputs = numpy.dot(self.who, hidden_outputs)
            double[,] final_inputs = Matrix_Dot(who, hidden_outputs);

            //final_outputs = self.activation_function(final_inputs)
            double[,] final_outputs = Activation_Function(final_inputs);

            //output_errors = targets - final_outputs
            double[,] output_errors = Matrix_Subtract(targets2, final_outputs);

            //hidden_errors = numpy.dot(self.who.T, output_errors)
            double[,] hidden_errors = Matrix_Dot(Matrix_Transpose(who), output_errors);

            //self.who += self.lr * numpy.dot((output_errors * final_outputs * (1 - final_outputs)), numpy.transpose(hidden_outputs))
            //(1 - final_outputs)
            double[,] who1 = Matrix_Subtract(Purely_Matrix(1, final_outputs.GetLength(0), final_outputs.GetLength(1)), final_outputs);
            //output_errors * final_outputs
            double[,] who2 = Matrix_Multiplied(output_errors, final_outputs);
            //(output_errors * final_outputs * (1 - final_outputs)
            double[,] who3 = Matrix_Multiplied(who2, who1);
            //numpy.transpose(hidden_outputs)
            double[,] who4 = Matrix_Transpose(hidden_outputs);
            //numpy.dot((
            double[,] who5 = Matrix_Dot(who3, who4);
            //self.lr * numpy.dot
            double[,] who6 = Matrix_Multiplied(Purely_Matrix(lr, who5.GetLength(0), who5.GetLength(1)), who5);
            //self.who +=
            double[,] o = who;
            who = Matrix_Add(o, who6);

            //self.wih += self.lr * numpy.dot((hidden_errors * hidden_outputs * (1 - hidden_outputs)), numpy.transpose(inputs))
            //(1 - hidden_outputs)
            double[,] wih1 = Matrix_Subtract(Purely_Matrix(1, hidden_outputs.GetLength(0), hidden_outputs.GetLength(1)), hidden_outputs);
            //hidden_errors * hidden_outputs
            double[,] wih2 = Matrix_Multiplied(hidden_errors, hidden_outputs);
            //(hidden_errors * hidden_outputs * (1 - hidden_outputs)
            double[,] wih3 = Matrix_Multiplied(wih2, wih1);
            //numpy.transpose(inputs)
            double[,] wih4 = Matrix_Transpose(inputs2);
            //numpy.dot((
            double[,] wih5 = Matrix_Dot(wih3, wih4);
            //self.lr * numpy.dot
            double[,] wih6 = Matrix_Multiplied(Purely_Matrix(lr, wih5.GetLength(0), wih5.GetLength(1)), wih5);
            //self.wih +=
            double[,] h = wih;
            wih = Matrix_Add(h, wih6);

            await Task.Delay(1);
        }

        public async Task<double[,]> Query(double[] input_list)
        {
            //inputs = numpy.array(inputs_list, ndmin=2).T
            double[,] input = DoubleArrayToDoubleMatrix(input_list, 1, input_list.GetLength(0));
            double[,] inputs = Matrix_Transpose(input);

            //hidden_inputs = numpy.dot(self.wih, inputs)
            double[,] hidden_inputs = Matrix_Dot(wih, inputs);

            //hidden_outputs = self.activation_function(hidden_inputs)
            double[,] hidden_outputs = Activation_Function(hidden_inputs);

            //final_inputs = numpy.dot(self.who, hidden_outputs)
            double[,] final_inputs = Matrix_Dot(who, hidden_outputs);

            //final_outputs = self.activation_function(final_inputs)
            double[,] final_outputs = Activation_Function(final_inputs);

            await Task.Delay(1);
            return final_outputs;
        }



        //py = lanbda x: scipy.special.expit(x)
        private double[,] Activation_Function(double[,] matrix)
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

        //py = array.T
        private double[,] Matrix_Transpose(double[,] input)
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
        private double[,] Matrix_Dot(double[,] matrix1, double[,] matrix2)
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
        private double[,] Matrix_Add(double[,] matrix1, double[,] matrix2)
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
        private double[,] Matrix_Subtract(double[,] matrix1, double[,] matrix2)
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
        private double[,] Matrix_Multiplied(double[,] matrix1, double[,] matrix2)
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
        private double[,] Matrix_Divide(double[,] matrix1, double[,] matrix2)
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
        private double[,] Purely_Matrix(double num, int rows, int columns)
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
        private double[,] Normal_Matrix(double mu, double sigma, int rows, int columns)
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


        //公開的工具
        public double Sigmoid(double x)
        {
            //def sigmoid(z):
            //return 1.0 / (1 + np.exp(-z))
            double z = Math.Exp(-x);
            return Math.Round(1 / (1 + z), 8);
        }

        public double Normal(double mu, double sigma)
        {
            //mu = 中心值, sigma = 單邊寬
            double u1 = _random.NextDouble();
            double u2 = _random.NextDouble();

            double rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double rand_normal = mu + (sigma * rand_std_normal);

            return Math.Round(rand_normal, 8);
        }

        public double[] Purely_Array(double num, int columns)
        {
            double[] output = new double[columns];
            for (int i = 0; i < columns; i++)
            {
                output[i] = num;
            }

            return output;
        }

        public double[] StringArrayToDoubleArray(string[] row)
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

        public string[] DoubleArrayToStringArray(double[] row)
        {
            string[] output = new string[784];

            for (int i = 0; i < 784; i++)
            {
                output[i] = row[i].ToString();
            }

            return output;
        }

        public double[] ArrayWeighted(double[] row)
        {
            double[] output = new double[row.GetLength(0)];
            for(int i = 0; i < row.GetLength(0); i++)
            {
                output[i] = (row[i] / 255.0 * 0.99) + 0.01;
            }

            return output;
        }

        public double[,] DoubleArrayToDoubleMatrix(double[] array, int rows, int columns)
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
