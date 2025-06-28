namespace Perform.Interfaces;

public interface IScriptEventHandler
{
    public void Initialize();

    public void Loop();

    public void Finally();

    public ScriptLoopStatus Status { get; }
}

public enum ScriptLoopStatus
{
    Continue,
    EndLoop,
    Finished
}