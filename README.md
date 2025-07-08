# Tetris Game (C++ Backend, C# Frontend)

This project implements a Tetris game with a C++ backend handling game logic and a C# WPF frontend for the user interface. Communication between the frontend and backend is established via TCP sockets.

## Project Structure

*   `src/`: Contains the C++ source code for the Tetris game server.
*   `TetrisClient/`: Contains the C# WPF source code for the Tetris game client.
*   `build/`: (Generated) Contains the build artifacts for the C++ backend.

## Current Status

### C++ Backend (Server)
*   **Basic TCP Server**: Listens on `127.0.0.1:12345` for client connections.
*   **Command Handling**: Recognizes and processes `move_left`, `move_right`, `rotate`, and `hard_drop` commands from the client.
*   **Game Logic (Partial)**:
    *   Initializes a game board.
    *   Spawns new tetrominoes.
    *   Basic collision detection.
    *   `hardDrop` functionality: Moves the current tetromino directly to the bottom and locks it.
    *   Line clearing.
*   **State Communication**: Converts the current game board (including the falling tetromino) into a string of '0's and '1's and sends it to the client.

### C# Frontend (Client)
*   **WPF UI**: Provides the graphical interface for the Tetris game.
*   **Server Connection**: Connects to the C++ backend via TCP.
*   **Input Handling**: Captures keyboard input (Left, Right, Down, Up, Space) and sends corresponding commands to the backend.
*   **UI Focus Fix**: Solved the issue where pressing spacebar would trigger button clicks by setting focus to the game canvas and marking key events as handled.

## Known Issues / Pending Tasks (To be addressed)

1.  **Frontend Visual Update for Hard Drop**: Although the backend processes `hard_drop` and sends the updated board state, the C# frontend currently does not parse this incoming board state and update its display accordingly. This is the primary reason why hard drop has no visual effect on the client.
2.  **Full Game Logic in C++ Backend**:
    *   Implement `move_left`, `move_right`, `rotate`, and `soft_drop` logic for tetrominoes.
    *   Implement continuous gravity/auto-drop for tetrominoes.
    *   Refine collision detection for all movements and rotations.
3.  **Frontend Game State Parsing and Rendering**:
    *   The C# client needs to parse the string representation of the board received from the C++ server.
    *   The client's `GameManager` and UI (`DrawBoard`, `DrawTetromino`) need to be updated based on the received server state, rather than relying solely on local `_gameManager` updates.
4.  **Improved Communication Protocol**: The current board state string is very basic. A more robust protocol might involve sending:
    *   The full board state.
    *   The current falling tetromino's shape and position separately.
    *   Next tetromino information.
    *   Score, lines cleared, time, etc.
5.  **Game Over Handling**: Implement proper game over detection and display on both backend and frontend.
6.  **Game Start/Reset Logic**: Ensure a clean game start and reset on both sides.

## Setup and Running the Project

### 1. C++ Backend Setup

1.  **Delete previous build artifacts (if any):**
    ```bash
    rmdir /s /q build
    ```
2.  **Generate build files using CMake:**
    ```bash
    cmake -B build
    ```
3.  **Compile the C++ project:**
    ```bash
    cmake --build build
    ```
4.  **Run the C++ server:**
    ```bash
    ./build/Debug/tetris_server.exe
    ```
    (Keep this console window open as it will display server logs and the game board state.)

### 2. C# Frontend Setup

1.  **Open the project in Visual Studio:**
    *   Open `TetrisClient/TetrisClient.csproj` in Visual Studio.
    *   Ensure you have .NET 6.0 SDK installed (the project targets `net6.0-windows`).
2.  **Run the C# client:**
    *   In Visual Studio, press `F5` or click the "Start" button to run the `TetrisClient` project.
3.  **Connect to the server:**
    *   Once the client UI appears, click the "Connect to Server" button.
    *   You should see "Connected to server!" message on the client and "Client connected." on the server console.

Now you can use the arrow keys and spacebar on the client. Observe the server console for command reception and board state updates.
