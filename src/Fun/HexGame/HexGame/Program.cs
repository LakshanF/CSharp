namespace HexGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Default is 3x3 Computer-Computer with 1,000,000 iteration monte-carlo simulation (press Enter)");
            Console.WriteLine("Choose a game: Single_or_Computer, [monte_carlo_iterations]");
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line) )
            {
                PlayComputerGame(3, 10_000);
            }
            var values = line.Split(",", StringSplitOptions.TrimEntries);
            int boardSize = 3;
        }

        private static void PlayComputerGame(int lenght, int iterations)
        {
            HexGraph graph = new HexGraph(lenght, PlayMode.Computer, iterations);
            graph.Play();
        }
    }
}