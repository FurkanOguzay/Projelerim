using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public abstract class AdminBaseController : Controller
    {
    }
}
