module View

open System
open Domain
open Engine

module Theme =
    // Colors
    let boardBackgroundColor = ConsoleColor.Black
    let snakeHeadColor = ConsoleColor.Green
    let snakeBodyColor = ConsoleColor.DarkGreen
    let normalGoodFoodColor = ConsoleColor.Red
    let specialGoodFoodColor = ConsoleColor.Yellow
    let badFoodColor = ConsoleColor.DarkBlue
    let wallColor = ConsoleColor.Gray
    
    // Characters
    let snakeHead = '$'
    let snakeBody = 'S'
    let normalGoodFood = '@'
    let specialGoodFood = ')'
    let badFood = 'X'
    let wallCornerTopLeft = '/'
    let wallCornerTopRight = '\\'
    let wallCornerBotLeft = '\\'
    let wallCornerBotRight = '/'
    let wallVertical = '|'
    let wallHorizontal = '-'

open Theme

let getCommand () : Command option =
    if not Console.KeyAvailable then 
        None
    else
        let keyInfo = Console.ReadKey(true)
        
        match keyInfo.Key with
        | ConsoleKey.W -> Some (ChangeDir Up)
        | ConsoleKey.S -> Some (ChangeDir Down)
        | ConsoleKey.A -> Some (ChangeDir Left)
        | ConsoleKey.D -> Some (ChangeDir Right)
        
        // StartGame takes (startingDelay, speedStep)
        | ConsoleKey.D1 -> Some (StartGame (150, 2)) // Easy
        | ConsoleKey.D2 -> Some (StartGame (100, 5)) // Medium
        | ConsoleKey.D3 -> Some (StartGame (60, 8))  // Hard
        
        | ConsoleKey.R -> Some RestartGame
        | ConsoleKey.Q -> Some QuitGame
        | _ -> None

let printElement (c: char) (color: ConsoleColor) =
    let originalForeground = Console.ForegroundColor
    let originalBackground = Console.BackgroundColor
    Console.ForegroundColor <- color
    Console.BackgroundColor <- boardBackgroundColor
    printf "%c" c

    // Preserve original console colors for the next element
    Console.ForegroundColor <- originalForeground
    Console.BackgroundColor <- originalBackground

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
    | GameOver (finalScore, _, _) ->
        printfn ""
        printfn "~~~ Game Over! ~~~"
        printfn "Your final score: %d" finalScore
        printfn ""
        printfn "Press 'R' to return to the main menu."
        printfn ""
    | Playing (snake, currentGoodFood, badFoods, score, _, _) ->
        printfn "Snake"
        
        // Extract the position of the good food regardless of its type
        let goodFoodPos = match currentGoodFood with | Normal p -> p | Special p -> p

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

                // Snake
                | _ when pos = snake.Head -> printElement snakeHead snakeHeadColor
                | _ when List.contains pos snake.Body -> printElement snakeBody snakeBodyColor
                
                // Bad Food
                | _ when List.exists (fun bf -> bf.Pos = pos) badFoods -> printElement badFood badFoodColor
                
                // Good Food
                | _ when pos = goodFoodPos ->
                    match currentGoodFood with
                    | Normal _ -> printElement normalGoodFood normalGoodFoodColor
                    | Special _ -> printElement specialGoodFood specialGoodFoodColor

                // Empty space
                | _ -> printElement ' ' boardBackgroundColor

            // Shift to next line after each row
            Console.WriteLine() 

        // Under-board information:
        printfn "Current score: %d" score
        printf "Foods:  Normal ("
        printElement normalGoodFood normalGoodFoodColor
        printf ")  Special ("
        printElement specialGoodFood specialGoodFoodColor
        printf ")  Danger ("
        printElement badFood badFoodColor
        printfn ")"

// Makes this renderer conform to the IGameRenderer interface, making it a valid rendering layer for the game
let consoleRenderer = 
    { new IGameRenderer with
        member this.Render(state) = draw state
        member this.GetCommand() = getCommand ()
    }