namespace DreamBot.Utils
{
    public delegate void Action();
    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T, out TResult>(T arg);
}