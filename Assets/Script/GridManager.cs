using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.InputSystem;

public class Node
{
    public int x;
    public int y;

    public bool isWalkable;
    public GameObject tile;

    public float fGCost;
    public float fHCost;
    public Node parent;

    public float fCost => fGCost + fHCost;

    public Node(int x, int y, bool isWalkable, GameObject tile)
    {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
        this.tile = tile;
        fGCost = float.PositiveInfinity;
        fHCost = 0;
        parent = null;
    }

}


namespace Lab2
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int iWidth = 10;
        public int iHeight = 10;
        public float fCellSize = 1f;

        [Header("Prefabs & Materials")]
        [SerializeField] GameObject tilePrefab;
        [SerializeField] Material walkableMaterial;
        [SerializeField] Material wallMaterial;

        [SerializeField] Node[,] nodes;

        Dictionary<GameObject, Node> tileToNode = new Dictionary<GameObject, Node>();

        private InputAction clickAction;

        private void Awake()
        {
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            nodes = new Node[iWidth, iHeight];

            for (int x = 0; x < iWidth; x++)
            {
                for (int y = 0; y < iHeight; y++)
                {
                    // World position for this tile
                    Vector3 worldPos = new Vector3(x * fCellSize, 0f, y * fCellSize);
                    GameObject tileGO = Instantiate(tilePrefab, worldPos, Quaternion.identity, this.transform);
                    tileGO.name = $"Tile_{x}_{y}";

                    //Create node
                    Node node = new Node(x, y, true, tileGO);
                    nodes[x, y] = node;
                    tileToNode[tileGO] = node;

                    //Set initial colour
                    SetTileMaterial(node, walkableMaterial);
                }
            }
        }

        public Node GetNode(int x, int y)
        {
            if (x < 0 || x >= iWidth || y < 0 || y >= iHeight) return null;
            return nodes[x, y];
        }

        public void SetWalkable(Node node, bool walkable)
        {
            node.isWalkable = walkable;
            SetTileMaterial(node, walkable ? walkableMaterial : wallMaterial);
        }

        private void SetTileMaterial(Node node, Material material)
        {
            var renderer = node.tile.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }

        public IEnumerable<Node> GetNeighbours(Node node, bool allowDiagonals = false)
        {
            int x = node.x;
            int y = node.y;

            // 4-neighbours

            yield return GetNode(x + 1, y);
            yield return GetNode(x - 1, y);
            yield return GetNode(x, y + 1);
            yield return GetNode(x, y - 1);

            if (allowDiagonals)
            {
                yield return GetNode(x + 1, y + 1);
                yield return GetNode(x - 1, y + 1);
                yield return GetNode(x + 1, y - 1);
                yield return GetNode(x - 1, y - 1);
            }
        }

        //Helper to get node from world position (for selecting start/goal)
        public Node GetNodeFromWorldPosition(Vector3 worldpos)
        {
            int x = Mathf.RoundToInt(worldpos.x / fCellSize);
            int y = Mathf.RoundToInt(worldpos.y / fCellSize);
            return GetNode(x, y);
        }

        //For clicking tiles; get node from tile GO
        public Node GetNodeFromTile(GameObject tileGO)
        {
            if (tileToNode.TryGetValue(tileGO, out var node))
            {
                return node;
            }
            return null;
        }

        private void OnEnable()
        {
            clickAction = new InputAction
                (
                name: "Click",
                type: InputActionType.Button,
                binding: "<Mouse>/leftButton"
                );

            clickAction.performed += OnClickPerformed;
            clickAction.Enable();
        }

        private void OnDisable()
        {
            if(clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext ctx)
        {
            HandleMouseClick();
        }

        private void HandleMouseClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                GameObject clicked = hitInfo.collider.gameObject;
                Node node = GetNodeFromTile(clicked);
                if (node != null)
                {
                    bool newWalkable = !node.isWalkable;
                    SetWalkable(node, newWalkable);
                }
            }
        }
    }
}

