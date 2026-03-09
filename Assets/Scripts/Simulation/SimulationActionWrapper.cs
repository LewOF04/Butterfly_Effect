public class SimulationActionWrapper
{
    public ActionInfoWrapper info;
    public float startTime;
    public float endTime;
    public float percentComplete;

    public SimulationActionWrapper(ActionInfoWrapper infoInput, float startInput, float endInput, float percInput)
    {
        info = infoInput;
        startTime = startInput;
        endTime = endInput;
        percentComplete = percInput;
    }

    public SimulationActionWrapper(){}
}