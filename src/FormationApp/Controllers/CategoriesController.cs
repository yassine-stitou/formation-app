using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize(Roles = Roles.Admin)]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Formations)
            .OrderBy(c => c.Nom)
            .ToListAsync();
        return View(categories);
    }

    public IActionResult Create() => View(new Categorie());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categorie categorie)
    {
        if (!ModelState.IsValid) return View(categorie);

        _context.Add(categorie);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Catégorie créée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var categorie = await _context.Categories.FindAsync(id);
        if (categorie is null) return NotFound();
        return View(categorie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categorie categorie)
    {
        if (id != categorie.Id) return NotFound();
        if (!ModelState.IsValid) return View(categorie);

        try
        {
            _context.Update(categorie);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Catégorie mise à jour.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var categorie = await _context.Categories
            .Include(c => c.Formations)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (categorie is null) return NotFound();
        return View(categorie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categorie = await _context.Categories.Include(c => c.Formations).FirstOrDefaultAsync(c => c.Id == id);
        if (categorie is null) return NotFound();

        if (categorie.Formations.Any())
        {
            TempData["Error"] = "Impossible de supprimer : des formations sont rattachées à cette catégorie.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(categorie);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Catégorie supprimée.";
        return RedirectToAction(nameof(Index));
    }
}
