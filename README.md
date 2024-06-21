# AStar-Pathfinding

Followed Sebastian Lague's [A* Pathfinding Tutorial](https://www.youtube.com/playlist?list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW) to implement an A* pathfinding algorithm in Unity. While the algorithm itself wasn't too difficult to understand, the various optimizations and features were pretty wild. To prove to myself that I truly digested everything from the videos, I'm going use this README to explain all of the optimizations and features as much depth as possible.

### The Algorithm

As I stated earlier, the algorithm it self isn't too complicated. We take the width and height of our world, and overlay it with a 2D grid of Nodes. Each node stores information that the algorithm needs, namely the f_cost, whether or not a node is traversable, and the parent of the node. F_cost is simply the sum of the g_cost (distance from starting node to current node) and h_cost (distance to current node to target/finish node). A* starts at the starting node, calculates the f_cost of all of its neighbors, sets the parent of each neigbor to the current node, and adds them to a list of nodes to be considered (it only does this if the neigbor is considered "traversable"). A* then takes the lowest f_cost from the list of nodes to be considered, marks it as the current node, and repeats. Once a neigboring node is found to be the target/finish node, the algorithm is done.

This is a bit of an oversimplification, but that's the essence of it. Pseudocode is easy to follow and all over the internet so all things considered this was one of the easiest parts of the tutorial to follow.

We can determine whether or not a node is traversable using Unity's LayerMask feature. Obstacles are marked with an "Unwalkable" layer within Unity, and in our script we simply check if a node collides with any object of the "unwalkable" layer type. If it does, we mark it as untraversable.

### Optimization - Heap Implementation

I mentioned earlier that each time A* wants to select a new current node, it searches through the list of nodes being considered and selects the one with the minimum f_cost. Doing this the traditional way via iterating through the entire list of nodes being considered is quite expensive. Thankfully, data structures, specifically a minimum heap exist. These data structres store data in such a way that the first item at index 0 is always the minimum value. 

In essence, a minimum heap can be visualized as a binary tree where nodes on the right are greater than nodes on the left. The node at the root always contains the smallest value. When a new node is added to the tree, it is added at the end. Then, it is compared to its parent and if it is less than its parent we swap it. We simply repeat that process till the new node is in its right place. Removing a node is a similar process. We remove the node at the root of the tree and return its value. We then insert whatever was at the end of our heap into this newly emptied root position. After that, we simply run a series of comparisons 
