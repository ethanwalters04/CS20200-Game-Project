module SnakeGame.View

open System
open Domain

module private Theme =
    // Colors
    let boardBackgroundColor = ConsoleColor.Black
    let snakeHeadColor = ConsoleColor.Green
    let snakeBodyColor = ConsoleColor.DarkGreen
    let normalGoodFoodColor = ConsoleColor.DarkBlue
    let specialGoodFoodColor = ConsoleColor.Yellow
    let badFoodColor = ConsoleColor.DarkRed
    let wallColor = ConsoleColor.Gray

    let deathFreezeTextColor = ConsoleColor.DarkRed

    let splashScreenTitleColor = ConsoleColor.Green
    let splashScreenSubtitleColor = ConsoleColor.Red

    let mainMenuTitleColor = ConsoleColor.White
    let mainMenuTextColor = ConsoleColor.DarkGray
    let mainMenuEasyColor = ConsoleColor.DarkGreen
    let mainMenuMediumColor = ConsoleColor.DarkYellow
    let mainMenuHardColor = ConsoleColor.DarkRed

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
        
        | ConsoleKey.D1 -> Some (StartGame Easy)
        | ConsoleKey.D2 -> Some (StartGame Medium)
        | ConsoleKey.D3 -> Some (StartGame Hard)
        
        | ConsoleKey.M -> Some ReturnToMainMenu
        | ConsoleKey.R -> Some RestartGame
        | ConsoleKey.Q -> Some QuitGame

        | ConsoleKey.Enter -> Some SkipOrNext

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
let private wallCell (boardConfig: BoardConfig) position =
    match position with
    // Wall positions - corners
    | { X = 0; Y = 0 } -> Some (wallCornerTopLeft, wallColor)
    | { X = x; Y = 0 } when x = boardConfig.Width - 1 -> Some (wallCornerTopRight, wallColor)
    | { X = 0; Y = y } when y = boardConfig.Height - 1 -> Some (wallCornerBotLeft, wallColor)
    | { X = x; Y = y } when x = boardConfig.Width - 1 && y = boardConfig.Height - 1 -> Some (wallCornerBotRight, wallColor)

    // Wall positions - edges
    | { X = 0 } -> Some (wallVertical, wallColor)
    | { X = x } when x = boardConfig.Width - 1 -> Some (wallVertical, wallColor)
    | { Y = 0 } -> Some (wallHorizontal, wallColor)
    | { Y = y } when y = boardConfig.Height - 1 -> Some (wallHorizontal, wallColor)

    | _ -> None

let private drawSplashScreen () =
    let originalForeground = Console.ForegroundColor

    printfn ""
    Console.ForegroundColor <- splashScreenTitleColor
    printfn "  ____  _   _    _    _  _______ "
    printfn " / ___|| \\ | |  / \\  | |/ / ____|"
    printfn " \\___ \\|  \\| | / _ \\ | ' /|  _|  "
    printfn "  ___) | |\\  |/ ___ \\| . \\| |___ "
    printfn " |____/|_| \\_/_/   \\_\\_|\\_\\_____|"
    Console.ForegroundColor <- splashScreenSubtitleColor
    printfn "              By Ethan J Walters"
    Console.ForegroundColor <- originalForeground
    printfn ""

let private drawMainMenu () =
    let originalForeground = Console.ForegroundColor

    printfn ""
    Console.ForegroundColor <- mainMenuTitleColor
    printfn "~~~ Welcome to Snake! ~~~"
    Console.ForegroundColor <- mainMenuTextColor
    printfn "Your goal is to eat the blueberries and bananas to grow as long as possible."
    printfn "Blueberries give 10 points, while bananas give 50 points and clear all bad food from the board!"
    printfn "Avoid the red bad food - it will end the game if you eat it!"
    printfn "Use WASD to change direction. Press 'Q' to quit."
    printfn ""
    printfn "Select difficulty by pressing 1, 2, or 3:"
    Console.ForegroundColor <- mainMenuEasyColor
    printfn "1. Easy (slow movement)"
    Console.ForegroundColor <- mainMenuMediumColor
    printfn "2. Medium (medium movement)"
    Console.ForegroundColor <- mainMenuHardColor
    printfn "3. Hard (fast movement)"
    Console.ForegroundColor <- originalForeground
    printfn ""

let private drawGameOver finalScore =
    printfn ""
    printfn "~~~ Game Over! ~~~"
    printfn "Your final score: %d" finalScore
    printfn ""
    printfn "Press 'M' to return to the main menu."
    printfn ""

let private drawPlaying (boardConfig: BoardConfig) snake currentGoodFood badFoods score isDead =
    printfn "Snake"

    let goodFoodPos = getGoodFoodPosition currentGoodFood
    let snakeBodySet = Set.ofList snake.Body // Set faster than list for lookups
    let badFoodSet = badFoods |> List.map (fun bf -> bf.Pos) |> Set.ofList // Set faster than list for lookups

    for y in 0 .. boardConfig.Height - 1 do
        for x in 0 .. boardConfig.Width - 1 do
            let pos = { X = x; Y = y }

            // Wall cells - take priority to prevent overlap by other elements
            match wallCell boardConfig pos with
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
    
    if isDead then
        let msg = "GAME OVER"
        let startX = (boardConfig.Width - msg.Length) / 2
        let startY = (boardConfig.Height / 2) + 1 
        
        Console.SetCursorPosition(startX, startY)
        let oldForeground = Console.ForegroundColor
        Console.ForegroundColor <- deathFreezeTextColor
        printf "%s" msg
        Console.ForegroundColor <- oldForeground
        
        // Reset cursor to the bottom so the score prints in the correct location
        Console.SetCursorPosition(0, boardConfig.Height + 1)

    // Under-board information:
    printfn "Current score: %d" score
    printf "Foods:  Normal ("
    printElement normalGoodFood normalGoodFoodColor
    printf ")  Special ("
    printElement specialGoodFood specialGoodFoodColor
    printf ")  Danger ("
    printElement badFood badFoodColor
    printfn ")"

let private draw (boardConfig: BoardConfig) (state: GameState) =
    Console.SetCursorPosition(0, 0)

    match state with
    | SplashScreen ->
        drawSplashScreen ()

    | MainMenu ->
        drawMainMenu ()

    | GameOver (finalScore, _, _) ->
        drawGameOver finalScore

    | DeathFreeze (snake, currentGoodFood, badFoods, score, _, _) ->
        drawPlaying boardConfig snake currentGoodFood badFoods score true

    | Playing (snake, currentGoodFood, badFoods, score, _, _) ->
        drawPlaying boardConfig snake currentGoodFood badFoods score false

    | Quitting ->
        ()

// Makes this renderer conform to the IGameRenderer interface, making it a valid rendering layer for the game
// It's also the only exposed thing in this module
let createConsoleRenderer (boardConfig: BoardConfig) =
    { new IGameRenderer with
        member _.Render(state) = draw boardConfig state
        member _.GetCommand() = getCommand ()
    }