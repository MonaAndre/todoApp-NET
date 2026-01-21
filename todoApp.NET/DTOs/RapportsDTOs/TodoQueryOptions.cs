using todoApp.NET.Models;

namespace todoApp.NET.DTOs.RapportsDTOs;
public enum SortBy
{
  duedate,
  title,
  createdat
}

public class TodoQueryOptions
{
  public string? Text { get; set; }
  public string? Category { get; set; }
  public bool? IsComplete { get; set; }

  public DateTime? DueDate { get; set; }

  public SortBy SortBy { get; set; } = SortBy.duedate;
  public bool SortDescending { get; set; } = false;
}