<br>
<br>
<br>
 
<p align="center">
 <img width="100px" src="https://www.seekpng.com/png/full/419-4199738_final-product-image-isometric-car-illustration-png.png" align="center" alt="GitHub Readme Stats" />
 <h2 align="center">RENTAR (RENT CAR) - BACKEND</h2>

 <p align="center">Rentar 'Backend' project with C# - ASP.NET Core</p>
 <p align="center">Click for <a href="https://github.com/sezRR/rent-a-car-front-end">The Frontend</a></p>
</p>



<br>
<br>
<br>
<br>
<br>



# RENTAR BACKEND

<br>

**Getting Started**

Requirements: .NET 10 SDK, Docker.

```bash
# 1. start PostgreSQL (defaults: rentar/rentar/rentar on port 5432, override via .env)
docker compose up -d

# 2. run the API - in Development it applies every module's pending migrations on startup
dotnet run --project src/Bootstrapper/Rentar.Api
```

Connection string lives in `src/Bootstrapper/Rentar.Api/appsettings.json` under `ConnectionStrings:RentarDb`, and can be
overridden with the `ConnectionStrings__RentarDb` environment variable. When neither is present the
modules fall back to the docker-compose defaults, which is also what `dotnet ef` uses.

Migrating on startup is controlled by `Database:MigrateOnStartup` (on in `appsettings.Development.json`, off otherwise).
To migrate by hand instead:

```bash
dotnet tool restore
for module in Identity Companies Fleet Customers Rentals; do
  project=src/Modules/$module/Rentar.Modules.$module.Infrastructure
  dotnet ef database update --project $project --startup-project $project
done
```

Adding a migration after changing a module's entities (e.g. Fleet):

```bash
project=src/Modules/Fleet/Rentar.Modules.Fleet.Infrastructure
dotnet ef migrations add <Name> --project $project --startup-project $project --output-dir Persistence/Migrations
```

> Upgrading a database created before the modular monolith: its tables live in `public` and are not moved.
> Recreate it (`docker compose down -v && docker compose up -d`) so each module creates its own schema.

> Upgrading a database from before the relationships were mapped: the `AddFleetRelationships` and
> `AddUserOperationClaimsManyToMany` migrations add foreign keys and a unique index on `Users.Email`, so they stop on
> rows that break them: car images of a deleted car, cars whose brand or color is gone, users sharing an email, or a
> claim granted to a user twice. Fix or delete those rows first. Required text columns that hold `NULL` are set to `''`.

**Tests**

- `src/Tests/Rentar.Api.IntegrationTests` drives the API in memory (`WebApplicationFactory`) against a throwaway
  PostgreSQL container started by Testcontainers, so Docker has to be running. Every module's schema is migrated once;
  each test then truncates all module tables and clears the aspect cache.
- `src/Tests/Rentar.ArchitectureTests` enforces the module boundaries and schema ownership described below. No Docker needed.

```bash
dotnet test --project src/Tests/Rentar.ArchitectureTests
dotnet test --project src/Tests/Rentar.Api.IntegrationTests
```

`global.json` opts the repository into the Microsoft.Testing.Platform runner that .NET 10 requires.
Package versions are managed centrally in `Directory.Packages.props`.

Code style lives in `.editorconfig`: file-scoped namespaces, no unused usings, and primary constructors. Running
`dotnet format RentarBackend.sln` applies all of them; for example, it turns a constructor that only stores its
parameters into a primary constructor.

<br>

**API versions**

The version is part of the URL (`/api/v1/...`, `/api/v2/...`). In Development, Scalar at `/docs` has both
versions in a dropdown, and their OpenAPI documents are at `/openapi/v1.json` and `/openapi/v2.json`.
Bruno requests for both versions are in `api-docs/`.

