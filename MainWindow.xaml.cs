using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
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
using MahApps.Metro.Controls.Dialogs;
using System.Text.RegularExpressions;




namespace MyWpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
{


    public static class DbConfig
    {
        public static string ConnectionString =>
            "Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Uid=SA;Pwd=;";
    }




    public async Task<string> ShowMetroInputBox()
    {
        var result = await this.ShowInputAsync(
            "Quantity Multiplier",
            "Enter quantity multiplier (e.g., 2 for double, 3 for triple):",
            new MetroDialogSettings
            {

                AnimateShow = true,
                AnimateHide = true
            });

        return result; // Can be null if user presses Cancel
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
            Grid_Clear();
        }

        if (e.Key == Key.Escape)
        {
            FocusLastCellInGrid();
            // FocusFirstCellInGrid();
            e.Handled = true;
        }
        if (e.Key == Key.F8)
        {
            TxtBox_Clear();
            ProductTextBox.Focus();
            e.Handled = true;
        }
        if (e.Key == Key.F9)
        {
            CustomerTextBox.Focus();
            CustomerTextBox.SelectAll(); // Optional: highlight the text
            e.Handled = true; // Prevent further propagation
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

    private void FocusLastCellInGrid()
    {
        int rowCount = ProductsDataGrid.Items.Count;
        int colCount = ProductsDataGrid.Columns.Count;

        if (rowCount > 0 && colCount > 0)
        {
            int lastRowIndex = rowCount - 1;
            int lastColIndex = colCount - 1;

            ProductsDataGrid.SelectedItem = ProductsDataGrid.Items[lastRowIndex];
            ProductsDataGrid.CurrentCell = new DataGridCellInfo(ProductsDataGrid.Items[lastRowIndex], ProductsDataGrid.Columns[lastColIndex]);

            ProductsDataGrid.ScrollIntoView(ProductsDataGrid.Items[lastRowIndex], ProductsDataGrid.Columns[lastColIndex]);

            if (ProductsDataGrid.ItemContainerGenerator.ContainerFromIndex(lastRowIndex) is DataGridRow row)
            {
                if (ProductsDataGrid.Columns[lastColIndex].GetCellContent(row)?.Parent is DataGridCell cell)
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
    public string SelectedPCode { get; set; } = "";



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

        ProductTextBox.Focus();

        // // Hook key navigation for DataGrid
        // ProductsDataGrid.KeyDown += ProductsDataGrid_KeyDown;

        BillNoTextBox.Text = getBillNo().ToString();
        DateTextBox.Text = DateTime.Today.ToString("dd/MM/yyyy");
        CustomerTextBox.Text = "COUNTER SALES";

    }

    private void TxtBox_Clear()
    {

        CustomerTextBox.Text = "COUNTER SALES";
        ProductTextBox.Clear();
        ProductTextBox.FontFamily = new FontFamily("Segoe UI");
        RateTextBox.Clear();
        QuantityTextBox.Clear();
        // masalaComboBox.Text = "";
        RateComboBox.SelectedIndex = -1;
        RateTextBox.Visibility = Visibility.Visible;
        RateComboBox.Visibility = Visibility.Collapsed;
        masalaComboBox.SelectedIndex = -1;
        GrandTotalTextBlock.Text = "";
        ProductCountTextBlock.Text = "";
        

    }

    private void Clear_Fields()
    {
        RateComboBox.Text = "";
        UnitName = string.Empty;
        SeparateRowsCheckBox.IsChecked = false;
        // GrandTotalTextBlock.Text = "";

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
        Grid_Clear();
    }

    private void Grid_Clear()
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
                    QuantityTextBox.SelectAll(); //To move the cursor to the end of the text
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
                    UpdateProductCount(); ;
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



    // private bool isTextChanging = false;

    // private void ProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
    // {
    //     if (isTextChanging) return; // Prevent re-entrancy

    //     try
    //     {
    //         isTextChanging = true;

    //         string searchText = ProductTextBox.Text?.Trim();

    //         if (string.IsNullOrEmpty(searchText))
    //         {
    //             // Clear the product list and hide the data grid
    //             Products.Clear();
    //             ProductsDataList.Visibility = Visibility.Collapsed;
    //             return;
    //         }

    //         // Load filtered product data
    //         LoadProductData(searchText);

    //         if (Products.Any())
    //         {
    //             // Show the data grid and select the first item
    //             ProductsDataList.Visibility = Visibility.Visible;
    //             ProductsDataList.SelectedIndex = 0;
    //             ProductsDataList.ScrollIntoView(ProductsDataList.Items[0]);

    //             // Move focus to the first row
    //             var firstRow = ProductsDataList.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
    //             if (firstRow != null)
    //             {
    //                 firstRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    //             }
    //         }
    //         else
    //         {
    //             // Hide the data grid if no products are found
    //             ProductsDataList.Visibility = Visibility.Collapsed;
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Handle any exceptions
    //         ShowError(ex);
    //     }
    //     finally
    //     {
    //         // Reset the flag to allow further text changes
    //         isTextChanging = false;
    //     }
    // }


    private bool isTextChanging = false;
    private bool isDataGridSelection = false;

    private bool _enterPressed = false;
    private void ProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {


        try
        {


            if (isTextChanging || isDataGridSelection) return; // Prevent re-entrancy and selection-triggered changes
            if (_enterPressed)
                return; // only respond to Enter key-triggered changes

            isTextChanging = true;

            string searchText = ProductTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                // Clear the product list and hide the data grid
                Products.Clear();
                ProductsDataList.Visibility = Visibility.Collapsed;
                return;
            }

            // Load filtered product data
            LoadProductData(searchText);

            if (Products.Any())
            {
                // Show the data grid and select the first item
                ProductsDataList.Visibility = Visibility.Visible;
                ProductsDataList.SelectedIndex = 0;
                ProductsDataList.ScrollIntoView(ProductsDataList.Items[0]);

                // Move focus to the first row
                var firstRow = ProductsDataList.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
                if (firstRow != null)
                {
                    firstRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
            else
            {
                // Hide the data grid if no products are found
                ProductsDataList.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            ShowError(ex);
        }
        finally
        {
            // Reset the flag to allow further text changes
            isTextChanging = false;
            _enterPressed = false; // reset the flag
        }

    }

    private void ProductsDataList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProductsDataList.SelectedItem != null)
        {
            try
            {
                isDataGridSelection = true;
                // Assuming you're binding to a Product object
                var selectedProduct = (Product)ProductsDataList.SelectedItem;
                ProductTextBox.Text = selectedProduct.pname;
            }
            finally
            {
                isDataGridSelection = false;
            }
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



    private void UpdateProductCount()
    {
        // int itemCount = ProductsDataGrid.Items
        //     .OfType<object>()
        //     .Count(item => ProductsDataGrid.ItemContainerGenerator.ContainerFromItem(item) != null);

        // ProductCountTextBlock.Text = $": {itemCount}";

        //   if (ProductsDataGrid.ItemsSource is IEnumerable<Product> products)
        // {
        //     int totalItems = products.Sum(p => p.NoOfItems); // Calculate total No Of Items
        //     ProductCountTextBlock.Text = $"No Of Items: {totalItems}";
        // }

        int itemCount = ProductsDataGrid.Items.Count;
        ProductCountTextBlock.Text = $": {itemCount}";

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

    public int getBillNo()
    {
        try
        {
            string getBillQuery = @"
            SELECT MAX(bno)
            FROM [dbo].[sales]
            WHERE bno IS NOT NULL 
              AND date = CONVERT(DATETIME, CONVERT(VARCHAR(10), GETDATE(), 120))";

            object result = DatabaseHelper.ExecuteScalar(getBillQuery, DbConfig.ConnectionString);

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result) + 1;
            }
            else
            {
                return 1; // Default starting bill number
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return -1; // Or any error-specific default value
        }
    }


    private void CustomerTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {

            if (e.Key == Key.Enter)
            {

                ProductTextBox.Focus();

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


                    // 🔹 Match product to get PCode 
                    // var matched = Products.FirstOrDefault(p => p.PName.Equals(pname, StringComparison.OrdinalIgnoreCase));
                    // SelectedPCode = matched?.PCode ?? "";

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
                        // _selectedRowToUpdate.PCode = SelectedPCode; // ✅ Add PCode
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
                                    Unit = unit,
                                    PCode = SelectedPCode
                                };
                                LoadedProducts.Add(newProduct);
                            }
                        }
                    }

                    // 🔹 Refresh UI and totals

                    ProductsDataGrid.ItemsSource = LoadedProducts;
                    UpdateGrandTotal();
                    UpdateProductCount(); ;
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
            if (e.Key == Key.Enter)
            {
                _enterPressed = true;
            }
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

                            if (RateTextBox.Text == "0.00")
                            {
                                RateTextBox.Focus();
                                RateTextBox.Focus();
                                RateTextBox.SelectAll();

                            }
                            else
                            {
                                QuantityTextBox.Text = "1";
                                QuantityTextBox.Focus();
                                QuantityTextBox.SelectAll();
                            }



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
                        AskQuantityMultiplier(selectedCategory);
                        // Prompt user for multiplier using a simple input dialog
                        // string input = Microsoft.VisualBasic.Interaction.InputBox(
                        //     "Enter quantity multiplier (e.g., 2 for double, 3 for triple):",
                        //     "Quantity Multiplier",
                        //     "1");

                        // // Try to parse the input as an integer multiplier
                        // if (decimal.TryParse(input, out decimal multiplier) && multiplier > 0)
                        // {
                        //     List<SelectedProduct> products = GetProductsByCategory(selectedCategory, multiplier);
                        //     ProductsDataGrid.ItemsSource = null;
                        //     ProductsDataGrid.Items.Clear();
                        //     foreach (var p in products)
                        //     {
                        //         LoadedProducts.Add(p);
                        //     }
                        //     ProductsDataGrid.ItemsSource = LoadedProducts;
                        //     PNameNative.FontFamily = new FontFamily("SunTommy y Tamil");
                        //     UpdateGrandTotal();
                        // }
                        // else
                        // {
                        //     MessageBox.Show("Invalid multiplier. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        // }

                    }
                }
            }
        }




        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async void AskQuantityMultiplier(string selectedCategory)
    {
        string input = await ShowMetroInputBox();

        if (string.IsNullOrWhiteSpace(input))
        {
            // User cancelled or left it empty
            MessageBox.Show("Multiplier input was cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

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
            UpdateProductCount(); ;
        }
        else
        {
            MessageBox.Show("Invalid multiplier. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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



    private string EnsureCustomerName()
    {
        if (string.IsNullOrWhiteSpace(CustomerTextBox.Text))
        {
            CustomerTextBox.Text = "Counter Sale";
        }
        else
        {
            CustomerTextBox.Text = CustomerTextBox.Text.Trim();
        }

        return CustomerTextBox.Text;
    }


    private async void SaveSalesToDatabase()
    {

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
            ([date], [bno], [party], [product], [rate], [qty], [category], [mrp], [sno], [costprice],
            [add1], [add2], [subs1], [subs2], [billvalue], [batchno], [expdate], [type], [payment], [pperbox],
            [caption], [note], [city], [codeno], [tax], [free], [st1], [tin], [bbookno], [billbook],
            [salesman], [commcode], [tonage], [less1], [less2], [less3], [less4], [less5], [useable], [less6],
            [add1caption], [add2caption], [less1caption], [less2caption], [add22], [less22], [category2], [time1],
            [rate1], [rate2], [rate3], [godown], [wsrs], [username], [dmyname], [dmybno], [company], [dmydate],
            [deleveryproduct], [address1], [address2], [phoneno], [podate], [pono], [reference], [despatch],
            [destination], [terms], [cess], [additionalcess], [bnochar], [chkrate], [dele_add_one], [dele_add_two],
            [dele_add_three], [dele_add_four], [dele_add_five], [dele_add_six], [packqty], [bonuspoints],
            [sizename], [sizevalue], [bnochar_end], [colour], [stcs], [sal_tonage], [point_or_not], [sal_barcode],
            [opincode], [gov_caption], [mailid], [distance], [sales_einvoice_irn], [sales_einvoice_ackno],
            [sales_einvoice_ackdate], [sales_einvoice_authtoken])
            VALUES
            (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";

                    using (OdbcCommand insertCmd = new OdbcCommand(insertQuery, connection))
                    {
                        decimal rateResult = (decimal)Calculate_Price(Convert.ToDouble(item.Rate2), Convert.ToDouble(item.tax));
                        object[] values = new object[]
                                    {
                        DateTime.Today,                  // [date]
                        newBillNo,                     // [bno]
                        EnsureCustomerName(),              // [party]
                        item.PName,                   // [product]
                        rateResult,                   // [rate]
                        item.Quantity,                // [qty]
                        "M PRODUCT",                  // [category]
                        0,                            // [mrp]
                        1343,                         // [sno]
                        146.02,                       // [costprice]

                        160.91,                       // [add1]
                        0,                            // [add2]
                        0,                            // [subs1]
                        0,                            // [subs2]
                        item.Quantity * item.Rate2,  // [billvalue]
                        item.PCode,                   // [batchno]
                        DateTime.Now,                 // [expdate]
                        "SALES",                      // [type]
                        item.Quantity * item.Rate2,  // [payment]
                        1,                            // [pperbox]

                        item.Unit,                    // [caption]
                        ".",                          // [note]
                        "CASH AREA",                  // [city]
                        "",                           // [codeno]
                        item.tax,                     // [tax]
                        0,                            // [free]
                        ".",                          // [st1]
                        ".",                          // [tin]
                        1,                            // [bbookno]
                        "GST JUNE 24-25",             // [billbook]

                        "DIRCET",                     // [salesman]
                        "361",                        // [commcode]
                        "0",                          // [tonage]
                        0,                            // [less1]
                        0,                            // [less2]
                        0,                            // [less3]
                        0,                            // [less4]
                        0,                            // [less5]
                        1,                            // [useable]
                        10,                           // [less6]

                        "Transport Charges",          // [add1caption]
                        "Wages",                      // [add2caption]
                        "Spl Pongan Offer",          // [less1caption]
                        "Disp(-)",                    // [less2caption]
                        0,                            // [add22]
                        0,                            // [less22]
                        item.Category,                // [category2]
                        DateTime.Today,               // [time1]
                        item.Rate2,                   // [rate1]
                        0,                            // [rate2]

                        0,                            // [rate3]
                        "MAINGODWON",                 // [godown]
                        0,                            // [wsrs]
                        "admin",                      // [username]
                        "",                           // [dmyname]
                        0,                            // [dmybno]
                        "M PRODUCT",                  // [company]
                        "",                           // [dmydate]
                        0,                            // [deleveryproduct]
                        ".",                          // [address1]

                        ".",                          // [address2]
                        ".",                          // [phoneno]
                        "",                           // [podate]
                        "",                           // [pono]
                        "",                           // [reference]
                        "",                           // [despatch]
                        "",                           // [destination]
                        "",                           // [terms]
                        0,                            // [cess]
                        0,                            // [additionalcess]

                        "1",                          // [bnochar]
                        0,                            // [chkrate]
                        "",                           // [dele_add_one]
                        "",                           // [dele_add_two]
                        "",                           // [dele_add_three]
                        "",                           // [dele_add_four]
                        "",                           // [dele_add_five]
                        "",                           // [dele_add_six]
                        0,                            // [packqty]
                        0,                            // [bonuspoints]

                        "",                           // [sizename]
                        1,                            // [sizevalue]
                        "",                           // [bnochar_end]
                        "",                           // [colour]
                        0,                            // [stcs]
                        0,                            // [sal_tonage]
                        1,                            // [point_or_not]
                        ".",                          // [sal_barcode]
                        "0",                          // [opincode]
                        "",                           // [gov_caption]

                        "",                           // [mailid]
                        0,                            // [distance]
                        ".",                          // [sales_einvoice_irn]
                        ".",                          // [sales_einvoice_ackno]
                        ".",                          // [sales_einvoice_ackdate]
                        "."                           // [sales_einvoice_authtoken]
                                    };


                        BindParameters(insertCmd, values);
                        insertCmd.ExecuteNonQuery();

                    }


                }

                // MessageBox.Show($"Sales saved successfully. Bill No: {newBillNo}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // await ShowBillSavedDialogAsync(newBillNo);
                await ShowBillSavedDialogAsync(newBillNo.ToString());
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



    public static void BindParameters(OdbcCommand cmd, params object[] values)
    {
        if (values.Length != 96)
        {
            throw new ArgumentException($"Expected 96 parameters, but received {values.Length}.");
        }

        foreach (var val in values)
        {
            cmd.Parameters.AddWithValue("", val ?? DBNull.Value); // Use DBNull.Value for nulls
        }
    }


    private async void SaveSalesToDatabase_Old()
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
                    string insertQuery = @"INSERT INTO [rs_gst_27].[dbo].[sales] ([date], [bno], [party], [product], [rate], [qty], [category], [mrp], [sno], [costprice], [add1], [add2], [subs1], [subs2], [billvalue], [batchno], [expdate], [type], [payment], [pperbox], [caption], [note], [city], [codeno], [tax], [free], [st1], [tin], [bbookno], [billbook], [salesman], [commcode], [tonage], [less1], [less2], [less3], [less4], [less5], [useable], [less6], [add1caption], [add2caption], [less1caption], [less2caption], [add22], [less22], [category2], [time1], [rate1], [rate2], [rate3], [godown], [wsrs], [username], [dmyname], [dmybno], [company], [dmydate], [deleveryproduct], [address1], [address2], [phoneno], [podate], [pono], [reference], [despatch], [destination], [terms], [cess], [additionalcess], [bnochar], [chkrate], [dele_add_one], [dele_add_two], [dele_add_three], [dele_add_four], [dele_add_five], [dele_add_six], [packqty], [bonuspoints], [sizename], [sizevalue], [bnochar_end], [colour], [stcs], [sal_tonage], [point_or_not], [sal_barcode], [opincode], [gov_caption], [mailid], [distance], [sales_einvoice_irn], [sales_einvoice_ackno], [sales_einvoice_ackdate], [sales_einvoice_authtoken]) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                    using (OdbcCommand insertCmd = new OdbcCommand(insertQuery, connection))
                    {
                        Decimal rateResult = (Decimal)Calculate_Price(Convert.ToDouble(item.Rate2), Convert.ToDouble(item.tax));



                        insertCmd.Parameters.AddWithValue("@date", DateTime.Now.ToString()); // [date]
                        insertCmd.Parameters.AddWithValue("@bno", newBillNo);    // [bno]
                        insertCmd.Parameters.AddWithValue("@party", EnsureCustomerName());       // [party]
                        insertCmd.Parameters.AddWithValue("@product", item.PName);   // [product]
                        insertCmd.Parameters.AddWithValue("@rate", rateResult);   // [rate]
                        insertCmd.Parameters.AddWithValue("@qty", item.Quantity); // [qty]
                        insertCmd.Parameters.AddWithValue("@category", "M PRODUCT");  // [category]
                        insertCmd.Parameters.AddWithValue("@mrp", 0);            // [mrp]
                        insertCmd.Parameters.AddWithValue("@sno", 1343);         // [sno]
                        insertCmd.Parameters.AddWithValue("@costprice", 146.02); // [costprice]
                        insertCmd.Parameters.AddWithValue("@add1", 160.91);      // [add1]
                        insertCmd.Parameters.AddWithValue("@add2", 0);           // [add2]
                        insertCmd.Parameters.AddWithValue("@subs1", 0);          // [subs1]
                        insertCmd.Parameters.AddWithValue("@subs2", 0);          // [subs2]
                        insertCmd.Parameters.AddWithValue("@billvalue", item.Quantity * item.Rate2); // [billvalue]
                        insertCmd.Parameters.AddWithValue("@batchno", item.PCode);          // [batchno]
                        insertCmd.Parameters.AddWithValue("@expdate", DateTime.Now); // [expdate]
                        insertCmd.Parameters.AddWithValue("@type", "SALES");      // [type]
                        insertCmd.Parameters.AddWithValue("@payment", item.Quantity * item.Rate2); // [payment]
                        insertCmd.Parameters.AddWithValue("@pperbox", 1);         // [pperbox]
                        insertCmd.Parameters.AddWithValue("@caption", item.Unit); // [caption]
                        insertCmd.Parameters.AddWithValue("@note", ".");          // [note]
                        insertCmd.Parameters.AddWithValue("@city", "CASH AREA");  // [city]
                        insertCmd.Parameters.AddWithValue("@codeno", "");         // [codeno]
                        insertCmd.Parameters.AddWithValue("@tax", item.tax);      // [tax]
                        insertCmd.Parameters.AddWithValue("@free", 0);            // [free]
                        insertCmd.Parameters.AddWithValue("@st1", ".");           // [st1]
                        insertCmd.Parameters.AddWithValue("@tin", ".");           // [tin]
                        insertCmd.Parameters.AddWithValue("@bbookno", 1);         // [bbookno]
                        insertCmd.Parameters.AddWithValue("@billbook", "GST JUNE 24-25"); // [billbook]
                        insertCmd.Parameters.AddWithValue("@salesman", "DIRCET"); // [salesman]
                        insertCmd.Parameters.AddWithValue("@commcode", "361");    // [commcode]
                        insertCmd.Parameters.AddWithValue("@tonage", "0");        // [tonage]
                        insertCmd.Parameters.AddWithValue("@less1", 0);           // [less1]
                        insertCmd.Parameters.AddWithValue("@less2", 0);           // [less2]
                        insertCmd.Parameters.AddWithValue("@less3", 0);           // [less3]
                        insertCmd.Parameters.AddWithValue("@less4", 0);           // [less4]
                        insertCmd.Parameters.AddWithValue("@less5", 0);           // [less5]
                        insertCmd.Parameters.AddWithValue("@useable", 1);         // [useable]
                        insertCmd.Parameters.AddWithValue("@less6", 10);          // [less6]
                        insertCmd.Parameters.AddWithValue("@add1caption", "Transport Charges"); // [add1caption]
                        insertCmd.Parameters.AddWithValue("@add2caption", "Wages"); // [add2caption]
                        insertCmd.Parameters.AddWithValue("@less1caption", "Spl Pongan Offer"); // [less1caption]
                        insertCmd.Parameters.AddWithValue("@less2caption", "Disp(-)"); // [less2caption]
                        insertCmd.Parameters.AddWithValue("@add22", 0);           // [add22]
                        insertCmd.Parameters.AddWithValue("@less22", 0);          // [less22]
                        insertCmd.Parameters.AddWithValue("@category2", item.Category); // [category2]
                        insertCmd.Parameters.AddWithValue("@time1", DateTime.Today); // [time1]
                        insertCmd.Parameters.AddWithValue("@rate1", item.Rate2);  // [rate1]
                        insertCmd.Parameters.AddWithValue("@rate2", 0);           // [rate2]
                        insertCmd.Parameters.AddWithValue("@rate3", 0);           // [rate3]
                        insertCmd.Parameters.AddWithValue("@godown", "MAINGODWON"); // [godown]
                        insertCmd.Parameters.AddWithValue("@wsrs", 0);            // [wsrs]
                        insertCmd.Parameters.AddWithValue("@username", "admin");  // [username]
                        insertCmd.Parameters.AddWithValue("@dmyname", "");        // [dmyname]
                        insertCmd.Parameters.AddWithValue("@dmybno", 0);          // [dmybno]
                        insertCmd.Parameters.AddWithValue("@company", "M PRODUCT"); // [company]
                        insertCmd.Parameters.AddWithValue("@dmydate", "");        // [dmydate]
                        insertCmd.Parameters.AddWithValue("@deleveryproduct", 0); // [deleveryproduct]
                        insertCmd.Parameters.AddWithValue("@address1", ".");      // [address1]
                        insertCmd.Parameters.AddWithValue("@address2", ".");      // [address2]
                        insertCmd.Parameters.AddWithValue("@phoneno", ".");       // [phoneno]
                        insertCmd.Parameters.AddWithValue("@podate", "");         // [podate]
                        insertCmd.Parameters.AddWithValue("@pono", "");           // [pono]
                        insertCmd.Parameters.AddWithValue("@reference", "");      // [reference]
                        insertCmd.Parameters.AddWithValue("@despatch", "");       // [despatch]
                        insertCmd.Parameters.AddWithValue("@destination", "");    // [destination]
                        insertCmd.Parameters.AddWithValue("@terms", "");          // [terms]
                        insertCmd.Parameters.AddWithValue("@cess", 0);            // [cess]
                        insertCmd.Parameters.AddWithValue("@additionalcess", 0);  // [additionalcess]
                        insertCmd.Parameters.AddWithValue("@bnochar", "1");       // [bnochar]
                        insertCmd.Parameters.AddWithValue("@chkrate", 0);         // [chkrate]
                        insertCmd.Parameters.AddWithValue("@dele_add_one", "");   // [dele_add_one]
                        insertCmd.Parameters.AddWithValue("@dele_add_two", "");   // [dele_add_two]
                        insertCmd.Parameters.AddWithValue("@dele_add_three", ""); // [dele_add_three]
                        insertCmd.Parameters.AddWithValue("@dele_add_four", "");  // [dele_add_four]
                        insertCmd.Parameters.AddWithValue("@dele_add_five", "");  // [dele_add_five]
                        insertCmd.Parameters.AddWithValue("@dele_add_six", "");   // [dele_add_six]
                        insertCmd.Parameters.AddWithValue("@packqty", 0);         // [packqty]
                        insertCmd.Parameters.AddWithValue("@bonuspoints", 0);     // [bonuspoints]
                        insertCmd.Parameters.AddWithValue("@sizename", "");       // [sizename]
                        insertCmd.Parameters.AddWithValue("@sizevalue", 1);       // [sizevalue]
                        insertCmd.Parameters.AddWithValue("@bnochar_end", "");    // [bnochar_end]
                        insertCmd.Parameters.AddWithValue("@colour", "");         // [colour]
                        insertCmd.Parameters.AddWithValue("@stcs", 0);            // [stcs]
                        insertCmd.Parameters.AddWithValue("@sal_tonage", 0);      // [sal_tonage]
                        insertCmd.Parameters.AddWithValue("@point_or_not", 1);    // [point_or_not]
                        insertCmd.Parameters.AddWithValue("@sal_barcode", ".");   // [sal_barcode]
                        insertCmd.Parameters.AddWithValue("@opincode", "0");      // [opincode]
                        insertCmd.Parameters.AddWithValue("@gov_caption", "");    // [gov_caption]
                        insertCmd.Parameters.AddWithValue("@mailid", "");         // [mailid]
                        insertCmd.Parameters.AddWithValue("@distance", 0);        // [distance]
                        insertCmd.Parameters.AddWithValue("@sales_einvoice_irn", "."); // [sales_einvoice_irn]
                        insertCmd.Parameters.AddWithValue("@sales_einvoice_ackno", "."); // [sales_einvoice_ackno]
                        insertCmd.Parameters.AddWithValue("@sales_einvoice_ackdate", "."); // [sales_einvoice_ackdate]
                        insertCmd.Parameters.AddWithValue("@sales_einvoice_authtoken", "."); // [sales_einvoice_authtoken]


                        //insertCmd.ExecuteNonQuery();
                        // int placeholderCount = Regex.Matches(insertQuery, @"@\w+").Count;

                        // // Compare with the number of parameters added
                        // if (placeholderCount != insertCmd.Parameters.Count)
                        // {
                        //     throw new InvalidOperationException($"Mismatch between placeholders ({placeholderCount}) and parameters ({insertCmd.Parameters.Count}).");
                        // }



                        int qMarks = insertQuery.Count(c => c == '?');
                        int paramsAdded = insertCmd.Parameters.Count;

                        if (qMarks != paramsAdded)
                        {
                            MessageBox.Show($"❌ Placeholder count: {qMarks} ≠ Parameters added: {paramsAdded}", "Mismatch Error");
                        }
                        else
                        {
                            MessageBox.Show($"✅ Placeholder count matches parameters: {qMarks}", "OK");
                        }
                        BuildOdbcQueryWithValues(insertCmd.CommandText, insertCmd.Parameters);
                        insertCmd.ExecuteNonQuery();
                    }


                }

                // MessageBox.Show($"Sales saved successfully. Bill No: {newBillNo}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // await ShowBillSavedDialogAsync(newBillNo);
                await ShowBillSavedDialogAsync(newBillNo.ToString());
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



    public static string BuildOdbcQueryWithValues(string query, OdbcParameterCollection parameters)
    {
        string[] parts = query.Split('?');
        var sb = new StringBuilder();

        for (int i = 0; i < parameters.Count; i++)
        {
            sb.Append(parts[i]);

            var param = parameters[i];
            if (param.Value == null || param.Value == DBNull.Value)
            {
                sb.Append("NULL");
            }
            else if (param.Value is string || param.Value is DateTime)
            {
                sb.AppendFormat("'{0}'", param.Value.ToString().Replace("'", "''"));
            }
            else
            {
                sb.Append(param.Value);
            }
        }

        if (parts.Length > parameters.Count)
            sb.Append(parts[parts.Length - 1]);

        return sb.ToString();
    }



    private async Task ShowBillSavedDialogAsync(string newBillNo)
    {
        var settings = new MetroDialogSettings
        {
            AffirmativeButtonText = "Copy Bill No",
            NegativeButtonText = "Close",
            AnimateShow = true,
            AnimateHide = true,
            ColorScheme = MetroDialogColorScheme.Theme
        };

        var result = await this.ShowMessageAsync(
            "Sales Saved Successfully",
            $"Bill No: {newBillNo}",
            MessageDialogStyle.AffirmativeAndNegative,
            settings);

        if (result == MessageDialogResult.Affirmative)
        {
            Clipboard.SetText(newBillNo);
            await this.ShowMessageAsync("Copied", "Bill No copied to clipboard.", MessageDialogStyle.Affirmative);
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
                pt.Category = ? ORDER BY DisplayOrder";

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