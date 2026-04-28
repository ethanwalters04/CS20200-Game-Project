module Program

open System
open Domain
open Engine
open View

[<EntryPoint>]
let main _ = 
    Console.CursorVisible <- false
    Console.Clear()
    
    // Testing
    // ----------------
    let fakeSnake = { 
        Head = {X=15; Y=7} 
        Body = [{X=15; Y=8}; {X=15; Y=9}; {X=15; Y=10}] 
        CurrentDir = Up 
    }
    
    let fakeState = Playing (fakeSnake, {X=20; Y=5}, 999, 100)
    
    View.draw fakeState
    
    printfn "\nTest complete."
    Console.ReadKey() |> ignore
    // ----------------
    
    Console.CursorVisible <- true
    Console.Clear()
    0