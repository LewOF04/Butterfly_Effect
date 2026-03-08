public class SimulationActionWrapper
{
    ActionInfoWrapper info;
    float startTime;
    float endTime;
    float percentComplete;

    public SimulationActionWrapper(ActionInfoWrapper infoInput, float startInput, float endInput, float percInput)
    {
        info = infoInput;
        startTime = startInput;
        endTime = endInput;
        percentComplete = percInput;
    }

    public ActionInfoWrapper(){}
}