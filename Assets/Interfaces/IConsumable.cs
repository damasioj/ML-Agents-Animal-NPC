public interface IConsumable
{
    /// <summary>
    /// Consumes for the given amount. Returns whether the object has been fully consumed or not.
    /// While interfaces are ideal for adding behaviour to objects, they're not practical with Unity.
    /// Interfaces are not used in the other environments for this reason.
    /// </summary>
    /// <param name="value">Value to consume</param>
    /// <returns></returns>
    float Consume(int value);
    bool IsConsumed { get; }
}
