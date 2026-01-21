using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using todoApp.NET.Data;
using todoApp.NET.DTOs.RapportsDTOs;
using todoApp.NET.Models;

namespace todoApp.NET.Services;

public class RapportsService(ToDoAppContext context)
{
    public async Task ShowActiveTodosWithFilters(TodoQueryOptions req)
    {
        try
        {
            var query = context.Todos
                .AsNoTracking()
                .Where(todo => !todo.IsArchived);

            if (!string.IsNullOrWhiteSpace(req.Text))
            {
                var text = req.Text.Trim();
                query = query.Where(todo =>
                    EF.Functions.ILike(todo.Title, $"%{text}%") ||
                    EF.Functions.ILike(todo.Description, $"%{text}%"));
            }

            if (!string.IsNullOrWhiteSpace(req.Category))
            {
                var category = req.Category.Trim();
                query = query.Where(todo => todo.Category == category);
            }

            if (req.IsComplete.HasValue)
            {
                query = query.Where(todo => todo.IsComplete == req.IsComplete.Value);
            }

            if (req.DueDate.HasValue)
            {
                var dueDate = req.DueDate.Value.Date;
                query = query.Where(todo => todo.DueDate.HasValue && todo.DueDate.Value.Date == dueDate);
            }


            query = ApplySorting(query, req);

            var results = await query.ToListAsync();

            if (results.Count == 0)
            {
                Console.WriteLine("Inga aktiva todos matchade dina filter.");
                return;
            }

            Console.WriteLine("\n--- Aktiva todos ---");
            foreach (var todo in results)
            {
                PrintTodo(todo);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    private static IQueryable<ToDoItem> ApplySorting(IQueryable<ToDoItem> query, TodoQueryOptions req)
    {
        query = req.SortBy switch
        {
            SortBy.title => query.OrderBy(todo => todo.Title),
            SortBy.createdat => query.OrderBy(todo => todo.CreatedAt),
            _ => query.OrderBy(todo => todo.DueDate ?? DateTime.MaxValue)
        };

        if (!req.SortDescending)
        {
            return query;
        }

        return req.SortBy switch
        {
            SortBy.title => query.OrderByDescending(todo => todo.Title),
            SortBy.createdat => query.OrderByDescending(todo => todo.CreatedAt),
            _ => query.OrderByDescending(todo => todo.DueDate ?? DateTime.MaxValue)
        };
    }

    private static void PrintTodo(ToDoItem todo)
    {
        var status = todo.IsComplete ? "Klar" : "Ej klar";
        var dueDateText = todo.DueDate.HasValue ? todo.DueDate.Value.ToShortDateString() : "Ingen deadline";
        Console.WriteLine($"[{todo.Id}] {todo.Title}");
        Console.WriteLine($"Kategori: {todo.Category} | Status: {status} | Deadline: {dueDateText}");
        Console.WriteLine(todo.Description);
        Console.WriteLine("-----------------------");
    }

    public async Task ShowLateTodos()
    {
        var currentDate = DateTime.UtcNow;
        var result = await context.Todos
            .AsNoTracking()
            .Where(todo => todo.IsComplete == false && todo.DueDate.HasValue && todo.DueDate < currentDate)
            .ToListAsync();
        if (result.Count == 0)
        {
            Console.WriteLine("Inga todos matchade dina filter.");
        }

        foreach (var todo in result)
        {
            PrintLateTodos(todo);
        }
    }

    private static void PrintLateTodos(ToDoItem todo)
    {
        var status = todo.IsComplete ? "Klar" : "Ej klar";
        var dueDateText = todo.DueDate.HasValue ? todo.DueDate.Value.ToShortDateString() : "Ingen deadline";
        Console.WriteLine($"[{todo.Id}] {todo.Title}");

        var daysLate = Math.Floor((DateTime.UtcNow - todo.DueDate!.Value).TotalDays);
        Console.WriteLine(
            $"Kategori: {todo.Category} | Status: {status} | Deadline: {dueDateText} | Late for {daysLate} days");
        Console.WriteLine(todo.Description);
        Console.WriteLine("-----------------------");
    }


    public async Task ShowUpcomingDeadlines(int daysAhead)
    {
        var currentDate = DateTime.UtcNow;
        var to = currentDate.AddDays(daysAhead);
        var result = await context.Todos
            .AsNoTracking()
            .Where(todo => !todo.IsArchived && !todo.IsComplete && todo.DueDate.HasValue &&
                           todo.DueDate.Value >= currentDate && todo.DueDate <= to)
            .OrderBy(todo => todo.DueDate)
            .ToListAsync();

        if (result.Count == 0)
        {
            Console.WriteLine("Inga aktiva todos matchade dina filter.");
            return;
        }

        foreach (var todo in result)
            PrintUpcoming(todo, currentDate);
    }

    private static void PrintUpcoming(ToDoItem todo, DateTime now)
    {
        {
            var daysUntilDue = (int)Math.Ceiling(
                (todo.DueDate!.Value - now).TotalDays
            );

            Console.WriteLine(
                $"[{todo.Id}] {todo.Title} | Due in {daysUntilDue} days ({todo.DueDate:yyyy-MM-dd})"
            );
        }
    }

    public async Task ShowSummaryForCategories()
    {
        var now = DateTime.UtcNow;
        var summary = await context.Todos
            .AsNoTracking()
            .Where(t => !t.IsArchived)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Count(),
                Completed = g.Count(t => t.IsComplete),
                NotCompleted = g.Count(t => t.IsComplete),
                CompletionRate = !g.Any()
                    ? 0
                    : (double)g.Count(t => t.IsComplete) / g.Count() * 100
            })
            .OrderBy(summary => summary.Category)
            .ToListAsync();

        if (summary.Count == 0)
        {
            Console.WriteLine("No todos found.");
            return;
        }

        foreach (var s in summary)
        {
            Console.WriteLine($"Category: {s.Category}");
            Console.WriteLine($"  Total: {s.Total}");
            Console.WriteLine($"  Completed: {s.Completed}");
            Console.WriteLine($"  Not completed: {s.NotCompleted}");
            Console.WriteLine($"  Completion rate: {s.CompletionRate:F1}%");
            Console.WriteLine("-----------------------");
        }
    }

    public async Task ShowTopListCat(int limit)
    {
        var result = await context.Todos
            .AsNoTracking()
            .Where(t => !t.IsArchived)
            .GroupBy(t => t.Category)
            .Select(g => new
                {
                    Category = g.Key,
                    TotalIncompleted = g.Count(t => !t.IsComplete),
                }
            )
            .OrderByDescending(result => result.TotalIncompleted)
            .Take(limit)
            .ToListAsync();

        if (result.Count == 0)
        {
            Console.WriteLine("No categories found.");
        }
        var rank = 1;
        foreach (var item in result)
        {
            Console.WriteLine(
                $"{rank}. {item.Category} | Incomplete active todos: {item.TotalIncompleted}"
            );
            rank++;
        }
    }

    public async Task ExportAllTodos()
    {
        var result = await context.Todos
            .AsNoTracking()
            .Where(t => !t.IsArchived)
            .ToListAsync();
        if (!result.Any())
        {
            Console.WriteLine("No todos to export.");
            return;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true, 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(result, options);

        var fileName = $"todos_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

        await File.WriteAllTextAsync(filePath, json);

        Console.WriteLine($"Todos exported successfully to:");
        Console.WriteLine(filePath);
    }
}