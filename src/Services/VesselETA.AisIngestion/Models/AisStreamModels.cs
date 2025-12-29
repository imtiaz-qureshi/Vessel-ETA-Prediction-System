using Newtonsoft.Json;

namespace VesselETA.AisIngestion;

public class AisStreamMessage
{
    public string MessageType { get; set; } = string.Empty;
    public AisMessage? Message { get; set; }
    public AisMetaData MetaData { get; set; } = new();
}

public class AisMessage
{
    public PositionReport? PositionReport { get; set; }
    public StandardClassBPositionReport? StandardClassBPositionReport { get; set; }
    public ShipStaticData? ShipStaticData { get; set; }
    public StaticDataReport? StaticDataReport { get; set; }
}

public class PositionReport
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Sog { get; set; } // Speed over ground
    public double Cog { get; set; } // Course over ground
    public int NavigationalStatus { get; set; }
    public double RateOfTurn { get; set; }
    public int TrueHeading { get; set; }
    public int Timestamp { get; set; }
    public bool Valid { get; set; }
    public int UserID { get; set; }
    public int MessageID { get; set; }
    public int RepeatIndicator { get; set; }
    public bool PositionAccuracy { get; set; }
    public bool Raim { get; set; }
    public int Spare { get; set; }
    public int SpecialManoeuvreIndicator { get; set; }
    public int CommunicationState { get; set; }
}

public class StandardClassBPositionReport
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Sog { get; set; } // Speed over ground
    public double Cog { get; set; } // Course over ground
    public int TrueHeading { get; set; }
    public int Timestamp { get; set; }
    public bool Valid { get; set; }
    public int UserID { get; set; }
    public int MessageID { get; set; }
    public int RepeatIndicator { get; set; }
    public bool PositionAccuracy { get; set; }
    public bool Raim { get; set; }
    public bool AssignedMode { get; set; }
    public bool ClassBUnit { get; set; }
    public bool ClassBDisplay { get; set; }
    public bool ClassBDsc { get; set; }
    public bool ClassBBand { get; set; }
    public bool ClassBMsg22 { get; set; }
    public int Spare1 { get; set; }
    public int Spare2 { get; set; }
    public int CommunicationState { get; set; }
    public bool CommunicationStateIsItdma { get; set; }
}

public class ShipStaticData
{
    public string Name { get; set; } = string.Empty;
    public string CallSign { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Type { get; set; }
    public int ImoNumber { get; set; }
    public double MaximumStaticDraught { get; set; }
    public Dimension Dimension { get; set; } = new();
    public Eta Eta { get; set; } = new();
    public int FixType { get; set; }
    public int AisVersion { get; set; }
    public bool Dte { get; set; }
    public bool Spare { get; set; }
    public bool Valid { get; set; }
    public int UserID { get; set; }
    public int MessageID { get; set; }
    public int RepeatIndicator { get; set; }
}

public class StaticDataReport
{
    public ReportA ReportA { get; set; } = new();
    public ReportB ReportB { get; set; } = new();
    public bool PartNumber { get; set; }
    public bool Valid { get; set; }
    public int UserID { get; set; }
    public int MessageID { get; set; }
    public int RepeatIndicator { get; set; }
    public int Reserved { get; set; }
}

public class ReportA
{
    public string Name { get; set; } = string.Empty;
    public bool Valid { get; set; }
}

public class ReportB
{
    public string CallSign { get; set; } = string.Empty;
    public Dimension Dimension { get; set; } = new();
    public int ShipType { get; set; }
    public int FixType { get; set; }
    public int VenderIDModel { get; set; }
    public int VenderIDSerial { get; set; }
    public string VendorIDName { get; set; } = string.Empty;
    public int Spare { get; set; }
    public bool Valid { get; set; }
}

public class Dimension
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
}

public class Eta
{
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
}

public class AisMetaData
{
    public long MMSI { get; set; }
    public string MMSI_String { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string time_utc { get; set; } = string.Empty;
}