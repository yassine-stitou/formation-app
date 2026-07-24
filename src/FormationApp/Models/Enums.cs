namespace FormationApp.Models;

public enum TypeModalite
{
    Cours = 0,
    Exercice = 1,
    Examen = 2
}

public enum StatutFormation
{
    Brouillon = 0,
    Publiee = 1,
    Archivee = 2
}

public enum StatutInscription
{
    EnAttente = 0,
    Active = 1,
    Terminee = 2,
    Annulee = 3
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Formateur = "Formateur";
    public const string Apprenant = "Apprenant";
}
