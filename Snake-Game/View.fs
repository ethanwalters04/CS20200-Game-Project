module View

open System
open Domain
open Engine

let getCommand () : Command option =
    None // implement

let draw (state: GameState) =
    Console.SetCursorPosition(0, 0)
    match state with
    | MainMenu ->
        printfn ""
        printfn "~~~ Welcome to Snake! ~~~"
        printfn "Your goal is to eat the food (red) and grow as long as possible."
        printfn "Use WASD to change direction. Press 'Q' to quit."
        printfn ""
        printfn "Select difficulty by pressing 1, 2, or 3:"
        printfn "1. Easy (slow movement)"
        printfn "2. Medium (medium movement)"
        printfn "3. Hard (fast movement)"
        printfn ""
    | GameOver finalScore ->
        printfn ""
        printfn "~~~ Game Over! ~~~"
        printfn "Your final score: %d" finalScore
        printfn ""
        printfn "Press 'R' to return to the main menu."
        printfn ""
    | Playing (snake, food, score, _) ->
        printfn "Snake"
        for y in 0 .. boardHeight - 1 do
            for x in 0 .. boardWidth - 1 do
                let pos = { X = x; Y = y }

                match pos with
                // Wall positions - corners and edges
                | { X = 0; Y = 0 } -> printf "/" // Top left corner
                | { X = x; Y = 0 } when x = boardWidth - 1 -> printf "\\" // Top right corner
                | { X = 0; Y = y } when y = boardHeight - 1 -> printf "\\" // Bottom left corner
                | { X = x; Y = y } when x = boardWidth - 1 && y = boardHeight - 1 -> printf "/" // Bottom right corner
                | { X = 0 } -> printf "|" // Left edge
                | { X = x } when x = boardWidth - 1 -> printf "|" // Right edge
                | { Y = 0 } -> printf "-" // Top edge
                | { Y = y } when y = boardHeight - 1 -> printf "-" // Bottom edge

                // Game elements - snake head, body, and food
                | pos when pos = snake.Head -> printf "$"
                | pos when List.contains pos snake.Body -> printf "S"
                | pos when pos = food -> printf "@"
                
                // Empty space
                | _ -> printf " "

            Console.WriteLine()

        printfn "Current score: %d" score