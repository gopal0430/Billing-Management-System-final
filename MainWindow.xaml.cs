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
using GDS_Test_data.Models;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using GDS_Test_data.Models;
using GDS_Test_data.Helpers;
using System.Data;
using System.Data.SqlClient;


namespace MyWpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{


    public static class DbConfig
    {
        public static string ConnectionString =>
            "Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Uid=SA;Pwd=;";
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F1) // Check if F1 key is pressed
        {
            var result = MessageBox.Show("Do you want to save the data?", "Save", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                SaveSalesToDatabase(); // Call your save logic here
            }
        }

        if (e.Key == Key.Escape)
        {
            FocusFirstCellInGrid();
            e.Handled = true;
        }
        if (e.Key == Key.F8)
        {
            TxtBox_Clear();
            ProductTextBox.Focus();
            e.Handled = true;
        }
    }


    private void FocusFirstCellInGrid()
    {
        if (ProductsDataGrid.Items.Count > 0)
        {
            ProductsDataGrid.SelectedItem = ProductsDataGrid.Items[0];
            ProductsDataGrid.CurrentCell = new DataGridCellInfo(ProductsDataGrid.Items[0], ProductsDataGrid.Columns[0]);

            ProductsDataGrid.ScrollIntoView(ProductsDataGrid.Items[0], ProductsDataGrid.Columns[0]);

            if (ProductsDataGrid.ItemContainerGenerator.ContainerFromIndex(0) is DataGridRow row)
            {
                if (ProductsDataGrid.Columns[0].GetCellContent(row)?.Parent is DataGridCell cell)
                {
                    cell.Focus();
                    Keyboard.Focus(cell);
                }
            }
        }
    }


    private void SaveData()
    {
        // Add your save logic here
        MessageBox.Show("Data saved successfully!");
    }


    public ObservableCollection<Product> Products { get; set; }
    // public ObservableCollection<Product.ProductList> Products { get; set; }
    // public ObservableCollection<SelectedProduct> LoadedProducts { get; set; } = new ObservableCollection<SelectedProduct>();
    // public ObservableCollection<SelectedProduct> LoadedProducts { get; set; }

    public ObservableCollection<SelectedProduct> LoadedProducts { get; set; } = new ObservableCollection<SelectedProduct>();
    public string UnitName = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this; // if using code-behind
                                 // this.PreviewKeyDown += Window_KeyDown; // Attach the event handler
        PName.FontFamily = new FontFamily("SunTommy y Tamil");
        Products = new ObservableCollection<Product>();
        ProductsDataList.ItemsSource = Products;
        // Hook key navigation for suggestion list
        ProductsDataList.PreviewKeyDown += ProductsDataList_PreviewKeyDown;
        // ProductsDataGrid.PreviewKeyDown += ProductsDataGrid_PreviewKeyDown;
        ProductsDataGrid.PreviewKeyDown += ProductsDataGrid_PreviewKeyDown;
        ProductTextBox.PreviewKeyDown += ProductTextBox_PreviewKeyDown;



        // // Hook key navigation for DataGrid
        // ProductsDataGrid.KeyDown += ProductsDataGrid_KeyDown;


    }

    private void TxtBox_Clear()
    {
        ProductTextBox.Clear();
        ProductTextBox.FontFamily = new FontFamily("Segoe UI");
        RateTextBox.Clear();
        QuantityTextBox.Clear();
        // masalaComboBox.Text = "";
        masalaComboBox.SelectedIndex = -1;
        GrandTotalTextBlock.Text = "";
    }

    private void Clear_Fields()
    {
        RateComboBox.Text = "";
        UnitName = string.Empty;

    }

    private void ProductsDataList_Loaded(object sender, RoutedEventArgs e)
    {
        if (ProductsDataList.Items.Count > 0) // Ensure there are rows in the DataGrid
        {
            // ProductsDataList.SelectedIndex = 0; // Select the first row
            // var firstRow = ProductsDataList.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
            // firstRow?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)); // Move focus to the first row

            ProductsDataList.SelectedIndex = 0;                         // select first row
            ProductsDataList.ScrollIntoView(ProductsDataList.Items[0]); // scroll into view
            var firstRow = ProductsDataList.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
            if (firstRow != null)
            {
                firstRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

        }
        if (ProductsDataList.Items.Count == 0) // Ensure there are rows in the DataGrid
        {

            ProductsDataList.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

    }
    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ProductsDataGrid.ItemsSource = null; // Clears the data grid
        LoadedProducts.Clear();
        TxtBox_Clear();
    }

    public static void ShowError(Exception ex)
    {
        MessageBox.Show($"❌Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    private SelectedProduct _selectedRowToUpdate;

    private void ProductsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
            {
                // if (ProductsDataGrid.SelectedItem !=null)
                if (ProductsDataGrid.SelectedItem is SelectedProduct selected)
                {

                    ProductTextBox.Text = selected.PName;
                    RateTextBox.Text = selected.Rate2.ToString("0.00");
                    QuantityTextBox.Text = selected.Quantity.ToString();
                    _selectedRowToUpdate = selected;

                    ProductTextBox.FontFamily = new FontFamily("SunTommy y Tamil");
                    // Optional: move focus to Quantity or another control
                    QuantityTextBox.Focus();
                    QuantityTextBox.Select(QuantityTextBox.Text.Length, 0); //To move the cursor to the end of the text
                    e.Handled = true; // To prevent the default DataGrid behavior
                                      // Ensure control is available
                    if (ProductsDataList != null)
                        ProductsDataList.Visibility = Visibility.Collapsed; // Hide when text is cleared
                }




            }
            if (e.Key == Key.Delete)
            {
                if (ProductsDataGrid.SelectedItem is SelectedProduct selected1)

                {
                    LoadedProducts.Remove(selected1); // ObservableCollection auto-updates UI
                    UpdateGrandTotal();
                }
            }




        }
        catch (Exception ex)
        {
            ShowError(ex);
        }




    }


    private void masalaComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBox comboBox = sender as ComboBox;
        double maxWidth = 0;

        foreach (var item in comboBox.Items)
        {
            ComboBoxItem cbi = comboBox.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
            if (cbi != null)
            {
                cbi.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                maxWidth = Math.Max(maxWidth, cbi.DesiredSize.Width);
            }
        }

        // Apply padding/margin adjustment
        comboBox.Width = maxWidth + 25;
    }






    private void ProductsDataGrid_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
            {
                var selectedItem = ProductsDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    dynamic item = selectedItem;

                    ProductTextBox.Text = item.PName != null ? item.PName.ToString() : string.Empty;
                    RateTextBox.Text = item.Rate2 != null ? item.Rate2.ToString() : string.Empty;
                    QuantityTextBox.Text = item.Quantity != null ? item.Quantity.ToString() : string.Empty;


                    ProductTextBox.FontFamily = new FontFamily("SunTommy y Tamil");
                    // Optional: move focus to Quantity or another control
                    QuantityTextBox.Focus();
                }

                e.Handled = true; // To prevent the default DataGrid behavior
            }
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }



    }

    private void ProductsDataGrid_MouseDoubleClick(object sender, KeyEventArgs e)
    {
        try
        {

            if (e.Key == Key.Enter)
            {
                if (ProductsDataGrid.SelectedItem is SelectedProduct selected)
                {
                    ProductTextBox.Text = selected.PName;
                    RateTextBox.Text = selected.Rate2.ToString("0.00");
                    QuantityTextBox.Text = selected.Quantity.ToString();
                    _selectedRowToUpdate = selected;

                    ProductTextBox.Focus();
                    e.Handled = true;
                }
            }
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }



    }

    private void ProductTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (ProductsDataList.Visibility == Visibility.Visible &&
            (e.Key == Key.Up || e.Key == Key.Down))
            {
                ProductsDataList.Focus();
                e.Handled = true;

                if (Products.Any())
                {
                    ProductsDataList.SelectedIndex = 0;
                    ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

   


    private void ProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            string searchText = ProductTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                Products.Clear();                              // Clear list
                ProductsDataList.Visibility = Visibility.Collapsed; // Hide grid
                return;
            }

            // Load filtered product data
            LoadProductData(searchText);

            if (Products.Any())
            {
                ProductsDataList.Visibility = Visibility.Visible;
                ProductsDataList.SelectedIndex = 0;
                ProductsDataList.ScrollIntoView(ProductsDataList.Items[0]);

                var firstRow = ProductsDataList.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
                if (firstRow != null)
                {
                    firstRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
            else
            {
                ProductsDataList.Visibility = Visibility.Collapsed;
            }


        }

        catch (Exception ex)
        {
            ShowError(ex);
        }
    }


    private Product _currentProduct; // <<--- ADD THIS LINE

    
    private void ApplySelectedRateFromComboBox()
    {
        try
        {
            if (RateComboBox.SelectedItem != null && _currentSelectedProduct != null)
            {
                string selectedValue = RateComboBox.SelectedItem.ToString();
                decimal rateToSet = 0;

                // Match based on selected label
                if (selectedValue == _currentSelectedProduct.Caption2) // Rate1
                {
                    if (decimal.TryParse(_currentSelectedProduct.Rate1.ToString(), out decimal caption2Rate))
                        rateToSet = caption2Rate;
                }
                else if (selectedValue == _currentSelectedProduct.Caption1) // CaseRate
                {
                    if (decimal.TryParse(_currentSelectedProduct.CaseRate.ToString(), out decimal caption1Rate))
                        rateToSet = caption1Rate;
                }
                else if (selectedValue == _currentSelectedProduct.Caption3) // Rate2
                {
                    if (decimal.TryParse(_currentSelectedProduct.Rate2.ToString(), out decimal caption3Rate))
                        rateToSet = caption3Rate;
                }

                // Set the RateTextBox value
                RateTextBox.Text = rateToSet.ToString("0.00");

                // Update the selected row if needed
                if (_selectedRowToUpdate != null)
                {
                    _selectedRowToUpdate.Rate2 = rateToSet;
                }

                RateTextBox.Visibility = Visibility.Visible;
                RateComboBox.Visibility = Visibility.Collapsed;
            }

        }

        catch (Exception ex)
        {
            ShowError(ex);
        }
    }





    public void LoadProductData(string productSearchText)
    {
        try
        {
            string connectionString = "Driver={SQL Server};Server=SERVER;Database=rs_gst_27;Trusted_Connection=Yes;";
            // string connectionString = "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=True;";
            string query = @"
                 SELECT TOP 50 pname, Rate2, mrp , CAPTION1,caserate,CAPTION2,rate1,CAPTION3,pcode,category,
                display as tax
                FROM Product
                WHERE active = '1.0' AND pcode LIKE ? 
                ORDER BY pcode ASC";

            OdbcParameter[] parameters = new OdbcParameter[]
            {
                new OdbcParameter("@pcode", productSearchText + "%")
            };

            var productsFromDb = DatabaseHelper.GetEntitiesFromDatabase<Product>(DbConfig.ConnectionString, query, parameters);

            // Update the ObservableCollection
            Products.Clear();
            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }

        }
        catch (Exception ex)
        {
            ShowError(ex);
        }






    }



    private void QtyTxtBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {

            if (e.Key == Key.Enter)
            {
                decimal qty = 0;
                string qtyInput = QuantityTextBox.Text.Trim();

                // 🔹 Handle quantity input with '+' sign (e.g., "1+0.5")
                if (qtyInput.Contains("+"))
                {
                    var parts = qtyInput.Split('+');
                    foreach (var part in parts)
                    {
                        if (decimal.TryParse(part.Trim(), out decimal val))
                        {
                            qty += val;
                        }
                        else
                        {
                            qty = 0;
                            break;
                        }
                    }
                }
                else
                {
                    decimal.TryParse(qtyInput, out qty);
                }

                // 🔹 Validate fields
                if (decimal.TryParse(RateTextBox.Text, out decimal rate) &&
                    qty > 0 &&
                    !string.IsNullOrWhiteSpace(ProductTextBox.Text))
                {
                    string pname = ProductTextBox.Text.Trim();

                    // 🔹 Determine Unit from UnitName and RateComboBox
                    string selectedUnit = RateComboBox.SelectedItem?.ToString()?.Trim() ?? "";
                    string unit = UnitName;

                    if (!string.IsNullOrWhiteSpace(selectedUnit))
                    {
                        if (!string.IsNullOrWhiteSpace(UnitName) && UnitName.Equals(selectedUnit, StringComparison.OrdinalIgnoreCase))
                        {
                            unit = UnitName;
                        }
                        else
                        {
                            unit = selectedUnit;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(UnitName))
                    {
                        unit = UnitName;
                    }

                    if (_selectedRowToUpdate != null)
                    {
                        // 🔹 Update existing selected row
                        _selectedRowToUpdate.PName = pname;
                        _selectedRowToUpdate.Rate2 = rate;
                        _selectedRowToUpdate.Quantity = qty;
                        _selectedRowToUpdate.Unit = unit;
                        _selectedRowToUpdate = null;
                    }
                    else
                    {
                        if (LoadedProducts == null)
                            LoadedProducts = new ObservableCollection<SelectedProduct>();

                        if (SeparateRowsCheckBox.IsChecked == true && qtyInput.Contains("+"))
                        {
                            // 🔹 Add separate rows for quantities split by '+'
                            var quantities = qtyInput.Split('+');
                            foreach (var q in quantities)
                            {
                                if (decimal.TryParse(q.Trim(), out decimal singleQty))
                                {
                                    var product = new SelectedProduct
                                    {
                                        PName = pname,
                                        Rate2 = rate,
                                        Quantity = singleQty,
                                        Unit = unit
                                    };
                                    LoadedProducts.Add(product);
                                }
                            }
                        }
                        else
                        {
                            // 🔹 Find product with same name and same unit
                            var sameProduct = LoadedProducts.FirstOrDefault(p =>
                                p.PName.Equals(pname, StringComparison.OrdinalIgnoreCase) &&
                                p.Unit.Equals(unit, StringComparison.OrdinalIgnoreCase));

                            if (sameProduct != null)
                            {
                                sameProduct.Quantity += qty;
                                sameProduct.Rate2 = rate;
                            }
                            else
                            {
                                // 🔹 No exact match found, add new row
                                var newProduct = new SelectedProduct
                                {
                                    PName = pname,
                                    Rate2 = rate,
                                    Quantity = qty,
                                    Unit = unit
                                };
                                LoadedProducts.Add(newProduct);
                            }
                        }
                    }

                    // 🔹 Refresh UI and totals

                    ProductsDataGrid.ItemsSource = LoadedProducts;
                    UpdateGrandTotal();
                    ProductsDataGrid.Items.Refresh();

                    // 🔹 Clear input fields
                    Clear_Fields();
                    ProductTextBox.Clear();
                    RateTextBox.Clear();
                    QuantityTextBox.Clear();
                    ProductTextBox.Focus();
                    ProductTextBox.FontFamily = new FontFamily("Segoe UI");
                }
            }

        }


        catch (Exception ex)
        {
            ShowError(ex);
        }

    }

    private Product _currentSelectedProduct;

   



    private void ProductsDataList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (Products.Count == 0) return;

            int currentIndex = ProductsDataList.SelectedIndex;

            if (e.Key == Key.Down)
            {
                if (currentIndex < Products.Count - 1)
                {
                    ProductsDataList.SelectedIndex = currentIndex + 1;
                    ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (currentIndex > 0)
                {
                    ProductsDataList.SelectedIndex = currentIndex - 1;
                    ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && ProductsDataList.SelectedItem is Product productFromList)
            {



                ProductTextBox.FontFamily = new FontFamily("SunTommy y Tamil");
                ProductTextBox.Text = productFromList.pname;

                if (!string.IsNullOrWhiteSpace(productFromList.Caption1) &&
                    !string.IsNullOrWhiteSpace(productFromList.Caption2))
                {
                    RateTextBox.Visibility = Visibility.Collapsed;
                    RateComboBox.Visibility = Visibility.Visible;



                    _currentSelectedProduct = productFromList;



                    if (productFromList != null)
                    {
                        // Check if Caption1, Caption2, Caption3 have different values
                        bool hasDifferentCaptions =
                            !string.IsNullOrWhiteSpace(productFromList.Caption1) &&
                            !string.IsNullOrWhiteSpace(productFromList.Caption2) &&
                            !string.IsNullOrWhiteSpace(productFromList.Caption3) &&
                            (!productFromList.Caption1.Equals(productFromList.Caption2, StringComparison.OrdinalIgnoreCase) ||
                             !productFromList.Caption1.Equals(productFromList.Caption3, StringComparison.OrdinalIgnoreCase) ||
                             !productFromList.Caption2.Equals(productFromList.Caption3, StringComparison.OrdinalIgnoreCase));

                        if (hasDifferentCaptions)
                        {
                            // Show ComboBox
                            RateTextBox.Visibility = Visibility.Collapsed;
                            RateComboBox.Visibility = Visibility.Visible;

                            var rateOptions = new List<string>();

                            if (!string.IsNullOrWhiteSpace(productFromList.Caption2)) // Rate1 = Caption2
                            {
                                if (!rateOptions.Contains(productFromList.Caption2))
                                {
                                    rateOptions.Add(productFromList.Caption2);
                                    UnitName = productFromList.Caption2;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(productFromList.Caption1)) // CaseRate = Caption1
                            {
                                if (!rateOptions.Contains(productFromList.Caption1))
                                {
                                    rateOptions.Add(productFromList.Caption1);
                                    UnitName = productFromList.Caption1;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(productFromList.Caption3)) // Rate2 = Caption3
                            {
                                if (!rateOptions.Contains(productFromList.Caption3))
                                {
                                    rateOptions.Add(productFromList.Caption3);
                                    UnitName = productFromList.Caption3;

                                }
                            }

                            RateComboBox.ItemsSource = rateOptions;
                            RateComboBox.SelectedIndex = 0;
                            RateComboBox.FontFamily = new FontFamily("SunTommy y Tamil");
                            RateComboBox.Focus();
                            //  e.Handled = false;
                        }
                        else
                        {
                            // Show TextBox
                            RateTextBox.Visibility = Visibility.Visible;
                            RateComboBox.Visibility = Visibility.Collapsed;
                            UnitName = productFromList.Caption3;
                            // Default to Rate2
                            RateTextBox.Text = productFromList.Rate2?.ToString("0.00") ?? "0.00";

                            QuantityTextBox.Text = "1";
                            QuantityTextBox.Focus();
                            QuantityTextBox.SelectAll();

                        }
                    }





                }
                else
                {
                    RateTextBox.Visibility = Visibility.Visible;
                    RateComboBox.Visibility = Visibility.Collapsed;
                    RateTextBox.Text = productFromList.Rate2?.ToString("0.00") ?? "0.00";
                }

                

                RateComboBox.Focus();
                ProductsDataList.Visibility = Visibility.Collapsed;
                e.Handled = true;




            }
            else
            {
                // Redirect all other keys to ProductTextBox
                ProductTextBox.Focus();

            }
        }

        catch (Exception ex)
        {
            ShowError(ex);
        }

    }










    private decimal PerformCalculation(decimal rateText, int quantityText)
    {
        // Convert inputs to appropriate types
        decimal parsedRate = Convert.ToDecimal(rateText);
        int parsedQuantity = Convert.ToInt32(quantityText);

        // Call the calculation method
        return CalculateTotal(parsedRate, parsedQuantity);

    }

    // Separate method for calculation
    private decimal CalculateTotal(decimal rate, int quantity)
    {
        // Perform the calculation and round off to the nearest whole number
        return Math.Round(rate * quantity);
    }



    private void SortProducts()
    {
        var sorted = LoadedProducts.OrderBy(p => p.Category == "Discount" || p.Category == "RICE" ? 1 : 0)
                             .ThenBy(p => p.PName)
                             .ToList();
        LoadedProducts.Clear();
        foreach (var item in sorted)
            LoadedProducts.Add(item);
    }


    



    private void UpdateGrandTotal()
    {
        try
        {
            decimal grandTotal = 0;

            foreach (var item in ProductsDataGrid.Items)
            {
                if (item is SelectedProduct product)
                {
                    decimal rate = product.Rate2;
                    decimal qty = product.Quantity;
                    grandTotal += rate * qty;
                }
            }

            GrandTotalTextBlock.Text = grandTotal.ToString("C", new System.Globalization.CultureInfo("en-IN"));

        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }


    private void ProductsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.InvokeAsync(UpdateGrandTotal); // Delay to ensure value is committed
    }



    private void masalaComboBox_KeyDown(object sender, KeyEventArgs e)
    {

        try
        {

            if (e.Key == Key.Enter)
            {
                // var selectedItem = (masalaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                // MessageBox.Show($"You selected: {selectedItem}");
                var selectedItem = masalaComboBox.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    string displayText = selectedItem.Content.ToString();   // Tamil text
                    string selectedCategory = selectedItem.Tag.ToString();             // Actual value
                                                                                       // MessageBox.Show($"Display Text: {displayText}\nValue: {Mvalue}");
                                                                                       //  string selectedCategory = "Kari_Masala";
                    if (!string.IsNullOrEmpty(selectedCategory))
                    {
                        // Prompt user for multiplier using a simple input dialog
                        string input = Microsoft.VisualBasic.Interaction.InputBox(
                            "Enter quantity multiplier (e.g., 2 for double, 3 for triple):",
                            "Quantity Multiplier",
                            "1");

                        // Try to parse the input as an integer multiplier
                        if (decimal.TryParse(input, out decimal multiplier) && multiplier > 0)
                        {
                            List<SelectedProduct> products = GetProductsByCategory(selectedCategory, multiplier);
                            ProductsDataGrid.ItemsSource = null;
                            ProductsDataGrid.Items.Clear();
                            foreach (var p in products)
                            {
                                LoadedProducts.Add(p);
                            }
                            ProductsDataGrid.ItemsSource = LoadedProducts;
                            PNameNative.FontFamily = new FontFamily("SunTommy y Tamil");
                            UpdateGrandTotal();
                        }
                        else
                        {
                            MessageBox.Show("Invalid multiplier. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }




        catch (Exception ex)
        {
            ShowError(ex);
        }
    }




    private bool _isRateComboInitialized = false;
    private void RateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isRateComboInitialized)
        {
            _isRateComboInitialized = true;
            return; // Skip this one-time auto-triggered event
        }
    }

    private void RateComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && RateComboBox.SelectedItem != null)
        {
            HandleRateSelection(moveFocusToQuantity: true);
        }
    }



    private void HandleRateSelection(bool moveFocusToQuantity = false)
    {
        if (RateComboBox.SelectedItem != null && _currentSelectedProduct != null)
        {
            string selectedValue = RateComboBox.SelectedItem.ToString();
            decimal rateToSet = 0;

            // Match based on selected label
            if (selectedValue == _currentSelectedProduct.Caption2) // Rate1
            {
                if (decimal.TryParse(_currentSelectedProduct.Rate1.ToString(), out decimal caption2Rate))
                    rateToSet = caption2Rate;
            }
            else if (selectedValue == _currentSelectedProduct.Caption1) // CaseRate
            {
                if (decimal.TryParse(_currentSelectedProduct.CaseRate.ToString(), out decimal caption1Rate))
                    rateToSet = caption1Rate;
            }
            else if (selectedValue == _currentSelectedProduct.Caption3) // Rate2
            {
                if (decimal.TryParse(_currentSelectedProduct.Rate2.ToString(), out decimal caption3Rate))
                    rateToSet = caption3Rate;
            }
            else
            {
                // If not matching label, try direct parse (for key entry)
                decimal.TryParse(selectedValue, out rateToSet);
            }

            // Set the RateTextBox value
            RateTextBox.Text = rateToSet.ToString("0.00");

            // Update model if needed
            if (_selectedRowToUpdate != null)
            {
                _selectedRowToUpdate.Rate2 = rateToSet;
            }

            // UI transitions
            RateTextBox.Visibility = Visibility.Visible;
            RateComboBox.Visibility = Visibility.Collapsed;

            if (moveFocusToQuantity)
                QuantityTextBox.Focus();
        }
    }




    public static double Calculate_Price1(double value, double percentage)
    {
        double result = value - (value * (percentage / 100));
        return Math.Round(result, 2); // Rounds to 2 decimal places
    }
    public static double Calculate_Price(double input, double percent)
    {
        // return Math.Ceiling(input / (1 + percent / 100));
        return (input / (1 + percent / 100));

    }

    private void SaveSalesToDatabase()
    {
        string connectionString = "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=Yes;";
        int newBillNo = 1; // default bill no

        try
        {
            using (OdbcConnection connection = new OdbcConnection(DbConfig.ConnectionString))
            {
                connection.Open();

                // Get new bill number
                string getBillQuery = "SELECT MAX(bno)FROM [dbo].[sales] where bno!='null' and date =CONVERT(DATETIME, CONVERT(VARCHAR(10), GETDATE(), 120))";
                using (OdbcCommand getBillCmd = new OdbcCommand(getBillQuery, connection))
                {
                    object result = getBillCmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        newBillNo = Convert.ToInt32(result) + 1;
                    }
                }

                foreach (var item in LoadedProducts)
                {
                    string insertQuery = @"INSERT INTO [rs_gst_27].[dbo].[sales] 
([date], [bno], [party], [product], [rate], [qty], [category], [mrp], [sno], [costprice], [add1], [add2], [subs1], [subs2], [billvalue], [batchno], [expdate], [type], [payment], [pperbox], [caption], [note], [city], [codeno], [tax], [free], [st1], [tin], [bbookno], [billbook], [salesman], [commcode], [tonage], [less1], [less2], [less3], [less4], [less5], [useable], [less6], [add1caption], [add2caption], [less1caption], [less2caption], [add22], [less22], [category2], [time1], [rate1], [rate2], [rate3], [godown], [wsrs], [username], [dmyname], [dmybno], [company], [dmydate], [deleveryproduct], [address1], [address2], [phoneno], [podate], [pono], [reference], [despatch], [destination], [terms], [cess], [additionalcess], [bnochar], [chkrate], [dele_add_one], [dele_add_two], [dele_add_three], [dele_add_four], [dele_add_five], [dele_add_six], [packqty], [bonuspoints], [sizename], [sizevalue], [bnochar_end], [colour], [stcs], [sal_tonage], [point_or_not], [sal_barcode], [opincode], [gov_caption], [mailid], [distance], [sales_einvoice_irn], [sales_einvoice_ackno], [sales_einvoice_ackdate], [sales_einvoice_authtoken])
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";


                    using (OdbcCommand insertCmd = new OdbcCommand(insertQuery, connection))
                    {
                        Decimal rateResult = (Decimal)Calculate_Price(Convert.ToDouble(item.Rate2), Convert.ToDouble(item.tax));
                        insertCmd.Parameters.AddWithValue("", DateTime.Now); // [date]
                        insertCmd.Parameters.AddWithValue("", newBillNo);    // [bno]
                        // insertCmd.Parameters.AddWithValue("", 93603);    // [bno]

                        insertCmd.Parameters.AddWithValue("", "COUNTER SALES");       // [party]
                        insertCmd.Parameters.AddWithValue("", item.PName);   // [product]
                        insertCmd.Parameters.AddWithValue("", rateResult);   // [rate]
                        insertCmd.Parameters.AddWithValue("", item.Quantity);// [qty]
                        insertCmd.Parameters.AddWithValue("", "M PRODUCT");  // [category]
                        insertCmd.Parameters.AddWithValue("", 0);            // [mrp]
                        insertCmd.Parameters.AddWithValue("", 1343);         // [sno]
                        insertCmd.Parameters.AddWithValue("", 146.02);       // [costprice]
                        insertCmd.Parameters.AddWithValue("", 160.91);       // [add1]
                        insertCmd.Parameters.AddWithValue("", 0);            // [add2]
                        insertCmd.Parameters.AddWithValue("", 0);            // [subs1]
                        insertCmd.Parameters.AddWithValue("", 0);            // [subs2]
                        insertCmd.Parameters.AddWithValue("", item.Quantity * item.Rate2); // [billvalue]
                        insertCmd.Parameters.AddWithValue("", item.PCode);          // [batchno]
                        insertCmd.Parameters.AddWithValue("", DateTime.Now); // [expdate]
                        insertCmd.Parameters.AddWithValue("", "SALES");      // [type]
                        insertCmd.Parameters.AddWithValue("", item.Quantity * item.Rate2); // [payment]
                        insertCmd.Parameters.AddWithValue("", 1);            // [pperbox]
                        insertCmd.Parameters.AddWithValue("", item.Unit);       // [caption]
                        insertCmd.Parameters.AddWithValue("", ".");          // [note]
                        insertCmd.Parameters.AddWithValue("", "CASH AREA");  // [city]
                        insertCmd.Parameters.AddWithValue("", "");           // [codeno]
                        insertCmd.Parameters.AddWithValue("", item.tax);          // [tax]
                        insertCmd.Parameters.AddWithValue("", 0);            // [free]
                        insertCmd.Parameters.AddWithValue("", ".");          // [st1]
                        insertCmd.Parameters.AddWithValue("", ".");          // [tin]
                        insertCmd.Parameters.AddWithValue("", 1);            // [bbookno]
                        insertCmd.Parameters.AddWithValue("", "GST JUNE 24-25"); // [billbook]
                        insertCmd.Parameters.AddWithValue("", "DIRCET");     // [salesman]
                        insertCmd.Parameters.AddWithValue("", "361");        // [commcode]
                        insertCmd.Parameters.AddWithValue("", "0");          // [tonage]
                        insertCmd.Parameters.AddWithValue("", 0);            // [less1]
                        insertCmd.Parameters.AddWithValue("", 0);            // [less2]
                        insertCmd.Parameters.AddWithValue("", 0);            // [less3]
                        insertCmd.Parameters.AddWithValue("", 0);            // [less4]
                        insertCmd.Parameters.AddWithValue("", 0);            // [less5]
                        insertCmd.Parameters.AddWithValue("", 1);            // [useable]
                        insertCmd.Parameters.AddWithValue("", 10);           // [less6]
                        insertCmd.Parameters.AddWithValue("", "Transport Charges"); // [add1caption]
                        insertCmd.Parameters.AddWithValue("", "Wages");      // [add2caption]
                        insertCmd.Parameters.AddWithValue("", "Spl Pongan Offer"); // [less1caption]
                        insertCmd.Parameters.AddWithValue("", "Disp(-)");    // [less2caption]
                        insertCmd.Parameters.AddWithValue("", 0);            // [add22]
                        insertCmd.Parameters.AddWithValue("", 0);            // [less22]
                        insertCmd.Parameters.AddWithValue("", item.Category);        // [category2]
                        insertCmd.Parameters.AddWithValue("", DateTime.Today); // [time1]
                        insertCmd.Parameters.AddWithValue("", item.Rate2);            // [rate1]
                        insertCmd.Parameters.AddWithValue("", 0);            // [rate2]
                        insertCmd.Parameters.AddWithValue("", 0);            // [rate3]
                        insertCmd.Parameters.AddWithValue("", "MAINGODWON"); // [godown]
                        insertCmd.Parameters.AddWithValue("", 0);            // [wsrs]
                        insertCmd.Parameters.AddWithValue("", "admin");      // [username]
                        insertCmd.Parameters.AddWithValue("", "");           // [dmyname]
                        insertCmd.Parameters.AddWithValue("", 0);            // [dmybno]
                        insertCmd.Parameters.AddWithValue("", "M PRODUCT");  // [company]
                        insertCmd.Parameters.AddWithValue("", "");           // [dmydate]
                        insertCmd.Parameters.AddWithValue("", 0);            // [deleveryproduct]
                        insertCmd.Parameters.AddWithValue("", ".");          // [address1]
                        insertCmd.Parameters.AddWithValue("", ".");          // [address2]
                        insertCmd.Parameters.AddWithValue("", ".");          // [phoneno]
                        insertCmd.Parameters.AddWithValue("", "");           // [podate]
                        insertCmd.Parameters.AddWithValue("", "");           // [pono]
                        insertCmd.Parameters.AddWithValue("", "");           // [reference]
                        insertCmd.Parameters.AddWithValue("", "");           // [despatch]
                        insertCmd.Parameters.AddWithValue("", "");           // [destination]
                        insertCmd.Parameters.AddWithValue("", "");           // [terms]
                        insertCmd.Parameters.AddWithValue("", 0);            // [cess]
                        insertCmd.Parameters.AddWithValue("", 0);            // [additionalcess]
                        insertCmd.Parameters.AddWithValue("", "1");          // [bnochar]
                        insertCmd.Parameters.AddWithValue("", 0);            // [chkrate]
                        insertCmd.Parameters.AddWithValue("", "");           // [dele_add_one]
                        insertCmd.Parameters.AddWithValue("", "");           // [dele_add_two]
                        insertCmd.Parameters.AddWithValue("", "");           // [dele_add_three]
                        insertCmd.Parameters.AddWithValue("", "");           // [dele_add_four]
                        insertCmd.Parameters.AddWithValue("", "");           // [dele_add_five]
                        insertCmd.Parameters.AddWithValue("", "");           // [dele_add_six]
                        insertCmd.Parameters.AddWithValue("", 0);            // [packqty]
                        insertCmd.Parameters.AddWithValue("", 0);            // [bonuspoints]
                        insertCmd.Parameters.AddWithValue("", "");           // [sizename]
                        insertCmd.Parameters.AddWithValue("", 1);            // [sizevalue]
                        insertCmd.Parameters.AddWithValue("", "");           // [bnochar_end]
                        insertCmd.Parameters.AddWithValue("", "");           // [colour]
                        insertCmd.Parameters.AddWithValue("", 0);            // [stcs]
                        insertCmd.Parameters.AddWithValue("", 0);            // [sal_tonage]
                        insertCmd.Parameters.AddWithValue("", 1);            // [point_or_not]
                        insertCmd.Parameters.AddWithValue("", ".");          // [sal_barcode]
                        insertCmd.Parameters.AddWithValue("", "0");          // [opincode]
                        insertCmd.Parameters.AddWithValue("", "");           // [gov_caption]
                        insertCmd.Parameters.AddWithValue("", "");           // [mailid]
                        insertCmd.Parameters.AddWithValue("", 0);            // [distance]
                        insertCmd.Parameters.AddWithValue("", ".");          // [sales_einvoice_irn]
                        insertCmd.Parameters.AddWithValue("", ".");          // [sales_einvoice_ackno]
                        insertCmd.Parameters.AddWithValue("", ".");          // [sales_einvoice_ackdate]
                        insertCmd.Parameters.AddWithValue("", ".");          // [sales_einvoice_authtoken]


                        // int qMarks = insertQuery.Count(c => c == '?');
                        // int paramsAdded = insertCmd.Parameters.Count;

                        // if (qMarks != paramsAdded)
                        // {
                        //     MessageBox.Show($"❌ Placeholder count: {qMarks} ≠ Parameters added: {paramsAdded}", "Mismatch Error");
                        // }
                        // else
                        // {
                        //     MessageBox.Show($"✅ Placeholder count matches parameters: {qMarks}", "OK");
                        // }

                        insertCmd.ExecuteNonQuery();
                    }


                }

                MessageBox.Show($"Sales saved successfully. Bill No: {newBillNo}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (OdbcException ex)
        {
            MessageBox.Show("ODBC Error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show("General Error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }







    public List<SelectedProduct> GetProductsByCategory(string category, decimal multiplier)
    {
        // string connectionString = "Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Trusted_Connection=Yes;";
        // string connectionString = "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=True;";
        var productList = new List<SelectedProduct>();
        string query = @"
        SELECT 
                p.pname, 
              p.Rate1, 
                p.Rate2, 
                pt.Category, 
                pt.QTY,
                pt.unit,
                p.pcode,
                p.display as tax
            FROM 
                Product p
            INNER JOIN 
                ProductTemplates pt ON p.Pcode = pt.ProductName
            WHERE 
                pt.Category = ?";

        // using (var conn = new OdbcConnection(connectionString))
        using (OdbcConnection conn = new OdbcConnection(DbConfig.ConnectionString))
        using (var cmd = new OdbcCommand(query, conn))
        {
            cmd.Parameters.Add(new OdbcParameter("", category)); // ODBC uses "?" as a placeholder, so parameter name is ignored
            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Multiply quantity by the entered multiplier
                    decimal baseQuantity = Convert.ToDecimal(reader["QTY"]);

                    // Determine which rate to use based on Unit
                    // string unit = reader["Unit"].ToString();
                    // decimal rate = unit == "kg"
                    //     ? Convert.ToDecimal(reader["Rate2"])
                    //     : Convert.ToDecimal(reader["Rate1"]);


                    string unit = reader["Unit"].ToString(); // normalize casing
                    decimal rate;

                    switch (unit)
                    {//gram
                        case "fpuhk;":
                        case "ml":
                            rate = Convert.ToDecimal(reader["Rate1"]);
                            if (rate == 0)
                            {
                                rate = Convert.ToDecimal(reader["Rate2"]);
                                rate /= 1000;
                            }
                            break;
                        //kilo
                        case "fpNyh":
                        case "liter":
                            rate = Convert.ToDecimal(reader["Rate2"]);

                            break;
                        //piece
                        case "gP];":
                        case "unit":
                            rate = Convert.ToDecimal(reader["Rate2"]);
                            break;

                        default:
                            rate = 0; // fallback rate
                            break;
                    }


                    productList.Add(new SelectedProduct
                    {
                        PName = reader["pname"].ToString(),
                        Quantity = baseQuantity * multiplier,
                        // Unit=reader["Unit"].ToString(),

                        // QTY = reader["QTY"] != DBNull.Value ? Convert.ToSingle(reader["QTY"]) : 0,
                        // Unit = reader["Unit"].ToString(),
                        // Rate2=Convert.ToDecimal(reader["Rate2"]),
                        Unit = unit,
                        Rate2 = rate,
                        Category = reader["Category"].ToString(),
                        PCode = reader["pcode"].ToString(),
                        tax = Convert.ToDecimal(reader["tax"])

                    });
                }
            }
        }

        return productList; // Ensures all code paths return a value
    }











}