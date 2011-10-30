using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;

namespace TaskRecorder
{
    class TaskDAOImpl : TaskDAO
    {
        public IList<Task> FindAll()
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                using (SqlCeCommand cmd = new SqlCeCommand("SELECT Id, TemplateRef, Name, Category, Minutes, Date FROM Task ORDER BY Date, Category, Name", con))
                {
                    return ReadTasks(cmd);
                }
            }
        }

        private IList<Task> ReadTasks(SqlCeCommand cmd)
        {
            IList<Task> result = new List<Task>();

            SqlCeDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Task task = new Task()
                {
                    Id = reader.GetString(0).ToString(),
                    TemplateRef = reader.IsDBNull(1) ? null : reader.GetString(1).ToString(),
                    Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Time = reader.GetInt32(4),
                    Date = reader.GetDateTime(5)
                };

                result.Add(task);
            }

            return result;
        }

        public IList<Task> FindByDateRange(DateTime begin, DateTime end)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                using (SqlCeCommand cmd = new SqlCeCommand("SELECT Id, TemplateRef, Name, Category, Minutes, Date FROM Task WHERE Date >= @begin AND Date <= @end ORDER BY Date, Category, Name", con))
                {
                    cmd.Parameters.AddWithValue("@begin", begin);
                    cmd.Parameters.AddWithValue("@end", end);

                    return ReadTasks(cmd);
                }
            }
        }

        public IList<Task> FindByDate(DateTime date)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                using (SqlCeCommand cmd = new SqlCeCommand("SELECT Id, TemplateRef, Name, Category, Minutes, Date FROM Task WHERE Date=@date ORDER BY Date, Category, Name", con))
                {
                    cmd.Parameters.AddWithValue("@date", date);

                    return ReadTasks(cmd);
                }
            }
        }

        public void Delete(Task task)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                if (task.Id != null)
                {
                    using (SqlCeCommand cmd = new SqlCeCommand("DELETE Task WHERE Id=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", task.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void InsertOrUpdate(Task task)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                using (SqlCeCommand cmd = CreateInsertOrUpdateCommand(task, con))
                {
                    if (task.Id == null)
                    {
                        task.Id = Guid.NewGuid().ToString();
                    }

                    cmd.Parameters.AddWithValue("@id", task.Id);
                    cmd.Parameters.AddWithValue("@template", task.TemplateRef);
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
        }

        private SqlCeCommand CreateInsertOrUpdateCommand(Task task, SqlCeConnection con)
        {
            if (task.Id != null)
            {
                return new SqlCeCommand("UPDATE Task SET Name=@name, TemplateRef=@template, Category=@category, Minutes=@minutes, Date=@date WHERE Id=@id", con);
            }
            else
            {
                return new SqlCeCommand("INSERT INTO Task(Id, TemplateRef, Name, Category, Minutes, Date) VALUES (@id, @template, @name, @category, @minutes, @date)", con);
            }
        }
    }
}
