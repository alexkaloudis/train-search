namespace Train.Search.WebApplication.Models.Dto;

public class StationIncomingData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double LocationX { get; set; }
    public double LocationY { get; set; }
    public string StandardName { get; set; }

    public StationIncomingData(string id, string name, double locationX, double locationY, string standardName)
    {
        Id = id;
        Name = name;
        LocationX = locationX;
        LocationY = locationY;
        StandardName = standardName;
    }
}