cmake_minimum_required(VERSION 3.10)
project(TetrisServer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

add_executable(tetris_server src/main.cpp src/Game.cpp)

if(WIN32)
    target_link_libraries(tetris_server ws2_32)
endif()
