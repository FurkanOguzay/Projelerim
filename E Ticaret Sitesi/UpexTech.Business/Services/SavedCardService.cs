using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface ISavedCardService
    {
        Task<IEnumerable<SavedCard>> GetUserCardsAsync(int userId);
        Task<SavedCard?> GetCardByIdAsync(int cardId);
        Task<SavedCard> AddCardAsync(int userId, string cardNumber, string cardHolderName, string expiryMonth, string expiryYear, string cardType);
        Task<bool> DeleteCardAsync(int cardId, int userId);
        Task<bool> SetDefaultCardAsync(int cardId, int userId);
    }

    public class SavedCardService : ISavedCardService
    {
        private readonly UpexTechDbContext _context;

        public SavedCardService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SavedCard>> GetUserCardsAsync(int userId)
        {
            return await _context.SavedCards
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.IsDefault)
                .ThenByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<SavedCard?> GetCardByIdAsync(int cardId)
        {
            return await _context.SavedCards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.IsActive);
        }

        public async Task<SavedCard> AddCardAsync(int userId, string cardNumber, string cardHolderName, string expiryMonth, string expiryYear, string cardType)
        {
            // Kart numarasından sadece son 4 haneyi sakla
            var last4Digits = cardNumber.Replace(" ", "").Replace("-", "");
            if (last4Digits.Length >= 4)
            {
                last4Digits = last4Digits.Substring(last4Digits.Length - 4);
            }
            var maskedNumber = $"**** **** **** {last4Digits}";

            // İlk kart ise varsayılan yap
            var hasCards = await _context.SavedCards.AnyAsync(c => c.UserId == userId && c.IsActive);

            var card = new SavedCard
            {
                UserId = userId,
                CardNumber = maskedNumber,
                CardHolderName = cardHolderName.ToUpperInvariant(),
                ExpiryMonth = expiryMonth.PadLeft(2, '0'),
                ExpiryYear = expiryYear,
                CardType = cardType,
                IsDefault = !hasCards,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            await _context.SavedCards.AddAsync(card);
            await _context.SaveChangesAsync();

            return card;
        }

        public async Task<bool> DeleteCardAsync(int cardId, int userId)
        {
            var card = await _context.SavedCards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && c.IsActive);

            if (card == null) return false;

            card.IsActive = false;
            card.UpdatedAt = DateTime.Now;

            // Silinen kart varsayılansa, başka bir kartı varsayılan yap
            if (card.IsDefault)
            {
                var anotherCard = await _context.SavedCards
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive && c.Id != cardId);
                if (anotherCard != null)
                {
                    anotherCard.IsDefault = true;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultCardAsync(int cardId, int userId)
        {
            var card = await _context.SavedCards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && c.IsActive);

            if (card == null) return false;

            // Önce tüm kartların varsayılan durumunu kaldır
            var allCards = await _context.SavedCards
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync();

            foreach (var c in allCards)
            {
                c.IsDefault = c.Id == cardId;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
