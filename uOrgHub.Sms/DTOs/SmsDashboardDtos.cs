namespace uOrgHub.Sms.DTOs;

public class SmsTotalsDto
{
    /// <summary>Messages handed to a gateway (everything except still-queued).</summary>
    public int Sent { get; set; }
    /// <summary>Accepted by the gateway (submitted or delivered).</summary>
    public int Successful { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
    public int Undetermined { get; set; }
    public int Pending { get; set; }
    public int Segments { get; set; }
    public decimal Cost { get; set; }
}

public class SmsDashboardDto
{
    public SmsGatewayResponseDto? ActiveGateway { get; set; }
    public SmsTotalsDto AllTime { get; set; } = new();
    public SmsTotalsDto Today { get; set; } = new();
    public SmsTotalsDto Last30Days { get; set; } = new();
    public int GatewayCount { get; set; }
}

public class SmsAnalyticsPointDto
{
    /// <summary>Start of the bucket, as a local calendar date (yyyy-MM-dd).</summary>
    public string Period { get; set; } = string.Empty;
    public int Sent { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public int Pending { get; set; }
    public int Segments { get; set; }
    public decimal Cost { get; set; }
}

public class SmsAnalyticsDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Granularity { get; set; } = "Day";
    public SmsTotalsDto Totals { get; set; } = new();
    public List<SmsAnalyticsPointDto> Points { get; set; } = new();
    public string Currency { get; set; } = "BDT";
}
