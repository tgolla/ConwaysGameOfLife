# Coding Exercise: Conway's Game Of Life API


[TOC]

## Objective

Implement a RESTful API for Conway's Game of Life. Your solution should be designed with production readiness in mind. Ref:  https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life  

## Functional Requirements

The API should include (at a minimum) the following endpoints:  

1. Upload Board State - Accept a new board state (2D grid of cells). - Return a unique identifier for the stored board.
2. Get Next State - Given a board ID, return the next generation state of the board.  
3. Get N States Ahead - Given a board ID and a number N, return the board state after N generations.  
4. Get Final State - Return the final stable state of the board (i.e., when it no longer changes or cycles). - If the board does not reach a stable conclusion within a reasonable number of iterations,  return a suitable error message.

## Database

The service must persist the board state so they are not lost if the application is restarted or crashes. This requirement eliminates persisting the board state through any type of memory storage such as server sessions or client memory. The following database tables will be used to maintain the board state.

### Boards Table

This table maintains a record for each active board.

| Field   | Type     | Description                                                  |
| ------- | -------- | ------------------------------------------------------------ |
| Id      | Guid     | This is the board id. Primary key.                           |
| Expires | DateTime | This is the date and time the board expires such that it can be deleted from the database should it be abandoned. |

### LivePoints Table

This table maintains the current state of the board represented in live points on the grid (board). Points not recorded in this table are considered dead.

| Field | Type | Description                         |
| ----- | ---- | ----------------------------------- |
| Id    | Guid | Foreign key reference to Boards Id. |
| X     | Int  | X grid value.                       |
| Y     | Int  | Y grid value.                       |

## Services

### Rules

