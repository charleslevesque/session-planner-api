#!/bin/bash
# ═══════════════════════════════════════════════════════════════
# SessionPlanner — Script complet de gestion du board GitHub
#
# Ce script :
#   1. Nettoie les issues obsolètes (doublons sprints 1-2)
#   2. Réaligne les issues existantes vers les bons sprints
#   3. Crée TOUTES les issues restantes (Sprint 4 → Sprint 6)
#
# Calendrier :
#   Sprint 3 (en cours, se termine ~10 mars) : #16 #17 #18
#   Sprint 4 (11-24 mars)  : Auth + Workflow Core
#   Sprint 5 (25 mars-7 avr): Matrice + Interface + Polish
#   Sprint 6 (8-18 avril)  : Rapport + Présentation + Docs
#
# Équipe :
#   Galledonium (Joseph)  — Auth, Matrice, Tests, UML
#   JEYKOD3 (Jean Emmanuel) — Session workflow, Seed data, Swagger, README
#   Amboura (Gregson)     — TeachingNeed, Frontend, Rapport
#
# Usage : bash create-issues.sh
# Prérequis : gh auth login
# ═══════════════════════════════════════════════════════════════

set -e
REPO="charleslevesque/session-planner-api"

echo "═══════════════════════════════════════"
echo "  SessionPlanner — Board Setup"
echo "═══════════════════════════════════════"
echo ""

if ! gh auth status &>/dev/null; then
  echo "❌ gh auth login requis"
  exit 1
fi
echo "✅ GitHub CLI authentifié"
echo ""

# ═══════════════════════════════════════════════════════════════
# ÉTAPE 1 : Nettoyage — Fermer les issues obsolètes
# ═══════════════════════════════════════════════════════════════
echo "🧹 Nettoyage des issues obsolètes..."

# #3, #4, #5, #6 sont des doublons des issues #7-#15 (déjà fermées/complétées)
for i in 3 4 5 6; do
  gh issue close $i --repo $REPO --comment "Fermée : travail déjà complété dans les issues #7-#15." 2>/dev/null || true
done

# #2 Docker Compose — garder ouverte mais déprioritiser
gh issue edit 2 --repo $REPO --remove-label "critical" --add-label "low" 2>/dev/null || true

echo "✅ Issues obsolètes fermées (#3, #4, #5, #6)"
echo ""

# ═══════════════════════════════════════════════════════════════
# ÉTAPE 2 : Création des labels manquants
# ═══════════════════════════════════════════════════════════════
echo "📋 Labels..."
gh label create "auth"  --repo $REPO --description "Authentification et RBAC"   --color "E4405F" 2>/dev/null || true
gh label create "ui"    --repo $REPO --description "Interface utilisateur"       --color "1D76DB" 2>/dev/null || true
gh label create "test"  --repo $REPO --description "Tests"                       --color "BFD4F2" 2>/dev/null || true
gh label create "algo"  --repo $REPO --description "Algorithmes"                 --color "7B2D8B" 2>/dev/null || true
echo "✅ Labels prêts"
echo ""

# ═══════════════════════════════════════════════════════════════
# ÉTAPE 3 : Réaligner les issues existantes
# ═══════════════════════════════════════════════════════════════
echo "🔄 Réalignement des issues existantes..."

# #20, #21, #22 → sprint 5 (matrice = après auth)
for i in 20 21 22; do
  gh issue edit $i --repo $REPO --remove-label "sprint: 4" --add-label "sprint: 5" 2>/dev/null || true
done
# Assigner #20 et #21 à Galledonium
gh issue edit 20 --repo $REPO --add-assignee "Galledonium" 2>/dev/null || true
gh issue edit 21 --repo $REPO --add-assignee "Galledonium" 2>/dev/null || true
gh issue edit 22 --repo $REPO --add-assignee "JEYKOD3" 2>/dev/null || true

# #23 → sprint 4 (session workflow = base du sprint 4)
gh issue edit 23 --repo $REPO --remove-label "sprint: 5" --add-label "sprint: 4" 2>/dev/null || true
gh issue edit 23 --repo $REPO --add-assignee "JEYKOD3" 2>/dev/null || true

# #19 → sprint 5 (tests complets après features)
gh issue edit 19 --repo $REPO --remove-label "sprint: 3" --add-label "sprint: 5" 2>/dev/null || true
gh issue edit 19 --repo $REPO --add-assignee "Galledonium" 2>/dev/null || true

# #24 Audit trail → sprint 5, nice-to-have
gh issue edit 24 --repo $REPO --remove-label "sprint: 5" --add-label "sprint: 5" 2>/dev/null || true
gh issue edit 24 --repo $REPO --remove-label "medium" 2>/dev/null || true
gh issue edit 24 --repo $REPO --add-label "low" 2>/dev/null || true

