module SnakeGame.Program

open System
open System.Threading
open Domain
open View

// Create the game as an actor that processes commands and updates the game state accordingly
let private createGameActor (config: GameConfig) (difficultyConfigFor: Difficulty -> DifficultyConfig) (randNumGen: Random) (renderer: IGameRenderer) (initialState: GameState) =
    MailboxProcessor.Start(fun inbox ->
        let rec loop (state: GameState) = async {
            renderer.Render state // Render the game according to its current state

            let timeout =
                match state with
                | Playing (snake, _, _, _, delay, _, _) -> 
                    match snake.CurrentDir with
                    | Up | Down -> int (delay * 1.8) // Characters aren't perfectly square, so vertical movement is slightly slower to feel more natural
                    | Left | Right -> int delay
                | SplashScreen -> 3000 // Splash screen delay placed here to allow optional early exit if user presses Enter immediately
                | DeathFreeze (_, _, _, _, delay, _, _) -> 3000 // Death freeze delay placed here to allow user to see what they collided with before proceeding to game over screen
                | _ -> Timeout.Infinite

            let! msg = inbox.TryReceive(timeout) // Wait for a command or timeout for the next tick

            let command =
                match msg with
                | Some cmd -> cmd
                | None -> Tick

            let nextState = Engine.update config difficultyConfigFor randNumGen state command

            match state, nextState with
            | SplashScreen, MainMenu -> Console.Clear()
            | MainMenu, Playing _ -> Console.Clear()
            | Playing _, GameOver _ -> Console.Clear()
            | DeathFreeze _, GameOver _ -> Console.Clear()
            | GameOver _, Playing _ -> Console.Clear()
            | GameOver _, MainMenu -> Console.Clear()
            | _ -> ()

            match nextState with
            | Quitting -> () // Don't recurse if quitting
            | _ -> return! loop nextState
        }

        loop initialState
    )
                
[<EntryPoint>]
let main _ = 
    Console.CursorVisible <- false
    Console.Clear()
    
    let randNumGen = Random()
    let config = {
        Board = {
            Width = 35
            Height = 15
        }

        Score = {
            NormalGoodFoodScore = 10
            SpecialGoodFoodScore = 50
        }
    }

    let difficultyConfig = function
        | Easy ->
            { StartDelay = 140.0; SpeedStep = 2.0 }
        | Medium ->
            { StartDelay = 100.0; SpeedStep = 4.5 }
        | Hard ->
            { StartDelay = 75.0; SpeedStep = 7.0 }

    let renderer = createConsoleRenderer config.Board
    let gameActor = createGameActor config difficultyConfig randNumGen renderer SplashScreen

    let rec inputLoop () =
        match renderer.GetCommand() with
        | Some QuitGame -> 
            gameActor.Post QuitGame 
            () // Don't recurse if quitting
                
        | Some cmd -> 
            gameActor.Post cmd
            Thread.Sleep(15)
            inputLoop () // Keep recursing for all other commands
                
        | None -> 
            Thread.Sleep(15)
            inputLoop () // Keep recursing if no key was pressed
        
    inputLoop ()

    0 // Exit code