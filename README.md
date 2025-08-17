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


## History

| Date      | Author        | Modification           |
| --------- | ------------- | ---------------------- |
| 8/14/2025 | Terence Golla | Initial Draft Document |
| 8/17/2025 | Terence Golla | Release 1.0            |
|           |               |                        |

