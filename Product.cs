using System;
using System.Text;
namespace GDS_Test_data.Models
{

    public class Product
    {
        public string PCode { get; set; }
        public string PName { get; set; }
        public float? Rate1 { get; set; }
        public decimal? Rate2 { get; set; }
        public float? MRP { get; set; }
        public string Category { get; set; }
        public float? CostPrice { get; set; }
        public float? MinStock { get; set; }
        public float? CaseRate { get; set; }
        public string Caption1 { get; set; }
        public string Caption2 { get; set; }
        public string Caption3 { get; set; }
        public float? UValue1 { get; set; }
        public float? UValue2 { get; set; }
        public float? UValue3 { get; set; }
        public string Display { get; set; }
        public float? SRate1 { get; set; }
        public float? SRate2 { get; set; }
        public float? SRate3 { get; set; }
        public float? PurDis { get; set; }
        public float? SalDis { get; set; }
        public float? Active { get; set; }
        public float? SalVAT { get; set; }
        public float? Stock { get; set; }
        public float? Trate1 { get; set; }
        public float? Trate2 { get; set; }
        public float? Trate3 { get; set; }
        public string CommCode { get; set; }
        public string DisText { get; set; }
        public float? Sno { get; set; }
        public string Barcode { get; set; }
        public float? MixReq { get; set; }
        public string Category2 { get; set; }
        public string CrDate { get; set; }
        public string LbDate { get; set; }
        public string Margin { get; set; }
        public string Company { get; set; }

        // Nullable decimal properties
        public decimal? Cess { get; set; }
        public decimal? AdditionalCess { get; set; }
        public decimal? PreStock { get; set; }

        // Barcode properties
        public string Barcode2 { get; set; }
        public string Barcode3 { get; set; }
        public string Barcode4 { get; set; }
        public string Barcode5 { get; set; }
        public string Barcode6 { get; set; }
        public string Barcode7 { get; set; }
        public string Barcode8 { get; set; }
        public string Barcode9 { get; set; }
        public string Barcode10 { get; set; }

        // Additional MRP properties
        public float? MRP2 { get; set; }
        public float? MRP3 { get; set; }
        public float? MRP4 { get; set; }
        public float? MRP5 { get; set; }

        // Additional SM Rate properties
        public float? SMRate2 { get; set; }
        public float? SMRate3 { get; set; }
        public float? SMRate4 { get; set; }
        public float? SMRate5 { get; set; }

        // Barcode captions
        public string BarcodeCaption1 { get; set; }
        public string BarcodeCaption2 { get; set; }

        // Size and Colour properties
        public int? Size1 { get; set; }
        public int? Colour1 { get; set; }

        // Cost properties
        public float? Cost2 { get; set; }
        public float? Cost3 { get; set; }
        public float? Cost4 { get; set; }
        public float? Cost5 { get; set; }

        // Product specifications
        public float? ProTonage { get; set; }
        public float? ProductExp { get; set; }
        public float? ProDeleMark { get; set; }
        // Sticker type and image properties
        public int? ProStickerType { get; set; }
        public string ProImage1 { get; set; }

        // SM Caption properties
        public string SMCaption2 { get; set; }
        public string SMCaption3 { get; set; }
        public string SMCaption4 { get; set; }
        public string SMCaption5 { get; set; }

        // SMU Value properties
        public int? SMUValue2 { get; set; }
        public int? SMUValue3 { get; set; }
        public int? SMUValue4 { get; set; }
        public int? SMUValue5 { get; set; }

        // SMW Rate properties
        public float? SMWRate2 { get; set; }
        public float? SMWRate3 { get; set; }
        public float? SMWRate4 { get; set; }
        public float? SMWRate5 { get; set; }
        public decimal? Total { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string pname { get; set; }
        public decimal mrp { get; set; }

        public int NoOfItems { get; set; }

        //  public string GetNativePName()
        //     {
        //         // Assuming PName is in Unicode and you want to convert it to UTF-8
        //         byte[] unicodeBytes = Encoding.Unicode.GetBytes(PName);
        //         byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, unicodeBytes);
        //         return Encoding.UTF8.GetString(utf8Bytes);
        //     }
        public string NativePName
        {
            get
            {
                // Assuming PName is in Unicode and you want to convert it to UTF-8
                // byte[] unicodeBytes = Encoding.Unicode.GetBytes(PName);
                // byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, unicodeBytes);
                // return Encoding.UTF8.GetString(utf8Bytes);
                //    byte[] unicodeBytes = Encoding.Unicode.GetBytes(PName);
                // // Convert the Unicode bytes to the native encoding (e.g., UTF-8)
                // string nativePName = Encoding.UTF8.GetString(unicodeBytes);
                return PName;
            }
        }



    }

    // public class ProductList
    // {
    //     public string pname { get; set; }
    //     public decimal Rate2 { get; set; }
    //     public decimal mrp { get; set; }
    // }

    public class SelectedProduct
    {
        public string PName { get; set; }
        public string PCode { get; set; }
        public decimal Rate2 { get; set; }
        public decimal Rate { get; set; }
        public decimal CaseRate { get; set; }

        public decimal Quantity { get; set; }
        public decimal Total => Rate2 * Quantity;
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal tax { get; set; }
        public string DisplayQuantity => $"{Quantity} {Unit}";
    }


}