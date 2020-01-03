//#define INVESTIGATE

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OnePerDay.Algorithms_1
{
    /// <summary>
    /// Randmoized contraction min cut picks two nodes that are connected and merges them into one
    /// </summary>
    public class GraphContraction
    {
        /// <summary>
        /// @TODO - modify the graph instead of re-creating every time its contracged
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int RunAlgorithm(string[] args)
        {
            int minCut = Int32.MaxValue;
            GraphContraction algo = new GraphContraction();

            int counter = 0, tempMinCut;
            while (counter++ < 100)
            {
                Console.WriteLine();
                //if ((tempMinCut = algo.UsingNormalRandom(args)) < minCut)
                Console.WriteLine(">");
                if ((tempMinCut = algo.UsingRNGCryptoRandom(args)) < minCut)
                {
                    minCut = tempMinCut;
                    counter = 0;
                }
            }
            Console.WriteLine(minCut);
            return minCut;
        }
        /// <summary>
        /// Uses RNGCryptoServiceProvider 
        /// @lesson learned
        ///  - sloppy way to re-start the calculating process resulted in a signficant time loss due to debugging the issue :-(
        /// instead of re-creating the graph, I persisted the original and used it subsequently :-(
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private int UsingRNGCryptoRandom(string[] args)
        {
            String fileName = args[0];
            Graph1 graph = GetGraphFromFile(fileName);
            int tempMinCut = Int32.MaxValue;
            int counter = 0, randomValue;
            UInt16 uintValue;
            byte[] randomNumber = new byte[2];
            while (counter < 10000)
            {
                if (counter++ % 1000 == 0)
                    Console.Write(".");
                using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
                {
                    while (graph.Nodes.Count > 2)
                    {
                        // Fill the array with a random value.
#if INVESTIGATE
                        Console.WriteLine(">");
#endif
                        //Lets pick a random Node
                        random.GetBytes(randomNumber);
                        uintValue = BitConverter.ToUInt16(randomNumber, 0);
                        randomValue = uintValue % graph.Nodes.Count;
                        Debug.Assert(randomValue < graph.Nodes.Count && graph.Nodes.Count <= UInt16.MaxValue, "Needs work on the Random Max for the node selection");
                        Node1 randomNode = graph.Nodes[randomValue];

                        //Lets pick a random edge
                        random.GetBytes(randomNumber);
                        uintValue = BitConverter.ToUInt16(randomNumber, 0);
                        randomValue = uintValue % randomNode.AdjacencyNodes.Count;
                        Debug.Assert(randomValue < randomNode.AdjacencyNodes.Count && randomNode.AdjacencyNodes.Count <= UInt16.MaxValue, "Needs work on the Random Max for the Edge selection");
                        int randomEdgeNode = randomNode.AdjacencyNodes[randomValue];

                        graph = GetModifiedGraph(graph, randomNode, randomEdgeNode);
                    }
                }

                Debug.Assert(graph.Nodes.Count == 2, "Count not correct");
                if (graph.Nodes[0].AdjacencyNodes.Count < tempMinCut)
                {
                    tempMinCut = graph.Nodes[0].AdjacencyNodes.Count;
                    Debug.Assert(tempMinCut == graph.Nodes[1].AdjacencyNodes.Count, "Counts not the same");
                    //@TODO - having a reference to the original graph isn't correct, we need to read from the original
                    graph = GetGraphFromFile(fileName); 
                    counter = 0;
                }
            }

            return tempMinCut;
        }
        /// <summary>
        /// Uses System.Random
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private int UsingNormalRandom(string[] args)
        {

            Graph1 graph = GetGraphFromFile(args[0]);

            int tempMinCut = Int32.MaxValue;
            int counter = 0;
            while (counter < 10000)
            {
                if (counter++ % 1000 == 0)
                    Console.Write(".");
                Random random = new Random();
                while (graph.Nodes.Count > 2)
                {
                    //Console.WriteLine("{0}", graph.Nodes.Count);
                    //Lets pick a random edge
                    Node1 randomNode = graph.Nodes[random.Next(graph.Nodes.Count)];
                    int randomEdgeNode = randomNode.AdjacencyNodes[random.Next(randomNode.AdjacencyNodes.Count)];
                    graph = GetModifiedGraph(graph, randomNode, randomEdgeNode);
                }
                //Console.WriteLine();

                //find the minCut
                Debug.Assert(graph.Nodes.Count == 2, "Count not correct");
                if (graph.Nodes[0].AdjacencyNodes.Count < tempMinCut)
                {
                    tempMinCut = graph.Nodes[0].AdjacencyNodes.Count;
                    counter = 0;
                    Debug.Assert(tempMinCut == graph.Nodes[1].AdjacencyNodes.Count, "Counts not the same");
                }
            }
            return tempMinCut;
        }

        private Graph1 GetGraphFromFile(string fileName)
        {
            Graph1 graph = new Graph1();
            graph.Nodes = new List<Node1>();

            foreach (String line in File.ReadLines(fileName))
            {
                String[] values = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 0)
                {
                    Node1 node = new Node1();
                    node.AdjacencyNodes = new List<int>();
                    node.Number = Int32.Parse(values[0]);
                    for (int i = 1; i < values.Length; i++)
                    {
                        node.AdjacencyNodes.Add(Int32.Parse(values[i]));
                    }
                    graph.Nodes.Add(node);
                }
            }
            return graph;
        }


        /// <summary>
        /// We create a new Graph here but @TODO to modify the existing graph
        /// 
        /// We will modify the graph by the following way
        ///  - fold the randomEdgeNode into the random node
        ///  - delete the edge between them as well as any other self loops
        ///  - Adjust all edges from the randomEdgeNode to point to the random node
        ///  - Go through all the nodes in the randomEdgeNode node adjacency list nodes and modify the node to be the random node
        ///  - assert that no other node has any links to the randomEdgeNode
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="randomNode"></param>
        /// <param name="randomEdgeNode"></param>
        /// <returns></returns>
        private Graph1 GetModifiedGraph(Graph1 graph, Node1 randomNode, int randomEdgeNode)
        {
#if INVESTIGATE
            Stopwatch watch = new Stopwatch();
            watch.Start();
#endif

            Graph1 newGraph = new Graph1();
            newGraph.Nodes = new List<Node1>();

            List<int> randomEdgeNodeEdges = null;
            List<int> list;

            //  - fold the randomEdgeNode into the random node
            //  - delete the edge between them as well as any other self loops

#if INVESTIGATE
            //check to see if the graph is correct, i.e.
            //are there nodes referencing randomEdgeNode that shouldn't 
            foreach (var node in graph.Nodes)
            {
                if (node.Number == randomEdgeNode)
                {
                    randomEdgeNodeEdges = node.AdjacencyNodes;
                }
            }
            foreach (var node in graph.Nodes)
            {
                if (!randomEdgeNodeEdges.Contains(node.Number))
                {
                    foreach (var nodeNumber in node.AdjacencyNodes)
                    {
                        Debug.Assert(nodeNumber!= randomEdgeNode, "EArly graph validation fails");
                    }
                }
            }
            Console.WriteLine(watch.Elapsed);

#endif

            bool adjacencyListFound = false;
            foreach (var node in graph.Nodes)
            {
                if (node.Number == randomEdgeNode)
                {
                    adjacencyListFound = true;
                    randomEdgeNodeEdges = node.AdjacencyNodes;
                }
                else if (node.Number == randomNode.Number)
                {
                    list = new List<int>();
                    foreach (var nodeNumber in node.AdjacencyNodes)
                    {
                        if (nodeNumber != randomEdgeNode)
                        {
                            list.Add(nodeNumber);
                        }
                    }
                    node.AdjacencyNodes = list;
                    newGraph.Nodes.Add(node);
                }
                else
                {
                    newGraph.Nodes.Add(node);
                }
            }
            Debug.Assert(adjacencyListFound, "huh3");

            //  - Adjust all edges from the randomEdgeNode to point to the random node
            foreach (var node in newGraph.Nodes)
            {
                if (node.Number == randomNode.Number)
                {
                    foreach (var nodeNumber in randomEdgeNodeEdges)
                    {
                        if (nodeNumber != randomNode.Number)//self loop
                        {
                            node.AdjacencyNodes.Add(nodeNumber);
                        }
                    }
                }
            }

            //  - Go through all the nodes in the randomEdgeNode node adjacency list nodes and modify the node to be the random node
            foreach (var node in newGraph.Nodes)
            {
                if ((node.Number != randomNode.Number) && randomEdgeNodeEdges.Contains(node.Number))
                {
                    list = new List<int>();
                    foreach (var nodeNumber in node.AdjacencyNodes)
                    {
                        if (nodeNumber == randomEdgeNode)
                        {
                            list.Add(randomNode.Number);
                        }
                        else
                        {
                            list.Add(nodeNumber);
                        }
                    }
                    node.AdjacencyNodes = list;
                }
            }

#if INVESTIGATE
            Console.WriteLine(watch.Elapsed);


            //Potential checks
            // 1) Check to see if there are any nodes that still have references to randomEdgeNode 

            Console.WriteLine(watch.Elapsed);
            //  - assert that no other node has any links to the randomEdgeNode
            foreach (var node in newGraph.Nodes)
            {
                Debug.Assert(node.Number != randomEdgeNode, "Node not removed");
                foreach (var nodeNumber in node.AdjacencyNodes)
                {
                    Debug.Assert(nodeNumber != randomEdgeNode, "Removed Node mentioned!");
                }
            }
            Console.WriteLine(watch.Elapsed);
#endif
            return newGraph;
        }
    }

    /// <summary>
    /// A naive implementation of a graph with nodes and adjacency lists
    /// </summary>
    public class Graph1
    {
        public List<Node1> Nodes { get; set; }
    }

    public class Node1
    {
        public int Number { get; set; }
        public List<int> AdjacencyNodes { get; set; }
    }
}
