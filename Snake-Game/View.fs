module View

open System
open Domain
open Engine

module Theme =
    // Colors
    let snakeHeadColor = ConsoleColor.Green
    let snakeBodyColor = ConsoleColor.DarkGreen
    let foodColor = ConsoleColor.Red
    let wallColor = ConsoleColor.White
    
    // Characters
    let snakeHead = '$'
    let snakeBody = 'S'
    let foodStuff = '@'
    let wallCornerTopLeft = '/'
    let wallCornerTopRight = '\\'
    let wallCornerBotLeft = '\\'
    let wallCornerBotRight = '/'
    let wallVertical = '|'
    let wallHorizontal = '-'

let getCommand () : Command option =
    None // implement

let printElement (c: char) (color: ConsoleColor) =
    let originalColor = Console.ForegroundColor
    Console.ForegroundColor <- color
    printf "%c" c
    Console.ForegroundColor <- originalColor

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
                // Wall positions - corners 
                | { X = 0; Y = 0 } -> printElement wallCornerTopLeft wallColor 
                | { X = x; Y = 0 } when x = boardWidth - 1 -> printElement wallCornerTopRight wallColor 
                | { X = 0; Y = y } when y = boardHeight - 1 -> printElement wallCornerBotLeft wallColor 
                | { X = x; Y = y } when x = boardWidth - 1 && y = boardHeight - 1 -> printElement wallCornerBotRight wallColor 

                // Wall positions - edges
                | { X = 0 } -> printElement wallVertical wallColor 
                | { X = x } when x = boardWidth - 1 -> printElement wallVertical wallColor 
                | { Y = 0 } -> printElement wallHorizontal wallColor 
                | { Y = y } when y = boardHeight - 1 -> printElement wallHorizontal wallColor 

                // Game elements - snake head, body, and food
                | _ when pos = snake.Head -> printElement snakeHead snakeHeadColor
                | _ when List.contains pos snake.Body -> printElement snakeBody snakeBodyColor
                | _ when pos = food -> printElement foodStuff foodColor

                // Empty space
                | _ -> printf " "

            Console.WriteLine()

        printfn "Current score: %d" score