using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Util
{
    public class NonOverlappingIntervalSet<T> : IEnumerable<NonOverlappingIntervalSet<T>.IntervalNode>
    {
        private IntervalNode _firstNode;

        public NonOverlappingIntervalSet()
        {
            //Always sorted left to right
            _firstNode = null;
        }

        public void AddInterval(Interval interval, params T[] intervalAssociatedItems)
        {
            AddInterval(interval, intervalAssociatedItems.AsEnumerable());
        }

        public void AddInterval(Interval interval, IEnumerable<T> intervalAssociatedItems)
        {
            var secondNode = new IntervalNode(interval, intervalAssociatedItems);
            if (_firstNode == null)
            {
                _firstNode = secondNode;
                return;
            }

            var node = _firstNode;
            while (node != null)
            {
                var split = new IntervalSplit(node, secondNode);
                if (!split.IsSplit)
                {
                    //No split was performed... and if the the secondNode was to the left of the right, we can just stop here
                    if (node.Interval.Max > secondNode.Interval.Max)
                    {
                        secondNode = node.InsertLeft(secondNode);
                        if (node == _firstNode)
                        {
                            _firstNode = secondNode;
                        }
                        return;
                    }
                    
                    if (node.Next == null)
                    {
                        //There is nothing to the right... so just insert
                        secondNode = node.InsertRight(secondNode);
                        return;
                    }

                    //Second is to the right... so we must continue!
                    node = node.Next;
                }
                else
                {
                    //A split occured!
                    var hasRemovedNode = false;
                    var left = split.Left;
                    if (left != null)
                    {
                        //Insert the left at the position of "sourceRange"
                        left = node.InsertLeft(left);
                        if (!hasRemovedNode)
                        {
                            node.Remove();
                            hasRemovedNode = true;
                        }    
                        if (node == _firstNode)
                            _firstNode = left;
                        node = left;
                    }

                    var intersection = split.Intersection;
                    if (intersection != null)
                    {
                        intersection = node.InsertRight(intersection);
                        if (!hasRemovedNode)
                        {
                            node.Remove();
                            hasRemovedNode = true;

                            if (node == _firstNode)
                                _firstNode = intersection;
                        }
                            
                        node = intersection;
                    }

                    var right = split.Right;
                    if(right == null)
                        return;

                    secondNode = right;

                    if (node.Next == null)
                    {
                        //There is nothing to the right... so just insert
                        secondNode = node.InsertRight(secondNode);
                        return;
                    }

                    node = node.Next;
                }
            }
        }

        public IEnumerator<IntervalNode> GetEnumerator()
        {
            return new Enumerator(_firstNode);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var node = _firstNode;
            while (node != null)
            {
                sb.Append(node.ToString() + ", ");
                node = node.Next;
            }

            sb.Length -= 2;

            return sb.ToString();
        }
        
        private class IntervalSplit
        {
            public IntervalSplit(IntervalNode first, IntervalNode second)
            {
                First = first;
                Second = second;

                Split();
            }
            
            public IntervalNode First { get; set; }
            public IntervalNode Second { get; set; }
            
            public bool IsSplit { get; set; }

            public IntervalNode Left { get; set; }
            public IntervalNode Intersection { get; set; }
            public IntervalNode Right { get; set; }

            private void Split()
            {
                var split = this;
                var firstNode = First;
                var secondNode = Second;

                var first = firstNode.Interval;
                var second = secondNode.Interval;
                
                split.IsSplit = false;

                var overlaps = first.Overlaps(second);
                if (!overlaps)
                {
                    return;
                }

                IsSplit = true;
                
                var intersectMin = first.Min;
                var leftTarget = secondNode;
                if (second.Min > intersectMin)
                {
                    leftTarget = firstNode;
                    intersectMin = second.Min;
                }
                if (second.Min == first.Min)
                    leftTarget = null;

                var intersectMax = first.Max;
                var rightTraget = secondNode;
                if (second.Max < intersectMax)
                {
                    rightTraget = firstNode;
                    intersectMax = second.Max;
                }
                if (second.Max == first.Max)
                    rightTraget = null;
                
                var intersectInterval = new Interval(intersectMin, intersectMax);
                Intersection = new IntervalNode(intersectInterval);
                Intersection.AddItems(firstNode.AssociatedItems);
                Intersection.AddItems(secondNode.AssociatedItems);
                
                if (leftTarget != null && !leftTarget.IntervalEquals(Intersection))
                {
                    var firstLocalNode = leftTarget;
                    var firstLocal = firstLocalNode.Interval;
                    var secondLocalNode = Intersection;
                    var secondLocal = Intersection.Interval;
                    
                    //Expect the first to be the one creating "left" range
                    var localMin = firstLocal.Min;
                    var localMax = secondLocal.Min;
                    if (secondLocal.Min < localMin)
                    {
                        //Second is lowest, so the second will be the one creating the "left" range
                        localMax = localMin;
                        localMin = secondLocal.Min;
                    }
                    if(firstLocal.Min != secondLocal.Min)
                        localMax = (char)(localMax - 1);

                    //Create a new left
                    var leftInterval = new Interval(localMin, localMax);
                    Left = new IntervalNode(leftInterval);
                    if (leftTarget.Interval.Min <= Intersection.Interval.Min)
                        Left.AddItems(leftTarget.AssociatedItems);
                }

                if (rightTraget != null && !rightTraget.IntervalEquals(Intersection))
                {
                    var firstLocalNode = Intersection;
                    var firstLocal = firstLocalNode.Interval;
                    var secondLocalNode = rightTraget;
                    var secondLocal = secondLocalNode.Interval;

                    //Expect the first to be the one creating "right" range
                    var localMin = firstLocal.Max;
                    var localMax = secondLocal.Max;
                    if (secondLocal.Max < localMin)
                    {
                        //Second is lowest, so the second will be the one creating the "left" range
                        localMax = localMin;
                        localMin = secondLocal.Max;
                    }
                    if (firstLocal.Max != secondLocal.Max)
                        localMin = (char)(localMin + 1);
                    
                    //Create a new left
                    var rightInterval = new Interval(localMin, localMax);
                    Right = new IntervalNode(rightInterval);
                    if(rightTraget.Interval.Max >= Intersection.Interval.Max)
                        Right.AddItems(rightTraget.AssociatedItems);
                }
            }
        }

        public class IntervalNode
        {
            private HashSet<T> _items;

            public IntervalNode(Interval interval)
                : this(interval, null)
            {
            }

            public IntervalNode(Interval interval, IEnumerable<T> associatedItems)
            {
                Interval = interval;

                _items = new HashSet<T>();

                AddItems(associatedItems);
            }

            public IntervalNode Next { get; private set; }

            public IntervalNode Prev { get; private set; }

            public Interval Interval { get; set; }
            
            public IEnumerable<T> AssociatedItems { get { return _items; } }

            public void AddItem(T associatedItem)
            {
                _items.Add(associatedItem);
            }

            public void AddItems(IEnumerable<T> associateItems)
            {
                if(associateItems == null)
                    return;

                foreach (var associateItem in associateItems)
                {
                    AddItem(associateItem);
                }
            }

            public IntervalNode InsertLeft(IntervalNode node)
            {
                var oldPrev = Prev;
                Prev = node;
                node.Next = this;
                node.Prev = oldPrev;
                if (oldPrev != null)
                {
                    oldPrev.Next = node;
                }
                return node;
            }

            public IntervalNode InsertRight(IntervalNode node)
            {
                var oldNext = Next;
                Next = node;
                node.Prev = this;
                node.Next = oldNext;
                if (oldNext != null)
                {
                    oldNext.Prev = node;
                }
                return node;
            }

            public void Remove()
            {
                var prev = Prev;
                var next = Next;
                
                if (prev != null)
                {
                    prev.Next = next;
                }
                if (next != null)
                {
                    next.Prev = prev;
                }
            }

            public bool IntervalEquals(IntervalNode other)
            {
                if (other == null)
                    return false;

                return Interval.Equals(other.Interval);
            }

            public override string ToString()
            {
                return Interval.ToString();
            }
        }

        public class Enumerator : IEnumerator<IntervalNode>
        {
            private IntervalNode _firstNode;
            private IntervalNode _currentNode;

            public Enumerator(IntervalNode first)
            {
                _firstNode = first;
            }

            public IntervalNode Current { get { return _currentNode; } }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (_currentNode == null)
                {
                    _currentNode = _firstNode;
                }
                else
                {
                    _currentNode = _currentNode.Next;
                }

                if (_currentNode == null)
                {
                    return false;
                }
                
                return true;
            }

            public void Reset()
            {
                _currentNode = null;
            }
            
            public void Dispose()
            {
            }
        }
    }
}
