using Aglet_Backend.Models;

namespace Aglet_Backend.DTO
{
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }


    public class ShoeDto
    {
        public int ShoeId { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Colorway { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Condition { get; set; } = "New";
        public decimal PurchasePrice { get; set; }
        public int CurrentStock { get; set; }
    }


    public class StockTransmissionDto
    {
        public int TransactionId { get; set; }
        public int ShoeId { get; set; }
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }


    public class PurchaseRecordDto
    {
        public int PurchaseId { get; set; }
        public int ShoeId { get; set; }
        public int SupplierId { get; set; }
        public DateTime Purchase { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
    }
}
