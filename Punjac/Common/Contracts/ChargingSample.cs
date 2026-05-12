using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ChargingSample
    {
        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public Measurement Voltage { get; set; } = new Measurement();

        [DataMember]
        public Measurement Current { get; set; } = new Measurement();

        [DataMember]
        public Measurement RealPower { get; set; } = new Measurement();

        [DataMember]
        public Measurement ReactivePower { get; set; } = new Measurement();

        [DataMember]
        public Measurement ApparentPower { get; set; } = new Measurement();

        [DataMember]
        public Measurement Frequency { get; set; } = new Measurement();

        [DataMember]
        public int RowIndex { get; set; }

        [DataMember]
        public string VehicleId { get; set; }

        public ChargingSample() { }

        // Helper konstruktor za parsiranje CSVa
        public ChargingSample(DateTime timestamp, int rowIndex, string vehicleId)
        {
            Timestamp = timestamp;
            RowIndex = rowIndex;
            VehicleId = vehicleId;
        }
    }
}