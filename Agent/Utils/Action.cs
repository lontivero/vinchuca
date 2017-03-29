namespace Vinchuca.Utils
{
    public delegate void Action();
    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T, out TResult>(T arg);
    public delegate TResult Func<in T, in U, out TResult>(T arg1, U arg2);
}