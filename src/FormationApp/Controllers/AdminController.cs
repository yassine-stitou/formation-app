using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.NbFormations = await _context.Formations.CountAsync();
        ViewBag.NbFormationsPubliees = await _context.Formations.CountAsync(f => f.Statut == StatutFormation.Publiee);
        ViewBag.NbFormateurs = await _context.Formateurs.CountAsync();
        ViewBag.NbCategories = await _context.Categories.CountAsync();
        ViewBag.NbApprenants = await _context.Users.CountAsync();
        ViewBag.NbInscriptions = await _context.Inscriptions.CountAsync();

        ViewBag.DernieresInscriptions = await _context.Inscriptions
            .Include(i => i.ApplicationUser)
            .Include(i => i.Formation)
            .OrderByDescending(i => i.DateInscription)
            .Take(8)
            .ToListAsync();

        return View();
    }
}
