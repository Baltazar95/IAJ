﻿using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public interface IHeuristic
    {
        float H(NavigationGraphNode node, NavigationGraphNode goalNode);
        float H(Vector3 initial, Vector3 final);
    }
}
