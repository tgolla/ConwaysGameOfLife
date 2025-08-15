# NetDocuments Coding Exercise: Conway's Game Of Life API


[TOC]

## Objective

Implement a RESTful API for Conway's Game of Life. Your solution should be designed with  production readiness in mind. Reference:  https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life  

## Functional Requirements

The API should include (at a minimum) the following endpoints:  

1. Upload Board State - Accept a new board state (2D grid of cells). - Return a unique identifier for the stored board.
2. Get Next State - Given a board ID, return the next generation state of the board.  
3. Get N States Ahead - Given a board ID and a number N, return the board state after N generations.  
4. Get Final State - Return the final stable state of the board (i.e., when it no longer changes or cycles). - If the board does not reach a stable conclusion within a reasonable number of iterations,  return a suitable error message.

## Database

The service must persist board states so they are not lost if the application is restarted or crashes. This requirement eliminates persisting the board state through any type of memory storage such as server sessions or client memory. The following database tables will be used to maintain board states.

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

TBD

## APIs

TBD

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

***** **Incomplete**

## History

| Date      | Author        | Modification           |
| --------- | ------------- | ---------------------- |
| 8/14/2025 | Terence Golla | Initial Draft Document |
|           |               |                        |

