namespace BoxerTrucks.Api.Dtos;

public enum HomeSizeType
{
    Studio,
    OneBedroom,
    TwoBedroom,
    ThreeBedroom
}

public class QuoteRequestDto
{
    public HomeSizeType HomeSize { get; set; }

    public int SquareFeet { get; set; }

    public decimal Miles { get; set; }

    public int HelperCount { get; set; }

    public int StairFlights { get; set; }

    public bool LongCarry { get; set; }

    public bool HasHeavyItem { get; set; }

    public string? ItemsNotes { get; set; }
}