- **v1** is the original API and is unchanged, except that it now also refuses input its validators reject (e.g. an
  update with an empty brand name), with the same error body as before, and refuses what the stored data forbids with
  `400 { success: false, message }`: a car whose brand or color is not stored, an image of a car that is not
  stored, a customer of a user that is not stored or not active, a rental of a car or customer that is not stored,
  deleting a brand or color that cars still have, and an email another user already has. Customers also carry the
  `companyId` field (`null` unless the customer rents for a company), and their `companyName` may be `null`. It uses action-style routes (`/brands/getall`,
  `/brands/add`, `POST /brands/delete`), answers 200 or 400, and wraps every body in `{ success, message, data }`.
- **v2** is RESTful:
  - Routes name resources, and the HTTP method says what to do with them.
  - Ids are in the path and filters are in the query string.
  - Bodies are the resources themselves, with no envelope.
  - Errors are [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) problem details (`application/problem+json`);
    validation errors are listed under `errors`.
  - `POST` answers `201 Created` with a `Location` header. `PUT` and `DELETE` answer `204 No Content`.
  - An unknown id answers `404`. The 22:00 maintenance hour answers `503`.
  - Users never include the password hash or salt.

| v2 route | v1 equivalent |
|----------|---------------|
| `POST /auth/tokens` (log in, `401` on bad credentials) | `POST /auth/login`, `POST /auth/checkpassword` |
| `GET/POST /users`, `GET/PUT /users/{id}`<br>filters: `?email=`, `?minFindeksRating=` | `users/getall`, `getbymail`, `getuserfindeksrating`, `getuserbyid`, `add`, `update`;<br>`POST /users` (`409` if the email is taken) also replaces `auth/register` |
| `GET/POST /brands`, `GET/PUT/DELETE /brands/{id}` | `brands/*` |
| `GET/POST /colors`, `GET/PUT/DELETE /colors/{id}` | `colors/*` |
| `GET/POST /cars`, `GET/PUT/DELETE /cars/{id}`<br>filters: `?brandId=`, `?colorId=` | `cars/getall`, `getcardetails`, `getcardetailsbyid`, `getcarsby*`, `add`, `addreturnabledata`, `update`, `delete` |
| `GET/POST/DELETE /cars/{carId}/images`, `GET/PUT/DELETE /cars/{carId}/images/{imageId}`, `GET /car-images` | `carimages/*` (form field `file`) |
| `GET/POST /customers`, `GET /customers/{id}`<br>filter: `?userId=` | `customers/*` |
| `GET/POST /rentals`, `GET /rentals/{id}`, `GET /rentals/details`, `GET /rentals/availability?carId=&rentDate=&returnDate=`<br>filter: `?carId=` | `rentals/*`, `isrentable` |
| `GET/POST /companies`, `GET/PUT /companies/{companyId}`, `GET /companies/{companyId}/roles` | - |
| `GET/POST /companies/{companyId}/employees`, `DELETE /companies/{companyId}/employees/{employeeId}` | - |
| `GET /companies/{companyId}/rentals` | - |
| `POST /payments` (`204`, or `402` when declined) | `GET /payments/pay` |
| `GET /findeks/rating` | `GET /findeks/calculate` |

Registering in v2 takes two calls: `POST /users`, then `POST /auth/tokens`.

The `/companies` routes are the only ones that need an access token (`Authorization: Bearer <token>` from
`POST /auth/tokens`); without one they answer `401`. They act for the caller the token was issued to: a company the
caller does not work at answers `404`, as if it did not exist, and a missing permission answers `403`.

A customer's `companyName` is optional free text, stored as `null` when blank. `companyId` is optional too: it makes the
customer one of that company's, which needs the user to be its employee, and a company customer without a name of its
own takes the company's name.

<br>

**Architecture: modular monolith**

One deployable host, split into modules that each own their data and talk to each other only through contracts.

