using System.ComponentModel.DataAnnotations;

namespace Aglet_Backend.Models
{
    public class PurchaseRecord
    {
        [Key]
        public int PurchaseId { get; set; }
        public int ShoeId { get; set; }
        public int SupplierId { get; set; }
        public DateTime Purchase { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }


        public Shoe? Shoe { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
