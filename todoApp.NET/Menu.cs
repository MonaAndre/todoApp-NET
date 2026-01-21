using todoApp.NET.Data;
using todoApp.NET.DTOs.RapportsDTOs;
using todoApp.NET.Services;

namespace todoApp.NET;

public class Menu
{
    private readonly ToDoAppService _toDoAppService;
    private readonly RapportsService _rapportsService;

    public Menu(ToDoAppContext context)
    {
        _toDoAppService = new ToDoAppService(context);
        _rapportsService = new RapportsService(context);
    }

    public async Task OpenMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Todo-application ---");
            Console.WriteLine("1. List all todos");
            Console.WriteLine("2. Add new todo");
            Console.WriteLine("3. Mark a todo as completed");
            Console.WriteLine("4. Delete todo");
            Console.WriteLine("5. Update todo");
            Console.WriteLine("6. Archive todo");
            Console.WriteLine("7. See todo details");
            Console.WriteLine("8. Queries/Rapporter");
            Console.WriteLine("9.Finish");
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
                    // öppna undermenu för rapports
                    await OpenRapportMenu();
                    break;
                case "9":
                    return;
            }
        }
    }

    private async Task OpenRapportMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Rapports menu ---");
            Console.WriteLine(" Filtering and sorting todos");
            Console.WriteLine("------------------------------");
            Console.WriteLine("1.Aktiv vy (filter + sortering)");
            Console.WriteLine("2. Show not completed late todos");
            Console.WriteLine("3. Show todos with upcoming deadlines");
            Console.WriteLine(" Summary");
            Console.WriteLine("------------------------------");
            Console.WriteLine("4. Summary for each category");
            Console.WriteLine("5. Category top-list");

            Console.WriteLine("6. Tillbaka ");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    //Aktiv vy (filter + sortering)
                    var res = BuildTodoQueryOptions();
                    await _rapportsService.ShowActiveTodosWithFilters(res);
                    break;
                case "2":
                    await _rapportsService.ShowLateTodos();
                    break;
                case "3":
                    Console.WriteLine("Deadline in how many days from now?");
                    var daysAhead = int.Parse(Console.ReadLine());
                    await _rapportsService.ShowUpcomingDeadlines(daysAhead);
                    break;
                case "4":
                    await _rapportsService.ShowSummaryForCategories();
                    break;
                case "5":
                    Console.WriteLine("Chose limit");
                    var limit = int.Parse(Console.ReadLine());
                    await _rapportsService.ShowTopListCat(limit);
                    break;
                case "6":
                    return;
            }
        }
    }

    private static TodoQueryOptions BuildTodoQueryOptions()
    {
        Console.Write("Text filter (optional): ");
        var text = (Console.ReadLine() ?? string.Empty).Trim();

        Console.Write("Category filter (optional): ");
        var category = (Console.ReadLine() ?? string.Empty).Trim();

        bool? isComplete = null;
        while (true)
        {
            Console.Write("Status (a=all, c=complete, i=incomplete): ");
            var statusInput = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

            if (statusInput.Length == 0 || statusInput == "a")
            {
                isComplete = null;
                break;
            }

            if (statusInput == "c")
            {
                isComplete = true;
                break;
            }

            if (statusInput == "i")
            {
                isComplete = false;
                break;
            }

            Console.WriteLine("Ogiltigt val. Ange a, c eller i.");
        }

        DateTime? dueDate = null;
        while (true)
        {
            Console.Write("Deadline (yyyy-MM-dd, optional): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();

            if (input.Length == 0)
            {
                dueDate = null;
                break;
            }

            if (!DateTime.TryParseExact(input, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None,
                    out var parsed))
            {
                Console.WriteLine("Ogiltigt datum. Använd format yyyy-MM-dd.");
                continue;
            }

            dueDate = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
            break;
        }

        SortBy sortBy;
        while (true)
        {
            Console.Write("Sort by (duedate/title/createdat): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

            if (input.Length == 0 || input == "duedate")
            {
                sortBy = SortBy.duedate;
                break;
            }

            if (input == "title")
            {
                sortBy = SortBy.title;
                break;
            }

            if (input == "createdAt")
            {
                sortBy = SortBy.createdat;
                break;
            }

            Console.WriteLine("Ogiltigt val. Ange duedate, title eller created.");
        }

        bool sortDescending = false;
        while (true)
        {
            Console.Write("Sort descending? (y/n): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

            if (input == "y")
            {
                sortDescending = true;
                break;
            }

            if (input == "n" || input.Length == 0)
            {
                sortDescending = false;
                break;
            }

            Console.WriteLine("Ogiltigt val. Ange y eller n.");
        }

        return new TodoQueryOptions
        {
            Text = text.Length == 0 ? null : text,
            Category = category.Length == 0 ? null : category,
            IsComplete = isComplete,
            DueDate = dueDate,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
    }
}