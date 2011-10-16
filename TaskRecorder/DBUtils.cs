using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Data.SqlServerCe;

namespace TaskRecorder
{
    class DBUtils
    {

        public static string DBFilePath
        {
            get
            {
                string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Resource.APP_DATA_PATH);
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }
                return Path.Combine(appFolder, "DB.sdf");
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
                SqlCeCommand cmd = new SqlCeCommand(Resource.DB_DDL, con);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
