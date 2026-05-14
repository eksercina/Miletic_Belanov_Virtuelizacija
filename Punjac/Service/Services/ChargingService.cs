using System;
using System.Configuration;
using System.ServiceModel;
using Common;

namespace Service.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChargingService : IChargingService, IDisposable
    {
        private ChargingFileManager _fileManager;
        private ChargingSample _previousSample;
        private string _currentVehicleId;
        private string _currentSessionId;


        public event EventHandler OnTransferStarted;
        public event EventHandler<SampleReceivedEventArgs> OnSampleReceived;
        public event EventHandler OnTransferCompleted;
        public event EventHandler<WarningEventArgs> OnWarningRaised;

        public string StartSession(string vehicleId)
        {
            if (string.IsNullOrEmpty(vehicleId))
                throw new FaultException<ValidationFault>(new ValidationFault("VehicleId is mandatory!"));

            _currentVehicleId = vehicleId;
            _currentSessionId = Guid.NewGuid().ToString();
            _fileManager = new ChargingFileManager();
            _fileManager.StartNewSession(vehicleId);
            _previousSample = null;

            OnTransferStarted += (s, e) => Console.WriteLine($"[EVENT] Transfer Started for {vehicleId}");
            OnSampleReceived += (s, e) => Console.WriteLine($"[EVENT] Sample Received - Row: {e.Sample.RowIndex}");
            OnTransferCompleted += (s, e) => Console.WriteLine($"[EVENT] Transfer Completed for {vehicleId}");
            OnWarningRaised += (s, e) => Console.WriteLine($"[WARNING] {e.Type} | {e.Message}");

            Console.WriteLine($"[START] Session started for {vehicleId} | Session: {_currentSessionId}");

            OnTransferStarted?.Invoke(this, EventArgs.Empty);

            return _currentSessionId;
        }

        public void PushSample(ChargingSample sample)
        {
            if (sample == null)
                throw new FaultException<ValidationFault>(new ValidationFault("Sample is null!"));

            ValidateSample(sample);
            sample.VehicleId = _currentVehicleId;

            _fileManager.SaveSample(sample);
            OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, _currentSessionId));

            CheckForSpikes(sample);

            Console.WriteLine($"[OK] Row {sample.RowIndex} | V:{sample.Voltage.Avg:F1}V | I:{sample.Current.Avg:F2}A");

            _previousSample = sample;
        }

        public void EndSession(string sessionId)
        {
            Console.WriteLine($"[END] Session ended: {sessionId}");
            OnTransferCompleted?.Invoke(this, EventArgs.Empty);
            _fileManager?.Dispose();
        }

        private void CheckForSpikes(ChargingSample current)
        {
            if (_previousSample == null) return;

            double deltaV = Math.Abs(current.Voltage.Avg - _previousSample.Voltage.Avg);
            double deltaI = Math.Abs(current.Current.Avg - _previousSample.Current.Avg);

            double voltageEdge = GetDoubleSetting("VoltageSpike");
            double currentEdge = GetDoubleSetting("CurrentSpike");
            double pfEdge = GetDoubleSetting("PowerFactor");

            if (deltaV > voltageEdge)
            {
                OnWarningRaised?.Invoke(this, new WarningEventArgs(
                    $"Voltage Spike! ΔV = {deltaV:F2} V",
                    current.VehicleId, current.RowIndex, "VoltageSpike"));
            }

            if (deltaI > currentEdge)
            {
                OnWarningRaised?.Invoke(this, new WarningEventArgs(
                    $"Current Spike! ΔI = {deltaI:F2} A",
                    current.VehicleId, current.RowIndex, "CurrentSpike"));
            }

            if (current.ApparentPower.Avg > 0)
            {
                double powerFactor = current.RealPower.Avg / current.ApparentPower.Avg;

                if (powerFactor < pfEdge)
                {
                    OnWarningRaised?.Invoke(this, new WarningEventArgs(
                        $"Low Power Factor! PF = {powerFactor:F3}",
                        current.VehicleId, current.RowIndex, "PowerFactorWarning"));
                }
            }
        }

        private double GetDoubleSetting(string key)
        {
            string str = ConfigurationManager.AppSettings[key];
            if (double.TryParse(str, out double result))
            {
                return result;
            }
            return 0;
        }

        private void ValidateSample(ChargingSample s)
        {
            if (s.Timestamp == default)
                throw new FaultException<ValidationFault>(new ValidationFault("Invalid Timestamp!"));

            if (s.Voltage.Avg <= 0 || s.Current.Avg <= 0 || s.Frequency.Avg <= 0)
                throw new FaultException<ValidationFault>(new ValidationFault("Voltage, current and frequency values must be greater than 0!"));
        }

        public void Dispose()
        {
            _fileManager?.Dispose();
        }

        public void SimulateConnectionBreak()
        {
            throw new NotImplementedException();
        }
    }
}