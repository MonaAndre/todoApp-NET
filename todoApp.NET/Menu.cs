using todoApp.NET.Data;
using todoApp.NET.Services;

namespace todoApp.NET;

public class Menu
{
    private readonly ToDoAppService _toDoAppService;

    public Menu(ToDoAppContext context)
    {
        _toDoAppService = new ToDoAppService(context);
    }

    public async Task OpenMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Todo-application ---");
            Console.WriteLine("1. List all todos");
            Console.WriteLine("2. Add new todo");
            Console.WriteLine("3. Mark a todo as completed");
            Console.WriteLine("4. Ta bort todo");
            Console.WriteLine("5. Update todo");
            Console.WriteLine("6. Archive todo");
            Console.WriteLine("7. See todo details");
            Console.WriteLine("8. Finish");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // List all
                    await _toDoAppService.GetTodos();

                    break;
                case "2":
                    // Add new
                    await _toDoAppService.AddTodo();

                    break;
                case "3":
                    // Mark as done
                    await _toDoAppService.CompleteTodo();
                    break;
                case "4":
                    // Delete 
                    await _toDoAppService.DeleteTodo();
                    break;
                case "5":
                    // Update 
                    await _toDoAppService.UpdateTodo();
                    break;
                case "6":
                    // Archive
                    await _toDoAppService.ArchiveTodo();
                    break;
                case "7":
                    // Get details
                    await _toDoAppService.GetToDoDetails();
                    break;
                case "8":
                    return;
            }
        }
    }
}