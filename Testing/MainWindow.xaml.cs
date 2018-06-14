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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            for (int i = 0; i < 4; i++)
            {
                RadioButton rb = new RadioButton()
                    {Content = "Radio button " + i, IsChecked = i == 0};
                rb.Checked += (sender, args) => { Console.WriteLine("Pressed " + (sender as RadioButton).Tag); };
                rb.Unchecked += (sender, args) => { };
                rb.Tag = i;
                StackPanelAnswers.Children.Add(rb);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string txt = "";
            foreach (var item in StackPanelAnswers.Children)
            {
                if (item is RadioButton)
                {
                    RadioButton rb = (RadioButton) item;
                    if (rb.IsChecked == true)
                    {
                        txt = rb.Content.ToString();
                        MessageBox.Show(txt);
                        break;
                    }
                }
               
            }
        }
    }
}
