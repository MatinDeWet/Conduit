namespace Conduit.Contract.Models
{
    public record Unit
    {
        private Unit() { }

        public static Unit Value { get; } = new();
    }
}
