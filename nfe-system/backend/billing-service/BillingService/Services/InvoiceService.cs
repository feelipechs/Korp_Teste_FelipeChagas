using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class InvoiceService(AppDbContext db, InventoryClient inventoryClient)
{
    private static InvoiceResponseDto ToResponse(Invoice i) => new(
        i.Id, i.Number, i.Status.ToString(), i.CreatedAt,
        i.Items.Select(ii => new InvoiceItemResponseDto(
            ii.Id, ii.ProductId, ii.ProductCode, ii.ProductDescription, ii.Quantity)).ToList());

    public async Task<List<InvoiceResponseDto>> GetAllAsync() =>
        await db.Invoices.Include(i => i.Items)
            .Select(i => ToResponse(i))
            .ToListAsync();

    public async Task<InvoiceResponseDto?> GetByIdAsync(int id)
    {
        var invoice = await db.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);
        return invoice is null ? null : ToResponse(invoice);
    }

    public async Task<InvoiceResponseDto> CreateAsync(CreateInvoiceDto dto, string? idempotencyKey)
    {
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await db.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.IdempotencyKey == idempotencyKey);

            if (existing is not null)
                return ToResponse(existing);
        }

        var nextNumber = await db.Invoices.AnyAsync()
            ? await db.Invoices.MaxAsync(i => i.Number) + 1
            : 1;

        var invoice = new Invoice
        {
            Number = nextNumber,
            IdempotencyKey = idempotencyKey,
            Items = dto.Items.Select(i => new InvoiceItem
            {
                ProductId = i.ProductId,
                ProductCode = i.ProductCode,
                ProductDescription = i.ProductDescription,
                Quantity = i.Quantity
            }).ToList()
        };

        db.Invoices.Add(invoice);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await db.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.IdempotencyKey == idempotencyKey);

                if (existing is not null)
                    return ToResponse(existing);
            }

            var existingByNumber = await db.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Number == invoice.Number);

            if (existingByNumber is not null)
                return ToResponse(existingByNumber);

            throw;
        }

        return ToResponse(invoice);
    }

    public async Task<(bool Success, string Error, InvoiceResponseDto? Invoice)> PrintAsync(int id)
    {
        var invoice = await db.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
            return (false, "Nota fiscal não encontrada.", null);

        if (invoice.Status != InvoiceStatus.Open)
            return (false, "Apenas notas com status Aberta podem ser impressas.", null);

        // deduz estoque de cada item via inventory-service
        foreach (var item in invoice.Items)
        {
            var (success, error) = await inventoryClient.DeductStockAsync(item.ProductId, item.Quantity);
            if (!success)
                return (false, error, null);
        }

        invoice.Status = InvoiceStatus.Closed;
        await db.SaveChangesAsync();
        return (true, string.Empty, ToResponse(invoice));
    }
}