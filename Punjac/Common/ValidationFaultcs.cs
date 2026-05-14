using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Details { get; set; }

        public ValidationFault(string message, string details = null)
        {
            Message = message;
            Details = details ?? string.Empty;
        }
    }
}