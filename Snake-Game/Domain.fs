module Domain

let boardWidth = 35
let boardHeight = 15

type Position = { X: int; Y: int }
type Direction = Up | Down | Left | Right

type Position = { X: int; Y: int }
type Direction = Up | Down | Left | Right

// The Snake type represents the snake's head position, body segments, and current movement direction. The head is a single position, while the body is a list of positions representing the segments following the head.
type Snake = {
    Head: Position
    Body: Position list
    CurrentDir: Direction
}

// GoodFood represents a food item that the snake should eat to grow and increase the score. Normal good food gives a standard score increase, while special good food gives a larger score increase and deletes all bad food from the board.
type GoodFood = 
    | Normal of Position
    | Special of Position

// BadFood represents a food item that the snake should avoid. If the snake eats it, it will end the game.
type BadFood = {
    Pos: Position
}

// The GameState type represents all possible states of the game, including the main menu, active gameplay, and game over screen
type GameState =
    | MainMenu
    | Playing of snake: Snake * currentFood: GoodFood * badFoods: BadFood list * score: int * tickDelay: int * speedStep: int
    | GameOver of finalScore: int

// This command type represents all possible inputs to the game logic
type Command =
    | Tick 
    | ChangeDir of Direction 
    | StartGame of delay: int * speedStep: int
    | QuitGame

// This interface allows a modular, replaceable rendering layer
type IGameRenderer =
    abstract member Render : GameState -> unit
    abstract member GetCommand : unit -> Command option