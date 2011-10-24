using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRecorder;

namespace TestProject
{
    [TestClass]
    public class TaskServiceTest
    {
        private readonly DateTime today = DateTime.Now.Date;
        private readonly DateTime tomorrow = DateTime.Now.Date.AddDays(1);

        [TestInitialize]
        public void Init()
        {
            DBUtils.DBFileName = "test.sdf";
            if (File.Exists(DBUtils.DBFilePath))
            {
                File.Delete(DBUtils.DBFilePath);
            }
        }

        [TestMethod]
        public void TestInitWorks()
        {
            Assert.AreEqual("test.sdf", DBUtils.DBFileName);
            Assert.IsFalse(File.Exists(DBUtils.DBFilePath));
        }

        [TestMethod]
        public void TestCrudOps()
        {
            Create();
            Update();
            Delete();
        }

        private void Create()
        {
            Assert.AreEqual(0, TaskService.Instance.Tasks.Count);
            Assert.AreEqual(today, TaskService.Instance.CurrentDate);

            Task task1 = new Task() { Name = "today1", Category = "todayCat1", Date = today, Time = 5 };
            Task task2 = new Task() { Name = "today2", Category = "todayCat2", Date = today, Time = 6 };
            TaskService.Instance.Tasks.Add(task1);
            TaskService.Instance.Tasks.Add(task2);
            Assert.AreEqual(2, TaskService.Instance.Tasks.Count);

            TaskService.Instance.CurrentDate = tomorrow;
            Assert.AreEqual(0, TaskService.Instance.Tasks.Count);
            Task task3 = new Task() { Name = "tomorrow1", Category = "tomorrowCat1", Date = tomorrow, Time = 7 };
            TaskService.Instance.Tasks.Add(task3);
            Assert.AreEqual(1, TaskService.Instance.Tasks.Count);

            TaskService.Instance.CurrentDate = today;
            Assert.AreEqual(2, TaskService.Instance.Tasks.Count);
            Assert.AreEqual("today1", TaskService.Instance.Tasks[0].Name);
            Assert.AreEqual("todayCat1", TaskService.Instance.Tasks[0].Category);
            Assert.AreEqual(today, TaskService.Instance.Tasks[0].Date);
            Assert.AreEqual(5, TaskService.Instance.Tasks[0].Time);

            Assert.AreEqual("today2", TaskService.Instance.Tasks[1].Name);
            Assert.AreEqual("todayCat2", TaskService.Instance.Tasks[1].Category);
            Assert.AreEqual(today, TaskService.Instance.Tasks[1].Date);
            Assert.AreEqual(6, TaskService.Instance.Tasks[1].Time);

            TaskService.Instance.CurrentDate = tomorrow;
            Assert.AreEqual(1, TaskService.Instance.Tasks.Count);
            Assert.AreEqual("tomorrow1", TaskService.Instance.Tasks[0].Name);
            Assert.AreEqual("tomorrowCat1", TaskService.Instance.Tasks[0].Category);
            Assert.AreEqual(tomorrow, TaskService.Instance.Tasks[0].Date);
            Assert.AreEqual(7, TaskService.Instance.Tasks[0].Time);
        }

        private void Update()
        {
            TaskService.Instance.CurrentDate = today;
            Task updated = TaskService.Instance.Tasks[0];
            updated.Name = "updated";
            updated.Category = "updatedCat";
            updated.Time = 10;
            Assert.IsNotNull(updated.Id);

            TaskService.Instance.CurrentDate = tomorrow;
            TaskService.Instance.CurrentDate = today;
            Assert.AreEqual("updated", TaskService.Instance.Tasks[1].Name);
            Assert.AreEqual("updatedCat", TaskService.Instance.Tasks[1].Category);
            Assert.AreEqual(10, TaskService.Instance.Tasks[1].Time);
        }

        private void Delete()
        {
            TaskService.Instance.CurrentDate = tomorrow;
            Assert.AreEqual(1, TaskService.Instance.Tasks.Count);
            TaskService.Instance.Tasks.RemoveAt(0);
            Assert.AreEqual(0, TaskService.Instance.Tasks.Count);
            TaskService.Instance.CurrentDate = today;
            Assert.AreEqual(2, TaskService.Instance.Tasks.Count);
            TaskService.Instance.Tasks.RemoveAt(0);
            Assert.AreEqual(1, TaskService.Instance.Tasks.Count);
            TaskService.Instance.CurrentDate = tomorrow;
            Assert.AreEqual(0, TaskService.Instance.Tasks.Count);
        }
    }
}
