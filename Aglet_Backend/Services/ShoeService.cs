using Aglet_Backend.DataAccess;
using Aglet_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Aglet_Backend.Services
{
    public class ShoeService : IShoeService
    {
        private readonly ShoeDbContext _context;

        public ShoeService(ShoeDbContext context)
        {
            _context = context;
        }

        public async Task UpdateStockAsync(int shoeId, int quantityChange)
        {
            var shoe = await _context.Shoes.FindAsync(shoeId);
            if (shoe != null)
            {
                shoe.CurrentStock += quantityChange;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CanAdjustStockAsync(int shoeId, int quantityChange)
        {
            var shoe = await _context.Shoes.FindAsync(shoeId);
            return shoe != null && shoe.CurrentStock + quantityChange >= 0;
        }

        public async Task<int> GetCurrentStockAsync(int shoeId)
        {
            var shoe = await _context.Shoes.FindAsync(shoeId);
            return shoe?.CurrentStock ?? 0;
        }
    }
}