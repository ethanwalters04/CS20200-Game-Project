module View

open System
open Domain

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

let private getCommand () : Command option =
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
        
        | ConsoleKey.M -> Some ReturnToMainMenu
        | ConsoleKey.R -> Some RestartGame
        | ConsoleKey.Q -> Some QuitGame
        | _ -> None

let private printElement (c: char) (color: ConsoleColor) =
    let originalForeground = Console.ForegroundColor
    let originalBackground = Console.BackgroundColor
    Console.ForegroundColor <- color
    Console.BackgroundColor <- boardBackgroundColor
    printf "%c" c

    // Preserve original console colors for the next element
    Console.ForegroundColor <- originalForeground
    Console.BackgroundColor <- originalBackground

// Retrieve the position of good food regardless of its type
let private getGoodFoodPosition = function
    | Normal position -> position
    | Special position -> position

// Determine whether a position contains a wall element
let private wallCell position =
    match position with
    // Wall positions - corners
    | { X = 0; Y = 0 } -> Some (wallCornerTopLeft, wallColor)
    | { X = x; Y = 0 } when x = boardWidth - 1 -> Some (wallCornerTopRight, wallColor)
    | { X = 0; Y = y } when y = boardHeight - 1 -> Some (wallCornerBotLeft, wallColor)
    | { X = x; Y = y } when x = boardWidth - 1 && y = boardHeight - 1 -> Some (wallCornerBotRight, wallColor)

    // Wall positions - edges
    | { X = 0 } -> Some (wallVertical, wallColor)
    | { X = x } when x = boardWidth - 1 -> Some (wallVertical, wallColor)
    | { Y = 0 } -> Some (wallHorizontal, wallColor)
    | { Y = y } when y = boardHeight - 1 -> Some (wallHorizontal, wallColor)

    | _ -> None

let private drawMainMenu () =
    Console.Clear()
    printfn ""
    printfn "~~~ Welcome to Snake! ~~~"
    printfn "Your goal is to eat the apples and bananas to grow as long as possible."
    printfn "Apples give 10 points, while bananas give 50 points and clear all bad food from the board!"
    printfn "Avoid the blue bad food - it will end the game if you eat it!"
    printfn "Use WASD to change direction. Press 'Q' to quit."
    printfn ""
    printfn "Select difficulty by pressing 1, 2, or 3:"
    printfn "1. Easy (slow movement)"
    printfn "2. Medium (medium movement)"
    printfn "3. Hard (fast movement)"
    printfn ""

let private drawGameOver finalScore =
    Console.Clear()
    printfn ""
    printfn "~~~ Game Over! ~~~"
    printfn "Your final score: %d" finalScore
    printfn ""
    printfn "Press 'M' to return to the main menu."
    printfn ""

let private drawPlaying snake currentGoodFood badFoods score =
    printfn "Snake"

    let goodFoodPos = getGoodFoodPosition currentGoodFood
    let snakeBodySet = Set.ofList snake.Body // Set faster than list for lookups
    let badFoodSet = badFoods |> List.map (fun bf -> bf.Pos) |> Set.ofList // Set faster than list for lookups

    for y in 0 .. boardHeight - 1 do
        for x in 0 .. boardWidth - 1 do
            let pos = { X = x; Y = y }

            // Wall cells - take priority to prevent overlap by other elements
            match wallCell pos with
            | Some (character, color) ->
                printElement character color

            // Snake
            | None when pos = snake.Head ->
                printElement snakeHead snakeHeadColor
            | None when Set.contains pos snakeBodySet ->
                printElement snakeBody snakeBodyColor

            // Bad Food
            | None when Set.contains pos badFoodSet ->
                printElement badFood badFoodColor

            // Good Food
            | None when pos = goodFoodPos ->
                match currentGoodFood with
                | Normal _ ->
                    printElement normalGoodFood normalGoodFoodColor
                | Special _ ->
                    printElement specialGoodFood specialGoodFoodColor

            // Empty space
            | None ->
                printElement ' ' boardBackgroundColor

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

let private draw (state: GameState) =
    Console.SetCursorPosition(0, 0)

    match state with
    | MainMenu ->
        drawMainMenu ()

    | GameOver (finalScore, _, _) ->
        drawGameOver finalScore

    | Playing (snake, currentGoodFood, badFoods, score, _, _) ->
        drawPlaying snake currentGoodFood badFoods score

    | Quitting ->
        ()

// Makes this renderer conform to the IGameRenderer interface, making it a valid rendering layer for the game
// It's also the only exposed thing in this module
let consoleRenderer =
    { new IGameRenderer with
        member _.Render(state) = draw state
        member _.GetCommand() = getCommand ()
    }