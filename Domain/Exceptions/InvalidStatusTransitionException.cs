namespace MiNegocioCR.Api.Domain.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public const string ErrorCode = "INVALID_STATUS_TRANSITION";

    public string CurrentStatus { get; }
    public string RequestedStatus { get; }

    public InvalidStatusTransitionException(string currentStatus, string requestedStatus)
        : base($"Invalid status transition from {currentStatus} to {requestedStatus}. " +
               "Allowed: Pending→InProcess/Cancelled, InProcess→Processed/Cancelled, Processed→Delivered/Cancelled.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }
}