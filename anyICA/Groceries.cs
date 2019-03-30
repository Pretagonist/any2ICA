using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace anyICA
{
    public class GroceryList : ObservableCollection<GroceryItem>
    {

        private ReplacerList replacer;

        public GroceryList() : base()
        {
            
        }

        public void SetReplacer(ReplacerList r)
        {
            this.replacer = r;

            //subscribe to replacement changes
            ReplacerList.ReplacerUpdated += DoReplacements;

        }

        //method called when replacercollection changes or when the list needs to update
        public void DoReplacements()
        {
            //run replacement loop
            foreach (GroceryItem g in Items)
            {

                foreach (ReplacerItem r in replacer)
                {
                    if (g.OriginalText.Equals(r.Original))
                    {
                        g.Name = r.Replacement;
                    }
                }

            }
          
        }

        internal void LoadFromFile()
        {
            //TODO: file exists?
            Clear();
            
            File.ReadAllText(@"D:\ica\grocerylist.xml", Encoding.UTF8).FromXml<GroceryList>().ToList().ForEach(this.Add);
            DoReplacements();
        }

        internal void SaveToFile()
        {
            File.WriteAllText(@"D:\ica\grocerylist.xml", this.ToXml(), Encoding.UTF8);
        }

        internal void IngestFromClipboard()
        {
            var clipboardHTML = new HtmlDocument();

            HtmlNodeCollection pNodes;
            string[] clipSep = new string[] { "<html>" };

            //get text from clipboard
            if (!Clipboard.ContainsText(TextDataFormat.Html))
            {
                MessageBox.Show("Clipboard does not contain valid data");
                return;
            }

            //remove stuff before <html> tag
            string clipboardRAW = Clipboard.GetText(TextDataFormat.Html);
            string[] clipboardRAWSplit = clipboardRAW.Split(clipSep, StringSplitOptions.None);

            clipboardRAW = "<html>" + clipboardRAWSplit[1];

            //turn clipboard data into html structure
            clipboardHTML.LoadHtml(clipboardRAW);

            //get all P nodes
            pNodes = clipboardHTML.DocumentNode.SelectNodes("//p");

            if (pNodes == null)
            {
                MessageBox.Show("Clipboard does not contain valid data");
                return;
            }

            //clear groceryList
            Clear();
                
            //fill grocery data structure

            foreach (HtmlNode n in pNodes)
            {
                if (n.Attributes["class"].Value.Contains("name"))
                {
                    Add(new GroceryItem(Tools.RemoveSpecialCharacters(n.InnerHtml)));
                }
                else if (n.Attributes["class"].Value.Contains("details"))
                {
                    string details = Tools.RemoveSpecialCharacters(n.InnerHtml);
                    this.Last().Description = details;
                    this.Last().SubstitutionOK = !details.Contains("EJ UTBYTE");
                }

            }
            DoReplacements();

            
        }


    }

    public class GroceryItem : INotifyPropertyChanged
    {
        public string OriginalText { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; } = 0;
        public bool Completed { get; set; } = false;
        public bool SubstitutionOK { get; set; } = true;

        //event handling
        public event PropertyChangedEventHandler PropertyChanged;

        public GroceryItem(string text)
        {
            OriginalText = text;
            Name = text;
        }

        public GroceryItem()
        {

        }

    }

   
}
