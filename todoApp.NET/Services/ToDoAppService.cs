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

    public async Task AddTodo()
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

    public async Task CompleteToDo()
    {
        var toDoToUpdate = Console.ReadLine();
        var findtodo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(toDoToUpdate));
        if (findtodo == null)
        {
            Console.WriteLine("to do does not exist");
        }

        findtodo.IsComplete = true;
        findtodo.ComplitedAt = DateTime.UtcNow;
        findtodo.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }


    public async Task DeleteTodo()
    {
        var todoToDelete = Console.ReadLine();
        var findtodo = await _context.Todos.FirstOrDefaultAsync(u => u.Id == int.Parse(todoToDelete));
        if (findtodo == null)
        {
            Console.WriteLine("to do does not exist");
        }

        _context.Todos.Remove(findtodo);
        var result = _context.SaveChanges();
        if (result > 0)
        {
            Console.WriteLine($"to do deleted: {findtodo.Title}");
        }
        else
        {
            Console.WriteLine("failed to delete");
        }
    }
}