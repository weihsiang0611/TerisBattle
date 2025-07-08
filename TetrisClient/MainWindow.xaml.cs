using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TetrisClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameManager _gameManager;
        private readonly ServerConnection _serverConnection;
        private const int BlockSize = 30; // Size of each block in pixels

        public MainWindow()
        {
            InitializeComponent();
            _gameManager = new GameManager();
            _gameManager.TimeChanged += (time) => TimeText.Text = time;
            _gameManager.ScoreChanged += (score) => ScoreText.Text = score.ToString();
            _gameManager.NewTetromino += DrawTetromino;
            _gameManager.BoardUpdated += DrawBoard;

            _serverConnection = new ServerConnection("127.0.0.1", 12345);
            _serverConnection.MessageReceived += HandleServerMessage;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            string message = string.Empty;
            switch (e.Key)
            {
                case Key.Left:
                    message = "move_left";
                    _gameManager.MoveTetromino(-1, 0);
                    break;
                case Key.Right:
                    message = "move_right";
                    _gameManager.MoveTetromino(1, 0);
                    break;
                case Key.Down:
                    message = "move_down";
                    _gameManager.MoveTetromino(0, 1);
                    break;
                case Key.Up:
                    message = "rotate";
                    _gameManager.RotateTetromino();
                    break;
                case Key.Space:
                    message = "hard_drop";
                    // We will let the server handle the logic for hard drop
                    break;
            }
            if (!string.IsNullOrEmpty(message))
            {
                _ = _serverConnection.SendMessageAsync(message);
                e.Handled = true; // Mark the event as handled
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _gameManager.StartGame();
            GameCanvas.Focus();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _serverConnection.ConnectAsync();
                MessageBox.Show("Connected to server!");
                GameCanvas.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}");
            }
        }

        private void HandleServerMessage(string message)
        {
            // This method will be called when a message is received from the server.
            // You'll need to parse the message and update the UI accordingly.
            // For example, if the server sends board updates or score updates.
            Dispatcher.Invoke(() =>
            {
                // Example: Update a status label or process game state
                // StatusText.Text = "Server: " + message;
                Console.WriteLine($"Received from server: {message}");
            });
        }

        private void DrawTetromino(Tetromino tetromino)
        {
            GameCanvas.Children.Clear(); // Clear previous tetromino and board
            DrawBoard(_gameManager._gameBoard); // Redraw the board

            for (int row = 0; row < tetromino.Shape.GetLength(0); row++)
            {
                for (int col = 0; col < tetromino.Shape.GetLength(1); col++)
                {
                    if (tetromino.Shape[row, col] == 1)
                    {
                        Rectangle rect = new Rectangle
                        {
                            Width = BlockSize,
                            Height = BlockSize,
                            Fill = Brushes.Blue,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };

                        Canvas.SetLeft(rect, (tetromino.X + col) * BlockSize);
                        Canvas.SetTop(rect, (tetromino.Y + row) * BlockSize);
                        GameCanvas.Children.Add(rect);
                    }
                }
            }
        }

        private void DrawBoard(int[,] board)
        {
            // Clear only the locked blocks, not the current tetromino
            // This is a simplified approach, a more robust solution would manage layers
            GameCanvas.Children.Clear();

            for (int r = 0; r < board.GetLength(0); r++)
            {
                for (int c = 0; c < board.GetLength(1); c++)
                {
                    if (board[r, c] == 1)
                    {
                        Rectangle rect = new Rectangle
                        {
                            Width = BlockSize,
                            Height = BlockSize,
                            Fill = Brushes.Gray, // Color for locked blocks
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };

                        Canvas.SetLeft(rect, c * BlockSize);
                        Canvas.SetTop(rect, r * BlockSize);
                        GameCanvas.Children.Add(rect);
                    }
                }
            }
        }
    }
}