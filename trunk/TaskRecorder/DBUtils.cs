using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows;

namespace TaskRecorder
{
    public class DBUtils
    {

        private static string dbFileName = "DB.sdf";
        public static string DBFileName
        {
            get { return dbFileName; }
            set { dbFileName = value; }
        }

        public static string DBFilePath
        {
            get
            {
                string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Resource.APP_DATA_PATH);
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }
                return Path.Combine(appFolder, dbFileName);
            }
        }

        public static string ConnectionString
        {
            get
            {
                return "Data Source=" + DBFilePath;
            }
        }

        public delegate void CheckDBContents();

        public static void EnsureDBExists(CheckDBContents checkContents)
        {
            if (File.Exists(DBFilePath))
            {
                try
                {
                    VerifyAndRepair();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot repair database file " + DBFilePath + ". Please delete it so that empty database will be created.\n\n" + e.ToString());
                    System.Environment.Exit(0);
                }

                try
                {
                    checkContents();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot read database file " + DBFilePath + ". Please delete it so that empty database will be created.\n\n" + e.ToString());
                    System.Environment.Exit(0);
                }
            }
            else
            {
                try
                {
                    CreateEmptyDB();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot create database file " + DBFilePath + ".\n\n" + e.ToString());
                    System.Environment.Exit(0);
                }
            }
        }

        private static void VerifyAndRepair()
        {
            using (SqlCeEngine engine = new SqlCeEngine(ConnectionString))
            {
                if (!engine.Verify())
                {
                    engine.Repair(null, RepairOption.RecoverAllOrFail);
                }
            }

        }

        private static void CreateEmptyDB()
        {
            using (SqlCeEngine engine = new SqlCeEngine(ConnectionString))
            {
                engine.CreateDatabase();
            }

            using (SqlCeConnection con = new SqlCeConnection(ConnectionString))
            {
                con.Open();
                using (SqlCeCommand cmd = new SqlCeCommand())
                {
                    cmd.Connection = con;

                    foreach (string statement in Resource.DB_DDL.Split(';'))
                    {
                        if (statement.Trim().Length > 0)
                        {
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
