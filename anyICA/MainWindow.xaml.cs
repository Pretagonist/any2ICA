using System.Windows;
using System.Windows.Input;
using WindowsInput.Native;
using WindowsInput;

namespace anyICA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GroceryList groceryList = new GroceryList();
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

            //advance selection
            //TODO strangely never goes out of bounds at all. Investigate.
            GroceryListBOX.SelectedIndex++;
            if (GroceryListBOX.SelectedIndex >= GroceryListBOX.Items.Count) GroceryListBOX.SelectedIndex = 0;
        }

        private void Ingest_BTN_Click(object sender, RoutedEventArgs e)
        {
            groceryList.IngestFromClipboard();           
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

        }

        private void LoadGroceriesBTN_Click(object sender, RoutedEventArgs e)
        {
            
            groceryList.LoadFromFile(); 
            GroceryListBOX.ItemsSource = groceryList;
            //GroceryListBOX.Items.Refresh();
        }


    }







}
