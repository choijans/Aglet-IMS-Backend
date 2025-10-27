using Aglet_Backend.DataAccess;
using Aglet_Backend.DTO;
using Aglet_Backend.Models;
using AutoMapper;

namespace Aglet_Backend.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ShoeDbContext _context;
        private readonly IMapper _mapper;
        private readonly IShoeService _shoeService;

        public PurchaseService(ShoeDbContext context, IMapper mapper, IShoeService shoeService)
        {
            _context = context;
            _mapper = mapper;
            _shoeService = shoeService;
        }

        public async Task<PurchaseRecordDto> CreatePurchaseAsync(PurchaseRecordDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var record = _mapper.Map<PurchaseRecord>(dto);
                _context.PurchaseRecords.Add(record);
                await _context.SaveChangesAsync();

                // Update stock
                await _shoeService.UpdateStockAsync(dto.ShoeId, dto.Quantity);

                await transaction.CommitAsync();
                return _mapper.Map<PurchaseRecordDto>(record);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}