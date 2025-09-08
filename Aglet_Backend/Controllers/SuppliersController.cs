using Aglet_Backend.DataAccess;
using Aglet_Backend.DTO;
using Aglet_Backend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aglet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ShoeDbContext _context;
        private readonly IMapper _mapper;


        public SuppliersController(ShoeDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("pageNumber and pageSize must be greater than 0.");

            var totalCount = await _context.Suppliers.CountAsync();

            var suppliers = await _context.Suppliers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = _mapper.Map<IEnumerable<SupplierDto>>(suppliers)
            });
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDto>> Get(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return Ok(_mapper.Map<SupplierDto>(supplier));
        }


        [HttpPost]
        public async Task<ActionResult<SupplierDto>> Post(SupplierDto dto)
        {
            var supplier = _mapper.Map<Supplier>(dto);
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = supplier.SupplierId }, _mapper.Map<SupplierDto>(supplier));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, SupplierDto dto)
        {
            if (id != dto.SupplierId) return BadRequest();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            _mapper.Map(dto, supplier);
            await _context.SaveChangesAsync();
            return NoContent();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