echo "✅ Issues réalignées"
echo ""

# ═══════════════════════════════════════════════════════════════
# SPRINT 4 — Auth + Workflow Core (11-24 mars)
#
# Galledonium : Auth (4 issues) ← branche existante
# JEYKOD3    : Session workflow (2 issues)
# Amboura    : TeachingNeed (3 issues)
# ═══════════════════════════════════════════════════════════════
echo "═══════════════════════════════════════"
echo "  SPRINT 4 — Auth + Workflow Core"
echo "  11-24 mars | 9 nouvelles issues"
echo "═══════════════════════════════════════"

# ── S4-1 : Entité User ──
gh issue create --repo $REPO \
  --title "[AUTH] Entité User + enum UserRole + migration EF Core" \
  --label "feature,auth,core,database,critical,sprint: 4,M" \
  --assignee "Galledonium" \
  --body "$(cat << 'EOF'
## Description
Première brique de l'authentification. Créer l'entité User avec les 5 rôles RBAC nécessaires au projet.

**Branche** : `feature/add-auth-and-permissions`

## Livrables
- `src/SessionPlanner.Core/Entities/User.cs`
- `src/SessionPlanner.Core/Enums/UserRole.cs`
- DbSet + configuration dans `AppDbContext.cs`
- Migration EF Core

## Spécifications
```
User : Id, Email (unique), PasswordHash, FirstName, LastName, Role, PersonnelId?, CreatedAt, IsActive
UserRole : Admin, ResponsableTechnique, Horaire, Direction, Enseignant
```

## Critères d'acceptation
- [ ] Entité User avec toutes les propriétés
- [ ] Enum UserRole avec 5 rôles
- [ ] Index unique sur Email
- [ ] FK optionnelle vers Personnel
- [ ] Migration créée et appliquée
- [ ] Tests unitaires sur l'entité
EOF
)"
echo "  ✅ S4-1 : Entité User"

# ── S4-2 : JWT Service ──
gh issue create --repo $REPO \
  --title "[AUTH] Service JWT + configuration Bearer dans Program.cs" \
  --label "feature,auth,core,api,critical,sprint: 4,M" \
  --assignee "Galledonium" \
  --body "$(cat << 'EOF'
## Description
Configurer l'authentification JWT Bearer pour sécuriser l'API. Le token contiendra les claims userId, email et role.

**Dépend de** : [AUTH] Entité User

## Livrables
- `src/SessionPlanner.Core/Interfaces/ITokenService.cs`
- `src/SessionPlanner.Infrastructure/Services/TokenService.cs`
- Configuration JWT dans `Program.cs` et `appsettings.json`
- NuGet : `Microsoft.AspNetCore.Authentication.JwtBearer`

## Spécifications
```csharp
public interface ITokenService {
    string GenerateAccessToken(User user);   // JWT avec claims
    string GenerateRefreshToken();            // token opaque
}
```
- JWT expiry : 30 min (configurable)
- Clé secrète via env var `SESSIONPLANNER_JWT_KEY`
- Issuer/Audience configurables

## Critères d'acceptation
- [ ] ITokenService dans Core, implémentation dans Infrastructure
- [ ] AddAuthentication + AddJwtBearer dans Program.cs
- [ ] Section JWT dans appsettings.json
- [ ] Clé via variable d'environnement (jamais hardcodée)
- [ ] Swagger configuré avec bouton "Authorize"
- [ ] Tests unitaires : génération token, validation claims
EOF
)"
echo "  ✅ S4-2 : JWT Service"

# ── S4-3 : AuthController ──
gh issue create --repo $REPO \
  --title "[AUTH] Controller Auth : register, login, me" \
  --label "feature,auth,api,critical,sprint: 4,L" \
  --assignee "Galledonium" \
  --body "$(cat << 'EOF'
## Description
Endpoints d'inscription et de connexion. Un enseignant peut créer son compte, se connecter, et voir son profil.

**Dépend de** : [AUTH] Service JWT

## Livrables
- `src/SessionPlanner.Api/Controller/AuthController.cs`
- `src/SessionPlanner.Api/Dtos/Auth/RegisterRequest.cs`
- `src/SessionPlanner.Api/Dtos/Auth/LoginRequest.cs`
- `src/SessionPlanner.Api/Dtos/Auth/AuthResponse.cs`
- `src/SessionPlanner.Core/Interfaces/IAuthService.cs`
- `src/SessionPlanner.Infrastructure/Services/AuthService.cs`
- NuGet : `BCrypt.Net-Next`

