using System.ComponentModel.DataAnnotations;

namespace Aglet_Backend.Models
{
    public enum TransactionType
    {
        In, Out, Adjustment
    }
    public class StockTransmission
    {
        [Key]
        public int TransactionId { get; set; }
        public int ShoeId { get; set; }
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }


        public Shoe? Shoe { get; set; }
    }
}
