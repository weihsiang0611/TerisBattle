#include "Game.h"
#include <iostream>
#include <thread>
#include <random>
#include <algorithm>
#include <chrono>

// Tetromino shapes (simplified for now)
// Each shape is a 4x4 matrix, 1 represents a block, 0 is empty
const std::vector<std::vector<std::vector<int>>> TETROMINO_SHAPES = {
    // I
    {{0, 0, 0, 0},
     {1, 1, 1, 1},
     {0, 0, 0, 0},
     {0, 0, 0, 0}},
    // J
    {{1, 0, 0, 0},
     {1, 1, 1, 0},
     {0, 0, 0, 0},
     {0, 0, 0, 0}},
    // L
    {{0, 0, 1, 0},
     {1, 1, 1, 0},
     {0, 0, 0, 0},
     {0, 0, 0, 0}},
    // O
    {{0, 1, 1, 0},
     {0, 1, 1, 0},
     {0, 0, 0, 0},
     {0, 0, 0, 0}},
    // S
    {{0, 1, 1, 0},
     {1, 1, 0, 0},
     {0, 0, 0, 0},
     {0, 0, 0, 0}},
    // T
    {{0, 1, 0, 0},
     {1, 1, 1, 0},
     {0, 0, 0, 0},
     {0, 0, 0, 0}},
    // Z
    {{1, 1, 0, 0},
     {0, 1, 1, 0},
     {0, 0, 0, 0},
     {0, 0, 0, 0}}
};

Game::Game() : isRunning(true), listenSocket(INVALID_SOCKET), clientSocket(INVALID_SOCKET),
               board(BOARD_HEIGHT, std::vector<int>(BOARD_WIDTH, 0)),
               currentTetrominoX(0), currentTetrominoY(0),
               lastDropTime(std::chrono::steady_clock::now()) { // Initialize lastDropTime
    WSADATA wsaData;
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0) {
        std::cerr << "WSAStartup failed: " << result << std::endl;
        isRunning = false;
    }
    initializeBoard();
    spawnTetromino();
}

Game::~Game() {
    closesocket(clientSocket);
    closesocket(listenSocket);
    WSACleanup();
}

void Game::startServer() {
    struct addrinfo* result = NULL, * ptr = NULL, hints;

    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    if (getaddrinfo("127.0.0.1", "12345", &hints, &result) != 0) {
        std::cerr << "getaddrinfo failed" << std::endl;
        isRunning = false;
        return;
    }

    listenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
    if (listenSocket == INVALID_SOCKET) {
        std::cerr << "Error at socket(): " << WSAGetLastError() << std::endl;
        freeaddrinfo(result);
        isRunning = false;
        return;
    }

    if (bind(listenSocket, result->ai_addr, (int)result->ai_addrlen) == SOCKET_ERROR) {
        std::cerr << "bind failed with error: " << WSAGetLastError() << std::endl;
        freeaddrinfo(result);
        closesocket(listenSocket);
        isRunning = false;
        return;
    }

    freeaddrinfo(result);

    if (listen(listenSocket, SOMAXCONN) == SOCKET_ERROR) {
        std::cerr << "Listen failed with error: " << WSAGetLastError() << std::endl;
        closesocket(listenSocket);
        isRunning = false;
        return;
    }

    std::cout << "Server is listening on port 12345..." << std::endl;

    clientSocket = accept(listenSocket, NULL, NULL);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "accept failed: " << WSAGetLastError() << std::endl;
        closesocket(listenSocket);
        isRunning = false;
        return;
    }

    std::cout << "Client connected." << std::endl;
    closesocket(listenSocket); // No longer need listener socket
}

