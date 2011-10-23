using System;
using System.ComponentModel;

namespace TaskRecorder
{
 
    public delegate void DeletingItemHandler<T>(object sender, T item);

    /// <summary>
    /// Workaround for odd behaviour that ListChangedType.ItemDeleted is useless because ListChangedEventArgs.NewIndex is already gone
    /// </summary>
    [Serializable]
    public class CustomBindingList<T> : BindingList<T>
    {

        public event DeletingItemHandler<T> ItemDeleting;

        protected override void RemoveItem(int index)
        {
            T item = this[index];
            if (ItemDeleting != null)
            {
                ItemDeleting(this, item);
            }
            base.RemoveItem(index);
        }
    }

}