namespace Eventually.Interfaces.Common
{
    public interface IOptionallySequenced
    {
        int? Sequence { get; }
    }
}