## Endpoints
| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| POST | `/api/v1/auth/register` | Non | Crée un compte (rôle = Enseignant) |
| POST | `/api/v1/auth/login` | Non | Retourne JWT + refresh token |
| GET | `/api/v1/auth/me` | Oui | Profil de l'utilisateur connecté |
| POST | `/api/v1/auth/refresh` | Non | Renouvelle le JWT |

## Critères d'acceptation
- [ ] Register : email unique, password hashé BCrypt, rôle par défaut Enseignant
- [ ] Login : retourne `{ token, refreshToken, expiresAt }`
- [ ] Me : retourne le profil (id, email, name, role)
- [ ] Validation : email format, password min 8 chars
- [ ] Tests unitaires sur AuthService
- [ ] Tests d'intégration : register → login → me
- [ ] Swagger documenté
EOF
)"
echo "  ✅ S4-3 : AuthController"

# ── S4-4 : RBAC ──
gh issue create --repo $REPO \
  --title "[AUTH] Autorisation RBAC : [Authorize] sur tous les controllers" \
  --label "feature,auth,api,critical,sprint: 4,L" \
  --assignee "Galledonium" \
  --body "$(cat << 'EOF'
## Description
Protéger tous les 12 controllers existants avec [Authorize] et appliquer les politiques de rôle. Sans cela, n'importe qui peut modifier les données.

**Dépend de** : [AUTH] Controller Auth

## Matrice des droits
| Ressource | Admin | RespTech | Horaire | Direction | Enseignant |
|-----------|-------|----------|---------|-----------|------------|
| CRUD Logiciels/Labs/OS | ✅ | ✅ | ❌ | 🔍 | 🔍 |
| CRUD Cours/Personnel | ✅ | ✅ | ✅ | 🔍 | 🔍 |
| Sessions (transitions) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Besoins (propres) | ✅ | ✅ | ❌ | ❌ | ✅ |
| Approbation besoins | ✅ | ✅ | ❌ | ❌ | ❌ |
| Matrice | ✅ | ✅ | ❌ | 🔍 | ❌ |
| Users | ✅ | ❌ | ❌ | ❌ | ❌ |

🔍 = lecture seule

## Livrables
- [Authorize] sur les 12 controllers existants
- [AllowAnonymous] sur AuthController (register/login)
- Policies dans Program.cs
- Mise à jour CustomWebApplicationFactory pour les tests

## Critères d'acceptation
- [ ] Tous les controllers protégés par [Authorize]
- [ ] Policies de rôle configurées
- [ ] Tests intégration : 401 sans token
- [ ] Tests intégration : 403 mauvais rôle
- [ ] Tests intégration : 200 bon rôle
- [ ] CustomWebApplicationFactory supporte l'auth de test
EOF
)"
echo "  ✅ S4-4 : RBAC"

# ── S4-5 : Machine à états Session ──
gh issue create --repo $REPO \
  --title "[FEATURE] Machine à états Session académique (Draft→Open→Closed→Archived)" \
  --label "feature,core,database,critical,sprint: 4,M" \
  --assignee "JEYKOD3" \
  --body "$(cat << 'EOF'
## Description
Le cycle de vie de la session est le pilier du workflow de Charles. L'entité Session doit porter sa propre logique de transitions d'état (Domain-Driven Design).

**Étend #23** (Entités Session académique)

## Machine à états
```
Draft ──→ Open ──→ Closed ──→ Archived
```

## Livrables
- `src/SessionPlanner.Core/Enums/SessionStatus.cs`
- Extension de `Session.cs` : Status, CreatedAt, OpenedAt, ClosedAt, ArchivedAt, CreatedByUserId
- Méthode `Session.TransitionTo(SessionStatus)` avec validation
- Migration EF Core

## Critères d'acceptation
- [ ] Enum SessionStatus : Draft, Open, Closed, Archived
- [ ] Timestamps par état
- [ ] TransitionTo valide : Draft→Open, Open→Closed, Closed→Archived
- [ ] TransitionTo invalide : lève InvalidOperationException
- [ ] Tests unitaires exhaustifs (toutes combinaisons)
- [ ] Migration appliquée
EOF
)"
echo "  ✅ S4-5 : Machine à états Session"

# ── S4-6 : Endpoints transitions Session ──
gh issue create --repo $REPO \
  --title "[FEATURE] Endpoints transitions de session (/open, /close, /archive)" \
  --label "feature,api,core,critical,sprint: 4,M" \
  --assignee "JEYKOD3" \
  --body "$(cat << 'EOF'
