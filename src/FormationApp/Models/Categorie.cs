using System.ComponentModel.DataAnnotations;

namespace FormationApp.Models;

public class Categorie
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Formation> Formations { get; set; } = new List<Formation>();
}