void Game::run() {
    startServer();
    if (!isRunning) {
        return;
    }

    char recvbuf[512];
    int recvbuflen = 512;

    // Send initial board state
    sendBoardState();

    while (isRunning) {
        int bytesReceived = recv(clientSocket, recvbuf, recvbuflen, 0);
        if (bytesReceived > 0) {
            recvbuf[bytesReceived] = '\0';
            processInput(recvbuf);
            update(); // Update and send state after processing input
            render();
        } else if (bytesReceived == 0) {
            std::cout << "Connection closing..." << std::endl;
            isRunning = false;
        } else {
            std::cerr << "recv failed: " << WSAGetLastError() << std::endl;
            isRunning = false;
        }
    }
}

void Game::initializeBoard() {
    for (int r = 0; r < BOARD_HEIGHT; ++r) {
        for (int c = 0; c < BOARD_WIDTH; ++c) {
            board[r][c] = 0;
        }
    }
}

void Game::spawnTetromino() {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> distrib(0, TETROMINO_SHAPES.size() - 1);
    currentTetrominoShape = TETROMINO_SHAPES[distrib(gen)];
    currentTetrominoX = BOARD_WIDTH / 2 - 2; // Center the tetromino
    currentTetrominoY = 0;

    // Check for game over condition (spawn collision)
    if (checkCollision(currentTetrominoX, currentTetrominoY, currentTetrominoShape)) {
        std::cout << "Game Over!" << std::endl;
        isRunning = false; // Or handle game over state
    }
}

bool Game::checkCollision(int x, int y, const std::vector<std::vector<int>>& tetrominoShape) {
    for (int row = 0; row < tetrominoShape.size(); ++row) {
        for (int col = 0; col < tetrominoShape[row].size(); ++col) {
            if (tetrominoShape[row][col] == 1) {
                int boardX = x + col;
                int boardY = y + row;

                // Check boundaries
                if (boardX < 0 || boardX >= BOARD_WIDTH || boardY < 0 || boardY >= BOARD_HEIGHT) {
                    return true; // Collision with wall or floor
                }
                // Check collision with existing blocks on the board
                if (board[boardY][boardX] == 1) {
                    return true; // Collision with existing block
                }
            }
        }
    }
    return false;
}

void Game::lockTetromino() {
    for (int row = 0; row < currentTetrominoShape.size(); ++row) {
        for (int col = 0; col < currentTetrominoShape[row].size(); ++col) {
            if (currentTetrominoShape[row][col] == 1) {
                board[currentTetrominoY + row][currentTetrominoX + col] = 1;
            }
        }
    }
    clearLines();
    spawnTetromino();
}

void Game::clearLines() {
    for (int r = BOARD_HEIGHT - 1; r >= 0; --r) {
        bool fullLine = true;
        for (int c = 0; c < BOARD_WIDTH; ++c) {
            if (board[r][c] == 0) {
                fullLine = false;
                break;
            }
        }

        if (fullLine) {
            // Shift lines down
            for (int rowToMove = r; rowToMove > 0; --rowToMove) {
                for (int c = 0; c < BOARD_WIDTH; ++c) {
                    board[rowToMove][c] = board[rowToMove - 1][c];
                }
            }
            // Clear the top line
            for (int c = 0; c < BOARD_WIDTH; ++c) {
                board[0][c] = 0;
            }
            r++; // Recheck the current line as it's now filled with the line above
        }
    }
}

void Game::hardDrop() {
    while (!checkCollision(currentTetrominoX, currentTetrominoY + 1, currentTetrominoShape)) {
        currentTetrominoY++;
    }
    lockTetromino();
}

void Game::moveLeft() {
    if (!checkCollision(currentTetrominoX - 1, currentTetrominoY, currentTetrominoShape)) {
        currentTetrominoX--;
    }
}

void Game::moveRight() {
    if (!checkCollision(currentTetrominoX + 1, currentTetrominoY, currentTetrominoShape)) {
        currentTetrominoX++;
    }
}

void Game::softDrop() {
    if (!checkCollision(currentTetrominoX, currentTetrominoY + 1, currentTetrominoShape)) {
        currentTetrominoY++;
    } else {
        lockTetromino();
    }
}