## Description
Exposer le workflow session via l'API. Charles doit pouvoir ouvrir la collecte, fermer pour planifier, archiver en fin de session.

**Dépend de** : Machine à états Session + Auth RBAC

## Endpoints
| Méthode | Route | Rôles | Transition |
|---------|-------|-------|------------|
| POST | `/api/v1/sessions/{id}/open` | Admin, RespTech | Draft → Open |
| POST | `/api/v1/sessions/{id}/close` | Admin, RespTech | Open → Closed |
| POST | `/api/v1/sessions/{id}/archive` | Admin, RespTech | Closed → Archived |

## Critères d'acceptation
- [ ] 3 endpoints dans SessionsController
- [ ] 200 OK si transition valide
- [ ] 409 Conflict avec message si transition invalide
- [ ] 403 si rôle non autorisé
- [ ] 404 si session inexistante
- [ ] Tests d'intégration pour chaque cas
- [ ] Swagger documenté
EOF
)"
echo "  ✅ S4-6 : Endpoints transitions"

# ── S4-7 : Entités TeachingNeed ──
gh issue create --repo $REPO \
  --title "[FEATURE] Entités TeachingNeed + TeachingNeedItem + migration" \
  --label "feature,core,database,critical,sprint: 4,M" \
  --assignee "Amboura" \
  --body "$(cat << 'EOF'
## Description
Modéliser les besoins technologiques des enseignants. Chaque besoin est lié à une session, un enseignant et un cours, et contient une liste d'items (logiciels demandés).

**Référence** : Le formulaire MS Forms H26 contient 41 réponses réelles (Naouel Moha LOG100, Luc Duong GTI660, Eric Paquette LOG750...)

## Livrables
- `src/SessionPlanner.Core/Entities/TeachingNeed.cs`
- `src/SessionPlanner.Core/Entities/TeachingNeedItem.cs`
- `src/SessionPlanner.Core/Enums/NeedStatus.cs`
- Configuration dans AppDbContext
- Migration EF Core

## Spécifications
```
TeachingNeed : Id, SessionId, PersonnelId, CourseId, Status, CreatedAt, SubmittedAt?, ReviewedAt?, ReviewedByUserId?, RejectionReason?, Notes?
TeachingNeedItem : Id, TeachingNeedId, SoftwareId?, SoftwareVersionId?, OSId?, Quantity?, Notes?
NeedStatus : Draft, Submitted, UnderReview, Approved, Rejected
```

## Critères d'acceptation
- [ ] Entités avec FK vers Session, Personnel, Course
- [ ] Enum NeedStatus
- [ ] Navigation properties configurées
- [ ] Migration créée et appliquée
- [ ] Tests unitaires sur les entités
EOF
)"
echo "  ✅ S4-7 : Entités TeachingNeed"

# ── S4-8 : API CRUD Besoins ──
gh issue create --repo $REPO \
  --title "[FEATURE] API CRUD Besoins enseignants (controller + DTOs)" \
  --label "feature,api,critical,sprint: 4,L" \
  --assignee "Amboura" \
  --body "$(cat << 'EOF'
## Description
Permettre aux enseignants de créer, consulter et modifier leurs besoins technologiques via l'API. Le responsable technique voit tous les besoins.

**Dépend de** : Entités TeachingNeed + Auth RBAC

## Livrables
- `src/SessionPlanner.Api/Controller/TeachingNeedsController.cs`
- `src/SessionPlanner.Api/Dtos/TeachingNeeds/`
  - CreateTeachingNeedRequest, UpdateTeachingNeedRequest
  - AddNeedItemRequest
  - TeachingNeedResponse, TeachingNeedItemResponse

## Endpoints
| Méthode | Route | Rôles |
|---------|-------|-------|
| POST | `/api/v1/sessions/{sessionId}/needs` | Enseignant, Admin, RespTech |
| GET | `/api/v1/sessions/{sessionId}/needs` | Tous (filtré par rôle) |
| GET | `/api/v1/sessions/{sessionId}/needs/{id}` | Propriétaire, Admin, RespTech |
| PUT | `/api/v1/sessions/{sessionId}/needs/{id}` | Propriétaire (si Draft/Rejected) |
| DELETE | `/api/v1/sessions/{sessionId}/needs/{id}` | Propriétaire (si Draft) |
| POST | `/api/v1/sessions/{sessionId}/needs/{id}/items` | Propriétaire (si Draft) |
| DELETE | `/api/v1/sessions/{sessionId}/needs/{id}/items/{itemId}` | Propriétaire (si Draft) |

## Règles métier
- Enseignant ne voit que SES besoins
- ResponsableTechnique/Admin voient TOUT
- Impossible de créer un besoin si session pas Open (409)
- Impossible de modifier si statut = Approved (409)

