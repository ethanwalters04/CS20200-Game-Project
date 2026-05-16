module Engine

open System
open Domain

let private rand = Random()

// Generate random positions indefinitely
let private randomPositions =
    Seq.unfold (fun _ -> 
        let x = rand.Next(1, boardWidth - 1)
        let y = rand.Next(1, boardHeight - 1)
        let pos = { X = x; Y = y }

        // Return the random position and continue the sequence
        Some (pos, ())
    ) ()

// Retrieve a random unoccupied position
let safePosition (occupied: Position list) =
    randomPositions
    |> Seq.filter (fun pos -> not (List.contains pos occupied)) // Filters out occupied positions
    |> Seq.head // Gets the first position of the filtered sequence

// Check if a position is out of bounds
let isOutOfBounds (pos: Position) =
    pos.X <= 0 || pos.X >= boardWidth - 1 || pos.Y <= 0 || pos.Y >= boardHeight - 1

// Check if a turn is valid (not 180 degrees)
let isValidTurn (currentDir: Direction) (newDir: Direction) =
    match currentDir, newDir with
    | Up, Down -> false
    | Down, Up -> false
    | Left, Right -> false
    | Right, Left -> false
    | _ -> true

// Move the snake's head in the current direction
let private moveHead (pos: Position) (dir: Direction) =
    match dir with
    | Up -> { pos with Y = pos.Y - 1 }
    | Down -> { pos with Y = pos.Y + 1 }
    | Left -> { pos with X = pos.X - 1 }
    | Right -> { pos with X = pos.X + 1 }

// Set up the initial game state
let private initGame (startDelay: int, speedStep: int) =
    let initSnake = {
        Head = { X = boardWidth / 2; Y = boardHeight / 2 } // Head in centre of board
        Body = [
            { X = boardWidth / 2; Y = (boardHeight / 2) + 1 }
            { X = boardWidth / 2; Y = (boardHeight / 2) + 2 }
        ]
        CurrentDir = Up
    }

    let initGoodFoodPos = safePosition (initSnake.Head :: initSnake.Body)
    let isSpecial = rand.Next(1, 101) <= 20 // 20% chance of special food
    let initGoodFood = if isSpecial then Special initGoodFoodPos else Normal initGoodFoodPos

    Playing (initSnake, initGoodFood, [], 0, startDelay, speedStep)

let update (state: GameState) (cmd: Command) : GameState =
    match state, cmd with
    | MainMenu, StartGame (delay, speedStep) -> initGame (delay, speedStep)
    | GameOver (_, delay, speedStep), RestartGame -> initGame (delay, speedStep)
    | GameOver (_, delay, speedStep), ReturnToMainMenu -> MainMenu
    | Playing (_, _, _, _, delay, speedStep), ReturnToMainMenu -> MainMenu
    | _, QuitGame -> Environment.Exit(0); state

    | Playing (snake, food, badFoods, score, delay, step), ChangeDir newDir ->
        if isValidTurn snake.CurrentDir newDir then
            Playing ({ snake with CurrentDir = newDir }, food, badFoods, score, delay, step)
        else
            state
    
    | Playing (snake, food, badFoods, score, delay, step), Tick ->
        let newHead = moveHead snake.Head snake.CurrentDir

        if isOutOfBounds newHead 
            || List.exists (fun pos -> pos = newHead) snake.Body
            || List.exists (fun bf -> bf.Pos = newHead) badFoods then
                GameOver (score, delay, step)
        else
            let goodFoodPos = match food with | Normal p -> p | Special p -> p

            if newHead = goodFoodPos then
                let newBody = snake.Head :: snake.Body
                let newSnake = { snake with Head = newHead; Body = newBody }

                let scoreIncrease, newBadFoods =
                    match food with
                    | Normal _ -> 
                        // Normal: +10 Points, Spawn 1 Bad Food
                        let badFoodPositions = List.map (fun bf -> bf.Pos) badFoods
                        let occupiedForBad = newSnake.Head :: newSnake.Body @ badFoodPositions
                        let newBadFood = { Pos = safePosition occupiedForBad }
                        (10, newBadFood :: badFoods)
                        
                    | Special _ -> 
                        // Special: +50 Points, Clear all Bad Foods
                        (50, [])
                    
                let newScore = score + scoreIncrease

                let newBadFoodPositions = List.map (fun bf -> bf.Pos) newBadFoods
                let occupiedForGood = newSnake.Head :: newSnake.Body @ newBadFoodPositions
                let newGoodFoodPos = safePosition occupiedForGood

                let isSpecial = rand.Next(1, 101) <= 20
                let newFood = if isSpecial then Special newGoodFoodPos else Normal newGoodFoodPos
                
                let newDelay = Math.Max(30, delay - step)
                
                Playing (newSnake, newFood, newBadFoods, newScore, newDelay, step)
            else
                let newBody = snake.Head :: (List.take (List.length snake.Body - 1) snake.Body)
                let newSnake = { snake with Head = newHead; Body = newBody }

                Playing (newSnake, food, badFoods, score, delay, step)
    | _ -> state
                