# HeadStart

Une application web moderne construite avec .NET 9, utilisant une architecture Backend-for-Frontend (BFF) avec Blazor WebAssembly et .NET Aspire pour l'orchestration des services.

## 📋 Table des matières

- [Architecture](#architecture)
- [Technologies](#technologies)
- [Prérequis](#prérequis)
- [Installation](#installation)
- [Utilisation](#utilisation)
- [Structure du projet](#structure-du-projet)
- [Authentification](#authentification)
- [API](#api)
- [Développement](#développement)
- [Contribution](#contribution)

## 🏗️ Architecture

HeadStart utilise une architecture moderne avec les composants suivants :

```
┌─────────────────────────────────────────────────────────────────┐
│                        .NET Aspire AppHost                     │
│                    (Orchestration des services)                │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  │
┌─────────────────────────────────┼─────────────────────────────────┐
│                                 │                                 │
│  ┌─────────────────────────────┐│┌─────────────────────────────┐  │
│  │         Keycloak            │││        PostgreSQL           │  │
│  │     (Authentification)      │││       (Base de données)     │  │
│  └─────────────────────────────┘││└─────────────────────────────┘  │
│                                 ││                                 │
│  ┌─────────────────────────────┐││┌─────────────────────────────┐  │
│  │           Seq               │││         Services            │  │
│  │        (Logging)            │││      (Monitoring)           │  │
│  └─────────────────────────────┘││└─────────────────────────────┘  │
└─────────────────────────────────┼─────────────────────────────────┘
                                  │
    ┌─────────────────────────────┼─────────────────────────────┐
    │                             │                             │
    │  ┌─────────────────────────┐│┌─────────────────────────┐  │
    │  │      HeadStart.BFF      │││    HeadStart.WebAPI     │  │
    │  │  (Backend-for-Frontend) │││   (API REST + GraphQL)  │  │
    │  │                         │││                         │  │
    │  │  • Serveur Blazor       │││  • FastEndpoints        │  │
    │  │  • Proxy YARP           │││  • Entity Framework     │  │
    │  │  • Authentification     │││  • OpenIddict           │  │
    │  └─────────────────────────┘││└─────────────────────────┘  │
    │              │               ││              │              │
    │              │               ││              │              │
    │  ┌─────────────────────────┐ ││              │              │
    │  │    HeadStart.Client     │ ││              │              │
    │  │  (Blazor WebAssembly)   │ ││              │              │
    │  │                         │ ││              │              │
    │  │  • MudBlazor UI         │ ││              │              │
    │  │  • Tailwind CSS         │ ││              │              │
    │  │  • Kiota API Client     │◄┘│              │              │
    │  └─────────────────────────┘  │              │              │
    └───────────────────────────────┘              │              │
                                                   │              │
    ┌──────────────────────────────────────────────┘              │
    │                                                             │
    │  ┌─────────────────────────────────────────────────────────┘
    │  │
    │  │  ┌─────────────────────────────────────────────────────┐
    │  │  │              SharedKernel                            │
    │  │  │         (Utilitaires partagés)                      │
    │  │  │                                                     │
    │  │  │  • Extensions                                       │
    │  │  │  • Modèles communs                                  │
    │  │  │  • Configuration JSON                               │
    │  │  └─────────────────────────────────────────────────────┘
    │  │
    │  └──────────────────────────────────────────────────────────
    │
    └─────────────────────────────────────────────────────────────
```

### Composants principaux

1. **HeadStart.Aspire.AppHost** - Orchestration des services avec .NET Aspire
2. **HeadStart.BFF** - Backend-for-Frontend ASP.NET Core servant le client Blazor et proxy des appels API
3. **HeadStart.WebAPI** - API basée sur FastEndpoints avec intégration PostgreSQL
4. **HeadStart.Client** - Frontend Blazor WebAssembly utilisant MudBlazor et Tailwind CSS
5. **HeadStart.SharedKernel** - Utilitaires, modèles et extensions partagés
6. **HeadStart.Aspire.ServiceDefaults** - Configurations communes Aspire

## 🚀 Technologies

- **.NET 9** - Framework principal
- **Blazor WebAssembly** - Frontend interactif
- **ASP.NET Core** - Backend et API
- **PostgreSQL** - Base de données
- **Entity Framework Core** - ORM
- **Keycloak** - Serveur d'authentification
- **MudBlazor** - Composants UI
- **Tailwind CSS** - Framework CSS utilitaire
- **FastEndpoints** - Organisation des endpoints API
- **YARP** - Reverse proxy
- **Serilog + Seq** - Logging centralisé
- **OpenTelemetry** - Observabilité
- **Kiota** - Génération de clients API

## 📋 Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (pour PostgreSQL et Keycloak)
- [Node.js](https://nodejs.org/) (pour Tailwind CSS)

## 🔧 Installation

1. **Cloner le repository**
   ```bash
   git clone https://github.com/Selmirrrrr/head-start.git
   cd head-start
   ```

2. **Restaurer les dépendances**
   ```bash
   dotnet restore
   ```

3. **Construire la solution**
   ```bash
   dotnet build
   ```

## 🚀 Utilisation

### Démarrer l'application complète

Pour lancer tous les services (PostgreSQL, Keycloak, Seq, WebAPI, BFF) :

```bash
dotnet run --project src/Aspire/AppHost
```

L'application sera accessible sur :
- **Dashboard Aspire** : https://localhost:17109
- **Application Web** : https://localhost:7264
- **API** : https://localhost:7233
- **Seq (Logs)** : https://localhost:5341

### Démarrer des services individuels

- **BFF seulement** : `dotnet run --project src/BFF`
- **WebAPI seulement** : `dotnet run --project src/WebAPI`
- **Client seulement** : `dotnet run --project src/Client`

## 📁 Structure du projet

```
HeadStart/
├── src/
│   ├── Aspire/
│   │   ├── AppHost/              # Orchestration Aspire
│   │   └── ServiceDefaults/      # Configurations communes
│   ├── BFF/                      # Backend-for-Frontend
│   │   ├── Controllers/          # Contrôleurs de comptes
│   │   ├── Extensions/           # Extensions de services
│   │   ├── Pages/               # Pages Razor
│   │   └── Utilities/           # Transformateurs de claims
│   ├── Client/                   # Application Blazor WebAssembly
│   │   ├── Authorization/        # Gestion de l'authentification
│   │   ├── Components/          # Composants réutilisables
│   │   ├── Generated/           # Clients API générés par Kiota
│   │   ├── Layout/              # Layout principal
│   │   └── Pages/               # Pages de l'application
│   ├── WebAPI/                   # API REST
│   │   ├── Data/                # Contexte Entity Framework
│   │   ├── Extensions/          # Extensions de services
│   │   └── Features/            # Endpoints organisés par fonctionnalité
│   ├── SharedKernel/             # Utilitaires partagés
│   └── HeadStart.SharedKernel.Models/  # Modèles partagés
├── CLAUDE.md                     # Documentation pour Claude Code
├── Directory.Build.props         # Propriétés MSBuild communes
├── Directory.Packages.props      # Gestion centralisée des packages NuGet
└── HeadStart.slnx               # Fichier solution
```

## 🔐 Authentification

L'application utilise un système d'authentification hybride :

- **Frontend (BFF)** : OpenID Connect avec Keycloak
- **API** : Validation de tokens OpenIddict
- **Flux de tokens** : Le BFF gère l'authentification utilisateur et transmet les tokens bearer à l'API via le reverse proxy YARP
- **Claims personnalisés** : Transformation dans `ClaimsTransformer.cs`

## 🌐 API

L'API utilise FastEndpoints pour l'organisation des endpoints :

- **Endpoints utilisateurs** : `/api/users/*`
- **Endpoints tenants** : `/api/tenants/*`
- **Génération automatique de clients** : Kiota génère les clients API pour Blazor lors de la compilation

Les clients générés sont placés dans `src/Client/Generated/`.

## 🛠️ Développement

### Commandes utiles

```bash
# Construire la solution
dotnet build

# Nettoyer la solution
dotnet clean

# Restaurer les packages
dotnet restore
```

### Rechargement à chaud

Le rechargement à chaud est activé pour le développement Blazor.

### Qualité du code

Le projet utilise :
- **Roslynator** - Analyseur de code statique
- **SonarAnalyzer** - Analyse de qualité du code
- **EditorConfig** - Règles de formatage standardisées

### Logging et monitoring

- **Serilog** : Logging structuré
- **Seq** : Visualisation centralisée des logs
- **OpenTelemetry** : Traces et métriques
- **Dashboard Aspire** : Monitoring des services

## 🤝 Contribution

1. Forkez le projet
2. Créez une branche pour votre fonctionnalité (`git checkout -b feature/nouvelle-fonctionnalite`)
3. Committez vos changements (`git commit -am 'Ajout d'une nouvelle fonctionnalité'`)
4. Poussez vers la branche (`git push origin feature/nouvelle-fonctionnalite`)
5. Créez une Pull Request

---

**HeadStart** - Une application moderne construite avec les dernières technologies .NET 9