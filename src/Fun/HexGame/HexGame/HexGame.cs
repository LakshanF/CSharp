using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
namespace HexGame
{
    public class HexGame
    {
        [Theory]
        [InlineData("Computer,9,1000000")]
        // [Fact]
        public static int TestEntryPoint(string line)
        {
        // static void Main(string[] args)
        // {
            // string? line;
            // if(args.Length==0)
            // {
                // Console.WriteLine($"Default is {HexGraph.DEFAULT_PLAY_MODE}, {HexGraph.DEFAULT_BOARD_LENGTH}x{HexGraph.DEFAULT_BOARD_LENGTH}, and {HexGraph.DEFAULT_MONTE_CARLO_SIMULATION_ITERATION} iterations of monte-carlo simulation (press Enter)");
                // Console.WriteLine("Choose a game [Options are in order]: [TwoPlayer_SinglePlayer_or_Computer], [Board_length], [monte_carlo_iterations]");
                // line = Console.ReadLine();
                // if (string.IsNullOrEmpty(line))
                // {
                //     new HexGraph().Play();
                // }
            // }
            // else
            //     line = args[0];

            var values = line?.Split(",", StringSplitOptions.TrimEntries);
            string? playMode = values![0];
            if (values.Length==3)
            {
                new HexGraph(Enum.Parse<PlayMode>(playMode), int.Parse(values[1]), int.Parse(values[2])).Play();
            }
            else if (values.Length==2)
            {
                new HexGraph(Enum.Parse<PlayMode>(playMode), int.Parse(values[1])).Play();
            }
            else
            {
                new HexGraph(Enum.Parse<PlayMode>(playMode)).Play();
            }

            if(new Random().Next()<0)
             return -1;
            else
                return 100;
        }
    }
}