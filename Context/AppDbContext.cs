using Microsoft.EntityFrameworkCore;
using MinimalApi.Entities;

namespace MinimalApi.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<Person> People { get; set; }
}