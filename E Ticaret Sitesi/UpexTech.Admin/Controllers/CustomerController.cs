using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class CustomerController : AdminBaseController
    {
        private readonly IUserService _userService;
        private readonly IPriceListService _priceListService;

        public CustomerController(IUserService userService, IPriceListService priceListService)
        {
            _userService = userService;
            _priceListService = priceListService;
        }

        public async Task<IActionResult> Index(string? searchQuery = null, string? filter = null, string? city = null, int page = 1, int pageSize = 10)
        {
            // Get all customers first
            var allCustomers = await _userService.GetAllCustomersAsync();
            var customers = allCustomers.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var query = searchQuery.ToLower();
                customers = customers.Where(c =>
                    c.FullName.ToLower().Contains(query) ||
                    (c.Phone != null && c.Phone.Contains(query)) ||
                    c.Email.ToLower().Contains(query) ||
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(query)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(query)));
            }

            // Apply type filter
            switch (filter?.ToLower())
            {
                case "b2b":
                    customers = customers.Where(c => c.Role == UserRole.B2B);
                    break;
                case "b2c":
                    customers = customers.Where(c => c.Role == UserRole.B2C);
                    break;
                case "active":
                    customers = customers.Where(c => c.Status == UserStatus.Active);
                    break;
            }

            // Apply city filter
            if (!string.IsNullOrWhiteSpace(city))
            {
                customers = customers.Where(c => c.Address != null && c.Address.Contains(city));
            }

            // Get total count before pagination
            var totalCount = customers.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            var pagedCustomers = customers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get unique cities from addresses for filter dropdown
            var cities = allCustomers
                .Where(c => !string.IsNullOrEmpty(c.Address))
                .Select(c => ExtractCity(c.Address!))
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Get price lists from database
            var priceLists = await _priceListService.GetAllPriceListsAsync();

            // Set ViewBag values
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentCity = city;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.Cities = cities;
            ViewBag.B2BCount = allCustomers.Count(c => c.Role == UserRole.B2B);
            ViewBag.B2CCount = allCustomers.Count(c => c.Role == UserRole.B2C);
            ViewBag.PriceLists = priceLists.ToList();

            return View(pagedCustomers);
        }

        private string? ExtractCity(string address)
        {
            // Try to extract city from address (simple implementation)
            // Expected format: "..., City" or just city name
            if (string.IsNullOrEmpty(address)) return null;
            
            var parts = address.Split(',');
            if (parts.Length >= 2)
            {
                return parts[^1].Trim(); // Last part is usually city
            }
            return address.Trim();
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _userService.GetCustomerWithOrdersAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTier(int id, CustomerTier tier)
        {
            await _userService.UpdateCustomerTierAsync(id, tier);
            TempData["SuccessMessage"] = "Müşteri grubu güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCreditLimit(int id, decimal limit)
        {
            await _userService.UpdateCreditLimitAsync(id, limit);
            TempData["SuccessMessage"] = "Kredi limiti güncellendi.";
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult ExportCustomers()
        {
            // TODO: Implement CSV/Excel export
            TempData["InfoMessage"] = "Dışa aktarım özelliği yakında eklenecek.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePriceList(int customerId, int? priceListId)
        {
            if (priceListId.HasValue && priceListId.Value > 0)
            {
                var priceList = await _priceListService.GetPriceListByIdAsync(priceListId.Value);
                if (priceList != null)
                {
                    await _userService.UpdatePriceListAsync(customerId, priceList.Id, priceList.Name);
                    TempData["SuccessMessage"] = $"Fiyat listesi '{priceList.Name}' olarak güncellendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Fiyat listesi bulunamadı.";
                }
            }
            else
            {
                // Clear price list
                await _userService.UpdatePriceListAsync(customerId, null, null);
                TempData["SuccessMessage"] = "Fiyat listesi kaldırıldı.";
            }

            return RedirectToAction("Index");
        }
    }
}

