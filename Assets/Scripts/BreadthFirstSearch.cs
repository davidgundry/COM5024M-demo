using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Search : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int[,] grid = new int[10,10];
        grid[5,5] = 1;
        grid[1,0] = -1;
        grid[2,1] = -1;
        grid[3,2] = -1;
        BFS<GridNode> bfs = new((LinkedList<GridNode> list) => { 
            foreach (GridNode gn in list)
                Debug.Log(gn.x + "," + gn.y);
            }, new GridNode(grid, 0, 0));
        bfs.Search();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

class GridNode : BFS<GridNode>.Node
{
    int[,] grid;
    public int x, y;

    public GridNode(int[,] grid, int x, int y) {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public GridNode[] GetChildren()
    {
        LinkedList<GridNode> nodes = new();
        if (y > 0 && grid[x,y-1] >= 0)
            nodes.AddLast(new GridNode(grid, x, y-1));
        if (x < grid.GetLength(0)-1  && grid[x+1,y] >= 0)
            nodes.AddLast(new GridNode(grid, x+1, y));
        if (y < grid.GetLength(1)-1  && grid[x,y+1] >= 0)
            nodes.AddLast(new GridNode(grid, x, y+1));
        if (x > 0  && grid[x-1,y] >= 0)
            nodes.AddLast(new GridNode(grid, x-1, y));
        return nodes.ToArray();
    }

    public bool IsGoal()
    {
        return grid[x,y] == 1;
    }
}

class BFS<T> where T: BFS<T>.Node {
    public delegate void PathFound(LinkedList<T> path);

    public interface Node {
        public bool IsGoal();
        public T[] GetChildren();
    }

    private class BFSNode {
        public BFSNode(T node)
        {
            this.node = node;
        }
        public T node;
        public BFSNode parent;
    }

    private readonly Queue<BFSNode> _queue = new();
    private readonly HashSet<T> _discovered = new();

    private readonly PathFound _onPathFound;

    public BFS(PathFound onPathFound, T startNode)
    {
        _onPathFound = onPathFound;
        _queue.Enqueue(new BFSNode(startNode));
        _discovered.Add(startNode);
    }

    public void Search()
    {
        while (Step()) {}
    }

    bool Step()
    {
        if (_queue.Count == 0)
            return false;
            
        BFSNode current = _queue.Dequeue();
        if (current.node.IsGoal())
        {
            LinkedList<T> path = ConstructPath(current);
            if (_onPathFound != null)
                _onPathFound(path);
            return false;
        }

        foreach (T child in current.node.GetChildren())
        {
            if (!_discovered.Contains(child))
            {
                BFSNode n = new BFSNode(child);
                _queue.Enqueue(n);
                n.parent = current;
                _discovered.Add(child);
            }
        }
        return true;
    }

    LinkedList<T> ConstructPath(BFSNode goalNode)
    {
        LinkedList<T> list = new();
        BFSNode current = goalNode;
        list.AddFirst(goalNode.node);
        while (current != null)
        {
            list.AddFirst(current.node);
            current = current.parent;
        }
        return list;
    }

}
