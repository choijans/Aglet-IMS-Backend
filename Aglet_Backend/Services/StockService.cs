using Aglet_Backend.DataAccess;
using Aglet_Backend.DTO;
using Aglet_Backend.Models;
using AutoMapper;

namespace Aglet_Backend.Services
{
    public class StockService : IStockService
    {
        private readonly ShoeDbContext _context;
        private readonly IMapper _mapper;
        private readonly IShoeService _shoeService;

        public StockService(ShoeDbContext context, IMapper mapper, IShoeService shoeService)
        {
            _context = context;
            _mapper = mapper;
            _shoeService = shoeService;
        }

        public async Task<StockTransmissionDto> CreateTransmissionAsync(StockTransmissionDto dto)
        {
            int quantityChange = dto.TransactionType switch
            {
                TransactionType.In => dto.Quantity,
                TransactionType.Out => -dto.Quantity,
                TransactionType.Adjustment => dto.Quantity, // Assuming positive for increase, negative for decrease
                _ => 0
            };

            if (!await _shoeService.CanAdjustStockAsync(dto.ShoeId, quantityChange))
            {
                throw new InvalidOperationException("Stock adjustment would result in negative stock.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transmission = _mapper.Map<StockTransmission>(dto);
                _context.StockTransmissions.Add(transmission);
                await _context.SaveChangesAsync();

                // Update stock
                await _shoeService.UpdateStockAsync(dto.ShoeId, quantityChange);

                await transaction.CommitAsync();
                return _mapper.Map<StockTransmissionDto>(transmission);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}