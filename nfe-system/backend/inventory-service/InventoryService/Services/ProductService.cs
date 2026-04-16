using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services;

public class ProductService(AppDbContext db)
{
    public async Task<List<ProductResponseDto>> GetAllAsync() =>
        await db.Products
            .Select(p => new ProductResponseDto(p.Id, p.Code, p.Description, p.Balance))
            .ToListAsync();

    public async Task<ProductResponseDto?> GetByIdAsync(int id) =>
        await db.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto(p.Id, p.Code, p.Description, p.Balance))
            .FirstOrDefaultAsync();

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Code = dto.Code,
            Description = dto.Description,
            Balance = dto.Balance
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return new ProductResponseDto(product.Id, product.Code, product.Description, product.Balance);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return null;

        product.Description = dto.Description;
        product.Balance = dto.Balance;
        await db.SaveChangesAsync();
        return new ProductResponseDto(product.Id, product.Code, product.Description, product.Balance);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string Error)> DeductStockAsync(int productId, int quantity)
    {
        // lock pessimista para tratar concorrência (opcional do teste)
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var product = await db.Products
                .FromSqlRaw("SELECT * FROM \"Products\" WHERE \"Id\" = {0} FOR UPDATE", productId)
                .FirstOrDefaultAsync();

            if (product is null)
                return (false, "Produto não encontrado.");

            if (product.Balance < quantity)
                return (false, $"Saldo insuficiente. Disponível: {product.Balance}");

            product.Balance -= quantity;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, string.Empty);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}