using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Features.Theaters;

namespace Selu383.SP25.P02.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Theater> Theaters { get; set; }
    } 
}