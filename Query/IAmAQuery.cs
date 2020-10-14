namespace SolidCqrsFramework.Query
{
    public interface IAmAQuery<out TReturn>
    {
        TReturn Execute();
    }
}
