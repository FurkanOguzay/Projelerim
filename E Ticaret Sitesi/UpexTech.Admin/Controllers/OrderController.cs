using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class OrderController : AdminBaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index(OrderStatus? status, DateTime? fromDate, DateTime? toDate, string? search, int page = 1)
        {
            var pageSize = 20;
            var (orders, totalCount) = await _orderService.GetOrdersPagedAsync(page, pageSize, status, fromDate, toDate, search);
            
            // Get counts for tabs
            var statusCounts = await _orderService.GetOrderCountByStatusAsync();
            
            ViewBag.StatusCounts = statusCounts;
            ViewBag.TotalCount = totalCount;
            ViewBag.AllCount = statusCounts.Values.Sum();
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentFromDate = fromDate;
            ViewBag.CurrentToDate = toDate;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Status dropdown
            ViewBag.Statuses = GetStatusSelectList(status);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!result)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }

            return Json(new { success = true, message = "Sipariş durumu güncellendi.", newStatus = GetStatusDisplayText(status) });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }

            return Json(new
            {
                success = true,
                order = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    createdAt = order.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    status = order.Status.ToString(),
                    statusText = GetStatusDisplayText(order.Status),
                    totalAmount = order.TotalAmount,
                    customerName = order.User?.FullName ?? "Bilinmiyor",
                    customerEmail = order.User?.Email,
                    customerPhone = order.User?.Phone,
                    customerType = order.User?.Role.ToString() ?? "B2C",
                    shippingAddress = order.ShippingAddress,
                    notes = order.Notes,
                    items = order.OrderItems.Select(oi => new
                    {
                        productName = oi.Product?.Name ?? "Ürün",
                        productImage = oi.Product?.Image,
                        productSku = oi.Product?.SKU,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        totalPrice = oi.TotalPrice
                    })
                }
            });
        }

        private SelectList GetStatusSelectList(OrderStatus? selectedStatus)
        {
            var statuses = new List<object>
            {
                new { Value = "", Text = "Tüm Durumlar" },
                new { Value = ((int)OrderStatus.Pending).ToString(), Text = "Beklemede" },
                new { Value = ((int)OrderStatus.Confirmed).ToString(), Text = "Onaylandı" },
                new { Value = ((int)OrderStatus.Shipped).ToString(), Text = "Kargoda" },
                new { Value = ((int)OrderStatus.Delivered).ToString(), Text = "Teslim Edildi" },
                new { Value = ((int)OrderStatus.Cancelled).ToString(), Text = "İptal Edildi" },
                new { Value = ((int)OrderStatus.Returned).ToString(), Text = "İade Edildi" }
            };

            return new SelectList(statuses, "Value", "Text", selectedStatus?.ToString());
        }

        private string GetStatusDisplayText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Beklemede",
                OrderStatus.Confirmed => "Onaylandı",
                OrderStatus.Shipped => "Kargoda",
                OrderStatus.Delivered => "Teslim Edildi",
                OrderStatus.Cancelled => "İptal Edildi",
                OrderStatus.Returned => "İade Edildi",
                _ => status.ToString()
            };
        }
    }
}