void Game::rotate() {
    // Create a temporary rotated shape
    std::vector<std::vector<int>> rotatedShape(currentTetrominoShape[0].size(), std::vector<int>(currentTetrominoShape.size()));
    for (size_t i = 0; i < currentTetrominoShape.size(); ++i) {
        for (size_t j = 0; j < currentTetrominoShape[i].size(); ++j) {
            rotatedShape[j][currentTetrominoShape.size() - 1 - i] = currentTetrominoShape[i][j];
        }
    }

    // Check collision with the rotated shape
    if (!checkCollision(currentTetrominoX, currentTetrominoY, rotatedShape)) {
        currentTetrominoShape = rotatedShape;
    }
    // Note: For a more robust Tetris, wall kicks would be implemented here
}

std::string Game::getBoardStateAsString() {
    std::string state = "";
    // First, add the current tetromino to a temporary board for sending
    std::vector<std::vector<int>> tempBoard = board;
    for (int row = 0; row < currentTetrominoShape.size(); ++row) {
        for (int col = 0; col < currentTetrominoShape[row].size(); ++col) {
            if (currentTetrominoShape[row][col] == 1) {
                // Only draw if within bounds (should be, due to collision checks)
                if (currentTetrominoY + row >= 0 && currentTetrominoY + row < BOARD_HEIGHT &&
                    currentTetrominoX + col >= 0 && currentTetrominoX + col < BOARD_WIDTH) {
                    tempBoard[currentTetrominoY + row][currentTetrominoX + col] = 1;
                }
            }
        }
    }

    for (int r = 0; r < BOARD_HEIGHT; ++r) {
        for (int c = 0; c < BOARD_WIDTH; ++c) {
            state += std::to_string(tempBoard[r][c]);
        }
    }
    return state;
}

void Game::sendBoardState() {
    std::string boardState = getBoardStateAsString();
    // Prepend length to the message
    std::string message = std::to_string(boardState.length()) + ":" + boardState;
    send(clientSocket, message.c_str(), message.length(), 0);
}

void Game::processInput(char* buffer) {
    std::cout << "Received command: " << buffer << std::endl;
    if (strcmp(buffer, "hard_drop") == 0) {
        hardDrop();
    } else if (strcmp(buffer, "move_left") == 0) {
        moveLeft();
    } else if (strcmp(buffer, "move_right") == 0) {
        moveRight();
    } else if (strcmp(buffer, "move_down") == 0) { // soft_drop is "move_down" from client
        softDrop();
    } else if (strcmp(buffer, "rotate") == 0) {
        rotate();
    }
    // After processing any input, send the updated board state
    sendBoardState();
}

void Game::update() {
    // Implement continuous gravity/auto-drop
    auto now = std::chrono::steady_clock::now();
    // Drop every 500 milliseconds (0.5 seconds)
    if (std::chrono::duration_cast<std::chrono::milliseconds>(now - lastDropTime).count() >= 500) {
        softDrop(); // Attempt to move down
        lastDropTime = now; // Reset timer
    }
    // After any update (input or auto-drop), send the updated board state
    sendBoardState();
}

void Game::render() {
    // For debugging, print board to console
    std::cout << "Current Board State:" << std::endl;
    std::vector<std::vector<int>> tempBoard = board;
    for (int row = 0; row < currentTetrominoShape.size(); ++row) {
        for (int col = 0; col < currentTetrominoShape[row].size(); ++col) {
            if (currentTetrominoShape[row][col] == 1) {
                if (currentTetrominoY + row >= 0 && currentTetrominoY + row < BOARD_HEIGHT &&
                    currentTetrominoX + col >= 0 && currentTetrominoX + col < BOARD_WIDTH) {
                    tempBoard[currentTetrominoY + row][currentTetrominoX + col] = 1;
                }
            }
        }
    }

    for (int r = 0; r < BOARD_HEIGHT; ++r) {
        for (int c = 0; c < BOARD_WIDTH; ++c) {
            std::cout << (tempBoard[r][c] == 1 ? '#' : '.');
        }
        std::cout << std::endl;
    }
    std::cout << std::endl;
}