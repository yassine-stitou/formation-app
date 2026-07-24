using System.ComponentModel.DataAnnotations;

namespace FormationApp.Models;

public class Modalite
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Module")]
    public int ModuleId { get; set; }
    public Module? Module { get; set; }

    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Type")]
    public TypeModalite Type { get; set; } = TypeModalite.Cours;

    [Display(Name = "Ordre")]
    public int Ordre { get; set; } = 1;

    // Pour un Cours : texte / support ; pour un Exercice : consigne
    [Display(Name = "Contenu / Consigne")]
    public string? Contenu { get; set; }

    [Display(Name = "Vidéo (URL)")]
    [StringLength(300)]
    public string? VideoUrl { get; set; }

    [Display(Name = "Durée estimée (minutes)")]
    [Range(0, 600)]
    public int DureeMinutes { get; set; } = 15;

    // Pour un Examen / Quiz
    [Display(Name = "Score minimum de réussite (%)")]
    [Range(0, 100)]
    public int ScoreMinReussite { get; set; } = 60;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ResultatExamen> Resultats { get; set; } = new List<ResultatExamen>();
}

public class Question
{
    public int Id { get; set; }

    [Required]
    public int ModaliteId { get; set; }
    public Modalite? Modalite { get; set; }

    [Required(ErrorMessage = "L'énoncé est obligatoire.")]
    [StringLength(1000)]
    [Display(Name = "Énoncé")]
    public string Enonce { get; set; } = string.Empty;

    public int Ordre { get; set; } = 1;

    public ICollection<OptionReponse> Options { get; set; } = new List<OptionReponse>();
}

public class OptionReponse
{
    public int Id { get; set; }

    [Required]
    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    [Required(ErrorMessage = "Le texte de l'option est obligatoire.")]
    [StringLength(500)]
    [Display(Name = "Texte")]
    public string Texte { get; set; } = string.Empty;

    [Display(Name = "Correcte")]
    public bool EstCorrecte { get; set; }
}
