# PetFinder

PetFinder är en .NET 10-lösning för rapportering av borttappade djur med:
- **API** (`src/PetFinder.Api`)
- **Blazor frontend** (`src/PetFinder.Blazor`)

Användaren kan skapa rapport som:
- **Owner** (ägare) – namn på djuret krävs
- **Observer** (observatör) – namn är valfritt

Rapporten innehåller beskrivning, platsnamn, GPS-koordinater och bild.

## Arkitektur

API följer lagerstruktur:
- Controller -> Service -> Repository

Teknikval:
- .NET 10
- Dapper + SQLite (lokal default)
- Mapster för mappning
- Serilog + Seq
- ProblemDetails + global exception handler
- API-key autentisering (`X-API-Key`, deny-by-default)
- Rate limiting (429 vid överskridna gränser)

## Konfiguration

### API (`src/PetFinder.Api/appsettings.json`)
- `ConnectionStrings:Default` – standard: `Data Source=petfinder.db`
- `ApiKeyAuthentication:HeaderName` – standard: `X-API-Key`
- `ApiKeyAuthentication:Keys` – tom som standard, sätt via secrets/env i utveckling/drift
- `ImageStorage:BasePath` – standard: `storage/images`
- `ImageStorage:MaxBytes` – standard: `5242880` (5 MB)
- `ImageStorage:AllowedContentTypes` – `image/jpeg`, `image/png`, `image/webp`

### Blazor (`src/PetFinder.Blazor/appsettings.json`)
- `ApiClient:BaseUrl` – standard: `https://localhost:7058/`
- `ApiClient:ApiKey` – sätt API-nyckel för frontend-anrop

## Lokal körning

1. Sätt en API-nyckel för API-projektet (user secrets eller miljövariabel).
2. Sätt samma nyckel i Blazor-konfigurationen (`ApiClient:ApiKey`).
3. Starta API:
   - `dotnet run --project src/PetFinder.Api`
4. Starta Blazor:
   - `dotnet run --project src/PetFinder.Blazor`

Standard-URL:er i Development:
- API: `https://localhost:7058`
- Blazor: `https://localhost:7011`

## API-endpoints

- `POST /api/reports` (multipart/form-data)
  - fält: `ReporterType`, `AnimalDescription`, `LocationName`, `Latitude`, `Longitude`, `PetName` (owner), `Image`
- `GET /api/reports/{id}`
- `GET /api/reports?latitude=..&longitude=..&radiusKm=..`

Alla endpoints kräver `X-API-Key`.

## Bildlagring

Uppladdade bilder sparas lokalt under `storage/images` (konfigurerbart).
Filer valideras mot tillåten content type och maxstorlek.

## Tester

Kör alla tester:

`dotnet test tests/PetFinder.Tests/PetFinder.Tests.csproj`

Täcker bland annat:
- servicevalidering (Owner kräver PetName)
- Mapster-mappning
- API-key (401/authorized)
- rate limiting (429)
