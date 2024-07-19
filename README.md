# AStar-Pathfinding

Followed Sebastian Lague's [A* Pathfinding Tutorial](https://www.youtube.com/playlist?list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW) to implement an A* pathfinding algorithm in Unity. Beyond just the basic algorithm, there are a few features and optimizations included in the tutorial

## Complicated Route:
![Screen Recording 2024-07-18 at 9 07 57 PM](https://github.com/user-attachments/assets/97a686ae-0d2a-4681-9d82-b272aa5c44e8)

## Path Weighing
![Screen Recording 2024-07-18 at 9 23 23 PM](https://github.com/user-attachments/assets/f1774809-4907-442e-9003-51f0d32726d7)

## Multiple Concurrent 
![Screen Recording 2024-07-18 at 9 28 20 PM](https://github.com/user-attachments/assets/5a007a94-6b13-410f-a334-b00d1a52010c)


### The Algorithm

This particular implementation of A* is limited to just a 2D plane. The algorithm creates a 2D grid of nodes that can be overlayed on top of a 2D world. Using Unity's LayerMask feature, obstacles can be placed and marked as untraversable. Objects can be assigned the "unit" script, which marks it as a seeker. The seekers will then run the pathfinding algorithm in search of a designated target.

### Optimization - Heap Implementation

This algorithm makes use of a custom minimum-heap data structure. A* is constantly looking for nodes with the lowest f_cost, so using a minimum heap and storing the node with the smallest f_cost in the front of the data structure cuts this search time down to O(1), making it a huge optimization.

### Feature - Desirable and Undesirable Paths

Using Unity's LayerMask function, this tutorial enables us to add movement costs to terrain. This movement cost can be considered in our pathfinding algorithm, allowing us to lay down paths that our algorithm will prefer.

### Feature - Blurring movement costs

Although paths can be layed down, objects will somethimes stick to the edges of curved paths if thats the shortest possible route. To make objects move more naturally, a blurring effect is applied on each node, meaning nodes with neighboring undesirable nodes will also inherit some of that movement cost. This causes objects to prefer the center of desired paths, and give undesirable and untraversable objects a wider berth.

### Feature - Path smoothing

Because are nodes are arranged in a 2D grid, the path that objects end up following will consist of straight, jagged edges. Instead of storing every single node the object follows, one small optimization is to only store nodes that change direction from the previous node instead, creating a list of waypoints. Then, behind every waypoint we draw a "turn boundry", basically a line perpendicaular to the path that the node is following. Once this turn bourdary is established, if our seeker object crosses it we begin to roatate it torwards the next waypoint. This gives the object much smoother movement.

### Optimization - Threading and IEnumerator

The last optimization involves concurency. Implementing this algorithm on multiple objects can cause the program to freeze while the computer tries to calculate each objects path at the same time. To prevent this, the tutorial uses IEnumerator to force the CPU to handle one path at a time, keeping the program from freezing. It then uses Threading to run each of these path requests on a seperate thread, allowing the algorithm to smoothly handle multiple seeker objects at the same time.
