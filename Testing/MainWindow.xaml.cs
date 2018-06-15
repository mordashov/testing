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
            GenerateRadioButton();
            BindComboBox(comboboxFio);
        }

        //Вывод вариантов ответов
        private void GenerateRadioButton()
        {
            for (int i = 0; i < 4; i++)
            {
                RadioButton rb = new RadioButton()
                    { Content = "Radio button " + i, IsChecked = i == 0 };
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
                        MessageBox.Show(txt);
                        break;
                    }
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InsertAnswer();
        }

        private void comboboxFio_DropDownClosed(object sender, EventArgs e)
        {
            textBoxTn.Text = comboboxFio.SelectedValue.ToString();
        }
    }
}
