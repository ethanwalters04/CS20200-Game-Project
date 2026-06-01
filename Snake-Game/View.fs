module SnakeGame.View

open System
open Domain

module private Theme =
    // Colors
    let playingTitleColor = ConsoleColor.Green
    let snakeHeadColor = ConsoleColor.Green
    let snakeBodyColor = ConsoleColor.DarkGreen
    let normalGoodFoodColor = ConsoleColor.DarkBlue
    let specialGoodFoodColor = ConsoleColor.Yellow
    let badFoodColor = ConsoleColor.DarkRed
    let wallColor = ConsoleColor.Gray

    let basicTextColor = ConsoleColor.DarkGray

    let deathFreezeTextColor = ConsoleColor.DarkRed

    let gameOverTitleColor = ConsoleColor.DarkRed

    let splashScreenTitleColor = ConsoleColor.Green
    let splashScreenSubtitleColor = ConsoleColor.Red

    let mainMenuTitleColor = ConsoleColor.White
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

let private withForegroundColor color f =
    let old = Console.ForegroundColor
    try
        Console.ForegroundColor <- color
        f()
    finally
        Console.ForegroundColor <- old

let private printElement (c: char) (color: ConsoleColor) =
    withForegroundColor color (fun () -> printf "%c" c)

// Retrieve the position of good food regardless of its type
let private getGoodFoodPosition = function
    | Normal position -> position
    | Special position -> position

// Determine whether a position contains a wall element
let private wallCell (boardConfig: BoardConfig) position =
    // Board edges
    let maxXBorder = boardConfig.Width - 1
    let maxYBorder = boardConfig.Height - 1
    
    match position with
    // Wall positions - corners
    | { X = 0; Y = 0 } -> Some (wallCornerTopLeft, wallColor)
    | { X = x; Y = 0 } when x = maxXBorder -> Some (wallCornerTopRight, wallColor)
    | { X = 0; Y = y } when y = maxYBorder -> Some (wallCornerBotLeft, wallColor)
    | { X = x; Y = y } when x = maxXBorder && y = maxYBorder -> Some (wallCornerBotRight, wallColor)

    // Wall positions - edges
    | { X = 0 } -> Some (wallVertical, wallColor)
    | { X = x } when x = maxXBorder -> Some (wallVertical, wallColor)
    | { Y = 0 } -> Some (wallHorizontal, wallColor)
    | { Y = y } when y = maxYBorder -> Some (wallHorizontal, wallColor)

    | _ -> None

let private drawSplashScreen () =
    withForegroundColor splashScreenTitleColor (fun () ->
        printfn ""
        printfn "  ____  _   _    _    _  _______ "
        printfn " / ___|| \\ | |  / \\  | |/ / ____|"
        printfn " \\___ \\|  \\| | / _ \\ | ' /|  _|  "
        printfn "  ___) | |\\  |/ ___ \\| . \\| |___ "
        printfn " |____/|_| \\_/_/   \\_\\_|\\_\\_____|"
    )

    withForegroundColor splashScreenSubtitleColor (fun () ->
        printfn "              By Ethan J Walters"
        printfn ""
    )

let private drawMainMenu () =
    withForegroundColor mainMenuTitleColor (fun () -> 
        printfn ""
        printfn "~~~ Welcome to Snake! ~~~"
    )
    withForegroundColor basicTextColor (fun () ->
        printfn "Your goal is to eat the blueberries and bananas to grow as long as possible."
        printfn "Blueberries give 10 points, while bananas give 50 points and clear all bad food from the board!"
        printfn "Avoid the red bad food - it will end the game if you eat it!"
        printfn "Use WASD to change direction. Press 'Q' to quit, and 'M' to return to the main menu at any time."
        printfn ""
        printfn "Select difficulty by pressing 1, 2, or 3:"
    )
    withForegroundColor mainMenuEasyColor (fun () ->
        printfn "1. Easy (slow movement)"
    )
    withForegroundColor mainMenuMediumColor (fun () ->
        printfn "2. Medium (medium movement)"
    )
    withForegroundColor mainMenuHardColor (fun () ->
        printfn "3. Hard (fast movement)"
        printfn ""
    )

let private drawGameOver finalScore =
    withForegroundColor gameOverTitleColor (fun () ->
        printfn ""
        printfn "~~~ Game Over! ~~~"
    )
    withForegroundColor basicTextColor (fun () ->
        printfn "Your final score: %d" finalScore
        printfn ""
        printfn "Press 'M' to return to the main menu,"
        printfn "or 'R' to restart."
        printfn ""
    )

let private drawPlaying (boardConfig: BoardConfig) snake currentGoodFood badFoods score isDead =
    withForegroundColor playingTitleColor (fun () ->
        printfn "Snake"
    )

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
                printf " "

        // Shift to next line after each row
        Console.WriteLine()
    
    if isDead then
        let msg = "GAME OVER"
        let startX = (boardConfig.Width - msg.Length) / 2
        let startY = (boardConfig.Height / 2) + 1 
        
        Console.SetCursorPosition(startX, startY)

        withForegroundColor deathFreezeTextColor (fun () ->
            printf "%s" msg
        )
        
        // Reset cursor to the bottom so the score prints in the correct location
        Console.SetCursorPosition(0, boardConfig.Height + 1)

    // Under-board information:
    withForegroundColor basicTextColor (fun () ->
        printfn "Current score: %d" score

        printf "Foods:  Normal ("
        withForegroundColor normalGoodFoodColor (fun () -> printf "%c" normalGoodFood)

        printf ")  Special ("
        withForegroundColor specialGoodFoodColor (fun () -> printf "%c" specialGoodFood)

        printf ")  Danger ("
        withForegroundColor badFoodColor (fun () -> printf "%c" badFood)

        printfn ")"
    )

let private draw (boardConfig: BoardConfig) (state: GameState) =
    Console.SetCursorPosition(0, 0)

    match state with
    | SplashScreen ->
        drawSplashScreen ()

    | MainMenu ->
        drawMainMenu ()

    | GameOver (finalScore, _, _, _) ->
        drawGameOver finalScore

    | DeathFreeze (snake, currentGoodFood, badFoods, score, _, _, _) ->
        drawPlaying boardConfig snake currentGoodFood badFoods score true

    | Playing (snake, currentGoodFood, badFoods, score, _, _, _) ->
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