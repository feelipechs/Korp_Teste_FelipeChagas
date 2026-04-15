namespace BillingService.DTOs;

public record CreateInvoiceItemDto(int ProductId, string ProductCode, string ProductDescription, int Quantity);
public record CreateInvoiceDto(List<CreateInvoiceItemDto> Items);

public record InvoiceItemResponseDto(int Id, int ProductId, string ProductCode, string ProductDescription, int Quantity);
public record InvoiceResponseDto(int Id, int Number, string Status, DateTime CreatedAt, List<InvoiceItemResponseDto> Items);