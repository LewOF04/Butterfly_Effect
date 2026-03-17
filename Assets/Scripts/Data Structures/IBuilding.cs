using System.Collections.Generic;

public interface IBuilding
{
    int id { get; set; }           
    string buildingName { get; set; }
    int buildingType { get; set; }
    float condition { get; set; }
    List<int> inhabitants { get; set; }
    string familyName { get; set; }
}