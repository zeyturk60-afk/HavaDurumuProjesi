using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HavaDurumuAPI.Data;
using HavaDurumuAPI.Models;

namespace HavaDurumuAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriSehirlerController : ControllerBase
    {
        private readonly HavaDurumuDbContext _context;

        public FavoriSehirlerController(HavaDurumuDbContext context)
        {
            _context = context;
        }

        // GET: api/FavoriSehirler
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriSehir>>> GetFavoriSehirler()
        {
            return await _context.FavoriSehirler.ToListAsync();
        }

        // GET: api/FavoriSehirler/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriSehir>> GetFavoriSehir(int id)
        {
            var favoriSehir = await _context.FavoriSehirler.FindAsync(id);

            if (favoriSehir == null)
            {
                return NotFound();
            }

            return favoriSehir;
        }

        // POST: api/FavoriSehirler
        [HttpPost]
        public async Task<ActionResult<FavoriSehir>> PostFavoriSehir(FavoriSehir favoriSehir)
        {
            _context.FavoriSehirler.Add(favoriSehir);
            await _context.SaveChangesAsync();

            return Ok(favoriSehir);
        }

        // PUT: api/FavoriSehirler/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFavoriSehir(int id, FavoriSehir favoriSehir)
        {
            if (id != favoriSehir.Id)
            {
                return BadRequest();
            }

            _context.Entry(favoriSehir).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/FavoriSehirler/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavoriSehir(int id)
        {
            var favoriSehir = await _context.FavoriSehirler.FindAsync(id);

            if (favoriSehir == null)
            {
                return NotFound();
            }

            _context.FavoriSehirler.Remove(favoriSehir);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}