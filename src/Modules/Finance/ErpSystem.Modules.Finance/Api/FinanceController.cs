using Asp.Versioning;
using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Finance.Domain.Entities;
using ErpSystem.Modules.Finance.Domain.ValueObjects;
using ErpSystem.Modules.Finance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Finance.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly FinanceDbContext _context;

    public FinanceController(FinanceDbContext context)
    {
        _context = context;
    }

    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "date",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        var query = _context.Transactions.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(t =>
                t.Reference.Contains(searchTerm) ||
                t.Description.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type.Code == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status.Code == status);
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "reference" => sortDirection == "asc" ? query.OrderBy(t => t.Reference) : query.OrderByDescending(t => t.Reference),
            "amount" => sortDirection == "asc" ? query.OrderBy(t => t.Amount.Amount) : query.OrderByDescending(t => t.Amount.Amount),
            "type" => sortDirection == "asc" ? query.OrderBy(t => t.Type.Code) : query.OrderByDescending(t => t.Type.Code),
            "status" => sortDirection == "asc" ? query.OrderBy(t => t.Status.Code) : query.OrderByDescending(t => t.Status.Code),
            _ => sortDirection == "asc" ? query.OrderBy(t => t.TransactionDate) : query.OrderByDescending(t => t.TransactionDate)
        };

        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var data = transactions.Select(t => new TransactionDto
        {
            Id = t.Id.ToString(),
            Reference = t.Reference,
            Type = t.Type.Code,
            Category = t.Category,
            Amount = t.Amount.Amount,
            Currency = t.Amount.Currency,
            Description = t.Description,
            Status = t.Status.Code,
            Date = t.TransactionDate,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<TransactionDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("transactions/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(string id)
    {
        if (!Guid.TryParse(id, out var transactionId))
            return NotFound(new { message = "المعاملة غير موجودة" });

        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null)
            return NotFound(new { message = "المعاملة غير موجودة" });

        return Ok(new TransactionDto
        {
            Id = transaction.Id.ToString(),
            Reference = transaction.Reference,
            Type = transaction.Type.Code,
            Category = transaction.Category,
            Amount = transaction.Amount.Amount,
            Currency = transaction.Amount.Currency,
            Description = transaction.Description,
            Status = transaction.Status.Code,
            Date = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        });
    }

    [HttpPost("transactions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var transactionType = TransactionType.FromCode(request.Type);
        if (transactionType == null)
            return BadRequest(new { message = "نوع المعاملة غير صالح" });

        var amount = Money.Create(request.Amount, request.Currency ?? "SAR");

        var transaction = Transaction.Create(
            transactionType,
            request.Category,
            amount,
            request.Description,
            request.Date);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, new TransactionDto
        {
            Id = transaction.Id.ToString(),
            Reference = transaction.Reference,
            Type = transaction.Type.Code,
            Category = transaction.Category,
            Amount = transaction.Amount.Amount,
            Currency = transaction.Amount.Currency,
            Description = transaction.Description,
            Status = transaction.Status.Code,
            Date = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        });
    }

    [HttpPatch("transactions/{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTransaction(string id)
    {
        if (!Guid.TryParse(id, out var transactionId))
            return NotFound(new { message = "المعاملة غير موجودة" });

        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null)
            return NotFound(new { message = "المعاملة غير موجودة" });

        try
        {
            transaction.Complete();
            await _context.SaveChangesAsync();

            return Ok(new { id = transaction.Id, status = transaction.Status.Code });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("transactions/{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelTransaction(string id, [FromBody] CancelTransactionRequest request)
    {
        if (!Guid.TryParse(id, out var transactionId))
            return NotFound(new { message = "المعاملة غير موجودة" });

        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null)
            return NotFound(new { message = "المعاملة غير موجودة" });

        try
        {
            transaction.Cancel(request.Reason ?? "تم الإلغاء بواسطة المستخدم");
            await _context.SaveChangesAsync();

            return Ok(new { id = transaction.Id, status = transaction.Status.Code });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialSummary()
    {
        var transactions = await _context.Transactions.ToListAsync();

        var totalIncome = transactions
            .Where(t => t.Type.Code == TransactionType.Income.Code)
            .Sum(t => t.Amount.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type.Code == TransactionType.Expense.Code)
            .Sum(t => t.Amount.Amount);

        var pendingPayments = transactions
            .Where(t => t.Status.Code == TransactionStatus.Pending.Code)
            .Sum(t => t.Amount.Amount);

        return Ok(new
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetBalance = totalIncome - totalExpenses,
            PendingPayments = pendingPayments,
            TransactionCount = transactions.Count
        });
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Transactions
            .Select(t => t.Category)
            .Distinct()
            .ToListAsync();

        return Ok(categories);
    }

    [HttpDelete("transactions/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransaction(string id)
    {
        if (!Guid.TryParse(id, out var transactionId))
            return NotFound(new { message = "المعاملة غير موجودة" });

        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null)
            return NotFound(new { message = "المعاملة غير موجودة" });

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
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
    public string Id { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateTransactionRequest(
    string Type,
    string Category,
    decimal Amount,
    string Description,
    string? Currency = "SAR",
    DateTime? Date = null);

public record CancelTransactionRequest(string? Reason = null);
