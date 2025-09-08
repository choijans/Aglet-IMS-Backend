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
    public class ShoesController : ControllerBase
    {
        private readonly ShoeDbContext _context;
        private readonly IMapper _mapper;


        public ShoesController(ShoeDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoeDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("pageNumber and pageSize must be greater than 0.");

            var totalCount = await _context.Shoes.CountAsync();

            var shoes = await _context.Shoes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = _mapper.Map<IEnumerable<ShoeDto>>(shoes)
            });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ShoeDto>> Get(int id)
        {
            var shoe = await _context.Shoes.FindAsync(id);
            if (shoe == null) return NotFound();
            return Ok(_mapper.Map<ShoeDto>(shoe));
        }


        [HttpPost]
        public async Task<ActionResult<ShoeDto>> Post(ShoeDto dto)
        {
            var shoe = _mapper.Map<Shoe>(dto);
            _context.Shoes.Add(shoe);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = shoe.ShoeId }, _mapper.Map<ShoeDto>(shoe));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ShoeDto dto)
        {
            if (id != dto.ShoeId) return BadRequest();
            var shoe = await _context.Shoes.FindAsync(id);
            if (shoe == null) return NotFound();
            _mapper.Map(dto, shoe);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var shoe = await _context.Shoes.FindAsync(id);
            if (shoe == null) return NotFound();
            _context.Shoes.Remove(shoe);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
