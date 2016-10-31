﻿using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using GeometryGraph = Microsoft.Msagl.Core.Layout.GeometryGraph;
using GeometryNode = Microsoft.Msagl.Core.Layout.Node;
using GeometryEdge = Microsoft.Msagl.Core.Layout.Edge;
using GeometryCluster = Microsoft.Msagl.Core.Layout.Cluster;

namespace GraphControlTest
{
    public static class FromGeometry
    {
        private static Graph dg;
        private static Dictionary<GeometryNode, Node> localMap;
        private static int counter;

        public static Graph CreateDrawingGraph(GeometryGraph gg)
        {
            counter = 0;
            localMap = new Dictionary<GeometryNode,Node>();
            dg = new Graph(counter++.ToString()) { GeometryGraph = gg };
            foreach (GeometryNode n in gg.Nodes)
            {
                Node node = new Node(counter++.ToString());
                node.Attr.Shape = Shape.Ellipse;
                node.GeometryNode = n;
                dg.AddNode(node);
                localMap[n]=node;
            }
            Subgraph cluster = new Subgraph(counter++.ToString());
            cluster.GeometryNode = gg.RootCluster;
            dg.RootSubgraph = cluster;
            PopulateClusters(cluster, gg.RootCluster);
        
            foreach (GeometryEdge e in gg.Edges)
            {
                Edge edge = new Edge(localMap[e.Source], localMap[e.Target], ConnectionToGraph.Disconnected);
                edge.Attr.ArrowheadAtSource = e.ArrowheadAtSource ? ArrowStyle.Normal : ArrowStyle.None;
                edge.Attr.ArrowheadAtTarget = e.ArrowheadAtTarget ? ArrowStyle.Normal : ArrowStyle.None;
                edge.GeometryEdge = e;
                dg.AddPrecalculatedEdge(edge);
            }
            //PopulateClusterEdges(dg.RootSubgraph, gg.RootCluster);
            
            return dg;
        }

        private static void PopulateClusterEdges(Subgraph cluster, GeometryCluster c)
        {
            foreach (GeometryEdge e in c.InEdges)
            {
                Edge edge = new Edge(localMap[e.Source], localMap[e.Target], ConnectionToGraph.Disconnected);
                edge.Attr.ArrowheadAtSource = e.ArrowheadAtSource ? ArrowStyle.Normal : ArrowStyle.None;
                edge.Attr.ArrowheadAtTarget = e.ArrowheadAtTarget ? ArrowStyle.Normal : ArrowStyle.None;
                edge.GeometryEdge = e;
                dg.AddPrecalculatedEdge(edge);
                localMap[e.Source].AddOutEdge(edge);
                localMap[e.Target].AddInEdge(edge);
            }
            foreach (GeometryEdge e in c.OutEdges)
            {
                if (e.Target is GeometryCluster)
                    continue;
                Edge edge = new Edge(localMap[e.Source], localMap[e.Target], ConnectionToGraph.Disconnected);
                edge.Attr.ArrowheadAtSource = e.ArrowheadAtSource ? ArrowStyle.Normal : ArrowStyle.None;
                edge.Attr.ArrowheadAtTarget = e.ArrowheadAtTarget ? ArrowStyle.Normal : ArrowStyle.None;
                edge.GeometryEdge = e;
                dg.AddPrecalculatedEdge(edge);
                localMap[e.Source].AddOutEdge(edge);
                localMap[e.Target].AddInEdge(edge);
            }
            foreach (GeometryEdge e in c.SelfEdges)
            {
                Edge edge = new Edge(localMap[e.Source], localMap[e.Target], ConnectionToGraph.Disconnected);
                edge.Attr.ArrowheadAtSource = e.ArrowheadAtSource ? ArrowStyle.Normal : ArrowStyle.None;
                edge.Attr.ArrowheadAtTarget = e.ArrowheadAtTarget ? ArrowStyle.Normal : ArrowStyle.None;
                edge.GeometryEdge = e;
                dg.AddPrecalculatedEdge(edge);
                localMap[e.Source].AddSelfEdge(edge);
            }
            foreach (GeometryCluster c2 in c.Clusters)
                PopulateClusterEdges(localMap[c2] as Subgraph, c2);
        }

        private static void PopulateClusters(Subgraph cluster, GeometryCluster c)
        {
            foreach (GeometryNode n in c.Nodes)
                cluster.AddNode(localMap[n]);
            foreach (GeometryCluster c2 in c.Clusters)
            {
                Subgraph cluster2 = new Subgraph(counter++.ToString());
                cluster2.GeometryNode = c2;
                localMap[c2] = cluster2;
                cluster.AddSubgraph(cluster2);
                PopulateClusters(cluster2, c2);
            }
        }
    }
}
