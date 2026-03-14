public struct SimulationProgress
{
    public float progress;
    public string message;

    public SimulationProgress(float prog, string msg)
    {
        progress = prog;
        message = msg;
    }
}