## Critères d'acceptation
- [ ] CRUD complet avec DTOs
- [ ] Filtrage par rôle (Enseignant = ses besoins seulement)
- [ ] Validation session Open pour création
- [ ] Validation statut pour modification/suppression
- [ ] Tests d'intégration
- [ ] Swagger documenté
EOF
)"
echo "  ✅ S4-8 : API CRUD Besoins"

# ── S4-9 : Workflow approbation ──
gh issue create --repo $REPO \
  --title "[FEATURE] Workflow approbation besoins (submit → approve/reject → revise)" \
  --label "feature,api,core,high,sprint: 4,M" \
  --assignee "Amboura" \
  --body "$(cat << 'EOF'
## Description
Après avoir rempli ses besoins, l'enseignant les soumet. Le responsable technique (Charles) révise et approuve ou rejette avec justification. L'enseignant peut réviser après un rejet.

**Dépend de** : API CRUD Besoins

## Workflow
```
Draft ──→ Submitted ──→ UnderReview ──→ Approved
              ↑               │
              └── (revise) ← Rejected (motif obligatoire)
```

## Endpoints
| Méthode | Route | Rôles | Transition |
|---------|-------|-------|------------|
| POST | `.../needs/{id}/submit` | Enseignant | Draft → Submitted |
| POST | `.../needs/{id}/review` | RespTech, Admin | Submitted → UnderReview |
| POST | `.../needs/{id}/approve` | RespTech, Admin | UnderReview → Approved |
| POST | `.../needs/{id}/reject` | RespTech, Admin | UnderReview → Rejected |
| POST | `.../needs/{id}/revise` | Enseignant | Rejected → Draft |

## Critères d'acceptation
- [ ] 5 endpoints de transition
- [ ] Rejet exige un motif (body: `{ reason: "..." }`) — 400 sinon
- [ ] 409 si transition invalide
- [ ] 403 si rôle non autorisé
- [ ] Tests d'intégration : workflow complet bout en bout
- [ ] Swagger documenté
EOF
)"
echo "  ✅ S4-9 : Workflow approbation"

# ═══════════════════════════════════════════════════════════════
# SPRINT 5 — Matrice + Interface + Polish (25 mars - 7 avril)
#
# Galledonium : Matrice (#20 #21) + Tests (#19)
# JEYKOD3    : Seed data + Swagger + Validation (#22)
# Amboura    : Frontend React (3 issues)
#
# Existants réalignés : #19 #20 #21 #22 (#24 nice-to-have)
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════"
echo "  SPRINT 5 — Matrice + Interface"
echo "  25 mars - 7 avril | 6 nouvelles issues"
echo "═══════════════════════════════════════"

# ── S5-1 : Seed data ──
gh issue create --repo $REPO \
  --title "[CHORE] Seed data réaliste depuis les Excel de Charles" \
  --label "chore,database,high,sprint: 5,L" \
  --assignee "JEYKOD3" \
  --body "$(cat << 'EOF'
## Description
Pré-remplir la base avec les VRAIES données de la planification H2026 de Charles pour la démo PFE et la validation de l'algorithme de matrice.

## Données à seeder (extraites des fichiers Excel)

### 9 Laboratoires
A-3220 (20 PC, Windows+GPU), A-3230 (20 PC, Windows+GPU), A-3340 (20 PC, Windows+GPU partagé), A-3342 (24 PC, Linux+Windows+GPU), A-3344 (35 PC, Windows+GPU), A-3346 (28 PC, Linux+Windows+GPU), A-3412 (35 PC, macOS), A-3446 (30 PC, Windows multimédia), A-3450 (35 PC, Windows+GPU)

### 3 OS
Windows, macOS, Linux (Ubuntu)

### ~20 Logiciels de base
7zip 25.1.0, Acrobat Reader 2025.1.x, Antidote 11.x, FileZilla 3.69.x, Firefox 145.x, Chrome 142.x, Edge 142.x, Git 2.52.0, GitHub Desktop 3.4.x, Office 2024, Notepad++ 8.8.x, WSL 2.x, VS 2022 17.10.x, VS Code 1.106.3, Draw.io 29.x, CrystalDiskInfo 9.x

### ~15 Logiciels spécialisés
Oracle Database 21c (GTI660), JDK 25 (GTI311), MATLAB (LOG750), Inception 38.6 (MTI881), IntelliJ IDEA (LOG121), Node.js (LOG430), Docker Desktop (LOG430), Wireshark (GTI611), Nmap (GTI619), Python (LOG710)...

