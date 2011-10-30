using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskRecorder
{
    /// <summary>
    /// Interaction logic for TaskTemplates.xaml
    /// </summary>
    public partial class TaskTemplates : UserControl
    {
        public TaskTemplates()
        {
            InitializeComponent();
        }

        private void TemplateGrid_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == DataGrid.DeleteCommand)
            {
                DataGrid dg = (DataGrid)e.Source;
                TaskTemplate template = (TaskTemplate)dg.CurrentItem;

                if (MessageBox.Show("Delete \"" + template.Name + "\"?", "Confirm delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
