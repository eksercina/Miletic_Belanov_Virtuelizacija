using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Measurement
    {
        [DataMember]
        public double Min { get; set; }

        [DataMember]
        public double Avg { get; set; }

        [DataMember]
        public double Max { get; set; }

        public Measurement() { }

        public Measurement(double min, double avg, double max)
        {
            Min = min;
            Avg = avg;
            Max = max;
        }

        public double Range => Max - Min;

        public override string ToString()
        {
            return $"Min: {Min:F3}, Avg: {Avg:F3}, Max: {Max:F3}";
        }
    }
}