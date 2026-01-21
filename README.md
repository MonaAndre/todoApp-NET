# todoApp.NET

A simple console-based to-do app built with C#/.NET, Entity Framework Core, and PostgreSQL. The app lets you create, list, update, archive, and export todos, and run reports for filtering, sorting, and summaries.

## Features

- List, create, update, and delete todos.
- Mark todos as complete/incomplete.
- Archive todos and filter archived items out of reports.
- Detail view for a single todo.
- Reports for active todos with filters and sorting.
- Reports for overdue and upcoming deadlines.
- Category summaries and a top list of incomplete todos per category.
- Export all active todos to JSON.

## Tech stack

- .NET (console app)
- Entity Framework Core
- PostgreSQL

## Getting started

### Prerequisites

- .NET SDK installed.
- PostgreSQL database (local or remote).

### Install dependencies

```bash
cd /workspace/todoApp-NET
```

### Configure the database

The app uses a hard-coded connection string in `ToDoAppContext`. Update it to match your environment:

```csharp
options.UseNpgsql("Host=localhost;Database=dbname;Username=username;Password=password");
```

File location: `todoApp.NET/Data/ToDoAppContext.cs`.

### Run the application

```bash
dotnet run --project todoApp.NET/todoApp.NET.csproj
```

## Menu overview

When the program starts, you will see the main menu:

1. List all todos
2. Add new todo
3. Mark a todo as completed
4. Delete todo
5. Update todo
6. Archive todo
7. See todo details
8. Queries/Rapporter
9. Finish

Under **Queries/Rapporter** you can:

- View active todos with filters and sorting (archived items are excluded)
- Show overdue incomplete todos
- Show upcoming deadlines (number of days ahead)
- Category summary
- Category top list
- Export to JSON

## Export

The export feature saves a JSON file in the current directory named:

```
todos_export_YYYYMMDD_HHMMSS.json
```
