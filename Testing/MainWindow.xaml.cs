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
using System.Data.OleDb;

namespace Testing
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BindComboBox(comboboxFio);
        }

        //Вывод вариантов ответов
        private void GenerateRadioButton()
        {

            MsAccess acs = new MsAccess();
            string connectionString = acs.MainConnectionString;
            string sql =
                $@"SELECT usr.usr_tn, qst.qst_nm, qst.qst_id, anw.anw_id, anw.anw_nm, DCount('[anw_id]','[anw]','[qst_id] = ' & [qst]![qst_id]) AS qst_cnt
                    FROM usr, qst INNER JOIN anw ON qst.qst_id = anw.qst_id
                    WHERE usr.usr_tn = {textBoxTn.Text} AND qst.qst_id Not In (
                        SELECT anw.qst_id
                        FROM anw INNER JOIN rez ON anw.anw_id = rez.anw_id
                        WHERE rez.[usr_tn] = [usr]![usr_tn]
                        GROUP BY anw.qst_id
                    )
                    ORDER BY usr.usr_tn, qst.qst_nm, anw.anw_id;
                    ";
            OleDbDataAdapter da = new OleDbDataAdapter(sql, connectionString);
            DataSet ds = new DataSet();
            da.Fill(ds, "t");

            int countQuestions = int.Parse(ds.Tables["t"].Rows[0]["qst_cnt"].ToString());

            string questionName = ds.Tables["t"].Rows[0]["qst_nm"].ToString();
            textBoxQuestion.Text = questionName;



            for (int i = 0; i < countQuestions; i++)
            {
                string value = ds.Tables["t"].Rows[i]["anw_nm"].ToString();
                string id = ds.Tables["t"].Rows[i]["anw_id"].ToString();
                RadioButton rb = new RadioButton()
                {
                    Content = value,
                    Uid = id,
                    IsChecked = i == 0
                };
                rb.Checked += (sender, args) => { Console.WriteLine(@"Pressed " + (sender as RadioButton)?.Tag); };
                rb.Unchecked += (sender, args) => { };
                rb.Tag = i;
                StackPanelAnswers.Children.Add(rb);
            }
        }

        public void BindComboBox(ComboBox comboBoxName)
        {
            MsAccess acs = new MsAccess();
            string connectionString = acs.MainConnectionString;
            string sql = "SELECT usr.usr_tn, usr.usr_fln FROM usr";
            OleDbDataAdapter da = new OleDbDataAdapter(sql, connectionString);
            DataSet ds = new DataSet();
            da.Fill(ds, "t");
            comboBoxName.DisplayMemberPath = ds.Tables["t"].Columns["usr_fln"].ToString();
            comboBoxName.SelectedValuePath = ds.Tables["t"].Columns["usr_tn"].ToString();
            comboBoxName.ItemsSource = ds.Tables["t"].DefaultView;

        }

        //Запись ответа в базу
        private void InsertAnswer()
        {
            foreach (var item in StackPanelAnswers.Children)
            {
                if (item is RadioButton rb)
                {
                    if (rb.IsChecked == true)
                    {
                        var txt = rb.Content.ToString();
                        var id = rb.Uid.ToString();

                        MsAccess acs = new MsAccess();
                        OleDbDataAdapter adapter = new OleDbDataAdapter();
                        OleDbCommand command = new OleDbCommand(
                            "INSERT INTO rez (usr_tn, anw_id) " +
                            "VALUES (?, ?)", acs.Connection());

                        command.Parameters.Add(
                            "usr_tn", OleDbType.Integer, 10, "usr_tn");
                        command.Parameters.Add(
                            "anw_id", OleDbType.Integer, 40, "anw_id");

                        adapter.InsertCommand = command;
                        adapter.InsertCommand.ExecuteNonQuery();
                        //MessageBox.Show(txt + "\n" + id);
                        break;
                    }
                }

            }
        }

        //Собития при выборе сотрудника в Combobox
        private void SelectWorker()
        {
            textBoxTn.Text = comboboxFio.SelectedValue.ToString();
            StackPanelAnswers.Children.Clear();
            GenerateRadioButton();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InsertAnswer();
        }

        private void comboboxFio_DropDownClosed(object sender, EventArgs e)
        {
            SelectWorker();
        }

        private void comboboxFio_KeyUp(object sender, KeyEventArgs e)
        {
            SelectWorker();
        }
    }
}
