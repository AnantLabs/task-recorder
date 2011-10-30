using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Linq;

namespace TaskRecorder
{
    public class TaskService : INotifyPropertyChanged
    {
        private static readonly TaskService instance = new TaskService();
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly TaskDAO taskDao = new TaskDAOImpl();
        private readonly TaskTemplateDAO templateDao = new TaskTemplateDAOImpl();

        private TaskService()
        {
            DBUtils.EnsureDBExists(CheckDBContents);
            AttachEventHandlers();
            SetTasksByDate();
        }

        public static TaskService Instance { get { return instance; } }

        private void CheckDBContents()
        {
            taskDao.FindByDate(DateTime.Now.Date);
            templateDao.FindAll();
        }

        private void AttachEventHandlers()
        {
            tasks.ListChanged += new ListChangedEventHandler(tasks_ListChanged);
            tasks.ItemDeleting += new DeletingItemHandler<Task>(tasks_ItemDeleting);
        }

        private void DetachEventHandlers()
        {
            tasks.ListChanged -= new ListChangedEventHandler(tasks_ListChanged);
            tasks.ItemDeleting -= new DeletingItemHandler<Task>(tasks_ItemDeleting);
        }

        void tasks_ItemDeleting(object sender, Task item)
        {
            taskDao.Delete(item);
        }

        void tasks_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemChanged)
            {
                Task task = Tasks[e.NewIndex];
                if (e.ListChangedType == ListChangedType.ItemAdded)
                {
                    task.Date = CurrentDate;
                }
                taskDao.InsertOrUpdate(task);
            }

            NotifyPropertyChanged("Tasks");
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private CustomBindingList<Task> tasks = new CustomBindingList<Task>();
        public CustomBindingList<Task> Tasks
        {
            get { return tasks; }
        }

        public void Reload()
        {
            SetTasksByDate();
        }

        private DateTime currentDate = DateTime.Now.Date;
        public DateTime CurrentDate
        {
            get { return currentDate; }
            set
            {
                currentDate = value;
                SetTasksByDate();
                NotifyPropertyChanged("CurrentDate");
            }
        }

        private void SetTasksByDate()
        {
            try
            {
                CustomBindingList<Task> newTasks = new CustomBindingList<Task>();
                foreach (Task task in taskDao.FindByDate(currentDate))
                {
                    newTasks.Add(task);
                }

                foreach (TaskTemplate template in templateDao.FindAll())
                {
                    bool found = newTasks.Any(task => task.TemplateRef == template.Id);
                    if (!found)
                    {
                        newTasks.Insert(0, new Task() { Name = template.Name, Category = template.Category, TemplateRef=template.Id });
                    }
                }

                DetachEventHandlers();
                tasks = newTasks;
                AttachEventHandlers();
                NotifyPropertyChanged("Tasks");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void ExportCSV(string path)
        {
            IList<Task> all = taskDao.FindAll();

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("Name,Category,Date,Minutes");

                foreach (Task task in all)
                {
                    writer.WriteLine(task.Name + "," + task.Category + "," + task.Date.ToShortDateString() + "," + task.Time);
                }
            }
        }

        public IList<Task> FindByDateRange(DateTime begin, DateTime end)
        {
            return taskDao.FindByDateRange(begin, end);
        }
    }
}
