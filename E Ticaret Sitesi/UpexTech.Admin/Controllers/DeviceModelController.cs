using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class DeviceModelController : AdminBaseController
    {
        private readonly IDeviceModelService _deviceModelService;

        public DeviceModelController(IDeviceModelService deviceModelService)
        {
            _deviceModelService = deviceModelService;
        }

        public async Task<IActionResult> Index()
        {
            var modelTree = await _deviceModelService.GetModelTreeAsync();
            return View(modelTree);
        }

        public async Task<IActionResult> Create(int? parentId)
        {
            var model = new DeviceModel { ParentId = parentId };
            
            if (parentId.HasValue)
            {
                var parent = await _deviceModelService.GetByIdAsync(parentId.Value);
                if (parent != null)
                {
                    model.Level = parent.Level + 1;
                    ViewBag.ParentName = parent.Name;
                }
            }
            else
            {
                model.Level = 0;
            }

            await PopulateParentDropdown();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeviceModel model)
        {
            if (ModelState.IsValid)
            {
                await _deviceModelService.CreateAsync(model);
                TempData["SuccessMessage"] = "Cihaz modeli başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            
            await PopulateParentDropdown(model.ParentId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _deviceModelService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            await PopulateParentDropdown(model.ParentId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeviceModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _deviceModelService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Cihaz modeli başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            await PopulateParentDropdown(model.ParentId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deviceModelService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Cihaz modeli başarıyla silindi.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Alt modelleri olan bir cihaz modeli silinemez. Önce alt modelleri silin.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetChildren(int parentId)
        {
            var children = await _deviceModelService.GetChildrenAsync(parentId);
            return Json(children.Select(c => new { id = c.Id, name = c.Name, level = c.Level }));
        }

        private async Task PopulateParentDropdown(int? selectedParentId = null)
        {
            var allModels = await _deviceModelService.GetAllAsync();
            // Sadece Level 0 ve 1'leri parent olarak göster (Marka ve Seri)
            var parents = allModels.Where(m => m.Level < 2).ToList();
            ViewBag.Parents = new SelectList(parents, "Id", "Name", selectedParentId);
        }
    }
}
