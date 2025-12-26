using System;

namespace MicroJpeg
{
    public class MicroJpegException : Exception
    {
        public MicroJpegException(string message) : base(message) { }
        public MicroJpegException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MicroJpegApiException : MicroJpegException
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public string ErrorMessage { get; }

        public MicroJpegApiException(int statusCode, string errorCode, string errorMessage) 
            : base($"{errorCode}: {errorMessage} (Status: {statusCode})")
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public bool IsLimitReached => StatusCode == 429 || ErrorCode == "limit_reached";
        public bool IsUnauthorized => StatusCode == 401 || ErrorCode == "unauthorized";
        public bool IsFileTooLarge => StatusCode == 413 || ErrorCode == "file_too_large";
        public bool IsFeatureRestricted => StatusCode == 403 && ErrorCode == "feature_restricted";
    }
}
