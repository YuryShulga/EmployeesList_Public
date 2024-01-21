using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EmployeesList.Models
{
    public class Employee : INotifyPropertyChanged
    {
        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string firstName;
        public string FirstName
        {
            get { return firstName; }
            set
            {
                if (firstName != value)
                {
                    firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        private string lastName;
        public string LastName
        {
            get { return lastName; }
            set
            {
                if (lastName != value)
                {
                    lastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }

        private string occupation;
        public string Occupation
        {
            get { return occupation; }
            set
            {
                if (occupation != value)
                {
                    occupation = value;
                    OnPropertyChanged(nameof(Occupation));
                }
            }
        }

        private Department currentDepartment;
        public Department CurrentDepartment
        {
            get { return currentDepartment; }
            set
            {
                if (currentDepartment != value)
                {
                    currentDepartment = value;
                    OnPropertyChanged(nameof(CurrentDepartment));
                }
            }
        }

        private ObservableCollection<Department> departmentList;
        public ObservableCollection<Department> DepartmentList
        {
            get { return departmentList; }
            set
            {
                if (departmentList != value)
                {
                    departmentList = value;
                    OnPropertyChanged(nameof(DepartmentList));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
        }
    }
}