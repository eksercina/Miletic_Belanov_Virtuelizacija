using System;
using System.IO;
using Common;

namespace Service.Services
{
    public class ChargingFileManager : IDisposable
    {
        private StreamWriter _sessionWriter;
        private StreamWriter _rejectsWriter;
        private bool _disposed = false;
        private string _currentVehicleId;

        public void StartNewSession(string vehicleId)
        {
            _currentVehicleId = vehicleId;

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", vehicleId, DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(basePath);

            string sessionFilePath = Path.Combine(basePath, $"session_{DateTime.Now:HHmmss}.csv");
            string rejectsFilePath = Path.Combine(basePath, "rejects.csv");

            _sessionWriter = new StreamWriter(sessionFilePath, true);
            _rejectsWriter = new StreamWriter(rejectsFilePath, true);

            WriteHeaderIfNewFile();
        }

        private void WriteHeaderIfNewFile()
        {
            if (_sessionWriter != null && new FileInfo(((FileStream)_sessionWriter.BaseStream).Name).Length == 0)
            {
                _sessionWriter.WriteLine("Timestamp,RowIndex,VoltageMin,VoltageAvg,VoltageMax,CurrentMin,CurrentAvg,CurrentMax,RealPowerMin,RealPowerAvg,RealPowerMax,ReactivePowerMin,ReactivePowerAvg,ReactivePowerMax,ApparentPowerMin,ApparentPowerAvg,ApparentPowerMax,FrequencyMin,FrequencyAvg,FrequencyMax");
            }
        }

        public void SaveSample(ChargingSample sample)
        {
            if (_disposed || _sessionWriter == null) return;

            try
            {
                string line = string.Format("{0:yyyy/MM/dd HH:mm:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}",
                    sample.Timestamp, sample.RowIndex,
                    sample.Voltage.Min, sample.Voltage.Avg, sample.Voltage.Max,
                    sample.Current.Min, sample.Current.Avg, sample.Current.Max,
                    sample.RealPower.Min, sample.RealPower.Avg, sample.RealPower.Max,
                    sample.ReactivePower.Min, sample.ReactivePower.Avg, sample.ReactivePower.Max,
                    sample.ApparentPower.Min, sample.ApparentPower.Avg, sample.ApparentPower.Max,
                    sample.Frequency.Min, sample.Frequency.Avg, sample.Frequency.Max);

                _sessionWriter.WriteLine(line);
                _sessionWriter.Flush();
            }
            catch (Exception ex)
            {
                LogReject(sample.RowIndex, ex.Message);
            }
        }

        private void LogReject(int rowIndex, string reason)
        {
            if (_rejectsWriter != null)
            {
                _rejectsWriter.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss},Row {rowIndex},{reason}");
                _rejectsWriter.Flush();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine($"[DISPOSE] Zatvaram resurse za vozilo: {_currentVehicleId ?? "N/A"}");

                    try { _sessionWriter?.Dispose(); } catch { }
                    try { _rejectsWriter?.Dispose(); } catch { }

                    Console.WriteLine("[DISPOSE] StreamWriter-i uspešno zatvoreni.");
                }

                _sessionWriter = null;
                _rejectsWriter = null;
                _disposed = true;
            }
        }

        ~ChargingFileManager()
        {
            Dispose(false);
        }
    }
}