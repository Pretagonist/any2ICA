using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using HtmlAgilityPack;
using WindowsInput.Native;
using WindowsInput;


namespace anyICA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<GroceryItem> groceryList = new List<GroceryItem>();
        private List<ReplacerItem> replacerList = new List<ReplacerItem>();
        private InputSimulator sim = new InputSimulator();

        public MainWindow()
        {
            InitializeComponent();

            //TODO: check if file exists and if not write an empty one
            //add temporary replaceritems

            /*
            replacerList.Add(new ReplacerItem() { Original = "Smör", Replacement = "Bregott" });
            replacerList.Add(new ReplacerItem() { Original = "Hönökaksmacka", Replacement = "Hönökaka" });
            replacerList.Add(new ReplacerItem() { Original = "Aktiv flerkorn", Replacement = "test" });
            */
            replacerList = System.IO.File.ReadAllText(@"D:\ica\replacer.xml", Encoding.UTF8).FromXml<List<ReplacerItem>>();

            ReplaceListDGC.ItemsSource = replacerList;

            

            //create hotkey binding
            HotKey _hotKey = new HotKey(Key.A, KeyModifier.Shift | KeyModifier.Win, OnHotKeyHandler);
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {

            //check if grocery is selected
            if (main_list.SelectedIndex < 0) return;

            //copy grocery string into clipboard - No longer uses clipboard
            //Clipboard.SetText(((GroceryItem)main_list.SelectedItem).Name);

            //paste grocery string 

            //sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

            //Input text into website textbox

            sim.Keyboard.TextEntry(((GroceryItem)main_list.SelectedItem).Name);
            sim.Keyboard.Sleep(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            //TODO add an enter?

            //advance selection
            //TODO out of bounds check
            main_list.SelectedIndex++;
        }

        private void Ingest_BTN_Click(object sender, RoutedEventArgs e)
        {
            //main_list.Items.Add("Hello World");
            //string[] clip_lines;
            var clipboardHTML = new HtmlDocument();

            //clear groceryList
            this.groceryList = new List<GroceryItem>();
           

            HtmlNodeCollection pNodes;   
            string[] clipSep = new string[] { "<html>" };

            //get text from clipboard
            if (Clipboard.ContainsText(TextDataFormat.Html))
            {
                //remove stuff before <html> tag

                string clipboardRAW = Clipboard.GetText(TextDataFormat.Html);
                string[] clipboardRAWSplit = clipboardRAW.Split(clipSep, StringSplitOptions.None);

                clipboardRAW = "<html>" + clipboardRAWSplit[1];

                //turn clipboard data into html structure
                clipboardHTML.LoadHtml(clipboardRAW);

                //get all P nodes
                pNodes = clipboardHTML.DocumentNode.SelectNodes("//p");

                //create grocery data structure

                

                foreach (HtmlNode n in pNodes)
                {
                    if (n.Attributes["class"].Value.Contains("name"))
                    {
                        groceryList.Add(new GroceryItem(RemoveSpecialCharacters(n.InnerHtml)));                      
                    }
                    else if (n.Attributes["class"].Value.Contains("details"))
                    {
                        groceryList.Last().Description = RemoveSpecialCharacters(n.InnerHtml);
                    }

                }


                main_list.ItemsSource = groceryList;



                /*
                foreach (HtmlNode item in items)
                {
                    //string clean_clip_line = RemoveSpecialCharacters(clip_line);

                   // if (!string.IsNullOrEmpty(clean_clip_line))
                    //{
                        //put text into main list
                    if (item.Attributes["class"].Value.Contains("name"))
                    {

                        //main_list.Items.Add(item.InnerHtml);
                        ListBoxItem new_row = new ListBoxItem();
                        new_row.Content = System.Net.WebUtility.HtmlDecode(item.InnerHtml);
                        new_row.IsSelected = false;
                        //item.HorizontalAlignment = HorizontalAlignment.Center;
                        new_row.FontWeight = FontWeights.Bold;
                        //item.FontFamily = new FontFamily("Tahoma");

                        main_list.Items.Add(new_row);
                    } else
                    {
                        main_list.Items.Add('\t' + System.Net.WebUtility.HtmlDecode(item.InnerHtml));

                    }
                        
                        //string get_class = item.Attributes["class"].Value;
                    //}

                   
                }

               */

            }
        }
        public static string RemoveSpecialCharacters(string str)
        {
            //return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
            return System.Net.WebUtility.HtmlDecode(str.Replace("\r", "").Replace("  ", ""));
        }

        private void Process_BTN_Click(object sender, RoutedEventArgs e)
        {
            //run replacement loop
            foreach (GroceryItem g in groceryList)
            {

                foreach (ReplacerItem r in replacerList)
                {
                    if (g.OriginalText.Equals(r.Original))
                    {
                        g.Name = r.Replacement;
                    }
                }

            }
            main_list.Items.Refresh();

        }


        private void Save_BTN_Click(object sender, RoutedEventArgs e)
        {
            //save new replace item

            //serialize replacelist to disk

            string replacerSerialized = replacerList.ToXml();
            System.IO.File.WriteAllText(@"D:\ica\replacer.xml", replacerSerialized, Encoding.UTF8);

            //serialize grocerylist to disk
            System.IO.File.WriteAllText(@"D:\ica\grocerylist.xml", groceryList.ToXml(), Encoding.UTF8);

            //string text = System.IO.File.ReadAllText(@"C:\Users\Public\TestFolder\WriteText.txt", Encoding.UTF8);

        }

        private void RemoveReplacementItemBTN_Click(object sender, RoutedEventArgs e)
        {
            //ReplacerItem kill = (ReplacerItem)ReplaceListDGC.SelectedItem;
            //kill.
            int index = ReplaceListDGC.SelectedIndex;
            replacerList.RemoveAt(index);
            //ReplaceListDGC.Items.RemoveAt(index);
           
            ReplaceListDGC.Items.Refresh();

        }

        private void GroceryReplaceAddBTN_Click(object sender, RoutedEventArgs e)
        {
            ReplacerItem i = new ReplacerItem() { Original = ((GroceryItem)main_list.SelectedItem).OriginalText };
            replacerList.Add(i);
            //TODO fix bindings so that this isn't needed
            ReplaceListDGC.Items.Refresh();
        }

        private void LoadGroceriesBTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO: file exists?
            groceryList = System.IO.File.ReadAllText(@"D:\ica\grocerylist.xml", Encoding.UTF8).FromXml<List<GroceryItem>>();
            main_list.ItemsSource = groceryList;
            //main_list.Items.Refresh();
        }
    }

    public class GroceryItem
    {
        public string OriginalText { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; } = 0;
        public bool Completed { get; set; } = false;

        public GroceryItem(string text)
        {
            OriginalText = text;
            Name = text;
        }

        public GroceryItem()
        {

        }

    }

    public class ReplacerItem
    {
        public string Original { get; set; }
        public string Replacement { get; set; }      
    }

    public static class Extensions
    {
        public static string ToXml(this object obj)
        {
            XmlSerializer s = new XmlSerializer(obj.GetType());
            using (StringWriter writer = new StringWriter())
            {
                s.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T FromXml<T>(this string data)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                object obj = s.Deserialize(reader);
                return (T)obj;
            }
        }
    }

    //sendkey alternative for wpf from https://michlg.wordpress.com/2013/02/05/wpf-send-keys/
    public static class SendKeys
    {
        /// <summary>
        ///   Sends the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                //if (Keyboard.PrimaryDevice.ActiveSource != null)
                //{
                    var e1 = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, key) { RoutedEvent = Keyboard.KeyDownEvent };
                    InputManager.Current.ProcessInput(e1);
                //}
            }
        }
    }


    //Hotkey capture code from https://stackoverflow.com/questions/48935/how-can-i-register-a-global-hot-key-to-say-ctrlshiftletter-using-wpf-and-ne
    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WmHotKey = 0x0312;

        private bool _disposed = false;

        public Key Key { get; private set; }
        public KeyModifier KeyModifiers { get; private set; }
        public Action<HotKey> Action { get; private set; }
        public int Id { get; set; }

        // ******************************************************************
        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
            if (register)
            {
                Register();
            }
        }

        // ******************************************************************
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(Id, this);

            Debug.Print(result.ToString() + ", " + Id + ", " + virtualKeyCode);
            return result;
        }

        // ******************************************************************
        public void Unregister()
        {
            HotKey hotKey;
            if (_dictHotKeyToCalBackProc.TryGetValue(Id, out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }

        // ******************************************************************
        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey)
                {
                    HotKey hotKey;

                    if (_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            hotKey.Action.Invoke(hotKey);
                        }
                        handled = true;
                    }
                }
            }
        }

        // ******************************************************************
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // ******************************************************************
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Unregister();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }

    // ******************************************************************
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }

    // ******************************************************************

}
