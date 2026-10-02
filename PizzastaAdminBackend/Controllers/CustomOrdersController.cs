using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Controllers
{
    public class CustomOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CustomOrdersController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize]
        public async Task<IActionResult> ListOrders() 
        {
            var data = _context.CustomOrders.ToListAsync();
            return Ok(new
            {
                success = true,
                message = "Orders fetched successfully.",
                data = data.Result
            });
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromForm] CustomOrders customOrder)
        {
            if (customOrder == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please fill the order details."
                });
            }
            
            try
            {
                _context.CustomOrders.Add(customOrder);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Unable to place order. Please try again later."
                });
            }
 

            return Ok(new
            {
                order = customOrder,
                success = true,
                message = "Order placed successfully"
            });
        }
    }
}
