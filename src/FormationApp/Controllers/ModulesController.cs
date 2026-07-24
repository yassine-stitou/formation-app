using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize(Roles = Roles.Admin)]
public class ModulesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ModulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int formationId)
    {
        var formation = await _context.Formations
            .Include(f => f.Modules.OrderBy(m => m.Ordre))
            .ThenInclude(m => m.Modalites)
            .FirstOrDefaultAsync(f => f.Id == formationId);

        if (formation is null) return NotFound();
        return View(formation);
    }

    public async Task<IActionResult> Create(int formationId)
    {
        var formation = await _context.Formations.FindAsync(formationId);
        if (formation is null) return NotFound();

        var prochainOrdre = await _context.Modules.Where(m => m.FormationId == formationId).CountAsync() + 1;

        ViewBag.Formation = formation;
        return View(new Module { FormationId = formationId, Ordre = prochainOrdre });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Module module)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Formation = await _context.Formations.FindAsync(module.FormationId);
            return View(module);
        }

        _context.Add(module);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Module ajouté avec succès.";
        return RedirectToAction(nameof(Index), new { formationId = module.FormationId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var module = await _context.Modules.Include(m => m.Formation).FirstOrDefaultAsync(m => m.Id == id);
        if (module is null) return NotFound();
        ViewBag.Formation = module.Formation;
        return View(module);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Module module)
    {
        if (id != module.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Formation = await _context.Formations.FindAsync(module.FormationId);
            return View(module);
        }

        try
        {
            _context.Update(module);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Module mis à jour.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Modules.AnyAsync(m => m.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index), new { formationId = module.FormationId });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var module = await _context.Modules.Include(m => m.Formation).Include(m => m.Modalites).FirstOrDefaultAsync(m => m.Id == id);
        if (module is null) return NotFound();
        return View(module);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module is null) return NotFound();
        var formationId = module.FormationId;

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Module supprimé.";
        return RedirectToAction(nameof(Index), new { formationId });
    }
}
