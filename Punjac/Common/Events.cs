using System;

namespace Common
{
    public class SampleReceivedEventArgs : EventArgs
    {
        public ChargingSample Sample { get; }
        public string SessionId { get; }
        public DateTime Timestamp { get; }

        public SampleReceivedEventArgs(ChargingSample sample, string sessionId)
        {
            Sample = sample;
            SessionId = sessionId;
            Timestamp = DateTime.Now;
        }
    }

    public class WarningEventArgs : EventArgs
    {
        public string Message { get; }
        public string VehicleId { get; }
        public int RowIndex { get; }
        public string Type { get; }

        public WarningEventArgs(string message, string vehicleId, int rowIndex, string type)
        {
            Message = message;
            VehicleId = vehicleId;
            RowIndex = rowIndex;
            Type = type;
        }
    }
}