namespace TetrisClient
{
    public static class TetrominoFactory
    {
        public static Tetromino CreateRandomTetromino()
        {
            Random random = new Random();
            int shape = random.Next(0, 7);

            switch (shape)
            {
                case 0: // I
                    return new Tetromino(new int[,] { { 1, 1, 1, 1 } });
                case 1: // J
                    return new Tetromino(new int[,] { { 1, 0, 0 }, { 1, 1, 1 } });
                case 2: // L
                    return new Tetromino(new int[,] { { 0, 0, 1 }, { 1, 1, 1 } });
                case 3: // O
                    return new Tetromino(new int[,] { { 1, 1 }, { 1, 1 } });
                case 4: // S
                    return new Tetromino(new int[,] { { 0, 1, 1 }, { 1, 1, 0 } });
                case 5: // T
                    return new Tetromino(new int[,] { { 0, 1, 0 }, { 1, 1, 1 } });
                case 6: // Z
                    return new Tetromino(new int[,] { { 1, 1, 0 }, { 0, 1, 1 } });
                default:
                    return new Tetromino(new int[,] { { 1, 1, 1, 1 } });
            }
        }
    }

}