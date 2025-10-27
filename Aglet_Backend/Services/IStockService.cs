using Aglet_Backend.DTO;

namespace Aglet_Backend.Services
{
    public interface IStockService
    {
        Task<StockTransmissionDto> CreateTransmissionAsync(StockTransmissionDto dto);
    }
}