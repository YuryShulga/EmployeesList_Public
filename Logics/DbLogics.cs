using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeesList.Models;
using System.Data;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using EmployeesList.Logics;

namespace EmployeesList
{
    internal class DbLogics
    {
        private string connectionString = "Server=(localdb)\\mssqllocaldb;Database=EmployeesList_db;Trusted_Connection=True;";
        private AppLogics appLogics;
        public DbLogics(AppLogics appLogics)
        {
            this.appLogics = appLogics;
            EnsureDatabaseCreated();
        }

        private void EnsureDatabaseCreated()
        {
            using (SqlConnection connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;"))
            {
                connection.Open();
                string query = $"SELECT COUNT(*) FROM sys.databases WHERE name = 'EmployeesList_db'";
                using (SqlCommand checkDbCommand = new SqlCommand(query, connection))
                {
                    int databaseExists = (int)checkDbCommand.ExecuteScalar();
                    if (databaseExists == 0)
                    {
                        query = "CREATE DATABASE EmployeesList_db";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        query = "USE EmployeesList_db;" +
                            " CREATE TABLE Employees_T (Id INT PRIMARY KEY IDENTITY" +
                            ", FirstName_F NVARCHAR(100) NOT NULL, " +
                            "LastName_F NVARCHAR(100) NOT NULL, " +
                            "Occupation_F NVARCHAR(100) NOT NULL, " +
                            "DepartmentId_F INT NOT NULL)";
                        using (SqlCommand checkTableCommand = new SqlCommand(query, connection))
                        {
                            checkTableCommand.ExecuteNonQuery();
                        }

                        query = "USE EmployeesList_db;" +
                            " CREATE TABLE Departments_T (Id INT PRIMARY KEY IDENTITY, " +
                            "Name_F NVARCHAR(100) NOT NULL)";
                        using (SqlCommand checkTableCommand = new SqlCommand(query, connection))
                        {
                            checkTableCommand.ExecuteNonQuery();
                        }

                        query = "USE EmployeesList_db;" +
                            " ALTER TABLE Employees_T " +
                            "ADD CONSTRAINT FK_Employees_T_Departments_T " +
                            "FOREIGN KEY (DepartmentId_F) " +
                            "REFERENCES Departments_T(Id);";
                        using (SqlCommand checkTableCommand = new SqlCommand(query, connection))
                        {
                            checkTableCommand.ExecuteNonQuery();
                        }
                    }

                    
                }
            }
        }

        public ObservableCollection<Employee> GetEmployees(ObservableCollection<Department> departmentsList = null)
        {
            ObservableCollection<Employee> employeeCollection = new();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {       
                connection.Open();
                string query = "SELECT " +
                               "Employees_T.Id as EmpId, " +
                               "Employees_T.FirstName_F as EmpFirstName, " +
                               "Employees_T.LastName_F as EmpLastName, " +
                               "Employees_T.Occupation_F as EmpOccupation, " +
                               "Departments_T.Id as DepId, " +
                               "Departments_T.Name_F as DepName " +
                               "FROM Employees_T " +
                               "LEFT JOIN Departments_T ON Employees_T.DepartmentId_F = Departments_T.Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Проверка, есть ли строки в результате запроса
                        if (reader.HasRows)
                        {
                            // Чтение данных построчно
                            while (reader.Read())
                            {
                                Employee employee = new();
                                employee.Id = Convert.ToInt32(reader["EmpId"]);
                                employee.FirstName = reader["EmpFirstName"].ToString();
                                employee.LastName = reader["EmpLastName"].ToString();
                                employee.Occupation = reader["EmpOccupation"].ToString();
                                employee.CurrentDepartment = new Department()
                                {
                                    Id = Convert.ToInt32(reader["DepId"]),
                                    Name = reader["DepName"].ToString()
                                };
                                employee.DepartmentList = departmentsList;
                                employee.PropertyChanged += EmployeeChanged;
                                employeeCollection.Add(employee);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Таблица \"Employees_T\" пустая");
                        }
                    }
                }
            }
            return employeeCollection;
        }

