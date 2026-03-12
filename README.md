# Cloud Budget

Progetto sample .NET 8 per la gestione del budget casalingo:
- .NET 8 (ASP.NET Core Web API)
- EF Core (SQL Server come esempio)
- Repository generico con supporto Id generico (BaseEntity<TId>)
- Soft-delete con global query filters
- Soft-delete propagata (Category -> Expenses) implementata in CategoryRepository
- DTO separati per Create/Update e validazioni (DataAnnotations)
- PATCH tramite DTO (AutoMapper che mappa solo proprietà non-null)
- Gestione DbUpdateConcurrencyException (409 Conflict)
- HostedService che genera report mensile e lo invia via email (CSV + MailKit)
- Swagger per esplorare le API

Requisiti
- .NET 8 SDK
- Database SQL Server (o PostgreSQL con piccoli adattamenti)

Come eseguire
1. Clona o copia i file in una cartella.
2. Imposta la ConnectionString e la sezione `Smtp` in `appsettings.json`.
3. Dal terminale nella cartella del progetto:
   - dotnet restore
   - dotnet ef migrations add InitialCreate
   - dotnet ef database update
   - dotnet run

Endpoints principali
- GET /api/categories
- GET /api/categories/{id}
- POST /api/categories
- DELETE /api/categories/{id}  (soft-delete propagata alle Expenses)

- GET /api/expenses
- GET /api/expenses/{id}
- POST /api/expenses
- PUT /api/expenses/{id}
- PATCH /api/expenses/{id} (partial update tramite DTO)
- DELETE /api/expenses/{id} (soft-delete)

Configurazione SMTP (esempio in appsettings.json)
- Server, Port, UseSsl, Username, Password, FromEmail, FromName.

Note su RowVersion e provider DB
- `IsRowVersion()` funziona nativamente per SQL Server.
- Per PostgreSQL potrebbe servire un approccio alternativo (es. `xmin`) o definire una colonna `byte` come token, adattando la configurazione se necessario.

Miglioramenti possibili
- Validazioni più robuste (FluentValidation), logging (Serilog), tests, CI/CD, containerizzazione.
- Report in Excel (ClosedXML) se si preferisce .xlsx invece di CSV.
```