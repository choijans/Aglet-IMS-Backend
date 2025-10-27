using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Aglet_Backend.DataAccess;
using Aglet_Backend.Models;
using Aglet_Backend.DTO;
using Aglet_Backend.Services;

namespace Aglet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseRecordController : ControllerBase
    {
        private readonly ShoeDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPurchaseService _purchaseService;

        public PurchaseRecordController(ShoeDbContext context, IMapper mapper, IPurchaseService purchaseService)
        {
            _context = context;
            _mapper = mapper;
            _purchaseService = purchaseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseRecordDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("pageNumber and pageSize must be greater than 0.");

            var totalCount = await _context.PurchaseRecords.CountAsync();

            var records = await _context.PurchaseRecords
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = _mapper.Map<IEnumerable<PurchaseRecordDto>>(records)
            });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseRecordDto>> GetPurchaseRecord(int id)
        {
            var record = await _context.PurchaseRecords.FindAsync(id);
            if (record == null) return NotFound();

            return Ok(_mapper.Map<PurchaseRecordDto>(record));
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseRecordDto>> CreatePurchaseRecord(PurchaseRecordDto recordDto)
        {
            var result = await _purchaseService.CreatePurchaseAsync(recordDto);
            return CreatedAtAction(nameof(GetPurchaseRecord), new { id = result.PurchaseId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseRecord(int id, PurchaseRecordDto recordDto)
        {
            if (id != recordDto.PurchaseId) return BadRequest();

            var record = await _context.PurchaseRecords.FindAsync(id);
            if (record == null) return NotFound();

            _mapper.Map(recordDto, record);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseRecord(int id)
        {
            var record = await _context.PurchaseRecords.FindAsync(id);
            if (record == null) return NotFound();

            _context.PurchaseRecords.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
