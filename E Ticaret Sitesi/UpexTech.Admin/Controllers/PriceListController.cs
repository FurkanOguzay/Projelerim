using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class PriceListController : AdminBaseController
    {
        private readonly IPriceListService _priceListService;

        public PriceListController(IPriceListService priceListService)
        {
            _priceListService = priceListService;
        }

        public async Task<IActionResult> Index()
        {
            var priceLists = await _priceListService.GetAllPriceListsAsync();
            return View(priceLists);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var priceLists = await _priceListService.GetAllPriceListsAsync();
            return Json(priceLists.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                basePriceListId = p.BasePriceListId,
                basePriceListName = p.BasePriceList?.Name ?? "-",
                factor = p.Factor,
                rounding = (int)p.Rounding,
                roundingText = GetRoundingText(p.Rounding),
                displayOrder = p.DisplayOrder
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] PriceListDto dto)
        {
            var priceList = new PriceList
            {
                Name = dto.Name,
                BasePriceListId = dto.BasePriceListId,
                Factor = dto.Factor,
                Rounding = (RoundingMethod)dto.Rounding,
                DisplayOrder = dto.DisplayOrder
            };

            await _priceListService.CreatePriceListAsync(priceList);
            return Json(new { success = true, id = priceList.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] PriceListDto dto)
        {
            var priceList = await _priceListService.GetPriceListByIdAsync(dto.Id);
            if (priceList == null)
            {
                return Json(new { success = false, message = "Fiyat listesi bulunamadı." });
            }

            priceList.Name = dto.Name;
            priceList.BasePriceListId = dto.BasePriceListId;
            priceList.Factor = dto.Factor;
            priceList.Rounding = (RoundingMethod)dto.Rounding;
            priceList.DisplayOrder = dto.DisplayOrder;

            await _priceListService.UpdatePriceListAsync(priceList);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _priceListService.DeletePriceListAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAll([FromBody] List<PriceListDto>? dtos)
        {
            try
            {
                if (dtos == null || !dtos.Any())
                {
                    return Json(new { success = false, message = "Kaydedilecek liste bulunamadı." });
                }

                var priceLists = dtos.Select(dto => new PriceList
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    BasePriceListId = dto.BasePriceListId,
                    Factor = dto.Factor,
                    Rounding = (RoundingMethod)dto.Rounding,
                    DisplayOrder = dto.DisplayOrder
                }).ToList();

                await _priceListService.SaveAllPriceListsAsync(priceLists);
                return Json(new { success = true, message = "Değişiklikler kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        private string GetRoundingText(RoundingMethod rounding)
        {
            return rounding switch
            {
                RoundingMethod.None => "Yuvarlama Yok",
                RoundingMethod.Ending90 => "Sonu .90 ile biten",
                RoundingMethod.Ending99 => "Sonu .99 ile biten",
                RoundingMethod.NearestFive => "En Yakın 5 TL",
                _ => rounding.ToString()
            };
        }
    }

    public class PriceListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? BasePriceListId { get; set; }
        public decimal Factor { get; set; } = 1.0m;
        public int Rounding { get; set; }
        public int DisplayOrder { get; set; }
    }
}
