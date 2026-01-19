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
            Console.WriteLine("\n--- Todo-appen ---");
            Console.WriteLine("1. Visa alla todos");
            Console.WriteLine("2. Lägg till todo");
            Console.WriteLine("3. Markera som klar");
            Console.WriteLine("4. Ta bort todo");
            Console.WriteLine("5. Update todo");
            Console.WriteLine("6. Archive todo");
            Console.WriteLine("7. Avsluta");
            Console.Write("Välj: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Lista alla todos
                    await _toDoAppService.GetTodos();

                    break;
                case "2":
                    // Lägg till ny todo
                    await _toDoAppService.AddTodo();

                    break;
                case "3":
                    // Markera en todo som klar
                    await _toDoAppService.CompleteTodo();
                    break;
                case "4":
                    // Ta bort en todo
                    await _toDoAppService.DeleteTodo();
                    break;
                case "5":
                    // Update en todo
                    await _toDoAppService.UpdateTodo();
                    break;
                case "6":
                    // Archive en todo
                    await _toDoAppService.ArchiveTodo();
                    break;
                case "7":
                    return;
            }
        }
    }
}