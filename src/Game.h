#ifndef GAME_H
#define GAME_H

#include <winsock2.h>
#include <ws2tcpip.h>
#include <vector>
#include <string>

#pragma comment(lib, "Ws2_32.lib")

const int BOARD_WIDTH = 10;
const int BOARD_HEIGHT = 20;

class Game {
public:
    Game();
    ~Game();
    void run();

private:
    void processInput(char* buffer);
    void update();
    void render();
    void startServer();
    void initializeBoard();
    void spawnTetromino();
    bool checkCollision(int x, int y, const std::vector<std::vector<int>>& tetrominoShape);
    void lockTetromino();
    void clearLines();
    void hardDrop();
    std::string getBoardStateAsString();
    void sendBoardState();

    bool isRunning;
    SOCKET listenSocket;
    SOCKET clientSocket;

    std::vector<std::vector<int>> board;
    std::vector<std::vector<int>> currentTetrominoShape;
    int currentTetrominoX;
    int currentTetrominoY;
};

#endif // GAME_H
