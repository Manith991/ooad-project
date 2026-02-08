using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOAD_Project.Domain
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public string CategoryName { get; set; } = string.Empty;
    }
}