The universe of the Game of Life is [an infinite, two-dimensional orthogonal grid of square](https://en.wikipedia.org/wiki/Square_tiling) *cells*, each of which is in one of two possible states, *live* or *dead* (or *populated* and *unpopulated*, respectively). Every cell interacts with its eight *[neighbours](https://en.wikipedia.org/wiki/Moore_neighborhood)*, which are the cells that are horizontally, vertically, or diagonally adjacent. At each step in time, the following transitions occur:

1. Any live cell with fewer than two live neighbours dies, as if by underpopulation.
2. Any live cell with two or three live neighbours lives on to the next generation.
3. Any live cell with more than three live neighbours dies, as if by overpopulation.
4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.

The initial pattern constitutes the *seed* of the system. The first generation is created by applying the above rules simultaneously to every cell in the seed, live or dead; births and deaths occur simultaneously, and the discrete moment at which this happens is sometimes called a *tick*.[[nb 1\]](https://en.wikipedia.org/wiki/Conway's_Game_of_Life#cite_note-7) Each generation is a *[pure function](https://en.wikipedia.org/wiki/Pure_function)* of the preceding one. The rules continue to be applied repeatedly to create further generations. Ref:  https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life  

### ConwaysGameOfLifeService

Conway's Game of Life services layer provides the following functionality.

#### Seed

Seeding initializes the the pattern on the board which is saved in the database. The method is passed a list of live points and returns the game board id. All other points are considered dead.

#### Transition

The transition increments through 1 or more generations by applying the above rules simultaneously to every cell on the board, live or dead; births and deaths occur simultaneously. The resulting generation is returned as a list of live points and saved in the database.

#### End

Ending the game increments through 1 or more generations (`MaximunGenerationBeforeEnding`) until such time as it can return the final stable state of the board (i.e., when it no longer changes or cycles). If the board does not reach a stable conclusion within a reasonable number of iterations an error is returned. In either case the last iteration of live points is returned. Upon completion the game board is deleted from the database. 

## APIs

### POST /ConwaysGameOfLife

The POST method of `/ConwaysGameOfLife` is used to create a new game. The method seeds the board with the array of points passed in the body (JSON) and returns a Guid board id.

```
POST https://localhost:7034/ConwaysGameOfLife
```

Example Body - Block Pattern (Still Life)

```
[
  {
    "x": 0,
    "y": 0
  },
  {
    "x": 1,
    "y": 0
  },
  {
    "x": 0,
    "y": 1
  },
  {
    "x": 1,
    "y": 1
  }
]
```

Blinker Pattern (Oscillator)

```
[
  {
    "x": 0,
    "y": 0
  },
  {
    "x": 1,
    "y": 0
  },
  {
    "x": 2,
    "y": 0
  }
]
```

### Get /ConwaysGameOfLife

The GET method of `/ConwaysGameOfLife` transitions the game board form one generation to the next. The method is called with the board id.

```
GET https://localhost:7034/ConwaysGameOfLife?boardid=b251ff1b-348d-45a7-87c8-fe5c1b5ea553
```

The method can iterate through one or more generations by passing an optional iteration count.

```
GET https://localhost:7034/ConwaysGameOfLife?boardid=b251ff1b-348d-45a7-87c8-fe5c1b5ea553?iterations=15
```

If you want to retrieve the current generation without first transitioning to the next generation set the iterations count to zero.

```
GET https://localhost:7034/ConwaysGameOfLife?boardid=b251ff1b-348d-45a7-87c8-fe5c1b5ea553?iterations=0
```

### Delete /ConwaysGameOfLife

The DELETE method of `/ConwaysGameOfLife` ends the game (no longer changing or cycling) deleting it from the database and attempts to return the last generation. If the game can not be ended an error is returned. The method is called with the board id. 

```
DELETE https://localhost:7034/ConwaysGameOfLife?boardid=b251ff1b-348d-45a7-87c8-fe5c1b5ea553
```

A Postman collection `Conway's Game of Life.postman_collection.json` to test execution is located in the root folder.

## Project Setup

To run this project locally you will need to install SQL Server Express and create the `ConwaysGameOfLifeApiDb` database using the following Entity Framework Core command. Ref: `ConwaysGameOfLifeApiDbContext.cs`

```
update-database -context ConwaysGameOfLifeApiDbContext
```

This command should be run is Visual Studio using the Package Manager Console with the Default project set to `ConwaysGameOfLife.Data`. Before executing you will need to add the DB connection string to the `ConwaysGameOfLife` project User Secrets.

```json
  "ConnectionStrings": {
    "ConwaysGameOfLifeApiDb": "Data Source=localhost\\sqlexpress;Database=ConwaysGameOfLifeApiDb;Integrated Security=false;Encrypt=false;User ID=xxxxxxxx;Password=xxxxxxxx;"
  }
```

To do this in Visual Studio right click the `ConwaysGameOfLife` project in Solutions Explorer view and select Manage User Secrets. Paste in the connection string JSON replacing the User Id and Password with a SQL Server Express user.

It may also be necessary to add the `logs` folder to the `ConwaysGameOfLife` project folder. The folder is the location where Serilog is configured to store log files in the development environment.

## Future Enhancements

The follow future enhancements for the project have been noted.

- Review with team possible end game logic possibilities.
- Refactor service layer (duplicate code, single responsibility)
- Add function testing for the APIs with Playwright.
- Add more pattern unit tests.
- Add API call to clean out expired games.
- Build graphic frontend.

## Segue

While this project originated as a coding challenge, it has also proven useful as a training example. The following such examples have been implemented.

### Code Coverage

This project provides an excellent opportunity to demonstrate Visual Studio code coverage which was recently added to the 2026 Community, and Professional editions. Prior to this it was only available in Enterprise. Live execution of test is still only available in Enterprise.

For general information check out the https://learn.microsoft.com/ article [Determine code testing coverage - Visual Studio (Windows) | Microsoft Learn](https://learn.microsoft.com/en-us/visualstudio/test/using-code-coverage-to-determine-how-much-code-is-being-tested?view=visualstudio&tabs=csharp).

For an actual demo script check out [Visual Studio Code Coverage Demo](docs/Visual%20Studio%20Code%20Coverage%20Demo.md) in the `/docs` folder.

### .NET 10 Upgrade

The following are the steps need to upgrade this project from .NET 8 to 10. While Microsoft has dropped Swashbuckle from the newer .NET 10 templates, it can still be used and in this case simply upgraded in place.

1. The `TargetFramework` in each of the project files has been updated to .NET 10.

   `<TargetFramework>net10.0</TargetFramework>`

   And confirmed build.

2. Upgraded packages for each of the projects.

3. Modify `using Microsoft.OpenApi.Models;` in `Programs.cs` to `using Microsoft.OpenApi;`. 

4. Added `using NUnit.Framework.Legacy;` to `ConwaysGameOfLife.Services.UnitTests` for `CollectionAssert.AreEquivalent(expected, result);` which nUnit now considers legacy code ([NUnit2049: Consider using Assert.That(...) instead of CollectionAssert(...)](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2049.md)).

5. In `Program.cs` remove `builder.Services.AddEndpointsApiExplorer();` and change `app.UseSwagger();` to `app.MapSwagger();`.

6. Use `dotnet solution migrate` to create a `.slnx` solution file and delete the old `.sln`  file.

### Swagger Alternatives

Starting with .NET 9, ASP.NET Core no longer includes Swagger ([domaindrivendev/Swashbuckle.AspNetCore: Swagger tools for documenting API's built on ASP.NET Core](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)) by default in web API templates. And with .NET 10, Microsoft has doubled down on native OpenAPI support - now generating OpenAPI 3.1 documents out of the box with improved transformer APIs and better tooling. If you’re wondering what changed, whether Swagger is truly dead (spoiler: it’s not), and what the best alternatives are, keep reading to find out! (Ref: [ASP.NET Core Dropped Swagger - Here’s What Replaced It in .NET 10 - codewithmukesh](https://codewithmukesh.com/blog/dotnet-swagger-alternatives-openapi/))

> **TL;DR.** *.NET 10 ships with native OpenAPI 3.1 document generation via the* `Microsoft.AspNetCore.OpenApi` *package - no Swashbuckle needed. The recommended stack for .NET 10 Web APIs is:* **`Microsoft.AspNetCore.OpenApi` for document generation + Scalar for the UI***. Scalar (*`Scalar.AspNetCore` *2.14.x) is the modern Swagger UI replacement - cleaner, faster, with better request/response visualization and dark mode by default. Swashbuckle 10.x still works on .NET 10 but is no longer in the default template. Use NSwag if you need client SDK generation; use Redoc if you publish public docs. The mistake most teams make is keeping Swashbuckle on new .NET 10 projects out of habit - the native pipeline is genuinely better.* (Ref: [ASP.NET Core Dropped Swagger - Here’s What Replaced It in .NET 10 - codewithmukesh](https://codewithmukesh.com/blog/dotnet-swagger-alternatives-openapi/))

The following alternatives are all implemented in this example in addition to the original Swashbuckle implementation.

#### Microsoft OpenApi

Microsoft's .NET 10 recommendation has dropped Swashbuckle, in particular `.AddSwaggerGen()` and `app.MapSwagger();` which generate an OpenAPI JSON document in favor of Microsoft's `Microsoft.AspNetCore.OpenApi`. Microsoft's recommendation goes as far as to suggest that you should not even expose OpenAPI JSON documentation.

This is obviously a product decision. The following step outline the steps taken to add Microsoft's OpenAPI document generation.

1. Add the `Microsoft.AspNetCore.OpenApi` package to the project.
2. In `Program.cs` add `builder.Services.AddOpenApi();` after `builder.Services.AddControllers();`.
3. In `Program.cs` add `app.MapOpenApi();` after `var app = builder.Build();`.

These to step are all that is necessary to generate the OpenAPI JSON document. Run the application. You will of course be taken to the newer v10 implementation of the Swashbuckle Swagger page. Now browse to ` /openapi/v1.json` to see the Microsoft OpenAPI JSON document. Note the under the covers Swashbuckle uses a current .NET 10 version of `Microsoft.OpenApi`, still based on the version the OpenAPI JSON file version may vary.

The most notable thing about Microsoft's drop of Swashbuckle from it's templates is that while replacing the OpenAPI JSON document generation, they don't offer a UI solution. The following are possible replacements for the UI.

#### SwaggerUI

If you want to upgrade to using `Microsoft.AspNetCore.OpenApi` but continue to use the the Swashbuckle Swagger UI you will need add the  described above and the following `app.UseSwaggerUI()` line.  The key thing to note is that the OpenAPI endpoint has been changed to `/OpenApi/v1.json`.

```c#
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/OpenApi/v1.json", "Microsoft OpenAPI");
})
```

Note that this line varies in this example adding `RoutePrefix` and a `DocumentTitle` to keep the second defined Swagger UI from defaulting to the `\swagger` URL. In the example you will need to browser to `/swaggerOpenApi`. With Swashbuckle the end point defaults to `/swagger/v1/swagger.json`.

#### ReDoc

Redoc ([redoc/README.md at main · Redocly/redoc](https://github.com/Redocly/redoc/blob/main/README.md#redoc-options-object)) is an open source tool for generating documentation from OpenAPI (formerly Swagger) definitions.  Is should be noted that Redoc only provides documentation. It does not allow the user to easily execute API as Swashbuckle and the other alternatives discussed do.

You can add Redoc to your project by adding the `Redoc.AspNetCore` package ([NuGet Gallery | Redoc.AspNetCore 1.0.0](https://www.nuget.org/packages/Redoc.AspNetCore#readme-body-tab)) and the following lines to you `Program.cs` file.

```c#
app.UseReDoc(options =>
{
    options.SpecUrl = "/OpenApi/v1.json";
});
```

See documentation ([jonashendrickx/Redoc.AspNetCore](https://github.com/jonashendrickx/Redoc.AspNetCore)) for additional customization.

#### NSwag

*To Be Completed in the a future release.*

#### Scaler

Scaler ([Scalar.AspNetCore](https://www.nuget.org/packages/Scalar.AspNetCore)) is an open-source NuGet package that provides a modern, interactive API documentation UI for ASP.NET Core applications. It renders beautiful API references from OpenAPI/Swagger documents, offering an alternative to tools like Swagger UI and ReDoc.

You can add Scaler to your project by adding the `Scalar.AspNetCore` package ([NuGet Gallery | Scalar.AspNetCore 2.16.4](https://www.nuget.org/packages/Scalar.AspNetCore)) and the following lines to you `Program.cs` file.

```c#
app.MapScalarApiReference();
```

See [Scaler](https://scalar.com/) documentation for additional customization.

#### Summary 

No mater which UI you decide to use, you should seriously look at dropping Swashbuckle's OpenAPI JSON document generator in favor of Microsoft's.

## History

| Date      | Author        | Modification                                                 |
| --------- | ------------- | ------------------------------------------------------------ |
| 8/14/2025 | Terence Golla | Initial Draft Document                                       |
| 8/17/2025 | Terence Golla | Release 1.0                                                  |
| 6/4/2026  | Terence Golla | Added Segue documentation for code coverage.                 |
| 6/11/2026 | Terence Golla | Added Segue documentation for .NET 10 Upgrade (Swagger Options) and upgraded to .NET 10. |
| 6/13/2026 | Terence Golla | Added SwaggerUI using Microsoft OpenApi.                     |

