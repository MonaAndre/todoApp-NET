using System.Globalization;
using Microsoft.EntityFrameworkCore;
using todoApp.NET.Data;
using todoApp.NET.Models;

namespace todoApp.NET.Services;

public class ToDoAppService(ToDoAppContext context)
{
    public async Task GetTodos()
    {
        try
        {
            var todolist = await context.Todos.ToListAsync();
            if (todolist.Count == 0)
            {
                Console.WriteLine("To Do list is empty");
            }

            foreach (var todo in todolist)
            {
                Console.WriteLine(todo.Title);
                Console.WriteLine(todo.IsComplete ? "completed" : "not completed");
                Console.WriteLine("-----------------------");
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
        try
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

            await context.Todos.AddAsync(newToDo);
            var result = await context.SaveChangesAsync();

            Console.WriteLine(result > 0
                ? $"To do added: {newToDo.Title}"
                : "Failed to add new to do");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task CompleteTodo()
    {
        try
        {
            Console.Write("Todo ID to mark as complete: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var todoId))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var foundTodo = await context.Todos.FirstOrDefaultAsync(t => t.Id == todoId);

            if (foundTodo == null)
            {
                Console.WriteLine("Todo does not exist.");
                return;
            }

            if (foundTodo.IsComplete)
            {
                Console.WriteLine("Todo is already completed.");
                return;
            }

            foundTodo.IsComplete = true;
            foundTodo.ComplitedAt = DateTime.UtcNow;
            foundTodo.UpdatedAt = DateTime.UtcNow;

            var result = await context.SaveChangesAsync();

            Console.WriteLine(result > 0
                ? "Todo marked as completed."
                : "Nothing was updated.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    public async Task UpdateTodo()
    {
        try
        {
            Console.Write("Todo ID to update: ");
            var toDoToUpdate = Console.ReadLine();
            var foundTodo = await context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(toDoToUpdate!));
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


                await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
                Console.WriteLine("Archive status updated.");
            }
            else
            {
                Console.WriteLine("No changes made.");
            }

            foundTodo.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
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
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var todoId))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var foundTodo = await context.Todos.FirstOrDefaultAsync(u => u.Id == todoId);
            if (foundTodo == null)
            {
                Console.WriteLine("Todo does not exist");
                return;
            }

            context.Todos.Remove(foundTodo!);
            var result = await context.SaveChangesAsync();
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

            var input = Console.ReadLine();
            if (!int.TryParse(input, out var todoId))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var foundToDo = await context.Todos.FirstOrDefaultAsync(u => u.Id == todoId);
            if (foundToDo == null)
            {
                Console.WriteLine("to do does not exist");
                return;
            }

            foundToDo!.IsArchived = true;
            var result = await context.SaveChangesAsync();
            if (result > 0)
            {
                Console.WriteLine("Archive status updated.");
            }
            else
            {
                Console.WriteLine("failed to archive");
            }
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
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var todoId))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var foundTodo = await context.Todos.FirstOrDefaultAsync(u => u.Id == todoId);
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