using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Sql;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Testing
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _mainConnectionString = @"Data Source=DURON\SQLEXPRESS;Initial Catalog=testing;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private string _mainBasePath;

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
        }

        public string MainBasePath
        {
            get => _mainBasePath;
            set => _mainBasePath = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            //SetBasePath();
            BindComboBox(comboboxFio);
        }

        //Вывод вариантов ответов
        private void GenerateQuestions()
        {

            StackPanelAnswers.Items.Clear();
            textBoxQuestion.Text = String.Empty;
            string connectionString = MainConnectionString;
            //string sql =
            //    $@"SELECT usr.usr_tn, 
            //        qst.qst_nm, 
            //        qst.qst_id, 
            //        anw.anw_id, 
            //        anw.anw_nm, 
            //        DCount('[anw_id]','[anw]','[qst_id] = ' & [qst].[qst_id]) AS qst_cnt,
            //        qst.qst_tp
            //    FROM usr, qst INNER JOIN anw ON qst.qst_id = anw.qst_id
            //    WHERE usr.usr_tn = {textBoxTn.Text} AND qst.qst_id Not In (
            //            SELECT anw.qst_id
            //            FROM anw INNER JOIN rez ON anw.anw_id = rez.anw_id
            //            WHERE rez.[usr_tn] = [usr].[usr_tn]
            //            GROUP BY anw.qst_id
            //        )
            //        ORDER BY qst.qst_id, usr.usr_tn, qst.qst_nm, anw.anw_id;
            //        ";
            string sql = $@"
                SELECT	[usr_tn],
		                [qstMain].[qst_nm], 
		                [qstMain].[qst_id], 
		                [anw].[anw_id], 
		                [anw].[anw_nm],
		                (SELECT COUNT([anw_id]) AS Expr1 FROM [anw] WHERE ([qst_id] = [qstMain].[qst_id])) AS [qst_cnt],
		                qstMain.qst_tp
                FROM [dbo].[usr], [dbo].[qst] as qstMain INNER JOIN [dbo].[anw] ON qstMain.[qst_id] = [anw].[qst_id]
                WHERE [usr].[usr_tn] = {textBoxTn.Text} AND [qstMain].[qst_id] Not In (
		                SELECT [anw].[qst_id]
		                FROM [testing].[dbo].[anw] INNER JOIN [testing].[dbo].[rez] ON [anw].[anw_id] = [rez].[anw_id]
		                WHERE [rez].[usr_tn] = [usr].[usr_tn]
		                GROUP BY [anw].[qst_id]
		                )
                ORDER BY [qstMain].[qst_id], [usr].[usr_tn], [qstMain].[qst_nm], [anw].[anw_id];
                ";
            SqlDataAdapter da = new SqlDataAdapter(sql, connectionString);
            DataSet ds = new DataSet();
            da.Fill(ds, "t");
            int countQuestions = 0;
            try
            {
                countQuestions = int.Parse(ds.Tables["t"].Rows[0]["qst_cnt"].ToString());
            }
            catch (Exception)
            {
                textBoxQuestion.Text = "Тест по данному сотруднику пройден!";
                return;
            }

            string questionName = ds.Tables["t"].Rows[0]["qst_nm"].ToString();
            textBoxQuestion.Text = questionName;

            if (ds.Tables["t"].Rows[0]["qst_tp"].ToString() == "radio")
            {
                //Генерация Radiobutton
                GenerateRadioButton(countQuestions, ds);
            }
            if (ds.Tables["t"].Rows[0]["qst_tp"].ToString() == "check")
            {
                //Генерация Radiobutton
                GenerateCheckBox(countQuestions, ds);
            }
            if (ds.Tables["t"].Rows[0]["qst_tp"].ToString() == "text")
            {
                //Генерация Порядка выбора
                GenerateTextBox(countQuestions, ds);
            }






            //textBoxQuestion.Background = Brushes.Transparent;
            buttonAnswer.IsEnabled = true;
            da.Dispose();
            ds.Dispose();
        }

        //Генерация RadioButton
        public void GenerateRadioButton(int countQuestions, DataSet ds)
        {
            for (int i = 0; i < countQuestions; i++)
            {
                string value = ds.Tables["t"].Rows[i]["anw_nm"].ToString();
                string id = ds.Tables["t"].Rows[i]["anw_id"].ToString();

                TextBlock tbl = new TextBlock()
                {
                    Text=value,
                    TextWrapping = TextWrapping.Wrap
                };

                RadioButton rb = new RadioButton()
                {
                    Content = tbl,
                    Uid = id,
                    Margin = new Thickness(0, 10, 0, 10),
                    FontSize = 14,
                    IsChecked = i == 0
                };

                rb.Checked += (sender, args) => { Console.WriteLine(@"Pressed " + (sender as RadioButton)?.Tag); };
                rb.Unchecked += (sender, args) => { };
                rb.Tag = i;
                StackPanelAnswers.Items.Add(rb);
            }
        }

        //Генерация CheckBox
        public void GenerateCheckBox(int countQuestions, DataSet ds)
        {
            for (int i = 0; i < countQuestions; i++)
            {
                string value = ds.Tables["t"].Rows[i]["anw_nm"].ToString();
                string id = ds.Tables["t"].Rows[i]["anw_id"].ToString();
                TextBlock txb = new TextBlock()
                {
                    Text = value,
                    TextWrapping = TextWrapping.Wrap
                };
                CheckBox ch = new CheckBox()
                {
                    Content = txb,
                    Uid = id,
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 16
                };
                ch.Checked += (sender, args) => { Console.WriteLine(@"Pressed " + (sender as CheckBox)?.Tag); };
                ch.Unchecked += (sender, args) => { };
                ch.Tag = i;
                StackPanelAnswers.Items.Add(ch);
            }
        }

        //Генерация TextBox
        public void GenerateTextBox(int countQuestions, DataSet ds)
        {
            for (int i = 0; i < countQuestions; i++)
            {
                string value = ds.Tables["t"].Rows[i]["anw_nm"].ToString();
                string id = ds.Tables["t"].Rows[i]["anw_id"].ToString();
                int cntCh = int.Parse(ds.Tables["t"].Rows[i]["qst_cnt"].ToString());
                //TextBox tx = new TextBox()
                //{
                //    Text = "",
                //    Uid = id,
                //    Margin = new Thickness(0, 10, 0, 0),
                //    FontSize = 16
                //};
                ComboBox tx = new ComboBox()
                {
                    Uid = id,
                    IsEditable = true,
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 16,
                    Width = 50
                };
                for (int j = 1; j <= cntCh; j++)
                {
                    tx.Items.Add(j);
                }
                Label txl = new Label()
                {
                    Content = value,
                    Uid = id,
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 16
                };
                //ch.Checked += (sender, args) => { Console.WriteLine(@"Pressed " + (sender as TextBox)?.Tag); };
                //ch.Unchecked += (sender, args) => { };
                //ch.Tag = i;
                StackPanel pnl = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                pnl.Children.Add(tx);
                pnl.Children.Add(txl);
                StackPanelAnswers.Items.Add(pnl);
                //StackPanelAnswers.Items.Add(txl);
            }
        }

        public void BindComboBox(ComboBox comboBoxName)
        {
            string connectionString = MainConnectionString;
            string sql = "SELECT usr.usr_tn, usr.usr_fln FROM usr";
            SqlDataAdapter da = new SqlDataAdapter(sql, connectionString);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, "t");
            }
            catch (Exception)
            {
                MessageBox.Show("Не могу получить доступ к базе данных!");
                Environment.Exit(0);
            }
            comboBoxName.DisplayMemberPath = ds.Tables["t"].Columns["usr_fln"].ToString();
            comboBoxName.SelectedValuePath = ds.Tables["t"].Columns["usr_tn"].ToString();
            comboBoxName.ItemsSource = ds.Tables["t"].DefaultView;
            da.Dispose();
            ds.Dispose();
        }

        //Запись ответа в базу
        private void InsertAnswer()
        {
            foreach (var item in StackPanelAnswers.Items)
            {
                if (item is RadioButton rb)
                {
                    //Вставляем radiobutton
                    if (rb.IsChecked == true)
                    {
                        string usrTn = textBoxTn.Text;
                        string anwId = rb.Uid;

                        SqlConnection connection = new SqlConnection
                        {
                            ConnectionString = MainConnectionString
                        };

                        connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter();
                        SqlCommand command = new SqlCommand(
                            "INSERT INTO rez (usr_tn, anw_id) " +
                            "VALUES (@usrTn, @anwId)", connection);

                        command.Parameters.Add("usrTn", SqlDbType.Int);
                        command.Parameters.Add("anwId", SqlDbType.Int);

                        command.Parameters["usrTn"].Value = usrTn;
                        command.Parameters["anwId"].Value = anwId;

                        //command.Parameters.Add(
                        //    usrTn , SqlDbType.Int, 10, "usr_tn");
                        //command.Parameters.Add(
                        //    anwId, SqlDbType.Int, 40, "anw_id");

                        da.InsertCommand = command;
                        da.InsertCommand.ExecuteNonQuery();
                        //MessageBox.Show(txt + "\n" + id);
                        da.Dispose();
                        connection.Close();
                        break;
                    }
                }

            }
            //Встаяляем checkbox
            foreach (var item in StackPanelAnswers.Items)
            {
                if (item is CheckBox ch)
                {
                    //Вставляем radiobutton
                    if (ch.IsChecked == true)
                    {
                        string usrTn = textBoxTn.Text;
                        string anwId = ch.Uid;

                        SqlConnection connection = new SqlConnection
                        {
                            ConnectionString = MainConnectionString
                        };

                        connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter();
                        SqlCommand command = new SqlCommand(
                            "INSERT INTO rez (usr_tn, anw_id) " +
                            "VALUES (@usrTn, @anwId)", connection);

                        command.Parameters.Add("@usrTn", SqlDbType.Int);
                        command.Parameters.Add("@anwId", SqlDbType.Int);

                        command.Parameters["@usrTn"].Value = usrTn;
                        command.Parameters["@anwId"].Value = anwId;

                        //command.Parameters.Add(
                        //    usrTn , SqlDbType.Int, 10, "usr_tn");
                        //command.Parameters.Add(
                        //    anwId, SqlDbType.Int, 40, "anw_id");

                        da.InsertCommand = command;
                        da.InsertCommand.ExecuteNonQuery();
                        //MessageBox.Show(txt + "\n" + id);
                        da.Dispose();
                        connection.Close();
                    }
                }

            }
            //Вставка значений из списка
            //Сначало проверка на наличие пустых значений
            foreach (var item in StackPanelAnswers.Items)
            {
                if (item is StackPanel stp)
                {
                    ComboBox cm = (ComboBox) stp.Children[0];
                    try
                    {
                        string sl = cm.SelectedValue.ToString();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Вы оставили пустое значене!\nПожалуйста, заполните его.");
                        return;
                    }
                }
            }

            foreach (var item in StackPanelAnswers.Items)
            {
                if (item is StackPanel stp)
                {
                    ComboBox cm = (ComboBox) stp.Children[0];
                    //Вставляем radiobutton
                    if (!string.IsNullOrEmpty(cm.SelectedValue.ToString()))
                    {
                        string usrTn = textBoxTn.Text;
                        string anwId = cm.Uid;
                        string rezVl = cm.SelectedValue.ToString();

                        SqlConnection connection = new SqlConnection
                        {
                            ConnectionString = MainConnectionString
                        };

                        connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter();
                        SqlCommand command = new SqlCommand(
                            "INSERT INTO rez (usr_tn, anw_id, rez_vl) " +
                            "VALUES (@usrTn, @anwId, @rezVl)", connection);

                        command.Parameters.Add("@usrTn", SqlDbType.Int);
                        command.Parameters.Add("@anwId", SqlDbType.Int);
                        command.Parameters.Add("@rezVl", SqlDbType.Int);

                        command.Parameters["@usrTn"].Value = usrTn;
                        command.Parameters["@anwId"].Value = anwId;
                        command.Parameters["@rezVl"].Value = rezVl;

                        //command.Parameters.Add(
                        //    usrTn , SqlDbType.Int, 10, "usr_tn");
                        //command.Parameters.Add(
                        //    anwId, SqlDbType.Int, 40, "anw_id");

                        da.InsertCommand = command;
                        da.InsertCommand.ExecuteNonQuery();
                        //MessageBox.Show(txt + "\n" + id);
                        da.Dispose();
                        connection.Close();
                    }
                }

            }

        }

        //Собития при выборе сотрудника в Combobox
        private void SelectWorker()
        {
            try
            {
                textBoxTn.Text = comboboxFio.SelectedValue.ToString();
            }
            catch (Exception)
            {
                return;
            }

            GenerateQuestions();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            buttonAnswer.IsEnabled = false;
            InsertAnswer();
            Thread.Sleep(1000);
            GenerateQuestions();
            
        }

        private void comboboxFio_DropDownClosed(object sender, EventArgs e)
        {
            SelectWorker();
        }

        private void comboboxFio_KeyUp(object sender, KeyEventArgs e)
        {
            SelectWorker();
        }

        //Проверка наличия пути к базе
        //private void SetBasePath()
        //{

        //    string baseDirectory = Environment.CurrentDirectory;
        //    string configFile = baseDirectory + @"\config.txt";
        //    if (File.Exists(configFile))
        //    {
        //        System.IO.StreamReader file = new System.IO.StreamReader(configFile);
        //        string line = file.ReadLine();
        //        MainBasePath = line;
        //        MainConnectionString = @"Provider=Microsoft.Jet.Sql.4.0;Data Source="+line+";Persist Security Info=True;Jet Sql:Database Password=lenovo";
        //        // + ";Persist Security Info=True;Jet Sql:Database Password=lenovo"
        //    }
        //    else
        //    {
        //        MessageBox.Show("Конфигурационный файл не найден!\n" +
        //                        "Создайте файл config.txt в папке с программой и " +
        //                        "укажите в нем путь к базе данных");
        //        Environment.Exit(0);
        //    }
        //}

        private void label_Standart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string path = MainBasePath.Replace("testing.mdb", null) + "Стандарт проведения СР.pptx";

            try
            {
                Process.Start(path);
            }
            catch (Exception)
            {
                MessageBox.Show("Проблема с открытием файла.\nВозможно файл недоступен");
                return;
            }
        }
    }
}
