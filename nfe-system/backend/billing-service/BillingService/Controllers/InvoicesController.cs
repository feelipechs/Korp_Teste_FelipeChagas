using BillingService.DTOs;
using BillingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController(InvoiceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await service.GetByIdAsync(id);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvoiceDto dto)
    {
        var invoice = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id}/print")]
    public async Task<IActionResult> Print(int id)
    {
        var (success, error, invoice) = await service.PrintAsync(id);
        return success ? Ok(invoice) : BadRequest(new { message = error });
    }
}