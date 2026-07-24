using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var formations = await _context.Formations
            .Include(f => f.Categorie)
            .Include(f => f.Formateur)
            .Where(f => f.Statut == StatutFormation.Publiee)
            .OrderByDescending(f => f.DateCreation)
            .Take(6)
            .ToListAsync();

        return View(formations);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
