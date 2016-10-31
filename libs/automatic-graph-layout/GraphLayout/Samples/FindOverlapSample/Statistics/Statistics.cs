﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Factorization;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Prototype.LayoutEditing;

namespace OverlapGraphExperiments.Statistics {
    class Statistics {


        /// <summary>
        /// Percentage of orientation change for a set of given triangles.
        /// </summary>
        /// <param name="graphOld"></param>
        /// <param name="graphNew"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static Tuple<String, double> TriangleOrientation(GeometryGraph graphOld, GeometryGraph graphNew,
            HashSet<Tuple<int, int, int>> triangles) {
            int orientChange = 0;
            foreach (Tuple<int, int, int> triangle in triangles) {
                Point a = graphOld.Nodes[triangle.Item1].Center;
                Point b = graphOld.Nodes[triangle.Item2].Center;
                Point c = graphOld.Nodes[triangle.Item3].Center;

                Point d = graphNew.Nodes[triangle.Item1].Center;
                Point e = graphNew.Nodes[triangle.Item2].Center;
                Point f = graphNew.Nodes[triangle.Item3].Center;

                double signOld = Point.SignedDoubledTriangleArea(a, b, c);
                double signNew = Point.SignedDoubledTriangleArea(d, e, f);

                if (signOld*signNew < 0) orientChange++;
            }
            double percentageOrientChange = ((double) orientChange)/triangles.Count;
            return Tuple.Create("TriangleOrientation", percentageOrientChange);
        }


        /// <summary>
        /// Computes the standard deviation of the edge length change for a set of given edges.
        /// </summary>
        /// <param name="graphOld"></param>
        /// <param name="graphNew"></param>
        /// <param name="proximityEdges"></param>
        /// <returns></returns>
        public static Tuple<String, double> RotationAngleMean(GeometryGraph graphOld, GeometryGraph graphNew,
            HashSet<Tuple<int, int>> proximityEdges) {
            if (proximityEdges.Count == 0) return Tuple.Create("RotationAngleMean", -1.0);
            double meanRotationAngle = 0;
            int n = proximityEdges.Count;
            foreach (var p in proximityEdges) {
                var oldDir = graphOld.Nodes[p.Item1].Center - graphOld.Nodes[p.Item2].Center;
                var newDir = graphNew.Nodes[p.Item1].Center - graphNew.Nodes[p.Item2].Center;

                double angle = Point.Angle(oldDir, newDir);
                Debug.Assert(angle >= 0);
                if (angle > Math.PI)
                    angle = 2*Math.PI - angle;
                Debug.Assert(angle >= 0);
                meanRotationAngle += angle;
            }
            meanRotationAngle /= n;
            meanRotationAngle *= (180/Math.PI);


            return Tuple.Create("RotationAngleMean", meanRotationAngle);
        }

        /// <summary>
        /// Additionally needed area of nodes compared to the optimal packing of the nodes.
        /// </summary>
        /// <param name="graphOriginal"></param>
        /// <param name="graphNew"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static Tuple<String, double> Area(GeometryGraph graph) {
            double minimalArea = graph.Nodes.Sum(v => v.BoundingBox.Area);

            Rectangle boundingBoxNew = new Rectangle(graph.Nodes.Select(v => v.BoundingBox));
            double areaNew = boundingBoxNew.Area;

