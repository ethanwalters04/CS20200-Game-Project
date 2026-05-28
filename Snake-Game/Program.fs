module Program

open System
open System.Threading
open Domain
open Engine
open View

// Create the game as an actor that processes commands and updates the game state accordingly
let createGameActor (renderer: IGameRenderer) (initialState: GameState) =
    MailboxProcessor.Start(fun inbox ->
        let rec loop (state: GameState) = async {
            renderer.Render state // Render the game according to its current state

            let timeout =
                match state with
                | Playing (snake, _, _, _, delay, _) -> 
                    match snake.CurrentDir with
                    | Up | Down -> int (delay + (delay / 3.5)) // Characters aren't perfectly square, so vertical movement is slightly slower to feel more natural
                    | Left | Right -> int delay
                | _ -> Timeout.Infinite

            let! msg = inbox.TryReceive(timeout) // Wait for a command or timeout for the next tick

            let command =
                match msg with
                | Some cmd -> cmd
                | None -> Tick

            let nextState = Engine.update state command

            match state, nextState with
            | MainMenu, Playing _ -> 
                Console.Clear() // Clear console to get rid of stray text around game board
            | Playing _, GameOver _ -> 
                do! Async.Sleep(1500) // Extra delay on game over to allow user to process what happened
                Console.Clear()
            | GameOver _, Playing _ -> Console.Clear()
            | GameOver _, MainMenu -> Console.Clear()
            | _, Quitting -> Environment.Exit(0)
            | _ -> ()

            return! loop nextState
        }

        loop initialState
    )
                
[<EntryPoint>]
let main _ = 
    Console.CursorVisible <- false
    Console.Clear()
    
    let renderer = consoleRenderer
    let gameActor = createGameActor renderer MainMenu

    let rec inputLoop () =
        match renderer.GetCommand() with
        | Some cmd -> gameActor.Post cmd
        | None -> ()

        Thread.Sleep(15)
        inputLoop ()
    
    inputLoop ()