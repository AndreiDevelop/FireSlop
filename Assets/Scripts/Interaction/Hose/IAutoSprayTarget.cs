namespace FireSlop.Interaction
{
    public interface IAutoSprayTarget : IWaterTarget
    {
        bool RequestsAutoSpray { get; }
    }
}