```
src/
  Bootstrapper/Rentar.Api           host: composes the modules, nothing else
  Shared/
    Rentar.Shared.Kernel            exceptions, entity markers (no dependencies)
    Rentar.Shared.Application       use case handler interfaces, validation, aspects (caching, transaction, ...)
    Rentar.Shared.Infrastructure    EF repository base, per-module DbContext wiring, cache, IModule
    Rentar.Shared.Presentation      endpoint discovery, versioned route groups, exception handler, v1 envelope
  Modules/<Module>/
    Rentar.Modules.<Module>.Contracts       public API other modules may call
    Rentar.Modules.<Module>.Domain          entities, init-only: a change builds a new one
    Rentar.Modules.<Module>.Application     use cases, contract implementation, and under Common/:
                                              Interfaces/ (repositories, ports) and Models/ (query responses)
    Rentar.Modules.<Module>.Presentation    minimal-API endpoints
    Rentar.Modules.<Module>.Infrastructure  DbContext, migrations, repositories, adapters, module registration
  Tests/
```

| Module    | Owns                                   | Schema      | Uses contracts of            |
|-----------|----------------------------------------|-------------|------------------------------|
| Identity  | users, operation claims, auth (JWT)    | `identity`  | -                            |
| Fleet     | brands, colors, cars, car images       | `fleet`     | -                            |
| Companies | companies, employees, roles (company IAM) | `companies` | Identity                  |
| Customers | customer accounts                      | `customers` | Identity, Companies          |
| Rentals   | rentals                                | `rentals`   | Customers, Identity, Fleet, Companies |
| Payments  | payments (stateless)                   | -           | -                            |
| Findeks   | credit ratings (stateless)             | -           | -                            |

Every module with state has its own `DbContext`, its own schema and its own `__EFMigrationsHistory` table inside that
schema, so modules migrate independently. Data in another module is referenced by id only: there are no cross-schema
foreign keys or joins. For example, rental details are put together by the `GetRentalDetailsQuery` use case from the
Customers, Identity and Fleet contracts.

Inside a module, relationships are mapped as EF Core navigations backed by foreign keys:

| Relationship | Kind | Foreign key | On delete |
|---|---|---|---|
| `Brand.Cars` / `Car.Brand` | one-to-many | `Cars.BrandId` | restrict: a brand goes once no car has it |
| `Color.Cars` / `Car.Color` | one-to-many | `Cars.ColorId` | restrict |
| `Car.Images` / `CarImage.Car` | one-to-many | `CarImages.CarId` | cascade: the rows go with the car, `DeleteCarCommand` deletes the files |
| `User.OperationClaims` / `OperationClaim.Users` | many-to-many through `UserOperationClaim` | `UserOperationClaims.(UserId, OperationClaimId)`, also its key | cascade |
| `Company.Employees` / `Employee.Company` | one-to-many | `Employees.CompanyId` | cascade |
| `Company.Roles` / `Role.Company` | one-to-many | `Roles.CompanyId` | cascade |
| `Employee.Roles` / `Role.Employees` | many-to-many through `EmployeeRole` | `EmployeeRoles.(EmployeeId, RoleId)`, also its key | cascade |

There is no one-to-one relationship: nothing in the model has exactly one of something else. References across
modules (`Customer.UserId`, `Customer.CompanyId`, `Employee.UserId`, `Rental.CarId`, `Rental.CustomerId`) are plain ids,
indexed where they are looked up. With no foreign key to guard them, creating a customer asks Identity whether its user
exists and is active and, for a company customer, Companies whether the user works there; registering a company or
adding an employee asks Identity about the user; creating a rental asks Fleet and Customers whether its car and customer
exist. Otherwise the request answers `400`.

- Each module's `Persistence/Configurations` holds an `IEntityTypeConfiguration<T>` per entity that needs more than the
  conventions, and its context applies them with `ApplyConfigurationsFromAssembly`. Domain projects compile with
  `<Nullable>annotations</Nullable>`, so a `string` property is a required column and only a `string?` may be `NULL`.
- Queries follow navigations instead of writing joins, e.g. `car.Brand.BrandName` in `EfCarDal`, and
  `u.OperationClaims` reads through the join table. Contexts live for one operation, so queries run without tracking.
- A handler checks what it needs from stored data before writing, so a request the keys would refuse gets an answer
  instead of a database error.
- Navigations are never written to JSON (`AddApiJson`): entities keep the shape v1 clients know, and a response that
  needs related data uses a `<Name>Response` model.
