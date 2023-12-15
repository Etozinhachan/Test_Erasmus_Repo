using Microsoft.EntityFrameworkCore;
using testingStuff.models;

namespace testingStuff.data;

public class DbDataContext : DbContext
{
    public DbDataContext(DbContextOptions<DbDataContext> options)
        : base(options)
    {
    }

    public DbSet<testingStuff.models.User> Users { get; set; } = null!;
}