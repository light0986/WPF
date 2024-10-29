using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 撈資料.Models
{
    public class SQL
    {
        public static string ConnectionString { get; private set; } = "";

        private static string Acc = "", Pass = "";

        public static SqlConnection connection;

        public static void SetString(string connectionString)
        {
            if (connectionString != null && connectionString != "")
            {
                ConnectionString = connectionString;
            }

            connection = new SqlConnection(ConnectionString);
        }

        public static string PingServer()
        {
            try
            {
                connection.Open();
                connection.Close();
                return "OK";
            }
            catch
            {
                return "Error";
            }
        }

        public static string CheckRoles(string Account, string Password)
        {
            try
            {
                string sql = "select RoleID from UserRoles where UserID = (select UserID from Users where UserName = '{0}' and Password = '{1}'); ";
                string queryString = String.Format(sql, Account, Password);

                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                string RoleID = "";
                                while (reader.Read())
                                {
                                    RoleID = reader["RoleID"].ToString();
                                }

                                if ("4e7914f2-7e12-4ef5-ab7f-fe7160771bb1" == RoleID)
                                {
                                    Acc = Account;
                                    Pass = Password;
                                    return "身分驗證正確";
                                }
                                else
                                {
                                    return "您不是系統管理者";
                                }
                            }
                            else
                            {
                                return "不存在";
                            }
                        }
                    }
                    catch
                    {
                        return "資料取得過程錯誤";
                    }
                }
            }
            catch
            {
                return "連線錯誤";
            }
        }

        public static DataTable Commend(string queryString)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                DataTable response = new DataTable();
                                response.Load(reader);
                                return response;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch {  }

            return null;
        }

        public static void ConnectionOpen()
        {
            connection.Open();
        }

        public static void ConnectionClose()
        {
            connection.Close();
        }
    }
}