### ~40 Cours
LOG100, LOG121, LOG210, LOG240, LOG320, LOG410, LOG430, LOG530, LOG550, LOG645, LOG660, LOG680, LOG710, LOG725, LOG750, GTI100, GTI210, GTI311, GTI320, GTI350, GTI411, GTI510, GTI525, GTI611, GTI619, GTI650, GTI660, GTI700, GTI745, GTI778, MTI727, MTI820, MTI881, MGL849...

### Enseignants fictifs (pas de données réelles)
Prof A, Prof B, ... (liés aux cours)

### 1 Session Open avec besoins
Session H2026, statut Open, avec ~5 besoins soumis/approuvés

## Livrables
- `src/SessionPlanner.Infrastructure/Data/SeedData.cs`
- Appel dans Program.cs (conditionnel : env Development ou flag --seed)

## Critères d'acceptation
- [ ] Toutes les données ci-dessus seedées
- [ ] Idempotent (pas de doublons)
- [ ] Session H2026 avec besoins pour tester la matrice
- [ ] Fonctionne sur base vierge
EOF
)"
echo "  ✅ S5-1 : Seed data"

# ── S5-2 : Frontend setup + auth ──
gh issue create --repo $REPO \
  --title "[UI] Setup projet React + pages login/register + routing" \
  --label "feature,ui,critical,sprint: 5,L" \
  --assignee "Amboura" \
  --body "$(cat << 'EOF'
## Description
Initialiser le frontend React et implémenter l'authentification. C'est la base pour toutes les autres pages.

## Setup
- Vite + React + TypeScript + TailwindCSS
- Répertoire : `frontend/`
- React Router v6
- Proxy vers API : `vite.config.ts`

## Pages
### Login (`/login`)
- Formulaire email + password
- POST /api/v1/auth/login → stockage JWT
- Redirect vers /dashboard

### Register (`/register`)
- Formulaire inscription
- POST /api/v1/auth/register → redirect login

## Composants transversaux
- `AuthContext` : stockage token, refresh, logout
- `ProtectedRoute` : redirect si non authentifié
- `Layout` : header avec nom utilisateur + rôle + bouton logout
- `Sidebar` : navigation (Dashboard, Besoins, Matrice)

## Critères d'acceptation
- [ ] Projet React initialisé et buildable
- [ ] Login fonctionnel connecté à l'API
- [ ] Register fonctionnel
- [ ] Routes protégées
- [ ] Layout responsive
- [ ] Token géré en mémoire
EOF
)"
echo "  ✅ S5-2 : Frontend setup + auth"

# ── S5-3 : Dashboard + soumission besoins ──
gh issue create --repo $REPO \
  --title "[UI] Dashboard session + formulaire soumission besoins" \
  --label "feature,ui,high,sprint: 5,L" \
  --assignee "Amboura" \
  --body "$(cat << 'EOF'
## Description
Page principale après connexion. Affiche les sessions et permet à un enseignant de soumettre ses besoins.

**Dépend de** : [UI] Setup React

## Pages

### Dashboard (`/dashboard`)
- Liste des sessions avec badge statut (Draft/Open/Closed/Archived)
- Boutons d'action selon rôle : Open, Close, Archive (ResponsableTechnique)
- Stats : X besoins soumis / Y total

### Soumission besoins — vue Enseignant (`/sessions/:id/needs`)
- Sélection du cours (dropdown depuis API)
- Ajout de logiciels : recherche dans la liste + sélection version
- Boutons : "Sauvegarder brouillon" / "Soumettre"
- Affichage statut (Draft, Submitted, Approved, Rejected + motif)

### Approbation — vue ResponsableTechnique (`/sessions/:id/needs`)
- Liste de tous les besoins soumis
- Boutons Approuver / Rejeter (avec champ motif)

## Critères d'acceptation
- [ ] Dashboard affiche les sessions depuis l'API
- [ ] Enseignant peut créer et soumettre un besoin
- [ ] ResponsableTechnique peut approuver/rejeter
- [ ] Loading states et gestion d'erreurs
- [ ] Responsive
EOF
)"
echo "  ✅ S5-3 : Dashboard + soumission"

# ── S5-4 : Vue matrice ──
gh issue create --repo $REPO \
  --title "[UI] Vue matrice d'installation interactive" \
  --label "feature,ui,high,sprint: 5,M" \
  --assignee "Amboura" \
  --body "$(cat << 'EOF'
## Description
Afficher la matrice d'installation générée par l'API. C'est le livrable le plus visuel pour la démo PFE — il montre que l'API résout le problème réel de Charles.

**Dépend de** : #21 (Endpoint matrice) + [UI] Setup React

