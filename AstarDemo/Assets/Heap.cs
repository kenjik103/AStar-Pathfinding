using System;

public class Heap <T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount;
    public int Count {
        get {
            return currentItemCount;
        }
    }
    
    public Heap(int maxHeapSize) {
        items = new T[maxHeapSize];
    }

    public void Add(T item){
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst(){
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public bool Contains (T item) {
        return Equals(items[item.HeapIndex], item);
    }

    //in our pathfinding there are situations where we need to decrease the f_cost of a node. Because we only ever decrease the value, SortDown is not necessary
    public void UpdateItem(T item) {
        SortUp(item);
    }

    void SortUp (T item) {
        int parentIndex = (item.HeapIndex -1 )/2;

        while (true) {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0){ //Swap if item is greather than its parent
                Swap(item, parentItem);
            } else {
                break;
            }
        }
    }

    void SortDown(T item){
        while(true) {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount) { //checking if left child exists
                swapIndex = childIndexLeft;
                if (childIndexRight < currentItemCount) {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) { //checking which child is smaller
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0){
                    Swap(item, items[swapIndex]);
                } else {
                    return; // if parent has lowest priority
                }
            } else {
                return; //if parent has no children
            }
        }
    }

    void Swap(T itemA, T itemB) {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int temp = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = temp;
    }
}

/**interface ensures items are comparable, assigns an index to each item. Comparing is neccessary because we must be able to sort items within
the heap **/
public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}