        public void EmployeeChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateEmployee(sender as Employee);
        }



        public async Task UpdateEmployee(Employee employee)
        {   
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE Employees_T SET FirstName_F = @firstName, LastName_F = @lastName, " +
                               "Occupation_F = @occupation, DepartmentId_F = @DepartmentId " +
                               "WHERE Id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", employee.Id);
                    command.Parameters.AddWithValue("@firstName", employee.FirstName);
                    command.Parameters.AddWithValue("@lastName", employee.LastName);
                    command.Parameters.AddWithValue("@occupation", employee.Occupation);
                    command.Parameters.AddWithValue("@DepartmentId", employee.CurrentDepartment.Id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> RemoveEmployee(Employee employee) 
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Employees_T WHERE Id = @EmployeeId;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", employee.Id);
                    try
                    {
                        int result = Convert.ToInt32(await command.ExecuteNonQueryAsync());
                        if (result > 0)
                        {
                            return result;
                        }
                    }
                    catch (DbException ){}//запрос не сработал, возращаю отридцательный вариант
                    return -1;
                }
            }
        }

        public async Task<int> RemoveDepartment(Department department)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Departments_T WHERE Id = @DepartmentId; " +
                    "SELECT @@ROWCOUNT;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentId", department.Id);
                    try
                    {
                         int result = Convert.ToInt32( await command.ExecuteNonQueryAsync());
                        
                        if (result > 0)
                        {
                            return result;
                        }
                    }
                    catch (DbException){}
                    return -1;
                }
            }
        }

        public async Task<int> InsertEmployeeAndGetId(Employee employee)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "INSERT INTO Employees_T (FirstName_F, LastName_F, Occupation_F, DepartmentId_F) " +
                               "VALUES (@firstName, @lastName, @occupation, @DepartmentId);" +
                               "SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@firstName", employee.FirstName);
                    command.Parameters.AddWithValue("@lastName", employee.LastName);
                    command.Parameters.AddWithValue("@occupation", employee.Occupation);
                    command.Parameters.AddWithValue("@DepartmentId", employee.CurrentDepartment.Id);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null && int.TryParse(result.ToString(), out int newEmployeeId))
                    {
                        return newEmployeeId;
                    }
                    return -1;
                }
            }
        }

        public async Task<int> InsertDepartmentAndGetId(Department department)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "INSERT INTO Departments_T (Name_F) " +
                               "VALUES (@Name);" +
                               "SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", department.Name);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null && int.TryParse(result.ToString(), out int newDepartmentId))
                    {
                        return newDepartmentId;
                    }
                    return -1;
                }
            }
        }

        public ObservableCollection<Department> GetDepartments()
        {
            ObservableCollection<Department> departmentCollection = new();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * " +
                               "FROM Departments_T";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Department department = new();
                                department.Id = Convert.ToInt32(reader["Id"]);
                                department.Name = reader["Name_F"].ToString();
                                department.PropertyChanged += DepartmentChanged;
                                departmentCollection.Add(department);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Таблица \"Departments_T\" пустая");
                        }
                    }
                }
            }
            return departmentCollection;
        }

        public void DepartmentChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateDepartment(sender as Department);
        }

        public async Task UpdateDepartment(Department department)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE Departments_T SET Name_F = @departmentName " +
                               "WHERE Id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", department.Id);
                    command.Parameters.AddWithValue("@departmentName", department.Name);
                    await command.ExecuteNonQueryAsync();

                    //обновляю сделанное изменение EmployeeCollection
                    foreach (Employee employee in appLogics.GetEmployeeCollection())
                    {
                        if (employee.CurrentDepartment.Id == department.Id)
                        {
                            employee.CurrentDepartment.Name = department.Name;
                        }
                    }
                    appLogics.GetMainWindow().dataGridEmployeesList.Items.Refresh();
                }
            }
        }
    }
}