- `ModuleSchemasTests` checks the database holds exactly these foreign keys.

Boundary rules (checked by `Rentar.ArchitectureTests` on the compiled assemblies):

- Domain uses only the Kernel. Contracts use nothing but the Kernel.
- Application uses its own Domain and Contracts, `Shared.Application`, and the **Contracts** of other modules - nothing else of theirs.
- Presentation uses its own Application/Domain, `Shared.Application` (to call use cases) and `Shared.Presentation`, never Infrastructure.
- Infrastructure is the module's composition root (`IModule`); it is the only module project the host references.
- Shared projects never reference a module.

Adding a module: create the five projects, implement `IModule` in its Infrastructure (register its context with
`AddModuleDbContext<TContext>("<schema>")` and its use cases with `RegisterUseCases(<Application assembly>)`), and add
it to the `modules` array in `Program.cs`.

**Use cases**

Each operation of a module is one use case in its Application project. A use case is one file that holds a record
describing it, its validator if it has one, and a sealed handler, e.g. `Brands/Commands/CreateBrandCommand.cs`:

```csharp
public sealed record CreateBrandCommand(string BrandName);

public sealed class CreateBrandCommandValidator : UseCaseValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator() => RuleFor(b => b.BrandName).ValidBrandName();
}

public sealed class CreateBrandCommandHandler(IBrandDal brandDal) : ICommandHandler<CreateBrandCommand, Brand>
{
    public async Task<Brand> HandleAsync(CreateBrandCommand command, CancellationToken cancellationToken = default) { ... }
}
```

- Commands change state and implement `ICommandHandler<TCommand>`, or `ICommandHandler<TCommand, TResult>` when the
  caller needs something back (e.g. the created record). Queries only read and implement `IQueryHandler<TQuery, TResult>`.
- Everything that waits on I/O is asynchronous, from the endpoint down: `HandleAsync`, repositories, module APIs, image
  storage and validation return a `Task` and take the request's `CancellationToken`, which every endpoint passes on.
  Work that never waits (hashing, issuing a token, the simulated payment and Findeks rating) stays synchronous inside
  them, and so does deleting an image file, which has no asynchronous API. A handler that only forwards a call
  returns its task; one with any logic of its own is `async`, so even a refusal arrives through the task.
- Naming follows the kind: `<UseCase>Command` in a `Commands` folder (`CreateCustomerCommand`), `<UseCase>Query` in a
  `Queries` folder (`GetCustomersQuery`), with `<UseCase>Command|QueryHandler` and `...Validator` next to the record.
  The folders sit inside the entity folder where a module has one (`Fleet.Application/Cars/Queries`), and the
  namespace follows the folder.
- What use cases share sits under the Application project's `Common` folder: the interfaces Infrastructure implements
  (`Common/Interfaces/ICarDal.cs`, `IImageStorage`, `ITokenHelper`) and the read models queries answer with,
  named `<Name>Response` (`Common/Models/CarDetailResponse.cs`). There are no DTOs.
- Endpoints and module APIs inject the handler they need, e.g. `ICommandHandler<CreateBrandCommand, Brand>`. There is
  no mediator.
- Handlers don't call each other; they use repositories, and endpoints combine use cases where needed (v1 login =
  `VerifyCredentialsQuery` + `CreateAccessTokenCommand`).
- `RegisterUseCases` registers every handler behind a Castle proxy, so aspect attributes on `HandleAsync` run around
  it. Castle calls an interceptor synchronously, so an aspect implements `InterceptAsync` and awaits the handler: the
  cache stores the finished result, the transaction commits once the task succeeds, and the timing covers all of it.
- `Rentar.ArchitectureTests` checks that no managers or services are left, that every handler is sealed and named
  after its record, that every command and query is named and filed after its kind, that every use case has exactly
  one handler, that every handler is registered, and the same for validators. It also checks that Application
  interfaces live in `Common.Interfaces`, that no type is named `*Dto`, that entity properties are `init`-only, and that
  every repository and module API method returns a `Task` and takes a `CancellationToken`.

<br>

