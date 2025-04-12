namespace TodoGrpc.Data;

using Microsoft.EntityFrameworkCore;
using TodoGrpc.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}