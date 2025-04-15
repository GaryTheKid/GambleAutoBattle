using System.Collections.Generic;

public class UnitIdPool
{
    private Queue<ushort> availableIds = new Queue<ushort>();
    private HashSet<ushort> usedIds = new HashSet<ushort>();

    private const ushort MAX_UNITS = 1000; // Maximum allowed units

    public UnitIdPool()
    {
        // Fill the queue with all possible IDs
        for (ushort i = 0; i < MAX_UNITS; i++)
        {
            availableIds.Enqueue(i);
        }
    }

    /// <summary>
    /// Requests a new unique Unit ID from the pool.
    /// Returns true if an ID is available, otherwise false.
    /// </summary>
    public bool TryGetId(out ushort id)
    {
        if (availableIds.Count > 0)
        {
            id = availableIds.Dequeue();
            usedIds.Add(id);
            return true;
        }

        id = 0; // Invalid ID (no IDs available)
        return false;
    }

    /// <summary>
    /// Releases a Unit ID back into the pool for reuse.
    /// </summary>
    public void ReleaseId(ushort id)
    {
        if (usedIds.Contains(id))
        {
            usedIds.Remove(id);
            availableIds.Enqueue(id);
        }
    }
}
