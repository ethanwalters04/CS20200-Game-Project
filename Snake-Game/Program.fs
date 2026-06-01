module SnakeGame.Program

open System
open System.Threading
open Domain
open Engine
open View

// Create the game as an actor that processes commands and updates the game state accordingly
let private createGameActor (config: GameConfig) (difficultyConfigFor: Difficulty -> DifficultyConfig) (randNumGen: Random) (renderer: IGameRenderer) (initialState: GameState) =
    MailboxProcessor.Start(fun inbox ->
        let rec loop (state: GameState) = async {
            renderer.Render state // Render the game according to its current state

            let timeout =
                match state with
                | Playing (snake, _, _, _, delay, _) -> 
                    match snake.CurrentDir with
                    | Up | Down -> int (delay + (delay / 3.5)) // Characters aren't perfectly square, so vertical movement is slightly slower to feel more natural
                    | Left | Right -> int delay
                | SplashScreen -> 3000 // Splash screen delay placed here to allow optional early exit if user presses Enter immediately
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
            | Playing _, GameOver _ -> 
                do! Async.Sleep(1500) // Extra delay on game over to allow user to process what happened
                Console.Clear()
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
            { StartDelay = 150.0; SpeedStep = 2.0 }
        | Medium ->
            { StartDelay = 100.0; SpeedStep = 5.0 }
        | Hard ->
            { StartDelay = 60.0; SpeedStep = 8.0 }

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