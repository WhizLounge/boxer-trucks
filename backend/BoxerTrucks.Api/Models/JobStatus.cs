namespace BoxerTrucks.Api.Models;

public enum JobStatus
{
    PendingApproval = 0,
    Approved = 1,
    Assigned = 2,
    InProgress = 3,
    Completed = 4,
    Paid = 5,
    Cancelled = 6
}
