using Microsoft.EntityFrameworkCore;
using SecurePasswordManager.Models;

namespace SecurePasswordManager.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VaultEntry> VaultEntries => Set<VaultEntry>();
    public DbSet<Session> Sessions => Set<Session>();
}