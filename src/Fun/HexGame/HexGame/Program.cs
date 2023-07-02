namespace HexGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int boardSize = 3;
            HexGraph graph = new HexGraph(boardSize);
            int row, col;
            for (int i = 0; i < boardSize * boardSize; i++)
            {
                graph.PrintBoard();
                graph.MakeUserAsComputerMove();
                //while (true)
                //{
                //    Console.WriteLine("Please make a move (row, column): ");
                //    string line = Console.ReadLine();
                //    var values = line.Split(",", StringSplitOptions.TrimEntries);
                //    row = int.Parse(values[0]);
                //    col = int.Parse(values[1]);
                //    if (graph.MakeUserMove(row, col))
                //        break;
                //}
                if (graph.GameState() == HexState.BlueWon)
                {
                    Console.WriteLine("You Won!");
                    graph.PrintBoard();
                    break;
                }
                graph.MakeComputerMove();
                if (graph.GameState() == HexState.RedWon)
                {
                    Console.WriteLine("Computer Won!");
                    graph.PrintBoard();
                    break;
                }
            }
        }
    }
}