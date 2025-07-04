using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ProductModel
    {
        public string Sku { get; set; } = "";
        public string ProductName { get; set; }
        public int Quantity { get; set; } = 0;
        public double UnitPrice { get; set; } = 0;
        public double UnitOriginalPrice { get; set; } = 0;
        public double UnitCostOfGood { get; set; }
        public bool IsTaxable { get; set; } = false;
        public double TaxAmount { get; set; } = 0;
    }
}
