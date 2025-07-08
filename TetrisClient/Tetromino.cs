namespace TetrisClient
{

    public class Tetromino
    {
        public int[,] Shape { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Tetromino(int[,] shape)
        {
            Shape = shape;
        }

        public void Rotate()
        {
            int width = Shape.GetLength(0);
            int height = Shape.GetLength(1);
            int[,] newShape = new int[height, width];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newShape[j, i] = Shape[width - 1 - i, j];
                }
            }

            Shape = newShape;
        }
    }

}