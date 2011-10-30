using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskRecorder
{
    interface TaskTemplateDAO
    {
        IList<TaskTemplate> FindAll();
        void Delete(TaskTemplate task);
        void InsertOrUpdate(TaskTemplate task);
    }
}
