using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Specialized;

namespace anyICA
{

    public delegate void ReplacerUpdatedHandler();

    public class ReplacerList : ObservableCollection<ReplacerItem>
    {

        public static event ReplacerUpdatedHandler ReplacerUpdated;

        public ReplacerList() : base()
        {
            //((List<ReplacerItem>)this.Items).ForEach(PropertyChanged += ReplacerList_PropertyChanged;);

            //subscribe to changes to the collection
            //subscribe to replacement changes
            this.CollectionChanged += ReplacerCollectionChanged;
        }

        private void ReplacerList_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal void LoadFromFile()
        {
            Clear();
            
            File.ReadAllText(@"D:\ica\replacer.xml", Encoding.UTF8).FromXml<ReplacerList>().ToList().ForEach(this.Add);
        }

        public void SaveToFile()
        {
            File.WriteAllText(@"D:\ica\replacer.xml", this.ToXml(), Encoding.UTF8);
        }

        

        private void ReplacerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Print("Replacer - CollectionChanged - Action was: " + e.Action.ToString());

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Debug.Print("Replacer - new item original: " + ((ReplacerItem)e.NewItems[0]).Original);
                ReplacerItem r = (ReplacerItem)e.NewItems[0];

                r.PropertyChanged += ReplacerItemPropertyChanged;
            }


        }
        public void ReplacerItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReplacerItem r = (ReplacerItem)sender;

            Debug.Print("Replacer - ItemPropertyChanged - Property was: " + e.PropertyName + " to value: " + r.GetType().GetProperty(e.PropertyName).GetValue(r, null));
            ReplacerUpdated();
        }

    }

 

    public class ReplacerItem : INotifyPropertyChanged

    { 
        public string Original { get; set; }
        public string Replacement { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }


}
