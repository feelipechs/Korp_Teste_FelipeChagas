namespace InventoryService.DTOs;

public record CreateProductDto(string Code, string Description, int Balance);
public record UpdateProductDto(string Description, int Balance);
public record ProductResponseDto(int Id, string Code, string Description, int Balance);
public record DeductStockDto(int ProductId, int Quantity);