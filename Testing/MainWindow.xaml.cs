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
using System.Windows.Threading;

namespace Testing
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private string _mainConnectionString = @"Data Source=DURON\SQLEXPRESS;Initial Catalog=testing;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private string _mainConnectionString = @"Data Source=LENOVO\SQLEXPRESS;Initial Catalog=ufs;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //private string _mainConnectionString = @"Data Source=alauda\alauda;Initial Catalog=ufs;User ID=prozorova_os;Password=q1w2e3r4";
        //private string _mainBasePath;

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
        }

        //public string MainBasePath
        //{
        //    get => _mainBasePath;
        //    set => _mainBasePath = value;
        //}
        DispatcherTimer _timer;
        TimeSpan _time;

        public MainWindow()
        {
            InitializeComponent();

            _time = TimeSpan.FromMinutes(30);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                labelTime.Content = _time.ToString("c");
                if (_time == TimeSpan.Zero) _timer.Stop();
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();

            //SetBasePath();
            //Поиск презентации стандарта в папке с исполняемым файлом
            CheckStandart();
            BindComboBox(comboboxFio);
        }

        //Поиск файла со стандартом
        private void CheckStandart()
        {
            string standartName = Environment.CurrentDirectory + "//Стандарт проведения СР.pptx";
            if (!File.Exists(standartName))
            {
                label_Standart.Visibility = Visibility.Collapsed;
            }
        }

        //Вывод вариантов ответов
        private void GenerateQuestions()
        {

            StackPanelAnswers.Items.Clear();
            textBoxQuestion.Text = String.Empty;
            string connectionString = MainConnectionString;
            //[sr]., [dbo].
            string sql = $@"
               SELECT  [usr_tn],
                                                [qstMain].[qst_nm], 
                                                [qstMain].[qst_id], 
                                                [anw].[anw_id], 
                                                [anw].[anw_nm],
                                                (SELECT COUNT([anw_id]) AS Expr1 FROM [dbo].[anw] WHERE ([qst_id] = [qstMain].[qst_id])) AS [qst_cnt],
                                                qstMain.qst_tp
                FROM [dbo].[usr], [dbo].[qst] as qstMain INNER JOIN [dbo].[anw] ON qstMain.[qst_id] = [anw].[qst_id]
                WHERE [usr].[usr_tn] = {textBoxTn.Text} AND [qstMain].[qst_id] Not In (
                                                SELECT [anw].[qst_id]
                                                FROM [dbo].[anw] INNER JOIN [dbo].[rez] ON [anw].[anw_id] = [rez].[anw_id]
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
                ComboBox tx = new ComboBox()
                {
                    Uid = id,
                    IsEditable = true,
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 16,
                    Width = 50,
                    Height = 30
                };
                for (int j = 1; j <= cntCh; j++)
                {
                    tx.Items.Add(j);
                }
                TextBlock txl = new TextBlock()
                {
                    Text = value,
                    Uid = id,
                    Margin = new Thickness(10, 10, 10, 0),
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 550
                };
                StackPanel pnl = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                pnl.Children.Add(tx);
                pnl.Children.Add(txl);
                StackPanelAnswers.Items.Add(pnl);
            }
        }

        //Генерация выпадающего списка
        public void BindComboBox(ComboBox comboBoxName)
        {
            string connectionString = MainConnectionString;
            string sql = "SELECT [dbo].[usr].[usr_tn], [dbo].[usr].[usr_fln] FROM [dbo].[usr]";
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
                            "INSERT INTO [dbo].rez (usr_tn, anw_id) " +
                            "VALUES (@usrTn, @anwId)", connection);

                        command.Parameters.Add("usrTn", SqlDbType.Int);
                        command.Parameters.Add("anwId", SqlDbType.Int);

                        command.Parameters["usrTn"].Value = usrTn;
                        command.Parameters["anwId"].Value = anwId;

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
                            "INSERT INTO [dbo].rez (usr_tn, anw_id) " +
                            "VALUES (@usrTn, @anwId)", connection);

                        command.Parameters.Add("@usrTn", SqlDbType.Int);
                        command.Parameters.Add("@anwId", SqlDbType.Int);

                        command.Parameters["@usrTn"].Value = usrTn;
                        command.Parameters["@anwId"].Value = anwId;

                        da.InsertCommand = command;
                        da.InsertCommand.ExecuteNonQuery();
                        da.Dispose();
                        connection.Close();
                    }
                }

            }
            //Вставка значений из списка
            //Сначала проверка на наличие пустых значений
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
                            "INSERT INTO [dbo].rez (usr_tn, anw_id, rez_vl) " +
                            "VALUES (@usrTn, @anwId, @rezVl)", connection);

                        command.Parameters.Add("@usrTn", SqlDbType.Int);
                        command.Parameters.Add("@anwId", SqlDbType.Int);
                        command.Parameters.Add("@rezVl", SqlDbType.Int);

                        command.Parameters["@usrTn"].Value = usrTn;
                        command.Parameters["@anwId"].Value = anwId;
                        command.Parameters["@rezVl"].Value = rezVl;

                        da.InsertCommand = command;
                        da.InsertCommand.ExecuteNonQuery();
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

        //Получение.отправка одиночого значения sql
        private string  SingleResult(string sql, string connectionString)
        {
            string result = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);

                try
                {
                    conn.Open();
                    result = cmd.ExecuteScalar().ToString();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    Console.WriteLine(ex.Message);
                }

                conn.Close();

            }

            return result;
        }

        //Подсчет времени
        private bool CountTime()
        {
            //ищу был ли старт по данному сотруднику
            bool result = true;
            int limit = 30;
            string user = Environment.UserName;
            string sql = 
                $@"SELECT   [usr_st]
                FROM        [dbo].[usr]
                WHERE       [usr_tn] = {textBoxTn.Text}";
            string resSql = SingleResult(sql, MainConnectionString);
            if (string.IsNullOrEmpty(resSql))
            {
                //DateTime dt = new DateTime();
                //dt = DateTime.Now;
                sql = $@"UPDATE	[dbo].[usr] 
                        SET		[usr_st] = CURRENT_TIMESTAMP, [usr_fn] = CURRENT_TIMESTAMP,[usr_login] = '{user}'
                        WHERE	[usr_tn] = {textBoxTn.Text}";
            }
            else
            {
                sql = $@"UPDATE	[dbo].[usr] 
                        SET		[usr_fn] = CURRENT_TIMESTAMP
                        WHERE	[usr_tn] = {textBoxTn.Text}";
            }
            SingleResult(sql, MainConnectionString);
            sql = $@"SELECT     DATEDIFF ( MI , [usr_st] , [usr_fn] ) 
                    FROM        [dbo].[usr]
                    WHERE       [usr_tn] = {textBoxTn.Text}";
            resSql = SingleResult(sql, MainConnectionString);
            try
            {
                if (int.Parse(resSql) >= limit)
                {
                    MessageBox.Show($"Превышен лимит времени ({limit} мин.) на выполнение теста");
                    result = false;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                //throw;
            }

            return result;

            //если был и старт и финишь, проверяю на 30 мин 
            //если не был записываю время старта
            //если был записываю время филана
        }

        void timer_Tick()
        {
            string sql = $@"SELECT     DATEDIFF ( SS , [usr_st] , [usr_fn] ) 
                    FROM        [dbo].[usr]
                    WHERE       [usr_tn] = {textBoxTn.Text}";

            string resSql = SingleResult(sql, MainConnectionString);
            try
            {
                //labelTime.Content = DateTime.Parse("20");
                _time = TimeSpan.FromMinutes(30) - TimeSpan.FromSeconds(int.Parse(resSql));
            }
            catch (Exception)
            {

                //labelTime.Content = DateTime.Now.ToLongTimeString();
                _time = TimeSpan.FromMinutes(30);
            }
        }

        //Нажатие кнопки "Ответить"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            buttonAnswer.IsEnabled = false;
            labelTime.Visibility = Visibility.Visible;
            timer_Tick();
            if (!CountTime()) return;
            InsertAnswer();
            Thread.Sleep(1000);
            //Расстановка времени
            GenerateQuestions();
        }

        //Закрытие ComboBox
        private void comboboxFio_DropDownClosed(object sender, EventArgs e)
        {
            SelectWorker();
            timer_Tick();
        }

        //Пепермещение по ComboBox кнопками
        private void comboboxFio_KeyUp(object sender, KeyEventArgs e)
        {
            SelectWorker();
            timer_Tick();
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

        //Изменение курсора при наведении мыши на надпись Стандарт
        private void label_Standart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string path = Environment.CurrentDirectory + "//Стандарт проведения СР.pptx";

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

        //Поиск пустых значений при расставлении порядка в ответах с комбобоксами
        private void checkEmpty()
        {
            foreach (var item in StackPanelAnswers.Items)
            {
                if (item is StackPanel stp)
                {
                        ComboBox cm = (ComboBox)stp.Children[0];
                    try
                    {
                        string sl = cm.SelectedValue.ToString();
                    }
                    catch (Exception)
                    {
                        buttonAnswer.Foreground = Brushes.Crimson;
                        labelEmptyValue.Visibility = Visibility.Visible;
                        //MessageBox.Show("Вы оставили пустое значене!\nПожалуйста, заполните его.");
                        return;
                    }
                }
            }
        }

        //при наведении мыши на кнопку крашу ее красным и вывожу предупреждение
        private void buttonAnswer_MouseEnter(object sender, MouseEventArgs e)
        {
            checkEmpty();
        }

        //сбрамыва все что натворил при наведении
        private void buttonAnswer_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonAnswer.Foreground = Brushes.Black;
            labelEmptyValue.Visibility = Visibility.Collapsed;
        }
    }
}
