using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize(Roles = Roles.Admin)]
public class FormateursController : Controller
{
    private readonly ApplicationDbContext _context;

    public FormateursController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var formateurs = await _context.Formateurs
            .Include(f => f.Formations)
            .OrderBy(f => f.Nom)
            .ToListAsync();
        return View(formateurs);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var formateur = await _context.Formateurs
            .Include(f => f.Formations)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (formateur is null) return NotFound();
        return View(formateur);
    }

    public IActionResult Create() => View(new Formateur());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Formateur formateur)
    {
        if (!ModelState.IsValid) return View(formateur);

        if (await _context.Formateurs.AnyAsync(f => f.Email == formateur.Email))
        {
            ModelState.AddModelError(nameof(formateur.Email), "Un formateur avec cet email existe déjà.");
            return View(formateur);
        }

        _context.Add(formateur);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Formateur créé avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var formateur = await _context.Formateurs.FindAsync(id);
        if (formateur is null) return NotFound();
        return View(formateur);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Formateur formateur)
    {
        if (id != formateur.Id) return NotFound();
        if (!ModelState.IsValid) return View(formateur);

        try
        {
            _context.Update(formateur);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Formateur mis à jour.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Formateurs.AnyAsync(f => f.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var formateur = await _context.Formateurs
            .Include(f => f.Formations)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (formateur is null) return NotFound();
        return View(formateur);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var formateur = await _context.Formateurs.Include(f => f.Formations).FirstOrDefaultAsync(f => f.Id == id);
        if (formateur is null) return NotFound();

        if (formateur.Formations.Any())
        {
            TempData["Error"] = "Impossible de supprimer : des formations sont rattachées à ce formateur.";
            return RedirectToAction(nameof(Index));
        }

        _context.Formateurs.Remove(formateur);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Formateur supprimé.";
        return RedirectToAction(nameof(Index));
    }
}
