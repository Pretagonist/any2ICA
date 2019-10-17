using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace anyICA
{
    public class GroceryList : ObservableCollection<GroceryItem>
    {

        private ReplacerList replacer;
        private Collection<GroceryItem> ajaxInput;

        public GroceryList() : base()
        {
            
        }

        public void IngestFromAjax(string input)
        {
            var testInput = JsonConvert.DeserializeObject<Collection<GroceryItem>>(input);
            if (testInput.First().Name != "")
            {
                ajaxInput = testInput;
            }
            
            //var jsonlist = JsonConvert.DeserializeObject<Collection<GroceryItem>>(Clipboard.GetText(TextDataFormat.Text));
            //foreach (GroceryItem item in jsonlist)
            //{
            //    Add(item);
            //}
            
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
            foreach (GroceryItem g in Items.ToList())
            {
                foreach (ReplacerItem r in replacer)
                {
                    if (g.OriginalText.Equals(r.Original))
                    {
                        if (r.Replacement == "#")
                        {
                            g.Name = "im-dead";
                            Items.Remove(g);
                            this.OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Remove,g));
                            break;
                        } 
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

        internal void IngestFromClipboard(string type)
        {
            if (type == "HTML")
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
                    if (n.Attributes["style"] == null) continue;
                    if (n.Attributes["style"].Value.Contains("font-size: 20px"))
                    {
                        string itemName = Tools.RemoveSpecialCharacters(n.InnerHtml);
                        string itemAmount = "1";
                        //Separate out eventual amount data
                        Regex matchParenthesis = new Regex(@"^(.*)\((\d+)\).*$");
                        Match match = matchParenthesis.Match(itemName);
                        if (match.Success)
                        {
                            itemName = match.Groups[1].Value;
                            itemAmount = match.Groups[2].Value;
                        }

                        Add(new GroceryItem(itemName));
                        this.Last().Amount = itemAmount;
                    }
                    else if (n.Attributes["style"].Value.Contains("font-size: 16px"))
                    {
                        string details = Tools.RemoveSpecialCharacters(n.InnerHtml);
                        this.Last().Description = details;
                        this.Last().SubstitutionOK = !details.Contains("EJ UTBYTE");
                    }

                }
            }
            else
            {
                //clear groceryList
                Clear();

                //var jsonlist = JsonConvert.DeserializeObject<Collection<GroceryItem>>(Clipboard.GetText(TextDataFormat.Text));
                foreach (GroceryItem item in ajaxInput)
                {
                    Add(item);
                }


            }




            DoReplacements();            
        }


    }

    
    [DebuggerDisplay("{_amount}", Name = "{_name}")]
    public class GroceryItem : INotifyPropertyChanged
    {
        public string OriginalText { get; set; }
        public string Description { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set {
                _name = value;
                OriginalText = (OriginalText == null) ? value : OriginalText;
            }
        }



        private int _amount = 1;

        public string Amount
        {
            get => _amount.ToString();
            set { _amount = int.TryParse(value, out _amount) ? _amount : 1; }
        }

        //public string Amount
        //{
        //    set {
        //        _amount = int.TryParse(value, out _amount) ? _amount : 0;
        //    }
        //}


        public int GetAmount()
        {
            return _amount;
        }

        public bool Completed { get; set; } = false;
        public bool SubstitutionOK { get; set; } = true;

        

        //event handling
        public event PropertyChangedEventHandler PropertyChanged;

        public GroceryItem(string text)
        {
            Name = text;
        }

        public GroceryItem()
        {

        }

    }

   
}
