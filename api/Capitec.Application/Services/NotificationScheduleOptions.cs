namespace Capitec.Application.Services;

public sealed class NotificationScheduleOptions
{
    public bool Enabled { get; set; }
    public int IntervalMinutes { get; set; } = 60;
    public int WindowDays { get; set; } = 7;
}