## Page : `/sessions/:id/matrix`
- Tableau : lignes = labos (A-3220, A-3230...), colonnes = logiciels
- Cellules : W (bleu), L (orange), M (gris) selon l'OS
- Logiciels de base en **gras**
- Filtre par labo, par OS
- Bouton "Exporter CSV" (stretch goal)

## Critères d'acceptation
- [ ] Tableau affiché depuis GET /sessions/{id}/installation-matrix
- [ ] Couleurs par OS
- [ ] Distinction logiciels de base vs spécialisés
- [ ] Responsive (scroll horizontal)
- [ ] Loading state
EOF
)"
echo "  ✅ S5-4 : Vue matrice"

# ── S5-5 : Swagger enrichi ──
gh issue create --repo $REPO \
  --title "[DOCS] Swagger/OpenAPI enrichi avec exemples et documentation" \
  --label "docs,api,high,sprint: 5,M" \
  --assignee "JEYKOD3" \
  --body "$(cat << 'EOF'
## Description
Enrichir Swagger pour la démo PFE. Le professeur et les évaluateurs consulteront cette page. Chaque endpoint doit être documenté avec des exemples.

## Livrables
- XML comments sur tous les endpoints (summary, param, response)
- Exemples de requêtes/réponses
- Codes d'erreur documentés (400, 401, 403, 404, 409)
- Bouton "Authorize" pour JWT
- Groupement logique des endpoints
- `GenerateDocumentationFile` activé dans les .csproj

## Critères d'acceptation
- [ ] Tous les endpoints documentés
- [ ] Exemples dans les DTOs
- [ ] Erreurs documentées
- [ ] Swagger utilisable pour la démo sans explication
EOF
)"
echo "  ✅ S5-5 : Swagger enrichi"

# ── S5-6 : Tests complets ──
gh issue create --repo $REPO \
  --title "[TEST] Tests complets : auth + workflow + matrice (couverture ≥ 70%)" \
  --label "test,high,sprint: 5,L" \
  --assignee "Galledonium" \
  --body "$(cat << 'EOF'
## Description
Les 135 tests existants couvrent les entités initiales. Il faut couvrir les nouvelles fonctionnalités : auth, session workflow, besoins, matrice. Objectif : ≥ 70% couverture Core.

**Complète #19** (Tests d'intégration)

## Tests à ajouter

### Unit Tests
- [ ] AuthService : hash, validation, rôle par défaut
- [ ] TokenService : JWT generation, claims, expiry
- [ ] Session.TransitionTo : toutes combinaisons
- [ ] NeedStatus transitions : toutes combinaisons
- [ ] MatrixGeneration : cas normaux + edge cases

### Integration Tests
- [ ] AuthController : register → login → me → refresh
- [ ] Session workflow : create → open → close → archive
- [ ] TeachingNeeds : CRUD + workflow complet
- [ ] Role-based access : 401/403/200 par rôle
- [ ] Matrix endpoint avec données seed

### Infrastructure
- [ ] CustomWebApplicationFactory avec helper auth
- [ ] Helper pour créer des users de test par rôle
- [ ] Rapport de couverture HTML

## Critères d'acceptation
- [ ] ≥ 70% couverture Core
- [ ] ≥ 60% couverture Api
- [ ] Tous les tests passent
- [ ] Aucun test flaky
- [ ] Rapport HTML générable
EOF
)"
echo "  ✅ S5-6 : Tests complets"

# ═══════════════════════════════════════════════════════════════
# SPRINT 6 — Documentation + Présentation (8-18 avril)
#
# Galledonium : Diagrammes UML
# JEYKOD3    : README + architecture
# Amboura    : Sections rapport
# Équipe     : Rapport (#25) + Présentation (#26)
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════"
echo "  SPRINT 6 — Documentation PFE"
echo "  8-18 avril | 2 nouvelles issues"
echo "═══════════════════════════════════════"

# ── S6-1 : Diagrammes UML ──
gh issue create --repo $REPO \
  --title "[DOCS] Diagrammes UML : Use Case, Séquence, Classes, Architecture, BPMN" \
  --label "docs,critical,sprint: 6,L" \
  --assignee "Galledonium" \
  --body "$(cat << 'EOF'
## Description
Exigence du professeur : "montrez les use case (UML)". Ces diagrammes seront dans le rapport technique et projetés lors de la présentation orale.

## Diagrammes requis

### 1. Use Case
Acteurs : Admin, ResponsableTechnique, Enseignant, Direction
Use cases : gérer sessions, soumettre besoins, approuver, matrice, consulter...

