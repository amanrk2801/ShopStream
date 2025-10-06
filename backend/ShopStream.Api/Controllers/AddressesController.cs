using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopStream.Data;
using System.Security.Claims;

namespace ShopStream.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AddressesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAddresses()
    {
        var userId = GetUserId();
        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new
            {
                a.Id,
                a.Street,
                a.City,
                a.State,
                a.ZipCode,
                a.Country,
                a.IsDefault
            })
            .ToListAsync();

        return Ok(addresses);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
