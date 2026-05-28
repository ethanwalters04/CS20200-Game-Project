module Engine

open System
open Domain

// Generate random positions indefinitely
let private randomPositions (randNumGen: Random) =
    Seq.unfold (fun _ -> 
        let x = randNumGen.Next(1, boardWidth - 1)
        let y = randNumGen.Next(1, boardHeight - 1)
        let pos = { X = x; Y = y }

        // Return the random position and continue the sequence
        Some (pos, ())
    ) ()

// Retrieve a random unoccupied position
let safePosition (randNumGen: Random) (occupied: Position list) =
    let maxSpaces = (boardWidth - 2) * (boardHeight - 2) // Get size of playable board area

    if List.length occupied >= maxSpaces then
        None // Board fully occupied - can't generate a safe position
    else
        randomPositions randNumGen
        |> Seq.filter (fun pos -> not (List.contains pos occupied)) // Filters out occupied positions
        |> Seq.tryHead // Gets the first position of the filtered sequence

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
let private initGame (randNumGen: Random) (startDelay, speedStep) =
    let initSnake = {
        Head = { X = boardWidth / 2; Y = boardHeight / 2 } // Head in centre of board
        Body = [
            { X = boardWidth / 2; Y = (boardHeight / 2) + 1 }
            { X = boardWidth / 2; Y = (boardHeight / 2) + 2 }
        ]
        CurrentDir = Up
    }

    match safePosition randNumGen (initSnake.Head :: initSnake.Body) with
    | Some initGoodFoodPos ->

        let isSpecial = randNumGen.Next(1,101) <= 20 // 20% chance of new food being special food
        let initGoodFood =
            if isSpecial then Special initGoodFoodPos
            else Normal initGoodFoodPos

        Playing (initSnake, initGoodFood, [], 0, startDelay, speedStep)

    | None ->
        GameOver (0, startDelay, speedStep) // Board full on initial spawn, e.g. if the playable area is too small, so end game immediately

let update (randNumGen: Random) (state: GameState) (cmd: Command) : GameState =
    match state, cmd with
    | MainMenu, StartGame (delay, speedStep) -> initGame randNumGen (delay, speedStep)
    | GameOver (_, delay, speedStep), RestartGame -> initGame randNumGen (delay, speedStep)
    | GameOver (_, delay, speedStep), ReturnToMainMenu -> MainMenu
    | Playing (_, _, _, _, delay, speedStep), ReturnToMainMenu -> MainMenu
    | _, QuitGame -> Quitting

    // Process direction change commands (when playing)
    | Playing (snake, food, badFoods, score, delay, step), ChangeDir newDir ->
        // Get actual direction rather than relying on the snake.CurrentDir, which may be inaccurate if the player makes multiple turn commands between ticks
        let actualDir = 
            match snake.Body with
            | [] -> snake.CurrentDir // If snake has no body
            | neck :: _ -> 
                // Compare the Head's pos to the neck's pos
                if snake.Head.X > neck.X then Right
                elif snake.Head.X < neck.X then Left
                elif snake.Head.Y > neck.Y then Down
                else Up
        
        if isValidTurn actualDir newDir then
            let newSnake = { snake with CurrentDir = newDir }
            Playing (newSnake, food, badFoods, score, delay, step)
        else
            state // Invalid turn - ignore command and keep current state
    
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
                        
                        // Try to find a safe pos for new bad food or skip if full
                        match safePosition randNumGen occupiedForBad with
                        | Some pos -> (10, { Pos = pos } :: badFoods)
                        | None -> (10, badFoods)
                        
                    | Special _ -> 
                        // Special: +50 Points, Clear all Bad Foods
                        (50, [])
                    
                let newScore = score + scoreIncrease
                let newDelay = Math.Max(30, delay - step)

                let newBadFoodPositions = List.map (fun bf -> bf.Pos) newBadFoods
                let occupiedForGood = newSnake.Head :: newSnake.Body @ newBadFoodPositions

                match safePosition randNumGen occupiedForGood with
                | Some newGoodFoodPos ->
                    // There is space so spawn the food and continue playing
                    let isSpecial = randNumGen.Next(1, 101) <= 20
                    let newFood = if isSpecial then Special newGoodFoodPos else Normal newGoodFoodPos
                    Playing (newSnake, newFood, newBadFoods, newScore, newDelay, step)
                    
                | None ->
                    // Board full - must end game
                    GameOver (newScore, newDelay, step)
            else
                let newBody = snake.Head :: (List.take (List.length snake.Body - 1) snake.Body)
                let newSnake = { snake with Head = newHead; Body = newBody }

                Playing (newSnake, food, badFoods, score, delay, step)
    | _ -> state
                