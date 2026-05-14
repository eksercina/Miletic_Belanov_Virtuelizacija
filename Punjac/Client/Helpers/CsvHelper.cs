using System;
using System.Globalization;
using Common;

namespace Client.Helpers
{
    public static class CsvHelper
    {
        public static ChargingSample ParseLine(string line, int rowIndex, string vehicleId)
        {
            try
            {
                var parts = line.Split(',');

                if (parts.Length < 19)
                    throw new Exception($"Small number of columns: {parts.Length}");

                var sample = new ChargingSample
                {
                    Timestamp = DateTime.ParseExact(parts[0].Trim(), "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                    RowIndex = rowIndex,
                    VehicleId = vehicleId
                };

                int idx = 1;

                sample.Voltage = new Measurement(
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture));

                sample.Current = new Measurement(
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture));

                sample.RealPower = new Measurement(
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture));

                sample.ReactivePower = new Measurement(
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture));

                sample.ApparentPower = new Measurement(
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture));

                sample.Frequency = new Measurement(
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(parts[idx++].Trim(), CultureInfo.InvariantCulture));

                return sample;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing row {rowIndex}: {ex.Message}");
            }
        }
    }
}