using Aglet_Backend.DTO;
using Aglet_Backend.Models;

namespace Aglet_Backend.Services
{
    public interface IShoeService
    {
        Task UpdateStockAsync(int shoeId, int quantityChange);
        Task<bool> CanAdjustStockAsync(int shoeId, int quantityChange);
        Task<int> GetCurrentStockAsync(int shoeId);
    }
}