using Aglet_Backend.DTO;

namespace Aglet_Backend.Services
{
    public interface IPurchaseService
    {
        Task<PurchaseRecordDto> CreatePurchaseAsync(PurchaseRecordDto dto);
    }
}