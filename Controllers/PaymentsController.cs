using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using TiendaApi.Data;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public PaymentsController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("create-checkout-session")]
    public async Task<ActionResult> CreateCheckoutSession()
    {
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == UserId);

        if (cart == null || !cart.Items.Any())
            return BadRequest(new { message = "El carrito esta vacio" });

        foreach (var item in cart.Items)
        {
            if (item.Product!.Stock < item.Quantity)
                return BadRequest(new { message = $"Stock insuficiente para {item.Product.Name}" });
        }

        var lineItems = cart.Items.Select(i => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = (long)(i.Product!.Price * 100),
                Currency = "mxn",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = i.Product.Name,
                    Images = !string.IsNullOrEmpty(i.Product.ImageUrl)
                        ? new List<string> { i.Product.ImageUrl }
                        : null
                }
            },
            Quantity = i.Quantity
        }).ToList();

        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = $"{frontendUrl}/checkout/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{frontendUrl}/cart",
            Metadata = new Dictionary<string, string>
            {
                { "userId", UserId.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return Ok(new { url = session.Url, sessionId = session.Id });
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult> GetSessionStatus(string sessionId)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId);

        if (session.PaymentStatus == "paid")
        {
            await CreateOrderIfNotExists(session);
        }

        return Ok(new
        {
            status = session.PaymentStatus,
            customerEmail = session.CustomerDetails?.Email,
            amountTotal = session.AmountTotal / 100.0,
            currency = session.Currency
        });
    }

    private async Task CreateOrderIfNotExists(Session session)
    {
        if (!session.Metadata.TryGetValue("userId", out var userIdStr)
            || !int.TryParse(userIdStr, out var userId))
            return;

        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any()) return; // ya se proceso (carrito vacio)

        var order = new Models.Order
        {
            UserId = userId,
            ShippingAddress = "Pago con tarjeta (Stripe)",
            Status = "processing",
            Total = cart.Items.Sum(i => i.Product!.Price * i.Quantity),
            Items = cart.Items.Select(i => new Models.OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Product!.Price
            }).ToList()
        };

        foreach (var item in cart.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product != null) product.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
    }
}
