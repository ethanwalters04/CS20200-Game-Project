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
                | Playing (_, _, _, _, delay, _) -> delay
                | _ -> Timeout.Infinite

            let! msg = inbox.TryReceive(timeout) // Wait for a command or timeout for the next tick

            let command =
                match msg with
                | Some cmd -> cmd
                | None -> Tick

            let nextState = Engine.update state command

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