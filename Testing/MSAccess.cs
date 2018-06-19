using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Access;
using System.Data.OleDb;
using System.Data;

namespace Testing
{
    class MsAccess
    {
        //private string _basePath = Environment.CurrentDirectory + "\\testing.accdb";
        //private string _mainConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Environment.CurrentDirectory + "\\testing.accdb";
        //D:\Dropbox\Task

        private string _basePath = @"C:\Users\ThinkPad\Documents\Dropbox\Task\testing.mdb";
        private string _mainConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\ThinkPad\Documents\Dropbox\Task\testing.mdb";

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
        }

        public string BasePath
        {
            get => _basePath;
            set => _basePath = value;
        }

        //Соединение с базой данных
        public OleDbConnection Connection()
        {
            //Подключение к базе данных Access
            OleDbConnection connection = new OleDbConnection
            {
                ConnectionString =
                    @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _basePath
            };
            //Проверяю подключение
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                return null;
            }
        }

        //Выполнение запроса и возвращение ридера
        public OleDbDataReader Reader(string sql, OleDbConnection connection)
        {
            OleDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            OleDbDataReader reader = command.ExecuteReader();
            return reader;
        }

        //Выполнение запроса и возвращение DataTable
        public DataTable CreateDataTable(OleDbDataReader reader)
        {
            DataTable dt = new DataTable();

            //Добавление колонок
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string hedNm = reader.GetName(i);
                dt.Columns.Add(hedNm);
            }
            //Заполенение таблицы
            while (reader.Read())
            {
                DataRow row = dt.NewRow();
                int i = 0;
                foreach (DataColumn column in dt.Columns)
                {
                    string hedNm = column.ColumnName; //Название столбца
                    row[hedNm] = reader.GetValue(i).ToString();
                    if (row[hedNm].ToString().Contains(" 0:00:00"))
                        row[hedNm] = row[hedNm].ToString().Replace(" 0:00:00", "");
                    i++;
                }
                dt.Rows.Add(row);
            }
            reader.Close();
            return dt;
        }
    }
}
