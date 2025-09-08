using System.ComponentModel.DataAnnotations;

namespace Aglet_Backend.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        public string Name { get; set; }

        public string ContactInfo { get; set; }

        public ICollection<PurchaseRecord>? PurchaseRecords { get; set; }
    }
}
