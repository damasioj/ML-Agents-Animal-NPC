public interface IConsumable
{
    /// <summary>
    /// Consumes for the given amount. Returns whether the object has been fully consumed or not.
    /// </summary>
    /// <param name="value">Value to consume</param>
    /// <returns></returns>
    bool Consume(float value);
    bool IsConsumed { get; }
}
