using System;
using System.ComponentModel;
using System.Windows;

namespace TaskRecorder
{
    public class TemplateService : INotifyPropertyChanged
    {
        private static readonly TemplateService instance = new TemplateService();
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly TaskTemplateDAO templateDao = new TaskTemplateDAOImpl();

        private TemplateService()
        {
            AttachEventHandlers();
            PopulateTemplates();
        }

        public static TemplateService Instance { get { return instance; } }

        private void AttachEventHandlers()
        {
            templates.ListChanged += new ListChangedEventHandler(templates_ListChanged);
            templates.ItemDeleting += new DeletingItemHandler<TaskTemplate>(templates_ItemDeleting);
        }

        private void DetachEventHandlers()
        {
            templates.ListChanged -= new ListChangedEventHandler(templates_ListChanged);
            templates.ItemDeleting -= new DeletingItemHandler<TaskTemplate>(templates_ItemDeleting);
        }

        void templates_ItemDeleting(object sender, TaskTemplate item)
        {
            templateDao.Delete(item);
        }

        void templates_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemChanged)
            {
                TaskTemplate template = TaskTemplates[e.NewIndex];
                templateDao.InsertOrUpdate(template);
            }

            NotifyPropertyChanged("TaskTemplates");
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private CustomBindingList<TaskTemplate> templates = new CustomBindingList<TaskTemplate>();
        public CustomBindingList<TaskTemplate> TaskTemplates
        {
            get { return templates; }
        }

        private void PopulateTemplates()
        {
            try
            {
                CustomBindingList<TaskTemplate> newTemplates = new CustomBindingList<TaskTemplate>();
                foreach (TaskTemplate template in templateDao.FindAll())
                {
                    newTemplates.Add(template);
                }

                DetachEventHandlers();
                templates = newTemplates;
                AttachEventHandlers();
                NotifyPropertyChanged("TaskTemplates");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
