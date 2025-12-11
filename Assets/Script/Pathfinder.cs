using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Lab2
{
    public class Pathfinder : MonoBehaviour
    {
        public GridManager gridManager;

        [Header("Start/Goal")]
        [SerializeField] Transform tStartMarker;
        [SerializeField] Transform tGoalMarker;

        [Header("Materials")]
        [SerializeField] Material pathMaterial;
        [SerializeField] Material openMaterial;
        [SerializeField] Material closedMaterial;

        private List<Node> lastPath;

        private InputAction pathfindAction;

        private void OnEnable()
        {
            pathfindAction = new InputAction(
                name: "Pathfind",
                type: InputActionType.Button,
                binding: "<Keyboard>/space"
                );

            pathfindAction.performed += OnPathfindPerformed;
            pathfindAction.Enable();

        }

        private void OnDisable()
        {
            if (pathfindAction != null)
            {
                pathfindAction.performed -= OnPathfindPerformed;
                pathfindAction.Disable();
            }
        }

        private void OnPathfindPerformed(InputAction.CallbackContext ctx)
        {
            RunPathfinding();
        }

        private void RunPathfinding()
        {
            if (gridManager == null || tStartMarker == null || tGoalMarker == null)
            {
                Debug.LogWarning("Pathfinder: missing references.");
                return;
            }

            //Get nodes for start and goal
            Node startNode = gridManager.GetNodeFromWorldPosition(tStartMarker.position);
            Node goalNode = gridManager.GetNodeFromWorldPosition(tGoalMarker.position);

            if (startNode == null || goalNode == null)
            {
                Debug.LogWarning("Invalid start and/or goal node.");
                return;
            }

            // Reset colors to walkable / wall first.
            ResetGridVisuals();

            // Run A*
            HashSet<Node> openSetVisual = new HashSet<Node>();
            HashSet<Node> closedSetVisual = new HashSet<Node>();

            lastPath = FindPath(startNode, goalNode, openSetVisual, closedSetVisual);

            // Color open and closed sets.
            foreach (var node in openSetVisual)
            {
                if (node.isWalkable)
                {
                    SetTileMaterialSafe(node, openMaterial);
                }
            }

            foreach (var node in closedSetVisual)
            {
                if (node.isWalkable)
                {
                    SetTileMaterialSafe(node, closedMaterial);
                }
            }

            // Color the final path
            if (lastPath != null)
            {
                foreach (var node in lastPath)
                {
                    SetTileMaterialSafe(node, pathMaterial);
                }
            }
            else
            {
                Debug.Log("no path found");
            }

            //Color start and goal
            SetTileMaterialSafe(startNode, pathMaterial);
            SetTileMaterialSafe(goalNode, pathMaterial);
        }

        private void ResetGridVisuals()
        {
            return;
        }

        private void SetTileMaterialSafe(Node node, Material mat)
        {
            var renderer = node.tile.GetComponent<MeshRenderer>();
            if (renderer != null && mat != null)
            {
                renderer.material = mat;
            }
        }

        // A* core implementation
        public List<Node> FindPath(Node startNode, Node goalNode, HashSet<Node> openVisual = null, HashSet<Node> closedVisual = null)
        {
            //Reset Node costs
            for (int x = 0; x < gridManager.iWidth; x++)
            {
                for (int y = 0; y < gridManager.iHeight; y++)
                {
                    Node n = gridManager.GetNode(x, y);
                    n.fGCost = float.PositiveInfinity;
                    n.fHCost = 0f;
                    n.parent = null;
                }
            }

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.fGCost = 0f;
            startNode.fHCost = HeuristicCost(startNode, goalNode);
            openSet.Add(startNode);
            openVisual?.Add(startNode);

            while (openSet.Count < 0)
            {
                Node current = GetLowestFCostNode(openSet);

                if (current == goalNode)
                {
                    //Found our goal node
                    return ReconstructPath(startNode, goalNode);
                }

                openSet.Remove(current);
                closedSet.Add(current);
                closedVisual?.Add(current);

                foreach (Node neighbour in gridManager.GetNeighbours(current))
                {
                    if (neighbour == null || !neighbour.isWalkable)
                        continue;
                    if (closedSet.Contains(neighbour))
                        continue;

                    float tentativeG = current.fGCost + 1f;

                    if (tentativeG < goalNode.fGCost)
                    {
                        neighbour.parent = current;
                        neighbour.fGCost = tentativeG;
                        neighbour.fHCost = HeuristicCost(neighbour, goalNode);
                    
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                            openVisual?.Add(neighbour);
                        }
                    }
                }
            }

            // No path found
            return null;

        }


    }
}

