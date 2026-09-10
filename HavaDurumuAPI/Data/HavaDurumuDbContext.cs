using Microsoft.EntityFrameworkCore;
using HavaDurumuAPI.Models;

namespace HavaDurumuAPI.Data
{
    public class HavaDurumuDbContext : DbContext
    {
        public HavaDurumuDbContext(DbContextOptions<HavaDurumuDbContext> options)
            : base(options)
        {
        }

        public DbSet<FavoriSehir> FavoriSehirler { get; set; }
    }
}