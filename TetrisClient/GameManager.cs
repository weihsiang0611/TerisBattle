using System.Windows.Threading;
using System.Windows;

namespace TetrisClient
{

    public class GameManager
    {
        private DispatcherTimer _timer;
        private int _timeRemaining = 120; // 2 minutes in seconds
        private Tetromino _currentTetromino;
        public int[,] _gameBoard; // Represents the game board
        private const int BoardRows = 20;
        private const int BoardCols = 10;
        private int _score = 0;

        public event Action<int> ScoreChanged;
        public event Action<string> TimeChanged;
        public event Action<Tetromino> NewTetromino;
        public event Action<int[,]> BoardUpdated;

        public GameManager()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5); // Tetromino falls every 0.5 seconds
            _timer.Tick += Timer_Tick;
            _gameBoard = new int[BoardRows, BoardCols];
        }

        public void StartGame()
        {
            _timeRemaining = 120;
            _score = 0;
            ScoreChanged?.Invoke(_score);
            TimeChanged?.Invoke(TimeSpan.FromSeconds(_timeRemaining).ToString(@"m\:ss"));

            // Clear the board
            for (int r = 0; r < BoardRows; r++)
            {
                for (int c = 0; c < BoardCols; c++)
                {
                    _gameBoard[r, c] = 0;
                }
            }
            BoardUpdated?.Invoke(_gameBoard);

            // GenerateNewTetromino();
            // _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeRemaining--;
            TimeChanged?.Invoke(TimeSpan.FromSeconds(_timeRemaining).ToString(@"m\:ss"));

            if (_timeRemaining <= 0)
            {
                _timer.Stop();
                // Game over logic
                return;
            }

            // Automatic falling
            // if (!MoveTetromino(0, 1)) // Try to move down
            // {
            //     LockTetromino();
            //     ClearLines();
            //     GenerateNewTetromino();
            // }
        }

        private void GenerateNewTetromino()
        {
            _currentTetromino = TetrominoFactory.CreateRandomTetromino();
            _currentTetromino.X = BoardCols / 2 - _currentTetromino.Shape.GetLength(1) / 2;
            _currentTetromino.Y = 0;

            if (CheckCollision(_currentTetromino.X, _currentTetromino.Y, _currentTetromino.Shape))
            {
                // Game Over
                _timer.Stop();
                MessageBox.Show("Game Over!");
            }
            NewTetromino?.Invoke(_currentTetromino);
        }

        public bool MoveTetromino(int deltaX, int deltaY)
        {
            int newX = _currentTetromino.X + deltaX;
            int newY = _currentTetromino.Y + deltaY;

            if (!CheckCollision(newX, newY, _currentTetromino.Shape))
            {
                _currentTetromino.X = newX;
                _currentTetromino.Y = newY;
                NewTetromino?.Invoke(_currentTetromino); // Update UI with new position
                return true;
            }
            return false;
        }

        public void RotateTetromino()
        {
            int[,] originalShape = _currentTetromino.Shape;
            _currentTetromino.Rotate();

            if (CheckCollision(_currentTetromino.X, _currentTetromino.Y, _currentTetromino.Shape))
            {
                _currentTetromino.Shape = originalShape; // Revert rotation if collision
            }
            NewTetromino?.Invoke(_currentTetromino); // Update UI with new rotation
        }

        private bool CheckCollision(int x, int y, int[,] shape)
        {
            for (int row = 0; row < shape.GetLength(0); row++)
            {
                for (int col = 0; col < shape.GetLength(1); col++)
                {
                    if (shape[row, col] == 1)
                    {
                        int boardX = x + col;
                        int boardY = y + row;

                        // Check boundaries
                        if (boardX < 0 || boardX >= BoardCols || boardY < 0 || boardY >= BoardRows)
                        {
                            return true;
                        }

                        // Check collision with existing blocks on the board
                        if (_gameBoard[boardY, boardX] != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void LockTetromino()
        {
            for (int row = 0; row < _currentTetromino.Shape.GetLength(0); row++)
            {
                for (int col = 0; col < _currentTetromino.Shape.GetLength(1); col++)
                {
                    if (_currentTetromino.Shape[row, col] == 1)
                    {
                        _gameBoard[_currentTetromino.Y + row, _currentTetromino.X + col] = 1; // Mark as occupied
                    }
                }
            }
            BoardUpdated?.Invoke(_gameBoard);
        }

        private void ClearLines()
        {
            int linesCleared = 0;
            for (int r = BoardRows - 1; r >= 0; r--)
            {
                bool lineIsFull = true;
                for (int c = 0; c < BoardCols; c++)
                {
                    if (_gameBoard[r, c] == 0)
                    {
                        lineIsFull = false;
                        break;
                    }
                }

                if (lineIsFull)
                {
                    linesCleared++;
                    // Move all lines above down
                    for (int moveR = r; moveR > 0; moveR--)
                    {
                        for (int c = 0; c < BoardCols; c++)
                        {
                            _gameBoard[moveR, c] = _gameBoard[moveR - 1, c];
                        }
                    }
                    // Clear the top line
                    for (int c = 0; c < BoardCols; c++)
                    {
                        _gameBoard[0, c] = 0;
                    }
                    r++; // Check the same row again as it's now a new line
                }
            }

            if (linesCleared > 0)
            {
                _score += linesCleared * 100; // Simple scoring
                ScoreChanged?.Invoke(_score);
                BoardUpdated?.Invoke(_gameBoard);
            }
        }
    }
}