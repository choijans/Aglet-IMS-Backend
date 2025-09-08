using System.ComponentModel.DataAnnotations;

namespace Aglet_Backend.Models
{
    public class Shoe
    {
        [Key]
        public int ShoeId { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string ColorWay { get; set; }

        public string Size { get; set; }

        public string Condition { get; set; }

        public decimal PurchasePrice { get; set; }

        public int CurrentStock { get; set; }

        public ICollection<StockTransmission>? StockTransmissions { get; set; }
        public ICollection<PurchaseRecord>? PurchaseRecords { get; set; }
    }
}
