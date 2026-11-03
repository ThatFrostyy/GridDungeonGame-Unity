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

    /// <summary>
    /// Whether this state applies to the enemy's current situation.
    /// </summary>
    public abstract bool ShouldEnter();

    /// <summary>
    /// Performs one action. Returns false if nothing could be done, letting the next state try.
    /// </summary>
    public abstract bool PerformAction();
}
