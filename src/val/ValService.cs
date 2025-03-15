namespace kr1.val
{
    public class ValService : IValService
    {
        public bool IsAmountValid(decimal amount)
        {
            return amount > 0;
        }
    }
}
