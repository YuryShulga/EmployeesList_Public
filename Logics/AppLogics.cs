using EmployeesList.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EmployeesList.Logics
{
    internal class AppLogics 
    {
        private MainWindow mainWindow;
        private DbLogics dbLogics;
        private ObservableCollection<Department> departmentCollection;
        private ObservableCollection<Employee> employeeCollection;

        public ObservableCollection<Employee> GetEmployeeCollection()
        {
            return employeeCollection;
        }

        public MainWindow GetMainWindow()
        {
            return mainWindow;
        }
        
        public AppLogics(MainWindow window) 
        {
            mainWindow = window;
            dbLogics = new(this);    
        }

        public void LoadData()
        {
            departmentCollection = dbLogics.GetDepartments();
            employeeCollection = dbLogics.GetEmployees(departmentCollection);

            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.dataGridEmployeesList.ItemsSource = employeeCollection;
                mainWindow.dataGridDepartmentList.ItemsSource = departmentCollection;
            });
        }

        public async Task RemoveSelectedDepartmentsFromCollection()
        {
            StringBuilder stringBuilder = null;
            //временная копия выбранных элементов datagrid
            List<Department> tempDepartmentList = new();
            foreach (Department department in mainWindow.dataGridDepartmentList.SelectedItems)
            {
                tempDepartmentList.Add(new Department() 
                {
                    Id = department.Id,
                    Name = department.Name
                });
            }
            //список id на удаление из коллекции
            List<int> tempIdList = new();
            foreach (Department department in tempDepartmentList)
            {
                int result = await dbLogics.RemoveDepartment(department);   
                if (result == -1)
                {
                    if (stringBuilder == null) { stringBuilder = new(); }
                    stringBuilder.Append($"Подразделение {department.Name} удалить нельзя. Оно используется.\n");
                }
                else 
                {
                    tempIdList.Add(department.Id);
                    departmentCollection.Remove(department);
                }
            }
            //удаляю из коллекции записи по id
            foreach (int id in tempIdList)
            {
                departmentCollection.Remove(
                    departmentCollection.FirstOrDefault(department => (department.Id == id)));
            }
            if (stringBuilder != null) 
            {
                MessageBox.Show(stringBuilder.ToString());
            }
        }

        public async Task RemoveSelectedEmployeeFromCollection()
        {
            StringBuilder stringBuilder = null;
            //временная копия выбранных элементов datagrid
            List<Employee> tempEmployeeList = new();
            foreach (Employee employee in mainWindow.dataGridEmployeesList.SelectedItems)
            {
                tempEmployeeList.Add(new Employee()
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Occupation = employee.Occupation,
                    CurrentDepartment = employee.CurrentDepartment,
                    DepartmentList = employee.DepartmentList
                });
            }
            //список id на удаление из datagrid
            List<int> tempIdList = new();
            foreach (Employee employee in tempEmployeeList)
            {
                int result = await dbLogics.RemoveEmployee(employee);
                if (result == -1)
                {
                    if (stringBuilder == null) { stringBuilder = new(); }
                    stringBuilder.Append($"Сотрудника {employee.FirstName} {employee.LastName} не получилось удалить.\n");
                }
                else
                {
                    tempIdList.Add(employee.Id);
                    employeeCollection.Remove(employee);
                }
            }
            //удаляю из datagrid записи по id
            foreach (int id in tempIdList)
            {
                MessageBox.Show(id.ToString());
                employeeCollection.Remove(
                    employeeCollection.FirstOrDefault(employee => (employee.Id == id)));
            }
            if (stringBuilder != null)
            {
                MessageBox.Show(stringBuilder.ToString());
            }
        }

        public async Task AddEmptyEmployeeToCollection()
        {
            if (departmentCollection.Count == 0) 
            {
                MessageBox.Show("Нет ни одного Подразделения. Для добавления сотрудника нужно сначала добавить Подразделение");
                return;
            }
            Employee employee = new()
            {
                Id = 0,
                FirstName = "First Name",
                LastName = "Last Name",
                Occupation = "Occupation",
                CurrentDepartment = departmentCollection[0],
                DepartmentList = departmentCollection
            };
            employee.PropertyChanged += dbLogics.EmployeeChanged;
            employee.Id = await dbLogics.InsertEmployeeAndGetId(employee);
            if (employee.Id == -1) 
            {
                MessageBox.Show("Ошибка при добавлении нового сотрудника в базу данных");
                return;
            }
            employeeCollection.Add(employee);
        }

        public async Task AddEmptyDepartmentToCollection()
        {
            Department department = new()
            {
                Id = 0,
                Name = "Department"
            };
            department.PropertyChanged += dbLogics.DepartmentChanged;
            department.Id = await dbLogics.InsertDepartmentAndGetId(department);
            if (department.Id == -1)
            {
                MessageBox.Show("Ошибка при добавлении нового подразделения в базу данных");
                return;
            }
            departmentCollection.Add(department);
        }
    }
}
