﻿using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathFinding : AStarPathfinding
    {
        protected NodeRecordArray NodeRecordArray { get; set; }
        public NodeArrayAStarPathFinding(NavMeshPathGraph graph, IHeuristic heuristic) : base(graph,null,null,heuristic)
        {
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
        }

        protected override void ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge)
        {
            float f;
            float g;
            float h;

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            if (childNodeRecord == null)
            {
                //this piece of code is used just because of the special start nodes and goal nodes added to the RAIN Navigation graph when a new search is performed.
                //Since these special goals were not in the original navigation graph, they will not be stored in the NodeRecordArray and we will have to add them
                //to a special structure
                //it's ok if you don't understand this, this is a hack and not part of the NodeArrayA* algorithm
                childNodeRecord = new NodeRecord
                {
                    node = childNode,
                    parent = bestNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childNodeRecord);
            }

            g = bestNode.gValue + connectionEdge.Cost;
            h = this.Heuristic.H(childNode, this.GoalNode);
            f = g + h;

            NodeRecord thisChildOpen = this.Open.SearchInOpen(childNodeRecord);
            NodeRecord thisChildClosed = this.Closed.SearchInClosed(childNodeRecord);
            bool inOpen = (thisChildOpen == null);
            bool inClosed = (thisChildClosed == null);
            if (inOpen && inClosed)
            {
                childNodeRecord.parent = bestNode;
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.fValue = f;

                this.Open.AddToOpen(childNodeRecord);
                if (this.MaxOpenNodes < this.Open.CountOpen())
                {
                    this.MaxOpenNodes = this.Open.CountOpen();
                }
            }
            else if (!inOpen && thisChildOpen.fValue >= childNodeRecord.fValue)
            {
                childNodeRecord.parent = bestNode;
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.fValue = f;

                this.Open.Replace(childNodeRecord, thisChildOpen);
            }
            else if (!inClosed && thisChildClosed.fValue > childNodeRecord.fValue)
            {
                childNodeRecord.parent = bestNode;
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.fValue = f;

                this.Closed.RemoveFromClosed(thisChildClosed);
                this.Open.AddToOpen(childNodeRecord);
                if (this.MaxOpenNodes < this.Open.CountOpen())
                {
                    this.MaxOpenNodes = this.Open.CountOpen();
                }
            }


            // implement the rest of your code here


        }

        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>) Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }
    }
}
