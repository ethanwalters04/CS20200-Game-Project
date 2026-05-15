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

// Boolean function - check if position is out of bounds
let isOutOfBounds (pos: Position) =
    pos.X <= 0 || pos.X >= boardWidth - 1 || pos.Y <= 0 || pos.Y >= boardHeight - 1
    
