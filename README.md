# CS20200-Game-Project: Snake Game

## How to Run

To run the game, follow these instructions:

1. Open your terminal or command prompt. Ensure it is quite large so the game can be rendered correctly.
2. Navigate to the root directory of the repository (where the `.fsproj` file is located).
3. Execute the following command:
```bash
dotnet run
```
4. Follow the on-screen instructions to play the game!

## Requirement Changes

Changed from 'Launch into main menu' to 'Launch into splash screen', which then transitions to the main menu after a delay. I added this to make the game feel more polished and visually stimulating.

## LLM Attribution

I used a Large Language Model (LLM) and GitHub Copilot during the development of this project. Below is a detailed breakdown of my experience:

Module Organisation: I used an LLM for understanding the general layout of a game in modules, based initially on the example project. For example, "Is it good practice to make a separate module for the theme colours of a game. If so, should they be in a separate file?" and "Are my current modules adequate for a snake game using best coding practices in F#?". The LLM kept flip-flopping its ideas on how to organise this so I ended up choosing what felt natural to me.

Code Generation and Refactoring: I have GitHub Copilot installed however I opted to 'Snooze' it for the majority of programming, as I wanted to better understand my own code. I did however occasionally turn it on when writing basic repetitive code or rewriting pre-written code, for example switching from my old print method (default printfn) to using my withForegroundColor method to print exactly the same lines of text. This required frequent manual changes and often only partly using the suggested code, as it would often jump ahead with completely new ideas or implementations outside of my intention.

Learning Tool: I used lots of LLM as a learning tool in my project, with questions like "this makes the signature something like int * (int -> 'a) right?" in tandem with YouTube videos such as Crews Code's 'Fast F#' series. The LLM worked well here and didn't need correction.

Asset Generation: I used an LLM to generate the ASCII title screen. This worked well.

API Discovery: I often used an LLM as a shortcut for reading documentation and discovering the F# API's features. For example, "What predefined functions could i use to cleanly filter a seq and get the first element of that result?" or "What can I use to represent a Tick?", and especially for understanding the usage of the Console class. The answers were basically correct however I often had to re-prompt many times to get the LLM to describe things to me more clearly, often something like "Please rephrase that in layman's terms".

Code Isolation (Side Effects): I also used an LLM for helping to search for side effects in the files of my projects as I was eager to try to isolate pure code and side-effect code in different files and modules. This worked okay but I found it would often miss things and I'd have to ask "Doesn't Environment.Exit(0) need to be relocated?" for example.

Comparing ideas: When I had two ideas for an implementation, such as where to place the delay for the splash screen (as a mutation of the tick delay or as a delay injected into the state change), I would sometimes ask the LLM for advice on which was better in the F# community/context and it would give me a good breakdown of the pros and cons of each approach, which helped me to make informed decisions. Again, it would often jump ahead with huge sweeping suggestions or blocks of code which I had to disregard and just ask for a basic gist of the pros and cons and how to implement each approach, but the advice was generally good.