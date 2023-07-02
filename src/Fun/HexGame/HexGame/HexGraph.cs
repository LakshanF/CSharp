using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace HexGame
{
    // Blue goes first (human if only 1 person is playing), and goes east-west
    // Red goes 2nd (computer if only 1 person is playing), and goes north-south
    enum Piece : short { empty, blue, red };
    enum PathColor : short { white, grey, black };

    internal record HexNode
    {
        public int Id { get; init; }
        public Piece Piece { get; set; }
        public PathColor Color { get; set; }
    }

    internal enum PlayMode
    {
        TwoPlayer,
        SinglePlayer,
        Computer
    }

    internal enum HexState
    {
        Playing,
        BlueWon,
        RedWon
    }
    internal class HexGraph
    {
        public int Length { get; init; }
        public List<HexNode> Nodes { get; set; }
        
        PlayMode mode;
        int monteCarloIteration;
        Stopwatch stopwatch;

        public HexGraph(int length, PlayMode mode = PlayMode.SinglePlayer, int monteCarloIteration=0)
        {
            Length = length;
            Nodes = new List<HexNode>();
            for (int i = 0; i < Length * Length; i++)
            {
                Nodes.Add(new HexNode() { Id=i });
            }
            this.mode = mode;
            this.monteCarloIteration = monteCarloIteration;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            HexEventSource.Log.GameStart($"Board Lenght:{length}, PlayMode:{mode}, MonteCarloIteration:{monteCarloIteration}");
        }

        public void PrintBoard()
        {
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < i; j++)
                    Console.Write("  "); // 2 spaces
                if (i != 0)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        Console.Write("\\ /");
                    }
                    Console.WriteLine();
                }
                for (int j = 0; j < i; j++)
                    Console.Write("  "); // 2 spaces
                for (int j = 0; j < Length; j++)
                {
                    switch (Nodes[i * Length+ j].Piece)
                    {
                        case Piece.empty:
                            Console.Write(".");
                            break;
                        case Piece.blue:
                            Console.Write("B");
                            break;
                        case Piece.red:
                            Console.Write("R");
                            break;
                    }
                    if (j != (Length - 1))
                        Console.Write(" - ");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Assumes first move, blue
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool MakeUserMove(int row, int col)
        {
            int position = row * Length + col;
            if (Nodes[position].Piece != Piece.empty)
                return false;
            Nodes[position].Piece = Piece.blue;
            return true;
        }

        /// <summary>
        /// We will do a Monte Carlo simulation with a basic shuffle function
        ///  - For all possible moves
        ///     + Pick a move that hasn't been analyzed yet
        ///     + prefill the empty squares with blue and red values
        ///     + repeat for n times
        ///         * shuffle the board
        ///         * determine who won
        ///     + note the probability of the move sucess
        ///  - Return the best move
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private int MakeMonteCarloMove(Piece pieceToMove)
        {
            // Get all possible moves
            Queue<int> queue = new Queue<int>();
            List<int> allEmptyNodes = new List<int>();
            for (int i = 0; i < Length * Length; i++)
            {
                if (Nodes[i].Piece == Piece.empty)
                {
                    queue.Enqueue(i);
                    allEmptyNodes.Add(i);
                }
            }

            // Calculate prob for each move
            Dictionary<int, double> map = new Dictionary<int, double>();
            Piece opposingMoveColor = pieceToMove==Piece.red ? Piece.blue : Piece.red;
            while(queue.Count > 0)
            {
                HexNode node = new HexNode() { Id = queue.Dequeue(), Piece=pieceToMove};
                List<int> emptyNodes = allEmptyNodes.Except(new List<int>() { node.Id }).ToList();
                List<HexNode> experimentalNodes = new List<HexNode>();
                // Fill in the emptynodes with blue and red in random (?) order (simple first)
                for(int i = 0;i < emptyNodes.Count; i++)
                {
                    if (i%2==0)
                        experimentalNodes.Add(new HexNode() { Id = emptyNodes[i], Piece = opposingMoveColor });
                    else
                        experimentalNodes.Add(new HexNode() { Id = emptyNodes[i], Piece = pieceToMove });
                }
                double successProb = CalculateSuccess(experimentalNodes, node);
                HexEventSource.Log.SuccessRatioForMove(node.Id%Length, node.Id/Length, successProb);
                map.Add(node.Id, successProb);
            }

            // Find which move is the best
            (int id, double maxSuccess) nodeToSelect = (0, 0.0d);
            foreach(var keyValue in map)
            {
                if(keyValue.Value>nodeToSelect.maxSuccess)
                {
                    nodeToSelect = (keyValue.Key, keyValue.Value);
                }
            }
            Nodes[nodeToSelect.id].Piece = pieceToMove;
            HexEventSource.Log.TimeForMove(stopwatch.Elapsed.TotalMilliseconds);
            return nodeToSelect.id;
        }

        /// <summary>
        /// Calculate the success ratio if the currentMode is made by running a monte carlo simulation
        ///  - Create a completed board (Node is full) with the following
        ///     + Node is initialized with the nodes that are already complete
        ///     + Add the currentNode
        ///     + randomly shuffle empty pieces with alternative colors
        ///  - Game is done (either blue or red has won)
        ///  - Repeat this for the specified number of iterations and return success ratio
        /// </summary>
        /// <param name="initialNodes"></param>
        /// <param name="currentNode"></param>
        /// <returns>success ratio of the move as a double</returns>
        private double CalculateSuccess(List<HexNode> initialNodes, HexNode currentNode)
        {
            var nodesToBeExcluded = initialNodes.Select(x => x.Id).ToList();
            nodesToBeExcluded.Add(currentNode.Id);
            var oldNodeIds = Nodes.Select(x => x.Id).Except(nodesToBeExcluded).ToList();
            List<int> nodesBeforeShuffled = initialNodes.Select(x => x.Id).ToList();

            Dictionary<int, Piece> cachedInitialMap = initialNodes.ToDictionary(x => x.Id, x => x.Piece);
            int numberOfWins = 0;
            
            for (int i = 0; i < monteCarloIteration; i++)
            {
                List<HexNode> experimentalNodes = new List<HexNode>();
                // Lets get the Ids of the original nodes
                for (int j = 0; j<oldNodeIds.Count; j++)
                {
                    experimentalNodes.Add(new HexNode() { Id= Nodes[oldNodeIds[j]].Id, Piece=Nodes[oldNodeIds[j]].Piece });
                }

                // Add the current node
                experimentalNodes.Add(currentNode);

                // Add the shuffled ones
                List<int> nodesToBeShuffled = initialNodes.Select(x => x.Id).ToList();
                Shuffle(nodesToBeShuffled);
                for (int j = 0; j<nodesToBeShuffled.Count; j++)
                {
                    // There are far better ways to do this
                    experimentalNodes.Add(new HexNode() { Id = nodesBeforeShuffled[j], Piece=cachedInitialMap[nodesToBeShuffled[j]]});
                }

                //Who won in this shuffle
                HexGraph tempGraph = new HexGraph(Length);
                experimentalNodes = experimentalNodes.OrderBy(x => x.Id).ToList();
                tempGraph.Nodes = experimentalNodes;
                HexState whoWon = tempGraph.GameState();
                Debug.Assert(whoWon==HexState.RedWon || whoWon==HexState.BlueWon);
                if (whoWon==HexState.RedWon && currentNode.Piece==Piece.red)
                    numberOfWins++;
                if (whoWon==HexState.BlueWon && currentNode.Piece==Piece.blue)
                    numberOfWins++;

            }

            return (double)numberOfWins/monteCarloIteration;
        }

        /// <summary>
        /// Fisher-Yates shuffle algorithm from ChatGPT
        /// </summary>
        static void Shuffle(List<int> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        private int MakeSimpleMove()
        {
            for (int i = 0; i < Length * Length; i++)
            {
                if (Nodes[i].Piece == Piece.empty)
                {
                    Nodes[i].Piece = Piece.red;
                    return Nodes[i].Id;
                }
            }
            return -1;
        }

        /// <summary>
        /// Blue goes first, and need to create a path from east-west
        /// Red goes from south-north
        /// 0 - playing
        /// 1 - blue won
        /// 2 - red won
        /// </summary>
        /// <returns></returns>
        public HexState GameState()
        {
            // short cut first to see that all columns (blue) or rows (red) have at least 

            // blue first
            bool blue_found = false;
            for (int i = 0; i < Length; i++)
            {
                blue_found = false;
                for (int j = 0; j < Length; j++)
                {
                    int col_pos = (j * Length) + i;
                    if (Nodes[col_pos].Piece == Piece.blue)
                    {
                        blue_found = true;
                        break;
                    }
                }
                if (!blue_found)
                    break;
            }

            if (blue_found)
            {
                // need to check with a breadth first search if there is a path
                // We will look at all the boxes in the left most column and check if there is a path to the right most column
                for (int i = 0; i < Length; i++)
                {
                    int col_pos = i * Length;
                    if (Nodes[col_pos].Piece == Piece.blue)
                    {
                        if (EndToEndPathFound(Nodes[col_pos]))
                        {
                            HexEventSource.Log.GameStop($"{HexState.BlueWon}");
                            return HexState.BlueWon;
                        }
                    }
                }
            }

            // red next
            bool red_found = false;
            for (int i = 0; i < Length; i++)
            {
                red_found = false;
                for (int j = 0; j < Length; j++)
                {
                    int red_pos = i * Length + j;
                    if (Nodes[red_pos].Piece == Piece.red)
                    {
                        red_found = true;
                        break;
                    }
                }
                if (!red_found)
                    break;
            }

            if (red_found)
            {
                // need to check with a breadth first search if there is a path
                // We will look at all the boxes in the first row and check if any has reached the last rown
                for (int i = 0; i < Length; i++)
                {
                    int row_pos = i;
                    if (Nodes[row_pos].Piece == Piece.red)
                    {
                        if (EndToEndPathFound(Nodes[row_pos]))
                        {
                            HexEventSource.Log.GameStop($"{HexState.RedWon}");
                            return HexState.RedWon;
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Given a starting position (westmost column or southmost row), determine if there is a traversing path
        /// Blue travels from east to west (right)
        /// Red travels from north to south (down)
        /// Algorithm (Breadth search first, with dynamically getting adjacency nodes based on the position)
        ///  - Make all the nodes white
        ///  - Add source to Q
        ///  - While Q is not empty
        ///		+ Deque
        ///		+ if node is on the opposite side, return as true
        ///		+ Color node black
        ///		+ for all white adjacency nodes with the same piece
        ///			+ color node grey
        ///			+ add to Q
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal bool EndToEndPathFound(HexNode root)
        {
            Queue<HexNode> node_queue = new();
            for (int i = 0; i < Length * Length; i++)
            {
                Nodes[i].Color = PathColor.white;
            }
            root.Color = PathColor.grey;
            node_queue.Enqueue(root);

            while (node_queue.Count != 0)
            {
                HexNode node = node_queue.Dequeue();
                if (node.Piece == Piece.blue && (node.Id % Length == (Length - 1)))
                    return true;
                if (node.Piece == Piece.red && (node.Id >= (Length * Length - Length)))
                    return true;
                List<int> adjacentIds = GetAdjacentNodes(node.Id);

                // @TODO: foreach?
                foreach(int i in adjacentIds)
                {
                    if (Nodes[i].Piece == node.Piece && Nodes[i].Color == PathColor.white)
                    {
                        Nodes[i].Color = PathColor.grey;
                        node_queue.Enqueue(Nodes[i]);
                    }
                }
                node.Color = PathColor.black;
            }
            return false;
        }

        internal List<int> GetAdjacentNodes(int nodeId)
        {
            List<int> ids = new();

            // based on the nodeId, we return the neighbor nodes

            // Corners
            if (nodeId == 0)
            {
                // corner (2 edges): top left
                ids.Add(1);
                ids.Add(Length);
            }
            else if (nodeId == (Length * Length - 1))
            {
                // corner (2 edges): bottom right
                ids.Add(Length * Length - 2);
                ids.Add(Length * Length - 1 - Length);
            }
            else if (nodeId == (Length - 1))
            {
                // corner (3 edges): top right
                ids.Add(Length - 2);
                ids.Add(Length - 1 + Length);
                ids.Add(Length - 1 + Length - 1);
            }
            else if (nodeId == (Length * Length - Length))
            {
                // corner (3 edges): bottom left
                ids.Add(Length * Length - Length + 1);
                ids.Add(Length * Length - Length - Length);
                ids.Add(Length * Length - Length - Length + 1);
            }
            // Edges
            else if (nodeId % Length == 0)
            {
                // left (4 edges)
                ids.Add(nodeId + Length);
                ids.Add(nodeId - Length);
                ids.Add(nodeId - Length + 1);
                ids.Add(nodeId + 1);
            }
            else if (nodeId % Length == (Length - 1))
            {
                // Right (4 edges)
                ids.Add(nodeId + Length);
                ids.Add(nodeId + Length - 1);
                ids.Add(nodeId - Length);
                ids.Add(nodeId - 1);
            }
            else if (nodeId < (Length - 1))
            {
                // top (4 edges)
                ids.Add(nodeId + 1);
                ids.Add(nodeId - 1);
                ids.Add(nodeId + Length);
                ids.Add(nodeId + Length - 1);
            }
            else if (nodeId > (Length * Length - Length))
            {
                // bottom (4 edges)
                ids.Add(nodeId + 1);
                ids.Add(nodeId - 1);
                ids.Add(nodeId - Length);
                ids.Add(nodeId - Length + 1);
            }
            // Middle nodes
            else
            {
                // (6 edges)
                ids.Add(nodeId + 1);
                ids.Add(nodeId - 1);
                ids.Add(nodeId - Length);
                ids.Add(nodeId - Length + 1);
                ids.Add(nodeId + Length);
                ids.Add(nodeId + Length - 1);
            }

            return ids;

        }

        internal void Play()
        {
            for (int i = 0; i < Length * Length; i++)
            {
                PrintBoard();
                MakeMonteCarloMove(Piece.blue);
                //MakeUserAsComputerMove();
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
                if (GameState() == HexState.BlueWon)
                {
                    Console.WriteLine("Blue Computer (first play) won!");
                    PrintBoard();
                    break;
                }
                MakeMonteCarloMove(Piece.red);
                if (GameState() == HexState.RedWon)
                {
                    Console.WriteLine("Red computer (seond play) won!");
                    PrintBoard();
                    break;
                }
            }
        }
    }
}
