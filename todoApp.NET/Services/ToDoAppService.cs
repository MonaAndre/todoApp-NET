using System.Globalization;
using Microsoft.EntityFrameworkCore;
using todoApp.NET.Data;
using todoApp.NET.Models;

namespace todoApp.NET.Services;

public class ToDoAppService
{
    private readonly ToDoAppContext _context;

    public ToDoAppService(ToDoAppContext context)
    {
        _context = context;
    }

    public async Task GetTodos()
    {
        try
        {
            var todolist = await _context.Todos.ToListAsync();
            if (todolist.Count == 0)
            {
                Console.WriteLine("To Do list is empty");
            }

            foreach (var todo in todolist)
            {
                Console.WriteLine(todo.Title);
                Console.WriteLine(todo.IsComplete ? "completed" : "not completed");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task AddTodo()
    {
        string title;
        while (true)
        {
            Console.Write("Title: ");
            title = (Console.ReadLine() ?? "").Trim();

            if (title.Length == 0)
            {
                Console.WriteLine("Title kan inte vara tom.");
                continue;
            }

            if (title.Length > 100)
            {
                Console.WriteLine("Title får vara max 100 tecken.");
                continue;
            }

            break;
        }

        string? description;
        while (true)
        {
            Console.Write("Description: ");
            description = (Console.ReadLine() ?? "").Trim();

            if (description.Length == 0)
            {
                Console.WriteLine("Description är obligatorisk.");
                continue;
            }

            if (description.Length > 500)
            {
                Console.WriteLine("Description får vara max 500 tecken.");
                continue;
            }

            break;
        }

        DateTime? dueDate = null;
        while (true)
        {
            Console.Write("Due Date (yyyy-MM-dd, valfritt): ");
            var input = (Console.ReadLine() ?? "").Trim();

            if (input.Length == 0)
            {
                dueDate = null;
                break;
            }

            if (!DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed))
            {
                Console.WriteLine("Ogiltigt datum. Använd format yyyy-MM-dd (t.ex. 2026-01-19).");
                continue;
            }

            dueDate = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
            break;
        }

        string category;
        while (true)
        {
            Console.Write("Category (default: general): ");
            var input = Console.ReadLine();

            category = string.IsNullOrWhiteSpace(input)
                ? "general"
                : input.Trim();

            if (category.Length > 50)
            {
                Console.WriteLine("Category får vara max 50 tecken.");
                continue;
            }

            break;
        }

        var newToDo = new ToDoItem
        {
            Title = title,
            Description = description,
            Category = category,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow,
            DueDate = dueDate
        };

        await _context.Todos.AddAsync(newToDo);
        var result = await _context.SaveChangesAsync();

        Console.WriteLine(result > 0
            ? $"To do added: {newToDo.Title}"
            : "Failed to add new to do");
    }


    public async Task CompleteTodo()
    {
        try
        {
            Console.Write("Todo ID to mark as complete: ");
            var toDoToUpdate = Console.ReadLine();
            var foundTodo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(toDoToUpdate!));
            if (foundTodo == null)
            {
                Console.WriteLine("to do does not exist");
            }

            foundTodo!.IsComplete = true;
            foundTodo.ComplitedAt = DateTime.UtcNow;
            foundTodo.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task UpdateTodo()
    {
        try
        {
            Console.Write("Todo ID to update: ");
            var toDoToUpdate = Console.ReadLine();
            var foundTodo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(toDoToUpdate!));
            if (foundTodo == null)
            {
                Console.WriteLine("to do does not exist");
            }

            Console.Write("New Title: ");
            var newTitle = Console.ReadLine();
            foundTodo!.Title = newTitle!.Trim();

            Console.Write("New Description: ");
            var newDescription = Console.ReadLine();
            foundTodo.Description = newDescription!.Trim();

            if (foundTodo.IsComplete)
            {
                Console.WriteLine("This todo is completed.");
                Console.Write("Do you want to mark it as NOT completed? (y/n): ");
            }
            else
            {
                Console.WriteLine("This todo is NOT completed.");
                Console.Write("Do you want to mark it as completed? (y/n): ");
            }

            var input = Console.ReadLine();

            if (input?.Trim().ToLower() == "y")
            {
                foundTodo.IsComplete = !foundTodo.IsComplete;
                if (foundTodo.IsComplete)
                {
                    foundTodo.ComplitedAt = DateTime.UtcNow;
                }
                else if (!foundTodo.IsComplete)
                {
                    foundTodo.ComplitedAt = null;
                }


                await _context.SaveChangesAsync();
                Console.WriteLine("Status updated.");
            }
            else
            {
                Console.WriteLine("No changes made.");
            }


            if (foundTodo.IsArchived)
            {
                Console.WriteLine("This todo is archived.");
                Console.Write("Do you want to unarchive it? (y/n): ");
            }
            else
            {
                Console.WriteLine("This todo is NOT archived.");
                Console.Write("Do you want to archive it? (y/n): ");
            }

            var inputArch = Console.ReadLine();

            if (inputArch?.Trim().ToLower() == "y")
            {
                foundTodo.IsArchived = !foundTodo.IsArchived;
                await _context.SaveChangesAsync();
                Console.WriteLine("Archive status updated.");
            }
            else
            {
                Console.WriteLine("No changes made.");
            }

            foundTodo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DeleteTodo()
    {
        try
        {
            Console.Write("Todo ID to delete: ");
            var todoToDelete = Console.ReadLine();
            var foundTodo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(todoToDelete!));
            if (foundTodo == null)
            {
                Console.WriteLine("to do does not exist");
            }

            _context.Todos.Remove(foundTodo!);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                Console.WriteLine("Deleted successfully.");
            }
            else
            {
                Console.WriteLine("failed to delete");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task ArchiveTodo()
    {
        try
        {
            Console.WriteLine("To do ID to archive: ");
            var toDoToArchive = Console.ReadLine();
            var foundToDo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(toDoToArchive!));
            if (foundToDo == null)
            {
                Console.WriteLine("to do does not exist");
            }

            foundToDo!.IsArchived = true;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task GetToDoDetails()
    {
        try
        {
            Console.Write("Todo ID to get details: ");
            var todoId = int.Parse(Console.ReadLine());
            var foundTodo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == todoId);
            if (foundTodo == null)
            {
                Console.WriteLine("to do does not exist");
            }

            Console.WriteLine($"{foundTodo!.Title} - {foundTodo.IsComplete}");
            Console.WriteLine($"{foundTodo.Description}");
            Console.WriteLine($"{foundTodo.DueDate?.ToShortDateString()}");
            Console.WriteLine($"{foundTodo.Category}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}