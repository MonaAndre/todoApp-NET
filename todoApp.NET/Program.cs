using todoApp.NET.Data;

namespace todoApp.NET;

class Program
{
    static async Task Main(string[] args)
    {
        var context = new ToDoAppContext();
        var menu = new Menu(context);
        await menu.OpenMenu();
    }
}