**Companies and their IAM**

A company is registered by a signed-in user, who becomes its first employee. Employees are Identity users; a user can
work at several companies, and can also be a customer of one of them (`Customer.CompanyId`).

- **Permissions** are a fixed catalogue in `CompanyPermissions` (Companies contracts, since other modules check them):
  `company.manage`, `employees.read`, `employees.manage`, `rentals.read`.
- **Roles** belong to one company and grant a set of permissions; an employee holds one or more of them. Every company
  starts with copies of the predefined roles: **Owner** (every permission), **Admin** (`employees.*`, `rentals.read`)
  and **Member** (none). `Role.IsPredefined` tells them from roles a company creates, which is not exposed yet.
- An employee grants or removes only what they hold themselves: adding someone with a role whose permissions the caller
  lacks, or removing someone who holds such permissions, answers `403`. So an Admin can not make an Owner or remove one.
- A company always keeps an employee who holds every permission; removing the last one answers `400`.
- Use cases take the acting user as part of the record (`ActorUserId`), read from the access token by the endpoint, so
  a handler never trusts a body for who is asking.
- Adding a permission later: add it to `CompanyPermissions.All`, and grant it to the stored Owner roles in a migration,
  since roles are copied when a company registers.

<br>

**Validation**

Each rule has one home, chosen by what it needs:

| Rule | Lives in | Fails with |
| --- | --- | --- |
| About the input alone: required, length, format, how fields relate (return date after rent date) | The use case's validator, a `UseCaseValidator<TUseCase>` next to its record | `ValidationException`: 400 with every failure, grouped by field |
| Needs stored data: unique email, record exists (a car's brand and color, an image's car), a brand or color no car has before it is deleted, at most 5 images per car | The handler, backed by the database's keys and unique indexes | A Kernel exception (`ConflictException`, `NotFoundException`, `BusinessRuleException`, ...) |
| Must hold for an entity however it is changed | The domain, once entities have behavior. Today they are plain data, so none live there yet | |

- Validators run on their own. `RegisterUseCases` registers them, and a decorator (`RegisterUseCaseValidation`, called
  once in `Program.cs`) runs a use case's validators before its handler, ahead of any aspect. A use case with no
  validator passes. Handlers have no attribute to forget.
- Each property stops at its first failing rule, so an empty name is reported as missing, not also as too short.
- When several use cases take the same field, its rules live once in `<Entity>Rules` as `IRuleBuilder` extensions,
  e.g. `ValidBrandName()` for `CreateBrandCommand` and `UpdateBrandCommand`.
- The one exception is `UpdateColorCommand`, which checks its name in the handler: v1 has always answered it with
  `"Color Name is Invalid"`, and `V1ContractTests` pins that.

<br>

**Cross-cutting**

- Errors: use cases return plain values and throw the Kernel exceptions (`BusinessRuleException`, `NotFoundException`,
  `ConflictException`, `ForbiddenException`, `InvalidCredentialsException`, `UnavailableException`) when they refuse a request.
  `ApiExceptionHandler`, an ASP.NET Core `IExceptionHandler`, turns them into problem details for v2 and into the
  `{ success, message, data }` body for v1. `V1ContractTests` pins every v1 response.
- Aspects on use case handlers, run by Autofac/Castle interceptors: caching, transaction scope, performance, secured
  operation. Cache keys are named after the handler class, e.g.
  `...Cars.Queries.GetCarsByBrandQueryHandler.HandleAsync(GetCarsByBrandQuery { BrandId = 1 })`; the cancellation
  token is not part of the key. The transaction scope flows across awaits (`TransactionScopeAsyncFlowOption.Enabled`).
- Security: HMACSHA512 password hashing, JWT access tokens (Identity module)
- Uploaded car images are stored under `src/Bootstrapper/Rentar.Api/wwwroot/uploads` (`Paths:DefaultCarImagePath`)

**Dependency Resolver**: _Autofac_

**Validation**: _FluentValidation_

**Database Operations & Data Access Operations**: _EntityFrameworkCore (PostgreSQL / Npgsql)_
