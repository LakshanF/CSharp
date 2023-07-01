using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace HexGame
{
    enum Piece : short { empty, blue, red };
    enum PathColor : short { white, grey, black };

    internal record HexNode
    {
        public int Id { get; init; }
        public Piece Piece { get; set; }
        public PathColor Color { get; set; }
    }

  //  class hex_graph
  //  {
  //      int Length;
  //      HexNode* Nodes;
  //      bool path_found(HexNode root);
  //      vector<int> GetAdjacentNodes(int node_id);
  //      public:
		//hex_graph(int size);
  //      ~hex_graph();
  //      void print_board();
  //      bool add_user_move(int row, int col);
  //      int game_finished();
  //      int make_computer_move();
  //  };


    internal enum HexState
    {
        Playing,
        BlueWon,
        RedWon
    }
    internal class HexGraph
    {
        public int Length { get; init; }
        public List<HexNode> Nodes { get; init; }
        public HexGraph(int length)
        {
            Length = length;
            Nodes = new List<HexNode>();
            for (int i = 0; i < Length * Length; i++)
            {
                Nodes.Add(new HexNode() { Id=i });
            }
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
        /// Dumb strategy first: make a move on an empty square
        /// </summary>
        /// <returns></returns>
        public int MakeComputerMove()
        {
            MakeMonteCarloMove();
            return MakeSimpleMove();
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
        private int MakeMonteCarloMove()
        {
            throw new NotImplementedException();
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
                            return HexState.BlueWon;
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
                            return HexState.RedWon;
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
    }
}
