using jekirdekCase.Data;
using jekirdekCase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace jekirdekCase.Controllers
{
    [Route("api/customers")]
    [ApiController]
    [Authorize] // Yetkilendirme gereklidir
    public class CustomerController : ControllerBase
    {
        private readonly CRMDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CRMDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ DataTables Uyumlu Müşteri Listesi
        [HttpGet]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int draw = 1, 
            [FromQuery] int start = 0, 
            [FromQuery] int length = 10,
            [FromQuery] string searchValue = "",
            [FromQuery] string orderColumn = "FirstName",
            [FromQuery] string orderDirection = "asc")
        {
            try
            {
                var query = _context.Customers.AsQueryable();

                // ✅ Desteklenen sıralama sütunları (CASE-SENSITIVE kontrolü)
                var columnMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "firstName", "FirstName" },
                    { "lastName", "LastName" },
                    { "email", "Email" },
                    { "region", "Region" },
                    { "registrationDate", "RegistrationDate" }
                };

                if (!columnMappings.TryGetValue(orderColumn, out string actualColumn))
                {
                    actualColumn = "FirstName"; // Varsayılan sıralama
                }

                // 🔍 Arama (Büyük/Küçük Harf Duyarsız)
                if (!string.IsNullOrEmpty(searchValue))
                {
                    var lowerSearch = searchValue.ToLower();
                    query = query.Where(c =>
                        EF.Functions.Like(c.FirstName.ToLower(), $"%{lowerSearch}%") ||
                        EF.Functions.Like(c.LastName.ToLower(), $"%{lowerSearch}%") ||
                        EF.Functions.Like(c.Email.ToLower(), $"%{lowerSearch}%")
                    );
                }

                // 📊 Güvenli Sıralama
                query = orderDirection.ToLower() == "asc"
                    ? query.OrderBy(c => EF.Property<object>(c, actualColumn))
                    : query.OrderByDescending(c => EF.Property<object>(c, actualColumn));

                // 📌 Toplam Kayıt Sayısı
                int totalRecords = await _context.Customers.CountAsync();
                int filteredRecords = await query.CountAsync();

                // ⏳ Sayfalama
                var customers = await query
                    .Skip(start)
                    .Take(length)
                    .ToListAsync();

                // 🔥 DataTables formatına uygun dönüş
                return Ok(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = customers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Müşteri verileri getirilirken hata oluştu: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // ✅ Yeni Müşteri Ekleme (POST)
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null)
                return BadRequest("Geçersiz müşteri verisi.");

            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Müşteri başarıyla eklendi", customer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Müşteri eklenirken hata oluştu: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // ✅ Müşteri Silme (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound("Müşteri bulunamadı.");

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Müşteri başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Müşteri silinirken hata oluştu: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }
    }
}
