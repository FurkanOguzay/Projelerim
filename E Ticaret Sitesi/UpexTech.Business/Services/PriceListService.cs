using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IPriceListService
    {
        Task<IEnumerable<PriceList>> GetAllPriceListsAsync();
        Task<PriceList?> GetPriceListByIdAsync(int id);
        Task<PriceList> CreatePriceListAsync(PriceList priceList);
        Task UpdatePriceListAsync(PriceList priceList);
        Task DeletePriceListAsync(int id);
        Task SaveAllPriceListsAsync(IEnumerable<PriceList> priceLists);
        
        /// <summary>
        /// Ürün fiyatını PriceList'e göre hesaplar
        /// </summary>
        /// <param name="purchasePrice">Ürünün satın alma fiyatı</param>
        /// <param name="priceList">Müşteriye atanan fiyat listesi (null ise varsayılan fiyat döner)</param>
        /// <returns>Hesaplanmış fiyat</returns>
        decimal CalculatePrice(decimal purchasePrice, PriceList? priceList);
    }

    public class PriceListService : IPriceListService
    {
        private readonly UpexTechDbContext _context;

        public PriceListService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PriceList>> GetAllPriceListsAsync()
        {
            return await _context.PriceLists
                .Include(p => p.BasePriceList)
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PriceList?> GetPriceListByIdAsync(int id)
        {
            return await _context.PriceLists
                .Include(p => p.BasePriceList)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<PriceList> CreatePriceListAsync(PriceList priceList)
        {
            _context.PriceLists.Add(priceList);
            await _context.SaveChangesAsync();
            return priceList;
        }

        public async Task UpdatePriceListAsync(PriceList priceList)
        {
            priceList.UpdatedAt = DateTime.Now;
            _context.PriceLists.Update(priceList);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePriceListAsync(int id)
        {
            var priceList = await _context.PriceLists.FindAsync(id);
            if (priceList != null)
            {
                priceList.IsActive = false;
                priceList.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveAllPriceListsAsync(IEnumerable<PriceList> priceLists)
        {
            foreach (var priceList in priceLists)
            {
                // Yeni kayıt kontrolü - Id 0 veya negatif ise yeni kayıt
                if (priceList.Id <= 0)
                {
                    var newPriceList = new PriceList
                    {
                        Name = priceList.Name,
                        Description = priceList.Description,
                        DisplayOrder = priceList.DisplayOrder,
                        BasePriceListId = priceList.BasePriceListId,
                        Factor = priceList.Factor,
                        Rounding = priceList.Rounding,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.PriceLists.Add(newPriceList);
                }
                else
                {
                    var existing = await _context.PriceLists.FindAsync(priceList.Id);
                    if (existing != null)
                    {
                        existing.Name = priceList.Name;
                        existing.BasePriceListId = priceList.BasePriceListId;
                        existing.Factor = priceList.Factor;
                        existing.Rounding = priceList.Rounding;
                        existing.DisplayOrder = priceList.DisplayOrder;
                        existing.UpdatedAt = DateTime.Now;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ürün fiyatını PriceList'e göre hesaplar
        /// </summary>
        public decimal CalculatePrice(decimal purchasePrice, PriceList? priceList)
        {
            // PriceList yoksa varsayılan fiyatı döndür
            if (priceList == null)
            {
                return purchasePrice;
            }

            // Factor (çarpan) uygula
            decimal calculatedPrice = purchasePrice * priceList.Factor;

            // Rounding (yuvarlama) uygula
            calculatedPrice = ApplyRounding(calculatedPrice, priceList.Rounding);

            return calculatedPrice;
        }

        /// <summary>
        /// Yuvarlama metodunu uygular
        /// </summary>
        private decimal ApplyRounding(decimal price, RoundingMethod rounding)
        {
            return rounding switch
            {
                RoundingMethod.Ending90 => Math.Floor(price) + 0.90m,
                RoundingMethod.Ending99 => Math.Floor(price) + 0.99m,
                RoundingMethod.NearestFive => Math.Round(price / 5) * 5,
                _ => Math.Round(price, 2) // None veya default
            };
        }
    }
}
