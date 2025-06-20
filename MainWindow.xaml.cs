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
        RateTextBox.Clear();
        QuantityTextBox.Clear();
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
    }

    private SelectedProduct _selectedRowToUpdate;

    private void ProductsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
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








    private void ProductsDataGrid_KeyDown(object sender, KeyEventArgs e)
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

    private void ProductsDataGrid_MouseDoubleClick(object sender, KeyEventArgs e)
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

    private void ProductTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
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

    private void ProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = ProductTextBox.Text;

        if (!string.IsNullOrEmpty(searchText))
        {
            ProductsDataList.Visibility = Visibility.Visible; // Show when text is typed
            LoadProductData(searchText);
            // ProductsDataList.SelectedIndex = 0; 

            // if (Products.Any())
            // {
            //     ProductsDataList.SelectedIndex = 0;
            //     ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
            // }

            // Selection must be set AFTER LoadProductData updates the collection
            if (Products.Any())
            {
                ProductsDataList.SelectedIndex = 0;
                ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
            }
            if (ProductsDataList.Items.Count == 0) // Ensure there are rows in the DataGrid
            {

                ProductsDataList.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
            if (ProductsDataList.Items.Count > 0)
            {
                ProductsDataList.SelectedIndex = 0;                         // select first row
                ProductsDataList.ScrollIntoView(ProductsDataList.Items[0]); // scroll into view
                var firstRow = ProductsDataList.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
                if (firstRow != null)
                {
                    firstRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
             e.Handled = true;
            }


        }
        else
        {
            ProductsDataList.Visibility = Visibility.Collapsed; // Hide when text is cleared
            Products.Clear(); // Clear the grid if the search text is empty
        }






    }




    private Product _currentProduct; // <<--- ADD THIS LINE

    // private void RateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    // {

    //     // Assuming the ListBox is bound to a list of Product objects
    //     _currentProduct = RateComboBox.SelectedItem as Product;

    //     if (RateComboBox.SelectedItem != null)
    //     {
    //         if (_selectedRowToUpdate == null)
    //         {
    //             _selectedRowToUpdate = new SelectedProduct();
    //         }

    //         string selectedLabel = RateComboBox.SelectedItem.ToString();

    //         if (selectedLabel == "Rate1" && !string.IsNullOrWhiteSpace(_currentProduct?.Caption2))
    //         {
    //             if (decimal.TryParse(_currentProduct.Caption2, out decimal caption2Rate))
    //                 _selectedRowToUpdate.Rate2 = caption2Rate;
    //         }
    //         else if (selectedLabel == "CaseRate" && !string.IsNullOrWhiteSpace(_currentProduct?.Caption1))
    //         {
    //             if (decimal.TryParse(_currentProduct.Caption1, out decimal caption1Rate))
    //                 _selectedRowToUpdate.Rate2 = caption1Rate;
    //         }
    //         else if (selectedLabel == "Rate2" && !string.IsNullOrWhiteSpace(_currentProduct?.Caption3))
    //         {
    //             if (decimal.TryParse(_currentProduct.Caption3, out decimal caption3Rate))
    //                 _selectedRowToUpdate.Rate2 = caption3Rate;
    //         }
    //         else
    //         {
    //             // Default fallback if nothing matches
    //             _selectedRowToUpdate.Rate2 = 0;
    //         }
    //     }
    // }



    // private void RateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    // {

    //     if (RateComboBox.SelectedItem != null && _currentSelectedProduct != null)

    //     {
    //         string selectedValue = RateComboBox.SelectedItem.ToString();

    //         decimal rateToSet = 0;

    //         // Match based on selected label
    //         if (selectedValue == _currentSelectedProduct.Caption2) // Rate1
    //         {
    //             if (decimal.TryParse(_currentSelectedProduct.Rate1.ToString(), out decimal caption2Rate))
    //                 rateToSet = caption2Rate;
    //         }
    //         else if (selectedValue == _currentSelectedProduct.Caption1) // CaseRate
    //         {
    //             if (decimal.TryParse(_currentSelectedProduct.CaseRate.ToString(), out decimal caption1Rate))
    //                 rateToSet = caption1Rate;
    //         }
    //         else if (selectedValue == _currentSelectedProduct.Caption3) // Rate2
    //         {
    //             if (decimal.TryParse(_currentSelectedProduct.Rate2.ToString(), out decimal caption3Rate))
    //                 rateToSet = caption3Rate;
    //         }

    //         // Set the RateTextBox value
    //         RateTextBox.Text = rateToSet.ToString("0.00");

    //         // Also update _selectedRowToUpdate if needed
    //         if (_selectedRowToUpdate != null)
    //         {
    //             _selectedRowToUpdate.Rate2 = rateToSet;
    //         }
    //         RateTextBox.Visibility = Visibility.Visible;
    //         RateComboBox.Visibility = Visibility.Collapsed;


    //     }
    // }



    // private void RateComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    // {
    //     if (e.Key == Key.Enter)
    //     {
    //         ApplySelectedRateFromComboBox();
    //         e.Handled = true; // Prevent default sound/beep
    //     }
    // }

    private void ApplySelectedRateFromComboBox()
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





    public void LoadProductData(string productSearchText)
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



    private void QtyTxtBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {

            Decimal qty = 0;

            // Check and parse quantity input with '+' handling
            if (QuantityTextBox.Text.Contains("+"))
            {
                var parts = QuantityTextBox.Text.Split('+');
                foreach (var part in parts)
                {
                    if (int.TryParse(part.Trim(), out int val))
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
                decimal.TryParse(QuantityTextBox.Text.Trim(), out qty);
            }



            if (decimal.TryParse(RateTextBox.Text, out decimal rate) &&
                 qty > 0 &&
                !string.IsNullOrWhiteSpace(ProductTextBox.Text))
            {
                string pname = ProductTextBox.Text.Trim();
                string qtyInput = QuantityTextBox.Text;




                if (_selectedRowToUpdate != null)
                {
                    // Overwrite quantity and rate for the selected row
                    _selectedRowToUpdate.PName = pname;
                    _selectedRowToUpdate.Rate2 = rate;
                    _selectedRowToUpdate.Quantity = qty;

                    _selectedRowToUpdate = null; // Clear edit mode
                }
                else
                {
                    // // Look for existing product
                    // var existingProduct = LoadedProducts
                    //     .FirstOrDefault(p => p.PName.Equals(pname, StringComparison.OrdinalIgnoreCase));

                    SelectedProduct existingProduct = null;

                    // if (!string.IsNullOrWhiteSpace(pname))
                    // {
                    //     existingProduct = LoadedProducts
                    //         .FirstOrDefault(p => !string.IsNullOrEmpty(p.PName) &&
                    //                              p.PName.Equals(pname, StringComparison.OrdinalIgnoreCase));
                    // }
                    if (LoadedProducts.Count > 0 &&

                     LoadedProducts != null && !string.IsNullOrWhiteSpace(pname))
                    {
                        existingProduct = LoadedProducts
                            .FirstOrDefault(p => !string.IsNullOrEmpty(p.PName) &&
                                                 p.PName.Equals(pname, StringComparison.OrdinalIgnoreCase));
                    }


                    if (existingProduct != null)
                    {
                        // Add to existing quantity if found
                        existingProduct.Quantity += qty;
                        existingProduct.Rate2 = rate; // Optional: overwrite or retain
                    }

                    else
                    {
                        // Ensure LoadedProducts is initialized
                        if (LoadedProducts == null)
                            LoadedProducts = new ObservableCollection<SelectedProduct>();

                        // Check if qtyInput contains '+'
                        if (SeparateRowsCheckBox.IsChecked == true)
                        {
                            // Split the input by '+' and add separate rows
                            var quantities = qtyInput.Split('+');
                            foreach (var q in quantities)
                            {
                                if (int.TryParse(q.Trim(), out int singleQty))
                                {
                                    var product = new SelectedProduct
                                    {
                                        PName = pname,
                                        Rate2 = rate,
                                        Quantity = singleQty
                                    };
                                    LoadedProducts.Add(product);
                                    ProductsDataGrid.ItemsSource = LoadedProducts;

                                }
                            }
                        }
                        else
                        {
                            // Add a single combined row
                            var newProduct = new SelectedProduct
                            {
                                PName = pname,
                                Rate2 = rate,
                                Quantity = qty,
                                Unit = UnitName // e.g., "kg" or "gram"
                            };
                            LoadedProducts.Add(newProduct);
                            ProductsDataGrid.ItemsSource = LoadedProducts;
                        }
                    }
                }

                // Font for native display (Tamil)
                // PNameNative.FontFamily = new FontFamily("SunTommy y Tamil");

                // Update UI
                UpdateGrandTotal();
                SortProducts();
                ProductsDataGrid.Items.Refresh();

                // Clear inputs
                ProductTextBox.Clear();
                RateTextBox.Clear();
                QuantityTextBox.Clear();
                ProductTextBox.Focus();
                ProductTextBox.FontFamily = new FontFamily("Segoe UI");
            }
        }
    }





    private Product _currentSelectedProduct;

    // private void ProductsDataList_PreviewKeyDown(object sender, KeyEventArgs e)
    // {
    //     if (Products.Count == 0) return;

    //     int currentIndex = ProductsDataList.SelectedIndex;

    //     if (e.Key == Key.Down)
    //     {
    //         if (currentIndex < Products.Count - 1)
    //         {
    //             ProductsDataList.SelectedIndex = currentIndex + 1;
    //             ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
    //         }
    //         e.Handled = true;
    //     }
    //     else if (e.Key == Key.Up)
    //     {
    //         if (currentIndex > 0)
    //         {
    //             ProductsDataList.SelectedIndex = currentIndex - 1;
    //             ProductsDataList.ScrollIntoView(ProductsDataList.SelectedItem);
    //         }
    //         e.Handled = true;
    //     }
    //     else if (e.Key == Key.Enter && ProductsDataList.SelectedItem is Product productFromList)
    //     {





    //         ProductTextBox.FontFamily = new FontFamily("SunTommy y Tamil");
    //         ProductTextBox.Text = productFromList.pname;

    //         if (!string.IsNullOrWhiteSpace(productFromList.Caption1) &&
    //             !string.IsNullOrWhiteSpace(productFromList.Caption2))
    //         {
    //             RateTextBox.Visibility = Visibility.Collapsed;
    //             RateComboBox.Visibility = Visibility.Visible;



    //             _currentSelectedProduct = productFromList;



    //             if (productFromList != null)
    //             {
    //                 // Check if Caption1, Caption2, Caption3 have different values
    //                 bool hasDifferentCaptions =
    //                     !string.IsNullOrWhiteSpace(productFromList.Caption1) &&
    //                     !string.IsNullOrWhiteSpace(productFromList.Caption2) &&
    //                     !string.IsNullOrWhiteSpace(productFromList.Caption3) &&
    //                     (!productFromList.Caption1.Equals(productFromList.Caption2, StringComparison.OrdinalIgnoreCase) ||
    //                      !productFromList.Caption1.Equals(productFromList.Caption3, StringComparison.OrdinalIgnoreCase) ||
    //                      !productFromList.Caption2.Equals(productFromList.Caption3, StringComparison.OrdinalIgnoreCase));

    //                 if (hasDifferentCaptions)
    //                 {
    //                     // Show ComboBox
    //                     RateTextBox.Visibility = Visibility.Collapsed;
    //                     RateComboBox.Visibility = Visibility.Visible;

    //                     var rateOptions = new List<string>();

    //                     if (!string.IsNullOrWhiteSpace(productFromList.Caption2)) // Rate1 = Caption2
    //                     {
    //                         rateOptions.Add(productFromList.Caption2);
    //                         UnitName = productFromList.Caption2;
    //                     }
    //                     if (!string.IsNullOrWhiteSpace(productFromList.Caption1)) // CaseRate = Caption1
    //                     {
    //                         rateOptions.Add(productFromList.Caption1);
    //                         UnitName = productFromList.Caption1;
    //                     }

    //                     if (!string.IsNullOrWhiteSpace(productFromList.Caption3)) // Rate2 = Caption3
    //                     {
    //                         rateOptions.Add(productFromList.Caption3);
    //                         UnitName = productFromList.Caption3;

    //                     }

    //                     RateComboBox.ItemsSource = rateOptions;
    //                     RateComboBox.SelectedIndex = 0;
    //                     RateComboBox.FontFamily = new FontFamily("SunTommy y Tamil");
    //                     RateComboBox.Focus();
    //                     //  e.Handled = false;
    //                 }
    //                 else
    //                 {
    //                     // Show TextBox
    //                     RateTextBox.Visibility = Visibility.Visible;
    //                     RateComboBox.Visibility = Visibility.Collapsed;
    //                     UnitName = productFromList.Caption3;
    //                     // Default to Rate2
    //                     RateTextBox.Text = productFromList.Rate2?.ToString("0.00") ?? "0.00";

    //                     QuantityTextBox.Focus();

    //                 }
    //             }





    //         }
    //         else
    //         {
    //             RateTextBox.Visibility = Visibility.Visible;
    //             RateComboBox.Visibility = Visibility.Collapsed;
    //             RateTextBox.Text = productFromList.Rate2?.ToString("0.00") ?? "0.00";
    //         }

    //         //QuantityTextBox.Text = productFromList.Quantity?.ToString() ?? "0";

    //         // _selectedRowToUpdate = new SelectedProduct
    //         // {
    //         //     PName = productFromList.PName,
    //         //     Rate2 = productFromList.Rate2 ?? 0,
    //         //     Quantity = productFromList.Quantity ?? 0
    //         // };

    //         RateComboBox.Focus();
    //         ProductsDataList.Visibility = Visibility.Collapsed;
    //         e.Handled = true;




    //     }


    // }




private void ProductsDataList_PreviewKeyDown(object sender, KeyEventArgs e)
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
        // Your existing selection logic
        ProductTextBox.Text = productFromList.pname;
        ProductTextBox.FontFamily = new FontFamily("SunTommy y Tamil");

        // ... existing code continues ...
        ProductsDataList.Visibility = Visibility.Collapsed;
        e.Handled = true;
    }
    else
    {
        // Redirect all other keys to ProductTextBox
        ProductTextBox.Focus();

        // Optional: simulate the keystroke in ProductTextBox
        // NOTE: You cannot easily simulate typed input, but redirecting focus lets user continue typing.
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


    // private void UpdateGrandTotal()
    // {
    //     decimal grandTotal = 0;

    //     foreach (SelectedProduct product in ProductsDataGrid.Items)
    //     {
    //         //  decimal rate = product.Rate2 ?? 0;
    //         //                 decimal qty = product.Quantity ?? 0;
    //         // grandTotal += rate * qty;
    //     }
    //     foreach (var item in ProductsDataGrid.Items)
    //     {
    //         if (item is SelectedProduct product)
    //         {
    //             // Handle nullable values for Rate2 and Quantity
    //             decimal rate = product.Rate2;
    //             decimal qty = product.Quantity;
    //             grandTotal += rate * qty;
    //         }
    //     }

    //     // GrandTotalTextBlock.Text = $"{grandTotal:C}";
    //     GrandTotalTextBlock.Text = grandTotal.ToString("C", new System.Globalization.CultureInfo("en-IN"));


    // }



    private void UpdateGrandTotal()
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


    private void ProductsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.InvokeAsync(UpdateGrandTotal); // Delay to ensure value is committed
    }



    private void masalaComboBox_KeyDown(object sender, KeyEventArgs e)
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


    // public List<SelectedProduct> GetProductsByCategory(string category, decimal multiplier)
    // {
    //     string connectionString = "Driver={SQL Server};Server=SERVER;Database=rs_gst_27;Trusted_Connection=Yes;";
    //     // string connectionString = "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=True;";
    //     var productList = new List<SelectedProduct>();
    //     string query = @"
    //     SELECT 
    //             p.pname, 
    //           p.Rate1, 
    //             p.Rate2, 
    //             pt.Category, 
    //             pt.QTY,
    //             pt.unit,
    //             p.pcode,
    //             p.display as tax
    //         FROM 
    //             Product p
    //         INNER JOIN 
    //             ProductTemplates pt ON p.Pcode = pt.ProductName
    //         WHERE 
    //             pt.Category = ?";

    //     // using (var conn = new OdbcConnection(connectionString))
    //     using (OdbcConnection conn = new OdbcConnection(DbConfig.ConnectionString))
    //     using (var cmd = new OdbcCommand(query, conn))
    //     {
    //         cmd.Parameters.Add(new OdbcParameter("", category)); // ODBC uses "?" as a placeholder, so parameter name is ignored
    //         conn.Open();

    //         using (var reader = cmd.ExecuteReader())
    //         {
    //             while (reader.Read())
    //             {
    //                 // Multiply quantity by the entered multiplier
    //                 decimal baseQuantity = Convert.ToDecimal(reader["QTY"]);

    //                 // Determine which rate to use based on Unit
    //                 // string unit = reader["Unit"].ToString();
    //                 // decimal rate = unit == "kg"
    //                 //     ? Convert.ToDecimal(reader["Rate2"])
    //                 //     : Convert.ToDecimal(reader["Rate1"]);


    //                 string unit = reader["Unit"].ToString(); // normalize casing
    //                 decimal rate;

    //                 switch (unit)
    //                 {//gram
    //                     case "fpuhk;":
    //                     case "ml":
    //                         rate = Convert.ToDecimal(reader["Rate1"]);
    //                         break;
    //                     //kilo
    //                     case "fpNyh":
    //                     case "liter":
    //                         rate = Convert.ToDecimal(reader["Rate2"]);
    //                         break;
    //                     //piece
    //                     case "gP];":
    //                     case "unit":
    //                         rate = Convert.ToDecimal(reader["Rate2"]);
    //                         break;

    //                     default:
    //                         rate = 0; // fallback rate
    //                         break;
    //                 }


    //                 productList.Add(new SelectedProduct
    //                 {
    //                     PName = reader["pname"].ToString(),
    //                     Quantity = baseQuantity * multiplier,
    //                     // Unit=reader["Unit"].ToString(),

    //                     // QTY = reader["QTY"] != DBNull.Value ? Convert.ToSingle(reader["QTY"]) : 0,
    //                     // Unit = reader["Unit"].ToString(),
    //                     // Rate2=Convert.ToDecimal(reader["Rate2"]),
    //                     Unit = unit,
    //                     Rate2 = rate,
    //                     Category = reader["Category"].ToString(),
    //                     PCode = reader["pcode"].ToString(),
    //                     tax = Convert.ToDecimal(reader["tax"])

    //                 });
    //             }
    //         }
    //     }

    //     return productList; // Ensures all code paths return a value
    // }


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


    //         private void SaveSalesToDatabase()
    // {
    //     try
    //     {
    //            string connectionString = "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=True;";

    //         using (OdbcConnection connection = new OdbcConnection(connectionString))
    //         {
    //             connection.Open();

    //             foreach (var item in LoadedProducts)
    //             {
    //                 string query = @"
    //                     INSERT INTO Sales (ProductName, Quantity, Rate, Total, Unit)
    //                     VALUES (@ProductName, @Quantity, @Rate, @Total, @Unit)";

    //                 using (OdbcCommand command = new OdbcCommand(query, connection))
    //                 {
    //                     command.Parameters.AddWithValue("@ProductName", item.PName);
    //                     command.Parameters.AddWithValue("@Quantity", item.Quantity);
    //                     command.Parameters.AddWithValue("@Rate", item.Rate2);
    //                     command.Parameters.AddWithValue("@Total", item.Rate2 * item.Quantity);
    //                     command.Parameters.AddWithValue("@Unit", item.Unit ?? string.Empty);

    //                     //command.ExecuteNonQuery();
    //                 }
    //             }

    //             MessageBox.Show("Sales data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         MessageBox.Show("Error saving sales data:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //     }
    // }


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
            using (OdbcConnection connection = new OdbcConnection(connectionString))
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

                    // using (OdbcCommand insertCmd = new OdbcCommand(insertQuery, connection))
                    // {
                    //     insertCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    //     insertCmd.Parameters.AddWithValue("@bno", newBillNo);
                    //     insertCmd.Parameters.AddWithValue("@party", "M.R.");
                    //     insertCmd.Parameters.AddWithValue("@product", item.PName);
                    //     insertCmd.Parameters.AddWithValue("@rate", item.Rate2);
                    //     insertCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    //     insertCmd.Parameters.AddWithValue("@category", "M PRODUCT");
                    //     insertCmd.Parameters.AddWithValue("@mrp", 0);
                    //     insertCmd.Parameters.AddWithValue("@sno", 1343);
                    //     insertCmd.Parameters.AddWithValue("@costprice", 146.02);
                    //     insertCmd.Parameters.AddWithValue("@add1", 160.91);
                    //     insertCmd.Parameters.AddWithValue("@add2", 0);
                    //     insertCmd.Parameters.AddWithValue("@subs1", 0);
                    //     insertCmd.Parameters.AddWithValue("@subs2", 0);
                    //     insertCmd.Parameters.AddWithValue("@billvalue", item.Quantity * item.Rate2);
                    //     insertCmd.Parameters.AddWithValue("@batchno", "M");
                    //     insertCmd.Parameters.AddWithValue("@expdate", DateTime.Now);
                    //     insertCmd.Parameters.AddWithValue("@type", "SALES");
                    //     insertCmd.Parameters.AddWithValue("@payment", item.Quantity * item.Rate2);
                    //     insertCmd.Parameters.AddWithValue("@pperbox", 1);
                    //     insertCmd.Parameters.AddWithValue("@caption", "fpuh");
                    //     insertCmd.Parameters.AddWithValue("@note", ".");
                    //     insertCmd.Parameters.AddWithValue("@city", "CASH AREA");
                    //     insertCmd.Parameters.AddWithValue("@codeno", "");
                    //     insertCmd.Parameters.AddWithValue("@tax", 5.0);
                    //     insertCmd.Parameters.AddWithValue("@free", 0);
                    //     insertCmd.Parameters.AddWithValue("@st1", ".");
                    //     insertCmd.Parameters.AddWithValue("@tin", ".");
                    //     insertCmd.Parameters.AddWithValue("@bbookno", 1);
                    //     insertCmd.Parameters.AddWithValue("@billbook", "GST JUNE 24-25");
                    //     insertCmd.Parameters.AddWithValue("@salesman", "DIRCET");
                    //     insertCmd.Parameters.AddWithValue("@commcode", "361");
                    //     insertCmd.Parameters.AddWithValue("@tonage", "0");
                    //     insertCmd.Parameters.AddWithValue("@less1", 0);
                    //     insertCmd.Parameters.AddWithValue("@less2", 0);
                    //     insertCmd.Parameters.AddWithValue("@less3", 0);
                    //     insertCmd.Parameters.AddWithValue("@less4", 0);
                    //     insertCmd.Parameters.AddWithValue("@less5", 0);
                    //     insertCmd.Parameters.AddWithValue("@useable", 1);
                    //     insertCmd.Parameters.AddWithValue("@less6", 10);
                    //     insertCmd.Parameters.AddWithValue("@add1caption", "Transport Charges");
                    //     insertCmd.Parameters.AddWithValue("@add2caption", "Wages");
                    //     insertCmd.Parameters.AddWithValue("@less1caption", "Spl Pongan Offer");
                    //     insertCmd.Parameters.AddWithValue("@less2caption", "Disp(-)");
                    //     insertCmd.Parameters.AddWithValue("@add22", 0);
                    //     insertCmd.Parameters.AddWithValue("@less22", 0);
                    //     insertCmd.Parameters.AddWithValue("@category2", "ALL");
                    //     insertCmd.Parameters.AddWithValue("@time1", DateTime.Now);
                    //     insertCmd.Parameters.AddWithValue("@rate1", 0);
                    //     insertCmd.Parameters.AddWithValue("@rate2", 0);
                    //     insertCmd.Parameters.AddWithValue("@rate3", 0);
                    //     insertCmd.Parameters.AddWithValue("@godown", "MAINGODWON");
                    //     insertCmd.Parameters.AddWithValue("@wsrs", 0);
                    //     insertCmd.Parameters.AddWithValue("@username", "admin");
                    //     insertCmd.Parameters.AddWithValue("@dmyname", "");
                    //     insertCmd.Parameters.AddWithValue("@dmybno", 0);
                    //     insertCmd.Parameters.AddWithValue("@company", "M PRODUCT");
                    //     insertCmd.Parameters.AddWithValue("@dmydate", "");
                    //     insertCmd.Parameters.AddWithValue("@deleveryproduct", 0);
                    //     insertCmd.Parameters.AddWithValue("@address1", ".");
                    //     insertCmd.Parameters.AddWithValue("@address2", ".");
                    //     insertCmd.Parameters.AddWithValue("@phoneno", ".");
                    //     insertCmd.Parameters.AddWithValue("@podate", "");
                    //     insertCmd.Parameters.AddWithValue("@pono", "");
                    //     insertCmd.Parameters.AddWithValue("@reference", "");
                    //     insertCmd.Parameters.AddWithValue("@despatch", "");
                    //     insertCmd.Parameters.AddWithValue("@destination", "");
                    //     insertCmd.Parameters.AddWithValue("@terms", "");
                    //     insertCmd.Parameters.AddWithValue("@cess", 0);
                    //     insertCmd.Parameters.AddWithValue("@additionalcess", 0);
                    //     insertCmd.Parameters.AddWithValue("@bnochar", "1");
                    //     insertCmd.Parameters.AddWithValue("@chkrate", 0);
                    //     insertCmd.Parameters.AddWithValue("@dele_add_one", "");
                    //     insertCmd.Parameters.AddWithValue("@dele_add_two", "");
                    //     insertCmd.Parameters.AddWithValue("@dele_add_three", "");
                    //     insertCmd.Parameters.AddWithValue("@dele_add_four", "");
                    //     insertCmd.Parameters.AddWithValue("@dele_add_five", "");
                    //     insertCmd.Parameters.AddWithValue("@dele_add_six", "");
                    //     insertCmd.Parameters.AddWithValue("@packqty", 0);
                    //     insertCmd.Parameters.AddWithValue("@bonuspoints", 0);
                    //     insertCmd.Parameters.AddWithValue("@sizename", "");
                    //     insertCmd.Parameters.AddWithValue("@sizevalue", 1);
                    //     insertCmd.Parameters.AddWithValue("@bnochar_end", "");
                    //     insertCmd.Parameters.AddWithValue("@colour", "");
                    //     insertCmd.Parameters.AddWithValue("@stcs", 0);
                    //     insertCmd.Parameters.AddWithValue("@sal_tonage", 0);
                    //     insertCmd.Parameters.AddWithValue("@point_or_not", 1);
                    //     insertCmd.Parameters.AddWithValue("@sal_barcode", ".");
                    //     insertCmd.Parameters.AddWithValue("@opincode", "0");
                    //     insertCmd.Parameters.AddWithValue("@gov_caption", "");
                    //     insertCmd.Parameters.AddWithValue("@mailid", "");
                    //     insertCmd.Parameters.AddWithValue("@distance", 0);
                    //     insertCmd.Parameters.AddWithValue("@sales_einvoice_irn", ".");
                    //     insertCmd.Parameters.AddWithValue("@sales_einvoice_ackno", ".");
                    //     insertCmd.Parameters.AddWithValue("@sales_einvoice_ackdate", ".");
                    //     insertCmd.Parameters.AddWithValue("@sales_einvoice_authtoken", ".");

                    //     insertCmd.ExecuteNonQuery();
                    // }
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




    List<string> connectionStrings = new List<string>
{
    "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=localhost;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=localhost\\SQLEXPRESS;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=169.254.53.203\\SQLEXPRESS;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=Yes;",
    "Driver={SQL Server};Server=DESKTOP-70FHR7B;Database=rs_gst_27;Trusted_Connection=Yes;",
    // Local machine
"Driver={SQL Server};Server=.;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=(local);Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=localhost;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=.\\SQLEXPRESS;Database=rs_gst_27;Trusted_Connection=Yes;",

// Remote by IP
"Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=169.254.53.203\\SQLEXPRESS;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=169.254.53.203,1433;Database=rs_gst_27;Trusted_Connection=Yes;",

// Remote by machine name
"Driver={SQL Server};Server=SERVER;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=SERVER\\SQLEXPRESS;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=SERVER,1433;Database=rs_gst_27;Trusted_Connection=Yes;",

"Driver={SQL Server};Server=169.254.53.203;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=SERVER;Database=rs_gst_27;Trusted_Connection=Yes;",
"Driver={SQL Server};Server=169.254.53.203,1433;Database=rs_gst_27;Trusted_Connection=Yes;"





   };


    public string FindWorkingConnectionString(List<string> connectionStrings)
    {
        foreach (string connStr in connectionStrings)
        {
            try
            {
                using (OdbcConnection conn = new OdbcConnection(connStr))
                {
                    conn.Open();
                    MessageBox.Show("✅ Working Connection:\n" + connStr, "Connection Success");
                    conn.Close();
                    //return connStr;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Failed: " + connStr + "\n" + ex.Message);
            }
        }

        MessageBox.Show("❌ No working connection string found.", "Connection Error");
        return null;
    }

    private void TestTrustedConnections()
    {
        string database = "rs_gst_27";

        // Replace with your server’s actual IP or add more if needed
        string[] possibleServers = new string[]
        {
        ".",                      // local machine
        "(local)",               // local
        "localhost",             // local loopback
        "169.254.53.203",             // local IP
        "DESKTOP-70FHR7B",   
        // Environment.MachineName, // your PC name
        // Dns.GetHostName(),       // DNS hostname
        "169.254.53.203",      // replace with actual SQL Server IP
        "SERVER",             // replace with actual server name
        };

        List<string> connectionStrings = new List<string>();

        foreach (string server in possibleServers)
        {
            // connectionStrings.Add($"Driver={{SQL Server}};Server={server};Database={database};Trusted_Connection=Yes;");
            // connectionStrings.Add($"Driver={{SQL Server}};Server={server},1433;Database={database};Trusted_Connection=Yes;");
            connectionStrings.Add($"Driver={{SQL Server}};Server={server},1433;Database={database};Uid=SA;Pwd=;");
        }

        foreach (var connStr in connectionStrings)
        {
            try
            {
                using (OdbcConnection conn = new OdbcConnection(connStr))
                {
                    conn.Open();
                    MessageBox.Show($"✅ Connected successfully connection str: '{conn}'", "Connection Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    conn.Close();


                    //return; // Stop on first success
                }
            }
            catch
            {
                // Try next one silently
            }
        }

        MessageBox.Show("❌ No trusted (Windows Authentication) connection succeeded.\n\n" +
                        "Make sure the SQL Server allows remote access, TCP/IP is enabled, and port 1433 is open.",
                        "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // string workingConnection = FindWorkingConnectionString(connectionStrings);
        //   TestTrustedConnections();

        // TestSaPasswords();
        // if (workingConnection != null)
        // {
        //     // // Store it in memory or settings for use
        //     // Properties.Settings.Default.WorkingConnectionString = workingConnection;
        //     // Properties.Settings.Default.Save();
        // }
    }

    private void TestSaPasswords()
    {
        string server = "169.254.53.203";         // Replace with your SQL Server IP
        string database = "rs_gst_27";           // Your database name
        string username = "sa";                  // SQL login name

        // Common passwords to try (for testing)
        List<string> passwordsToTry = new List<string>
    {
        "sa",
        "admin",
        "1234",
        "12345",
        "sql",
        "password",
        "" // empty password
    };

        foreach (string password in passwordsToTry)
        {
            string connStr = $"Driver={{SQL Server}};Server={server};Database={database};Uid={username};Pwd={password};";

            try
            {
                using (OdbcConnection conn = new OdbcConnection(connStr))
                {
                    conn.Open();
                    MessageBox.Show($"✅ Connected successfully with password: '{password}'", "Connection Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    conn.Close();
                    return; // Stop after first successful login
                }
            }
            catch
            {
                // Failed login, try next
            }
        }

        MessageBox.Show("❌ None of the passwords worked for 'sa'.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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