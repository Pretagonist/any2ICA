using System.Windows;
using System.Windows.Input;
using WindowsInput.Native;
using WindowsInput;
using Nancy.Hosting.Self;
using System;
using System.Windows.Controls;

//tasks that aren't properly localized
//TODO Style selected item
//TODO Current item in hero bar
//TODO Mark items as done
//DONE Get amounts in someway


namespace anyICA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GroceryList groceryList = new GroceryList();
        private ReplacerList replacerList = new ReplacerList();

        private InputSimulator sim = new InputSimulator();

        public MainWindow()
        {
            InitializeComponent();

            //TODO: check if file exists and if not write an empty one

            /*
            replacerList.Add(new ReplacerItem() { Original = "Smör", Replacement = "Bregott" });
            replacerList.Add(new ReplacerItem() { Original = "Hönökaksmacka", Replacement = "Hönökaka" });
            replacerList.Add(new ReplacerItem() { Original = "Aktiv flerkorn", Replacement = "test" });
            */
            replacerList.LoadFromFile(); 
                
            //grocery list needs to know about replacements
            groceryList.SetReplacer(replacerList);

            //setup sources
            GroceryListBOX.ItemsSource = groceryList;
            ReplaceListDGC.ItemsSource = replacerList;

            //create hotkey binding
            HotKey _hotKey = new HotKey(Key.A, KeyModifier.Shift | KeyModifier.Win, OnHotKeyHandler);

            //setup Comms
            var hostConfigs = new HostConfiguration
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            //using (var host = new NancyHost(hostConfigs, new Uri("http://localhost:8234")))

            //{
            NancyHost host = new NancyHost(hostConfigs, new Uri("http://localhost:8234"));
                host.Start();
                Console.WriteLine("Running on http://localhost:8234");
                //Console.ReadLine();
                //var comms = new Communicator();
                
            //};
            
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {

            //check if grocery is selected
            if (GroceryListBOX.SelectedIndex < 0) return;

            //Input text into website textbox

            sim.Keyboard.TextEntry(((GroceryItem)GroceryListBOX.SelectedItem).Name);
            sim.Keyboard.Sleep(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            //TODO Enter yes or no?

            //TODO Mark previous as done

            //scroll into center view
            GroceryListBOX.ScrollToCenterOfView(GroceryListBOX.SelectedItem);

            //advance selection
            //TODO strangely never goes out of bounds at all. Investigate.
            GroceryItem g;
            do
            {
                GroceryListBOX.SelectedIndex++;
                g = GroceryListBOX.SelectedItem as GroceryItem;
            } while (g.Name == "!");
            


            if (GroceryListBOX.SelectedIndex >= GroceryListBOX.Items.Count) GroceryListBOX.SelectedIndex = 0;
        }

        public string GetItem()
        {
            //check if grocery is selected
            if (GroceryListBOX.SelectedIndex < 0) return "Nothing selected!";

            //scroll into center view
            GroceryListBOX.ScrollToCenterOfView(GroceryListBOX.SelectedItem);
            var ret = ((GroceryItem)GroceryListBOX.SelectedItem).Name;
            GroceryListBOX.SelectedIndex++;
            if (GroceryListBOX.SelectedIndex >= GroceryListBOX.Items.Count) GroceryListBOX.SelectedIndex = 0;

            return ret;

        }



        private void Ingest_BTN_Click(object sender, RoutedEventArgs e)
        {
            groceryList.IngestFromClipboard("HTML");           
        }

        private void Process_BTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO check if depracted
            groceryList.DoReplacements();
        }

        private void Save_BTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO default files and file names
            //serialize replacer to file
            replacerList.SaveToFile(); 

            //serialize grocerylist to file
            groceryList.SaveToFile(); 

        }

        private void GroceryReplaceAddBTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO should be handled by groceryclass?
            ReplacerItem i = new ReplacerItem() { Original = ((GroceryItem)GroceryListBOX.SelectedItem).OriginalText };
            replacerList.Add(i);
            //GroceryListBOX.ScrollToCenterOfView(GroceryListBOX.SelectedItem);
            //ReplaceListDGC.ScrollToCenterOfView(ReplaceListDGC.SelectedItem);
            //ReplaceListDGC.ScrollIntoView(ReplaceListDGC.Items[ReplaceListDGC.Items.Count - 1]);
            ReplaceListDGC.Focus();
            DataGridCellInfo cellInfo = new DataGridCellInfo(ReplaceListDGC.Items[ReplaceListDGC.Items.Count - 2], ReplaceListDGC.Columns[1]);
            //set the cell to be the active one
            ReplaceListDGC.CurrentCell = cellInfo;
            //scroll the item into view
            ReplaceListDGC.ScrollIntoView(ReplaceListDGC.Items[ReplaceListDGC.Items.Count - 2]);
            //cellInfo.Focus();
            ReplaceListDGC.BeginEdit();
            ReplaceListDGC.UpdateLayout();



            //var item = this.ReplaceListDGC.Items[ReplaceListDGC.Items.Count - 1];
            //var column = this.ReplaceListDGC.Columns[2];
            //var cellToEdit = new GridViewCellInfo(item, column, this.ReplaceListDGC);

            //ReplaceListDGC.CurrentCellInfo = cellToEdit;
            //ReplaceListDGC.BeginEdit();

            //Tools.SelectCellByIndex(ReplaceListDGC, ReplaceListDGC.Items.Count - 1, 2);
            //DataGridCell cell = GetCell(rowIndex, colIndex);
            //cell.Focus;
            //ReplaceListDGC.UpdateLayout();
            //dataGridView.ScrollIntoView(DataGrid1.Items[itemIndex]);

        }

        private void LoadGroceriesBTN_Click(object sender, RoutedEventArgs e)
        {
            
            groceryList.LoadFromFile(); 
            GroceryListBOX.ItemsSource = groceryList;
            //GroceryListBOX.Items.Refresh();
        }

        private void JSON_Ingest_BTN_Click(object sender, RoutedEventArgs e)
        {
            groceryList.IngestFromClipboard("JSON");
        }
    }







}
