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
    public class StockTransmissionController : ControllerBase
    {
        private readonly ShoeDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStockService _stockService;

        public StockTransmissionController(ShoeDbContext context, IMapper mapper, IStockService stockService)
        {
            _context = context;
            _mapper = mapper;
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransmissionDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("pageNumber and pageSize must be greater than 0.");

            var totalCount = await _context.StockTransmissions.CountAsync();

            var transmissions = await _context.StockTransmissions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = _mapper.Map<IEnumerable<StockTransmissionDto>>(transmissions)
            });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransmissionDto>> GetStockTransmission(int id)
        {
            var transmission = await _context.StockTransmissions.FindAsync(id);
            if (transmission == null) return NotFound();

            return Ok(_mapper.Map<StockTransmissionDto>(transmission));
        }

        [HttpPost]
        public async Task<ActionResult<StockTransmissionDto>> CreateStockTransmission(StockTransmissionDto transmissionDto)
        {
            var result = await _stockService.CreateTransmissionAsync(transmissionDto);
            return CreatedAtAction(nameof(GetStockTransmission), new { id = result.TransactionId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStockTransmission(int id, StockTransmissionDto transmissionDto)
        {
            if (id != transmissionDto.TransactionId) return BadRequest();

            var transmission = await _context.StockTransmissions.FindAsync(id);
            if (transmission == null) return NotFound();

            _mapper.Map(transmissionDto, transmission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockTransmission(int id)
        {
            var transmission = await _context.StockTransmissions.FindAsync(id);
            if (transmission == null) return NotFound();

            _context.StockTransmissions.Remove(transmission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
