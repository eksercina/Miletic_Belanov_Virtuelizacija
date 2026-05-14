using System.ServiceModel;
using Common;

namespace Common
{
    [ServiceContract]
    public interface IChargingService
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        string StartSession(string vehicleId);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(ChargingSample sample);

        [OperationContract]
        void EndSession(string sessionId);
        void SimulateConnectionBreak();
    }
}