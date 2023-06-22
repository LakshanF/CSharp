using System;
using System.Collections.Generic;
using System.Linq;
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
  //      int size_one_side;
  //      hex_node* nodes;
  //      bool path_found(hex_node root);
  //      vector<int> GetAdjacentNodes(int node_id);
  //      public:
		//hex_graph(int size);
  //      ~hex_graph();
  //      void print_board();
  //      bool add_user_move(int row, int col);
  //      int game_finished();
  //      int make_computer_move();
  //  };


    internal class HexGraph
    {
        public int Length { get; init; }
        public List<HexNode> Nodes { get; init; }
        public HexGraph(int length)
        {
            Length = length;
        }
    }
}
