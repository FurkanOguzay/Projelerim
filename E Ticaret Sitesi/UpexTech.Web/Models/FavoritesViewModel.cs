using UpexTech.Entity;

namespace UpexTech.Web.Models
{
    public class FavoritesViewModel
    {
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();
        public List<FavoriteCollection> Collections { get; set; } = new List<FavoriteCollection>();
        public string ActiveTab { get; set; } = "favorites";
    }
}
