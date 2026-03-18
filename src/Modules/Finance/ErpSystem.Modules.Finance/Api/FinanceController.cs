using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Finance.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FinanceController : ControllerBase
{
    private static readonly List<TransactionDto> _transactions = GenerateMockTransactions();

    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "date",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        var query = _transactions.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(t =>
                t.Reference.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        query = sortBy?.ToLower() switch
        {
            "reference" => sortDirection == "asc" ? query.OrderBy(t => t.Reference) : query.OrderByDescending(t => t.Reference),
            "amount" => sortDirection == "asc" ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
            "type" => sortDirection == "asc" ? query.OrderBy(t => t.Type) : query.OrderByDescending(t => t.Type),
            "status" => sortDirection == "asc" ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
            _ => sortDirection == "asc" ? query.OrderBy(t => t.Date) : query.OrderByDescending(t => t.Date)
        };

        var totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PaginatedResponse<TransactionDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("transactions/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTransaction(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null)
            return NotFound(new { message = "المعاملة غير موجودة" });

        return Ok(transaction);
    }

    [HttpPost("transactions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var newId = _transactions.Max(t => t.Id) + 1;
        var transaction = new TransactionDto
        {
            Id = newId,
            Reference = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{newId:D4}",
            Type = request.Type,
            Category = request.Category,
            Amount = request.Amount,
            Description = request.Description,
            Status = "Pending",
            Date = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _transactions.Add(transaction);
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetFinancialSummary()
    {
        var totalIncome = _transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
        var totalExpenses = _transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
        var pendingPayments = _transactions.Where(t => t.Status == "Pending").Sum(t => t.Amount);

        return Ok(new
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetBalance = totalIncome - totalExpenses,
            PendingPayments = pendingPayments,
            TransactionCount = _transactions.Count
        });
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        var categories = _transactions.Select(t => t.Category).Distinct().ToList();
        return Ok(categories);
    }

    private static List<TransactionDto> GenerateMockTransactions()
    {
        var types = new[] { "Income", "Expense" };
        var statuses = new[] { "Completed", "Pending", "Cancelled" };
        var categories = new[]
        {
            ("Sales", "Income"),
            ("Services", "Income"),
            ("Salaries", "Expense"),
            ("Utilities", "Expense"),
            ("Supplies", "Expense"),
            ("Rent", "Expense"),
            ("Marketing", "Expense"),
            ("Refunds", "Expense")
        };

        var transactions = new List<TransactionDto>();
        var random = new Random(42);

        for (int i = 1; i <= 50; i++)
        {
            var category = categories[random.Next(categories.Length)];
            transactions.Add(new TransactionDto
            {
                Id = i,
                Reference = $"TXN-{DateTime.UtcNow.AddDays(-random.Next(1, 90)):yyyyMMdd}-{i:D4}",
                Type = category.Item2,
                Category = category.Item1,
                Amount = Math.Round((decimal)(random.NextDouble() * 10000 + 100), 2),
                Description = $"معاملة {category.Item1} رقم {i}",
                Status = statuses[random.Next(statuses.Length)],
                Date = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
            });
        }

        return transactions;
    }
}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateTransactionRequest(
    string Type,
    string Category,
    decimal Amount,
    string Description);
