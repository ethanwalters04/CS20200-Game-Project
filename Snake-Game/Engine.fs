module Engine

open System
open Domain

let private rand = Random()

// Generate random positions indefinitely
let private randomPositions =
    Seq.unfold (fun _ -> 
        let x = rand.Next(1, boardWidth - 1)
        let y = rand.Next(1, boardHeight - 1)
        let pos = { X = x; Y = y }

        // Return the random position and continue the sequence
        Some (pos, ())
    ) ()

// Retrieve a random unoccupied position
let safePosition (occupied: Position list) =
    randomPositions
    |> Seq.filter (fun pos -> not (List.contains pos occupied)) // Filters out occupied positions
    |> Seq.head // Gets the first position of the filtered sequence

// Check if a position is out of bounds
let isOutOfBounds (pos: Position) =
    pos.X <= 0 || pos.X >= boardWidth - 1 || pos.Y <= 0 || pos.Y >= boardHeight - 1

// Set up the initial game state
let private initGame (startDelay: int, speedStep: int) =
    let initSnake = {
        Head = { X = boardWidth / 2; Y = boardHeight / 2 } // Head in centre of board
        Body = [
            { X = boardWidth / 2; Y = (boardHeight / 2) + 1 }
            { X = boardWidth / 2; Y = (boardHeight / 2) + 2 }
        ]
        CurrentDir = Up
    }

    let initGoodFoodPos = safePosition (initSnake.Head :: initSnake.Body)
    let initGoodFood = Normal initGoodFoodPos

    let initBadFoodPos = safePosition (initGoodFoodPos :: initSnake.Head :: initSnake.Body)
    let initBadFood = { Pos = initBadFoodPos }

    Playing (initSnake, initGoodFood, [initBadFood], 0, startDelay, speedStep)