            double ratio = areaNew/minimalArea;
//            return Tuple.Create("AreaIncreaseToMinPacking",ratio - 1);//we are interested in increase compared to optimal packing.
            return Tuple.Create("AreaAbsolute/(1E6)", areaNew/(1E6));
                //we are interested in increase compared to optimal packing.
        }

        /// <summary>
        /// Procustes statistics which gives a (di)similiarity measure of two set of points, by removing translation, rotation and dilation(stretching) degrees of freedom.
        /// Zero as result means the two sets of points are basically the same after translation, rotation and dilation with the corresponding matrices.
        /// Reference: Modern Multidimensional Scaling, Theory and Applications, page 436, Procrustes Analysis
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Tuple<String, double> ProcrustesStatistics(List<Point> A, List<Point> B) {
            int n = A.Count;
            //make A to be unitlength
            double minX = A.Min(p => p.X);
            double maxX = A.Max(p => p.X);
            double minY = A.Min(p => p.Y);
            double maxY = A.Max(p => p.Y);

            double deltaX = maxX - minX;
            double deltaY = maxY - minY;
            double scale = Math.Max(deltaX, deltaY);


            A = A.Select(p => new Point(p.X/scale, p.Y/scale)).ToList();


            var centerA = new Point(A.Average(a => a.X), A.Average(a => a.Y));
            var centerB = new Point(B.Average(b => b.X), B.Average(b => b.Y));

            Matrix X = DenseMatrix.Create(n, 2, (i, j) => j == 0 ? A[i].X : A[i].Y);
            Matrix Y = DenseMatrix.Create(n, 2, (i, j) => j == 0 ? B[i].X : B[i].Y);

            Matrix Xc = DenseMatrix.Create(n, 2, (i, j) => j == 0 ? A[i].X - centerA.X : A[i].Y - centerA.Y);
            Matrix Yc = DenseMatrix.Create(n, 2, (i, j) => j == 0 ? B[i].X - centerB.X : B[i].Y - centerB.Y);

            //Reference: Modern Multidimensional Scaling, Theory and Applications, page 436, Procrustes Analysis
            DenseMatrix C = (DenseMatrix) (Xc.Transpose()*Y);

            Svd svd = new DenseSvd(C, true);
            //rotation
            Matrix<double> T = (svd.VT().Transpose())*(svd.U().Transpose());
            //dilation
            double s = ((C*T).Trace())/((Yc.Transpose()*Y).Trace());
            //column Vector with n times 1
            Vector<double> vector1 = DenseVector.Create(n, i => 1);
            //translation vector
            Vector<double> t = (1.0/n)*(X - s*Y*T).Transpose()*vector1;

            Matrix translationMatrix = DenseMatrix.Create(n, 2, (i, j) => t.At(j));

            Matrix<double> YPrime = s*Y*T + translationMatrix;
            Matrix<double> delta = X - YPrime;

            double rSquare = 0;
            for (int i = 0; i < n; i++) {
                rSquare += delta.Row(i)*delta.Row(i);
            }
            return Tuple.Create("ProcrustesStatistics", Math.Sqrt(rSquare));
        }

        /// <summary>
        /// Method to test the ProcrustesStatistics
        /// </summary>
        public static void TestProcrustesStatistics() {


            List<Point> pointsX = new List<Point>();
            pointsX.Add(new Point(1, 2));
            pointsX.Add(new Point(-1, 2));
            pointsX.Add(new Point(-1, -2));
            pointsX.Add(new Point(1, -2));

            List<Point> pointsY = new List<Point>();
            pointsY.Add(new Point(0.07, 2.62));
            pointsY.Add(new Point(0.93, 3.12));
            pointsY.Add(new Point(1.93, 1.38));
            pointsY.Add(new Point(1.07, 0.88));

            double res = Statistics.ProcrustesStatistics(pointsX, pointsY).Item2;
            Console.WriteLine("Result should nearly Zero: {0}", res);
            List<Point> randomPoints = new List<Point>();
            Random rand = new Random(2);
            for (int i = 0; i < pointsX.Count; i++) {
                randomPoints.Add(new Point(rand.NextDouble(), rand.NextDouble()));
            }

            double res2 = Statistics.ProcrustesStatistics(pointsY, randomPoints).Item2;
            Console.WriteLine("Result should big much bigger then zero: {0}", res2);

        }

        public static Tuple<string, double> TrianglePropertyError(GeometryGraph graphOld, GeometryGraph graphNew,
            HashSet<Tuple<int, int, int>> triangles) {
            double distortionErrorTotal = 0;
            foreach (Tuple<int, int, int> triangle in triangles) {
                var distortionOld = TriangleDistortion(graphOld, triangle);
                var distortionNew = TriangleDistortion(graphNew, triangle);
                distortionErrorTotal += Math.Abs(distortionNew - distortionOld)/distortionOld;
            }

            return Tuple.Create("TriangleDistortion", distortionErrorTotal);
        }

        private static double TriangleDistortion(GeometryGraph graphOld, Tuple<int, int, int> triangle) {
            Point d = graphOld.Nodes[triangle.Item1].Center;
            Point e = graphOld.Nodes[triangle.Item2].Center;
            Point f = graphOld.Nodes[triangle.Item3].Center;
            double de = (d - e).Length;
            double ef = (e - f).Length;
            double fd = (f - d).Length;
            double distortionOld = Math.Max(de, Math.Max(ef, fd))/Math.Min(de, Math.Min(ef, fd));
            return distortionOld;
        }

        public static Tuple<string, double> SharedFamily(GeometryGraph graphOld, GeometryGraph graphNew, int familySize) {
            double ret = 0;
            int n = graphNew.Nodes.Count();
            for (int i = 0; i < n; i++) {
                int interseciontSize = SizeOfFamilyIntersection(i, graphNew, graphOld, familySize);
                int del = familySize - interseciontSize;
                del*=del;
                ret += del;
            }
            ret /= n;
            return new Tuple<string, double>("f"+familySize, ret);
        }

        private static int SizeOfFamilyIntersection(int i, GeometryGraph graphNew, GeometryGraph graphOld,
            int familySize) {
            Set<int> oldFamily = GetNodeFamily(i, graphOld, familySize);
            Set<int> newFamily = GetNodeFamily(i, graphNew, familySize);
            return oldFamily.Intersect(newFamily).Count();
        }

        private static Set<int> GetNodeFamily(int origin,
            GeometryGraph graph, int familySize) {
            List<Tuple<int, double>> family = new List<Tuple<int, double>>();
            for (int i = 0; i < familySize; i++) {
                family.Add(new Tuple<int, double>(-1, 0)); // -1 mean not initialized
            }
            for (int i = 0; i < graph.Nodes.Count(); i++) {
                if (i == origin) continue;
                double dist = (graph.Nodes[i].Center - graph.Nodes[origin].Center).Length;
                TryToAddToFamily(family, i, dist);
            }
            Set<int> ret = new Set<int>(family.Select(t => t.Item1).Where(i => i >= 0));
            return ret;
        }

        private static void TryToAddToFamily(List<Tuple<int, double>> family, int i, double dist) {
            for (int j = 0; j < family.Count; j++) {
                if (family[j].Item1 == -1 ) {
                    family[j] = new Tuple<int, double>(i, dist);
                    return;
                }
            }
            for (int j = 0; j < family.Count; j++) {
                if (family[j].Item2 > dist) {
                    family[j] = new Tuple<int, double>(i, dist);
                    return;
                }
            }
        }
    }
}