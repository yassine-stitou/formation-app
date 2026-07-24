# FormationApp

Application web de gestion des formations avec **formateur IA** (avatar interactif), développée en **ASP.NET Core (.NET 10)** + **Entity Framework Core** + **SQLite**.

## Sommaire

- [Fonctionnalités](#fonctionnalités)
- [Architecture technique](#architecture-technique)
- [Installation locale](#installation-locale)
- [Configuration du formateur IA (HeyGen / Anam.ai)](#configuration-du-formateur-ia-heygen--anamai)
- [Comptes de démonstration](#comptes-de-démonstration)
- [Où héberger gratuitement en 2026](#où-héberger-gratuitement-en-2026)
- [Déploiement sur GitHub + Render](#déploiement-sur-github--render)
- [Structure du projet](#structure-du-projet)
- [Limites connues et pistes d'amélioration](#limites-connues-et-pistes-damélioration)

## Fonctionnalités

- **Authentification / Inscription** : ASP.NET Core Identity, rôles Admin / Formateur / Apprenant
- **Interface d'administration** : tableau de bord avec statistiques
- **Gestion des catégories, formateurs, formations, modules**
- **Gestion des modalités** : Cours (texte/vidéo), Exercices, Examens/Quiz (QCM notés automatiquement)
- **Inscription des apprenants** avec suivi de progression
- **Formateur IA** : avatar intégré (HeyGen ou Anam.ai) qui présente le module en cours et répond à l'oral aux questions de l'apprenant, à partir du contenu pédagogique du module

## Architecture technique

| Composant | Technologie |
|---|---|
| Backend | ASP.NET Core MVC (.NET 10) |
| Accès aux données | Entity Framework Core 10 (**SQLite** — fichier unique, aucun serveur à installer) |
| Authentification | ASP.NET Core Identity |
| Frontend | Razor Views + Bootstrap 5 (CDN) |
| Formateur IA | HeyGen Streaming Avatar SDK **ou** Anam.ai JS SDK (au choix, configurable) |
| Conteneurisation | Docker (multi-stage) |
| Déploiement | Render (free tier) — ou tout hébergeur supportant Docker |

Le jeton de session de l'avatar IA est **toujours généré côté serveur** (`AvatarController` → `AvatarService`) : la clé API du fournisseur n'est jamais transmise au navigateur.

> **Pourquoi SQLite plutôt que SQL Server ?** SQLite stocke toute la base dans un simple fichier (`formationapp.db`), sans processus serveur, sans mot de passe, sans configuration réseau. C'est le choix le plus simple pour développer en local et pour un hébergement gratuit léger. Si vous avez besoin de montée en charge en production, EF Core permet de basculer vers PostgreSQL ou SQL Server en changeant uniquement le paquet NuGet et l'appel `UseSqlite(...)` → `UseNpgsql(...)` / `UseSqlServer(...)` dans `Program.cs`.

## Installation locale

### Prérequis

- [.NET SDK 10](https://dotnet.microsoft.com/download) uniquement — **aucune base de données à installer**.

### Étapes

```bash
# 1. Cloner le dépôt
git clone <url-de-votre-repo>
cd FormationApp

# 2. Restaurer les paquets NuGet
dotnet restore src/FormationApp/FormationApp.csproj

# 3. Installer l'outil EF Core (une seule fois sur votre machine)
dotnet tool install --global dotnet-ef

# 4. Créer la migration initiale et la base SQLite
cd src/FormationApp
dotnet ef migrations add InitialCreate
dotnet ef database update

# 5. Lancer l'application
dotnet run
```

Un fichier `formationapp.db` est créé automatiquement dans `src/FormationApp/`. Les rôles, le compte admin et les données de démonstration sont injectés au premier démarrage via `DbInitializer`.

L'application est accessible sur l'URL affichée dans la console (ex. `https://localhost:5001`).

## Configuration du formateur IA (HeyGen / Anam.ai)

La configuration se fait dans `appsettings.json` (ou via variables d'environnement / `dotnet user-secrets` — **ne committez jamais de vraie clé API**) :

```json
"Avatar": {
  "Provider": "HeyGen",          // "HeyGen", "Anam" ou "None"
  "ApiKey": "VOTRE_CLE_API",
  "AvatarId": "identifiant_avatar",
  "VoiceId": "identifiant_voix",  // Anam.ai uniquement
  "KnowledgeId": "",              // HeyGen : identifiant de "context" — OBLIGATOIRE, voir ci-dessous
  "Sandbox": true,                // HeyGen uniquement : mode test, ne consomme aucun crédit
  "LlmId": "",                    // Anam.ai uniquement, laisser vide pour le LLM par défaut
  "PersonaName": "Assistant Formateur"
}
```

Équivalent en variables d'environnement (utile pour Render/Railway) :

```
Avatar__Provider=HeyGen
Avatar__ApiKey=xxxxx
Avatar__AvatarId=xxxxx
Avatar__KnowledgeId=xxxxx
Avatar__Sandbox=true
```

**Où obtenir les identifiants :**
- **HeyGen (LiveAvatar)** : créez un compte sur [app.liveavatar.com](https://app.liveavatar.com), récupérez votre clé API depuis la page **Developers**, puis créez ou choisissez un avatar pour obtenir son `AvatarId`. **Important :** LiveAvatar exige aussi un `context_id` — rendez-vous sur [app.liveavatar.com/contexts](https://app.liveavatar.com/contexts), créez un contexte (nom + instructions/prompt pour l'avatar), et collez son identifiant dans `KnowledgeId`. Laissez `Sandbox: true` pour tester sans consommer de crédits — passez à `false` en production.
  > ⚠️ HeyGen a migré son ancienne "Streaming Avatar API" (`/v1/streaming.*`) vers une nouvelle API **LiveAvatar** (`api.liveavatar.com`) en 2026 ; l'intégration de ce projet utilise déjà la nouvelle API. Si HeyGen fait encore évoluer son API après la publication de ce projet, consultez [docs.liveavatar.com](https://docs.liveavatar.com/).
  > ⚠️ **Bug constaté** : certains avatars/contexts échouent au démarrage de session en mode `Sandbox: true` (erreur "Bad Request" sur `/v1/sessions/start`, visible dans la console du navigateur) alors que la création de l'embed réussit côté serveur. Si "Démarrer le formateur IA" affiche un message d'erreur générique, passez `Sandbox` à `false` — cela consomme de vrais crédits HeyGen mais contourne le problème.
- **Anam.ai** : créez un compte sur [anam.ai](https://anam.ai), créez une persona, puis récupérez votre clé API dans le tableau de bord.

Tant qu'aucun fournisseur n'est configuré (`Provider: "None"`), le bouton "Démarrer le formateur IA" reste fonctionnel mais affiche un message indiquant que le service n'est pas encore configuré — le reste de l'application fonctionne normalement.

## Comptes de démonstration

| Rôle | Email | Mot de passe |
|---|---|---|
| Admin | `admin@formationapp.local` | `Admin@12345` |

(Modifiable via `SeedAdmin:Email` / `SeedAdmin:Password` dans `appsettings.json`.) Les apprenants créent leur propre compte via "Inscription".

## Où héberger gratuitement en 2026

L'écosystème d'hébergement gratuit a beaucoup changé ces dernières années — voici l'état réel début 2026 :

| Plateforme | Vraiment gratuit ? | Remarques |
|---|---|---|
| **Render** | ✅ Oui, en permanence | Web service Docker gratuit (512 Mo RAM), pas de carte bancaire requise. Se met en veille après 15 min d'inactivité (redémarrage ~1 min), 750h d'instance gratuites/mois. **C'est l'option recommandée pour ce projet.** |
| Railway | ⚠️ Essai seulement | Crédit de $5 valable 30 jours, puis payant (~$5/mois minimum). Ne convient plus pour un hébergement 100% gratuit et durable. |
| Fly.io | ❌ Non | Essai limité à 2h ou 7 jours, carte bancaire requise ensuite. |
| Azure App Service (F1) | ✅ Oui | Gratuit en permanence, mais quotas très limités (60 min CPU/jour) ; nécessite un compte Azure. |

**Recommandation : Render.**

### ⚠️ Point important sur SQLite + hébergement gratuit

Les services web gratuits de Render n'ont **pas de disque persistant** par défaut : le système de fichiers est réinitialisé à chaque redéploiement et à chaque redémarrage après une mise en veille. Concrètement, le fichier `formationapp.db` (SQLite) sera **recréé vide** à chaque redémarrage du conteneur.

- Pour une **démo / projet scolaire / portfolio**, ce n'est pas gênant : `DbInitializer` réinjecte automatiquement le compte admin et les données de démonstration à chaque démarrage.
- Pour conserver les **vraies inscriptions des apprenants** dans la durée, deux options :
  1. Ajouter un disque persistant Render (payant, ~1$/Go/mois) et pointer `ConnectionStrings__DefaultConnection` vers ce disque monté (ex. `Data Source=/var/data/formationapp.db`).
  2. Basculer vers **Render PostgreSQL gratuit** (1 Go, mais expire et doit être recréée tous les 30 jours) en remplaçant le paquet `Microsoft.EntityFrameworkCore.Sqlite` par `Npgsql.EntityFrameworkCore.PostgreSQL` et `UseSqlite(...)` par `UseNpgsql(...)`.

## Déploiement sur GitHub + Render

### 1. Publier le code sur GitHub

```bash
git init
git add .
git commit -m "Initial commit - FormationApp"
git branch -M main
git remote add origin https://github.com/<votre-compte>/formation-app.git
git push -u origin main
```

### 2. Déployer sur Render

1. Créez un compte sur [render.com](https://render.com) (aucune carte bancaire requise pour le tier gratuit).
2. **New** → **Web Service** → connectez votre dépôt GitHub.
3. Render détecte automatiquement le `Dockerfile` à la racine du projet (build via Docker).
4. Choisissez l'instance **Free**.
5. Dans l'onglet **Environment**, ajoutez les variables :
   ```
   ConnectionStrings__DefaultConnection=Data Source=formationapp.db
   Avatar__Provider=HeyGen
   Avatar__ApiKey=xxxxx
   Avatar__AvatarId=xxxxx
   Avatar__Sandbox=true
   SeedAdmin__Email=admin@votredomaine.com
   SeedAdmin__Password=UnMotDePasseFort123!
   ```
6. Cliquez sur **Create Web Service**. Render construit l'image Docker et déploie automatiquement — les migrations EF Core s'appliquent au démarrage (`DbInitializer.SeedAsync` appelle `Database.MigrateAsync()`).
7. Chaque nouveau `git push` sur `main` redéclenche un déploiement automatique.

Votre application sera accessible sur une URL du type `https://formation-app.onrender.com`.

## Structure du projet

```
FormationApp/
├── Dockerfile
├── railway.json         # Optionnel — utile seulement si vous déployez sur Railway
├── FormationApp.sln
└── src/FormationApp/
    ├── Program.cs
    ├── appsettings.json
    ├── Controllers/        # Home, Account, Admin, Categories, Formateurs,
    │                        # Formations, Modules, Modalites, Inscriptions,
    │                        # Apprenant, Avatar (API)
    ├── Models/              # Entités EF Core (Formation, Module, Modalite, ...)
    ├── ViewModels/          # Login/Register, Quiz, Player
    ├── Services/            # AvatarService (intégration HeyGen / Anam.ai)
    ├── Data/                # ApplicationDbContext, DbInitializer (seed)
    ├── Views/               # Razor Views (Bootstrap 5)
    └── wwwroot/             # css/site.css, js/site.js, js/avatar.js
```

## Limites connues et pistes d'amélioration

- Les migrations EF Core ne sont pas pré-générées dans ce dépôt (dossier `Migrations/`) : exécutez `dotnet ef migrations add InitialCreate` lors de la première installation, comme indiqué ci-dessus.
- SQLite convient très bien pour une démo ou un usage à trafic modéré ; pour une charge concurrente élevée en écriture, préférez PostgreSQL ou SQL Server.
- La gestion des questions de quiz est volontairement simplifiée (4 options maximum, une seule bonne réponse) ; elle peut être étendue pour des questions à choix multiples ou d'autres formats.
- L'appel à l'API du formateur IA n'est pas soumis à une limite de débit (rate limiting) applicative : à ajouter avant une mise en production à grande échelle.
- Les méthodes exactes des SDK HeyGen / Anam.ai évoluent régulièrement ; en cas d'erreur JS dans `wwwroot/js/avatar.js`, vérifiez la documentation officielle à jour du fournisseur choisi.
