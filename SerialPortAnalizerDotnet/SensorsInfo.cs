using System.Globalization;

public class SensorsInfo
{
    public int Distance;
    public float Humidity;
    public float Temperature;
    public float GasSensorValue;
    private List<string> list = new List<string>();
    private NumberFormatInfo numberFormatInfo = new NumberFormatInfo() {NumberDecimalSeparator = "."};
    public SensorsInfo() { }
    public void ParseSensorsLineData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return;
        if (data.Contains("Distance"))
            int.TryParse(data.Split(" ", 3)[1], out Distance);
        if (data.Contains("Humidity"))
        {
            float.TryParse(data.Split(" ", 5)[1], NumberStyles.Number, numberFormatInfo, out Humidity);
            float.TryParse(data.Split(" ", 5)[3], NumberStyles.Number, numberFormatInfo, out Temperature);
        }
        if (data.Contains("MQ"))
            float.TryParse(data.Split(" ", 3)[2], NumberStyles.Number, numberFormatInfo, out GasSensorValue);
    }

    public override string ToString()
    {
        return $"Distance: {Distance}, Humidity: {Humidity}, Temperature: {Temperature}, GasValue: {GasSensorValue} ----- CurrentTime: {DateTime.Now}";
    }
}