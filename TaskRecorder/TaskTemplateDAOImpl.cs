using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;

namespace TaskRecorder
{
    class TaskTemplateDAOImpl : TaskTemplateDAO
    {
        public IList<TaskTemplate> FindAll()
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                using (SqlCeCommand cmd = new SqlCeCommand("SELECT Id, Name, Category FROM TaskTemplate ORDER BY Category, Name", con))
                {
                    return ReadTemplates(cmd);
                }
            }
        }

        private IList<TaskTemplate> ReadTemplates(SqlCeCommand cmd)
        {
            IList<TaskTemplate> result = new List<TaskTemplate>();

            SqlCeDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TaskTemplate template = new TaskTemplate()
                {
                    Id = reader.GetString(0).ToString(),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Category = reader.IsDBNull(2) ? null : reader.GetString(2)
                };

                result.Add(template);
            }

            return result;
        }

        public void Delete(TaskTemplate template)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                if (template.Id != null)
                {
                    using (SqlCeCommand cmd = new SqlCeCommand("DELETE TaskTemplate WHERE Id=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", template.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void InsertOrUpdate(TaskTemplate template)
        {
            using (SqlCeConnection con = new SqlCeConnection(DBUtils.ConnectionString))
            {
                con.Open();

                using (SqlCeCommand cmd = CreateInsertOrUpdateCommand(template, con))
                {
                    if (template.Id == null)
                    {
                        template.Id = Guid.NewGuid().ToString();
                    }

                    cmd.Parameters.AddWithValue("@id", template.Id);
                    cmd.Parameters.AddWithValue("@name", template.Name);
                    cmd.Parameters.AddWithValue("@category", template.Category);

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

        private SqlCeCommand CreateInsertOrUpdateCommand(TaskTemplate template, SqlCeConnection con)
        {
            if (template.Id != null)
            {
                return new SqlCeCommand("UPDATE TaskTemplate SET Name=@name, Category=@category WHERE Id=@id", con);
            }
            else
            {
                return new SqlCeCommand("INSERT INTO TaskTemplate(Id, Name, Category) VALUES (@id, @name, @category)", con);
            }
        }
    }
}
