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
        try
        {
            Console.Write("Title: ");
            var title = Console.ReadLine();

            Console.Write("Description: ");
            var description = Console.ReadLine();

            Console.Write("Due Date (yyyy-mm-dd): ");
            var input = Console.ReadLine();

            if (!DateTime.TryParse(input, out var parsedDueDate))
            {
                Console.WriteLine("Ogiltigt datum.");
                return;
            }

            var dueDate = DateTime.SpecifyKind(parsedDueDate, DateTimeKind.Utc);


            Console.Write("Category: ");
            var category = string.IsNullOrWhiteSpace(Console.ReadLine())
                ? "general"
                : Console.ReadLine()!.Trim();


            var newToDo = new ToDoItem
            {
                Title = title?.Trim() ?? "",
                Description = description?.Trim() ?? "",
                IsComplete = false,
                DueDate = dueDate,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Todos.AddAsync(newToDo);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                Console.WriteLine($"to do added: {newToDo.Title}");
            }
            else
            {
                Console.WriteLine("failed to add new to do");
            }
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
}