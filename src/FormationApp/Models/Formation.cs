using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormationApp.Models;

public class Formation
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(200)]
    public string Titre { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Catégorie")]
    public int CategorieId { get; set; }
    public Categorie? Categorie { get; set; }

    [Required]
    [Display(Name = "Formateur")]
    public int FormateurId { get; set; }
    public Formateur? Formateur { get; set; }

    [Display(Name = "Image (URL)")]
    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Date de début")]
    [DataType(DataType.Date)]
    public DateTime? DateDebut { get; set; }

    [Display(Name = "Date de fin")]
    [DataType(DataType.Date)]
    public DateTime? DateFin { get; set; }

    [Display(Name = "Places disponibles")]
    [Range(0, 10000)]
    public int PlacesDisponibles { get; set; } = 30;

    public StatutFormation Statut { get; set; } = StatutFormation.Brouillon;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();

    [NotMapped]
    public int NombreInscrits => Inscriptions?.Count(i => i.Statut != StatutInscription.Annulee) ?? 0;
}
