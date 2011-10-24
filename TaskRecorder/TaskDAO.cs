using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskRecorder
{
    interface TaskDAO
    {
        IList<Task> FindAll();
        IList<Task> FindByDateRange(DateTime begin, DateTime end);
        IList<Task> FindByDate(DateTime date);
        void Delete(Task task);
        void InsertOrUpdate(Task task);
    }
}
