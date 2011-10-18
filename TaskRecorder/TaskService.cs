using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data.SqlServerCe;
using System.Windows;
using System.IO;

namespace TaskRecorder
{
    public class TaskService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static readonly TaskService instance = new TaskService();

        private TaskService()
        {
            DBUtils.EnsureDBExists(CheckDBContents);
            AttachEventHandlers();
            SetTasksByDate();
        }

        public static TaskService Instance { get { return instance; } }

        private void CheckDBContents()
        {
            FindByDate(DateTime.Now.Date);
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
            Delete(item);
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
                InsertOrUpdate(task);
            }
        }

        private CustomBindingList<Task> tasks = new CustomBindingList<Task>();
        public CustomBindingList<Task> Tasks
        {
            get { return tasks; }
            set
            {
                tasks = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Tasks"));
                }
            }
        }

        private DateTime currentDate = DateTime.Now.Date;
        public DateTime CurrentDate
        {
            get { return currentDate; }
            set
            {
                currentDate = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentDate"));
                }

                SetTasksByDate();
            }
        }

        private void SetTasksByDate()
        {
            try
            {
                CustomBindingList<Task> newTasks = new CustomBindingList<Task>();
                foreach (Task task in FindByDate(currentDate))
                {
                    newTasks.Add(task);
                }

                DetachEventHandlers();
                Tasks = newTasks;
                AttachEventHandlers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private IList<Task> FindByDate(DateTime date)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                SqlCeCommand cmd = new SqlCeCommand("SELECT Id, Name, Category, Minutes FROM Task WHERE Date=@date", con);
                cmd.Parameters.AddWithValue("@date", date);

                IList<Task> result = new List<Task>();

                SqlCeDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Task task = new Task()
                    {
                        Id = reader.GetString(0).ToString(),
                        Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Time = reader.GetInt32(3),
                        Date = date
                    };

                    result.Add(task);
                }

                return result;
            }
        }

        private IList<Task> FindAll()
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                SqlCeCommand cmd = new SqlCeCommand("SELECT Id, Name, Category, Minutes, Date FROM Task ORDER BY Date, Category, Name", con);

                IList<Task> result = new List<Task>();

                SqlCeDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Task task = new Task()
                    {
                        Id = reader.GetString(0).ToString(),
                        Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Time = reader.GetInt32(3),
                        Date = reader.GetDateTime(4)
                    };

                    result.Add(task);
                }

                return result;
            }
        }

        private void InsertOrUpdate(Task task)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                SqlCeCommand cmd = null;

                if (task.Id != null)
                {
                    cmd = new SqlCeCommand("UPDATE Task SET Name=@name, Category=@category, Minutes=@minutes, Date=@date WHERE Id=@id", con);
                }
                else
                {
                    cmd = new SqlCeCommand("INSERT INTO Task(Id, Name, Category, Minutes, Date) VALUES (@id, @name, @category, @minutes, @date)", con);
                    task.Id = Guid.NewGuid().ToString();
                }

                cmd.Parameters.AddWithValue("@id", task.Id);
                cmd.Parameters.AddWithValue("@name", task.Name);
                cmd.Parameters.AddWithValue("@category", task.Category);
                cmd.Parameters.AddWithValue("@minutes", task.Time);
                cmd.Parameters.AddWithValue("@date", task.Date);

                foreach (SqlCeParameter param in cmd.Parameters)
                {
                    if (param.Value == null)
                    {
                        param.Value = DBNull.Value;
                    }
                }

                cmd.ExecuteNonQuery();
            }
        }

        private void Delete(Task task)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                if (task.Id != null)
                {
                    SqlCeCommand cmd = new SqlCeCommand("DELETE Task WHERE Id=@id", con);
                    cmd.Parameters.AddWithValue("@id", task.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExportCSV(string path)
        {
            IList<Task> all = FindAll();

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("Name,Category,Date,Minutes");

                foreach (Task task in all)
                {
                    writer.WriteLine(task.Name + "," + task.Category + "," + task.Date.ToShortDateString() + "," + task.Time);
                }
            }
        }
    }
}
