using System.ComponentModel.DataAnnotations;

namespace FormationApp.Models;

public class Module
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Formation")]
    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    [Required(ErrorMessage = "Le titre du module est obligatoire.")]
    [StringLength(200)]
    public string Titre { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Ordre")]
    public int Ordre { get; set; } = 1;

    // Résumé fourni à l'avatar IA comme base de connaissance pour ce module
    [Display(Name = "Contexte pour le formateur IA")]
    public string? ContexteIA { get; set; }

    public ICollection<Modalite> Modalites { get; set; } = new List<Modalite>();
}
