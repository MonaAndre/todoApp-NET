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
            Console.WriteLine("6. Avsluta");
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
                    await _toDoAppService.CompleteToDo();
                    break;
                case "4":
// Ta bort en todo
                    await _toDoAppService.DeleteTodo();
                    break;
                case "5":
//
                    break;
                case "6":
                    return;
            }
        }
    }
}