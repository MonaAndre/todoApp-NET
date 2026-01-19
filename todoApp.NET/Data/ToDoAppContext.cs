using Microsoft.EntityFrameworkCore;
using todoApp.NET.Models;

namespace todoApp.NET.Data;

public class ToDoAppContext : DbContext
{
    public DbSet<ToDoItem> Todos => Set<ToDoItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=todo;Username=mona;Password=mona123");
    }
}