### 2. Séquence (×3)
1. Workflow session : création → open → collecte → close → matrice
2. Soumission besoin : enseignant → API → validation → notification
3. Authentification : register → login → accès → refresh

### 3. Classes
Toutes les entités (13 + User + TeachingNeed) avec relations, interfaces services

### 4. Architecture
Clean Architecture : Client (React) → Api → Core ← Infrastructure → DB

### 5. BPMN
Cycle de vie session avec swimlanes par acteur

## Format
- Source en Mermaid (fichiers .md) dans `docs/diagrams/`
- PNG/SVG exportés pour le rapport Word

## Critères d'acceptation
- [ ] 5 types de diagrammes
- [ ] Source versionnable (Mermaid ou PlantUML)
- [ ] Images exportées
- [ ] Cohérent avec le code
- [ ] README les référence
EOF
)"
echo "  ✅ S6-1 : Diagrammes UML"

# ── S6-2 : README ──
gh issue create --repo $REPO \
  --title "[CHORE] README complet + documentation architecture + usage IA" \
  --label "chore,docs,high,sprint: 6,M" \
  --assignee "JEYKOD3" \
  --body "$(cat << 'EOF'
## Description
Le README est la vitrine du projet pour les évaluateurs. Il doit permettre de comprendre, installer et lancer le projet en < 10 minutes. La section "Usage IA" est une exigence explicite du professeur.

## Sections
1. Introduction + problème résolu
2. Architecture (Clean Architecture, diagramme)
3. Stack technique (.NET 10, React, SQLite)
4. Prérequis (SDK, Node.js, Git)
5. Installation pas à pas
6. Configuration (env vars)
7. Lancer le projet (API + frontend)
8. Tests (commandes, couverture)
9. API Documentation (lien Swagger)
10. Équipe (Jean Emmanuel Yao, Joseph Feghali, Gregson Destin)
11. **Usage IA** : outils utilisés (Cursor, Claude), pour quoi, impact
12. Licence GPL

## Critères d'acceptation
- [ ] Setup fonctionnel en < 10 minutes
- [ ] Badges CI, .NET version
- [ ] Section Usage IA documentée
- [ ] Aucune donnée sensible
EOF
)"
echo "  ✅ S6-2 : README complet"

# ═══════════════════════════════════════════════════════════════
# RÉSUMÉ FINAL
# ═══════════════════════════════════════════════════════════════
echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  ✅ SCRIPT TERMINÉ"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "  🧹 Nettoyage : #3 #4 #5 #6 fermées (doublons)"
echo "  🔄 Réalignées : #19→S5  #20→S5  #21→S5  #22→S5  #23→S4"
echo ""
echo "  ┌─────────────────────────────────────────────────────┐"
echo "  │  SPRINT 4 (11-24 mars) — Auth + Workflow Core       │"
echo "  │  9 nouvelles issues                                 │"
echo "  │  Galledonium : Auth (4 issues)  ← branche existante│"
echo "  │  JEYKOD3     : Session (2 issues)                   │"
echo "  │  Amboura     : TeachingNeed (3 issues)              │"
echo "  │  + #23 (réalignée)                                  │"
echo "  ├─────────────────────────────────────────────────────┤"
echo "  │  SPRINT 5 (25 mars - 7 avril) — Matrice + UI       │"
echo "  │  6 nouvelles issues                                 │"
echo "  │  Galledonium : Tests + Matrice (#20 #21)            │"
echo "  │  JEYKOD3     : Seed data + Swagger + Validation(#22)│"
echo "  │  Amboura     : Frontend React (3 issues)            │"
echo "  │  + #19 #20 #21 #22 #24 (réalignées)                │"
echo "  ├─────────────────────────────────────────────────────┤"
echo "  │  SPRINT 6 (8-18 avril) — Docs + Présentation       │"
echo "  │  2 nouvelles issues                                 │"
echo "  │  Galledonium : Diagrammes UML                       │"
echo "  │  JEYKOD3     : README                               │"
echo "  │  Équipe      : Rapport (#25) + Présentation (#26)   │"
echo "  └─────────────────────────────────────────────────────┘"
echo ""
echo "  TOTAL : 17 nouvelles issues créées"
echo "         6 issues réalignées"
echo "         4 issues obsolètes fermées"
echo ""
echo "  📅 Deadline : mi-avril 2026"
echo "  👥 3 personnes × 12h/sem × 5.5 sem = ~200 heures"
echo ""
echo "  🚀 Prochaines actions :"
echo "     Galledonium → continuer feature/add-auth-and-permissions"
echo "     JEYKOD3     → commencer machine à états Session"
echo "     Amboura     → commencer entités TeachingNeed"
