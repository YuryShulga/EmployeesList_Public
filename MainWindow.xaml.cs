using EmployeesList.Logics;
using EmployeesList.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace EmployeesList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppLogics appLogics;
        public MainWindow()
        {
            InitializeComponent();
            appLogics = new(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            appLogics.LoadData();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Keyboard.ClearFocus();
            e.Cancel = false;
        }

        private void menuItemEmployees_Click(object sender, RoutedEventArgs e)
        {
            dataGridEmployeesList.Visibility = Visibility.Visible;
            dataGridDepartmentList.Visibility = Visibility.Collapsed;
        }

        private void menuItemDepartments_Click(object sender, RoutedEventArgs e)
        {
            dataGridEmployeesList.Visibility = Visibility.Collapsed;
            dataGridDepartmentList.Visibility = Visibility.Visible;
        }

        private void menuItemDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridEmployeesList.Visibility == Visibility.Visible)
            {
                appLogics.RemoveSelectedEmployeeFromCollection();
            }
            else
            {
                appLogics.RemoveSelectedDepartmentsFromCollection();
            }
        }

        private void menuItemAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridEmployeesList.Visibility == Visibility.Visible)
            {
                appLogics.AddEmptyEmployeeToCollection();
            }
            else 
            {
                appLogics.AddEmptyDepartmentToCollection();
            }
        }

        private void dataGridEmployeesList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                menuItemDeleteItem_Click(sender, new RoutedEventArgs());
            }
        }

        private void dataGridDepartmentList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                menuItemDeleteItem_Click(sender, new RoutedEventArgs());
            }
        }
    }
}