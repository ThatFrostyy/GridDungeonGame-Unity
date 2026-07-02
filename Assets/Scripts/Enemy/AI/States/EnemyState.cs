/// <summary>
/// Base state for enemy AI behavior.
/// </summary>
public abstract class EnemyState
{
    protected EnemyAIContext Context { get; private set; }

    public abstract string Name { get; }

    public void Initialize(EnemyAIContext context)
    {
        Context = context;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public abstract bool ShouldEnter();
    public abstract bool PerformAction();
}
