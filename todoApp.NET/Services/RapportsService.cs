using Microsoft.EntityFrameworkCore;
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
            .Where(todo => todo.IsComplete == false && todo.DueDate.HasValue && todo.DueDate < currentDate).ToListAsync();
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
}