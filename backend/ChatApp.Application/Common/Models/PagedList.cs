namespace ChatApp.Application.Common.Models;

public record PagedList<T>(IEnumerable<T> Items, int PageNumber, int TotalCount);