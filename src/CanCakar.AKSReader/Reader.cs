using CanCakar.AKSReader.Communication;
using CanCakar.AKSReader.Enums;
using System.Net;
using System.Text;

namespace CanCakar.AKSReader
{
    /// <summary>
    /// Main reader class
    /// </summary>
    public class Reader : IDisposable
    {
        private readonly IAksDeviceCommunication aksDevice;
        private readonly IDeviceCommandHandler deviceCommandHandler;

        /// <summary>
        /// Connect to serial port aks reader (e.g. ACS-503-USB)
        /// </summary>
        /// <param name="portName">Port name</param>
        /// <param name="baudRate">BaudRate</param>
        public Reader(string portName, int baudRate)
            : this(new AksSerialPortCommunication(portName, baudRate), new DeviceCommandHandler())
        {
        }

        /// <summary>
        /// Connect to aks reader via TCP/IP (e.g. ACS-551)
        /// </summary>
        /// <param name="ipAddress">Device IP</param>
        /// <param name="port">Device Port (1001 is common for aks devices)</param>
        public Reader(IPAddress ipAddress, int port)
            : this(new AksTcpCommunication(ipAddress, port), new DeviceCommandHandler())
        {

        }

        internal Reader(IAksDeviceCommunication aksDeviceComm, IDeviceCommandHandler commandHandler)
        {
            aksDevice = aksDeviceComm ?? throw new ArgumentNullException(nameof(aksDeviceComm));
            deviceCommandHandler = commandHandler;
        }

        /// <summary>
        /// Check device is connected
        /// </summary>
        public bool IsConnected => aksDevice.IsConnected;

        /// <summary>
        /// Open connection
        /// </summary>
        public void Connect()
        {
            aksDevice.Connect();
        }
        /// <summary>
        /// Asynchronously open connection
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            await aksDevice.ConnectAsync();
        }
        /// <summary>
        /// Disconnect from device
        /// </summary>
        public void Disconnect()
        {
            aksDevice.Disconnect();
        }

        /// <summary>
        /// Send raw device command
        /// </summary>
        /// <param name="readerId">Reader id (150,151 etc.)</param>
        /// <param name="commandId">AKS command id</param>
        /// <param name="commandParam">Parameter for given command</param>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns>Device response as string</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<string?> SendRawCommandAsync(byte readerId, byte commandId, string commandParam = "", CancellationToken cancellationToken = default)
        {
            byte[] commandAndParamsBytes = [commandId, .. Encoding.UTF8.GetBytes(commandParam)];
            byte[] deviceCommand = deviceCommandHandler.CreateCommand(readerId, commandAndParamsBytes);

            await aksDevice.WriteAsync(deviceCommand, cancellationToken);

            // The device does not respond after this operation. No need to read the device.
            if (commandId == (byte)AksCommand.DeviceLogHandled)
                return "o";

            try
            {
                byte[] headerPart = new byte[4];
                await aksDevice.ReadAsync(headerPart, 0, headerPart.Length, cancellationToken);
                int dataPartLength = headerPart[3];
                if (dataPartLength < 4)
                {
                    return null;
                }
                byte[] otherPart = new byte[dataPartLength];
                await aksDevice.ReadAsync(otherPart, 0, otherPart.Length, cancellationToken);

                byte[] fullResponse = new byte[headerPart.Length + otherPart.Length];
                headerPart.CopyTo(fullResponse, 0);
                otherPart.CopyTo(fullResponse, headerPart.Length);

                // TODO: bcc validasyonu yapılabilir

                var dataPart = deviceCommandHandler.GetDataPart(fullResponse);

                return dataPart != null ? Encoding.UTF8.GetString(dataPart) : null;
            }
            catch (EndOfStreamException)
            {
                throw new InvalidOperationException("The device did not respond correctly.");
            }
        }
        /// <summary>
        /// Send raw device command
        /// </summary>
        /// <param name="readerId">Reader id (150,151 etc.)</param>
        /// <param name="commandId">AKS command id</param>
        /// <param name="commandParam">Parameter for given command</param>
        /// <returns>Device response as string</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public string? SendRawCommand(byte readerId, byte commandId, string commandParam = "")
        {
            byte[] commandAndParamsBytes = [commandId, .. Encoding.UTF8.GetBytes(commandParam)];
            byte[] deviceCommand = deviceCommandHandler.CreateCommand(readerId, commandAndParamsBytes);

            aksDevice.Write(deviceCommand);

            if (commandId == (byte)AksCommand.DeviceLogHandled)
                return "o";

            try
            {
                byte[] headerPart = new byte[4];
                aksDevice.Read(headerPart, 0, headerPart.Length);
                int dataPartLength = headerPart[3];
                if (dataPartLength < 4)
                {
                    return null;
                }
                byte[] otherPart = new byte[dataPartLength];
                aksDevice.Read(otherPart, 0, otherPart.Length);

                byte[] fullResponse = new byte[headerPart.Length + otherPart.Length];
                headerPart.CopyTo(fullResponse, 0);
                otherPart.CopyTo(fullResponse, headerPart.Length);

                var dataPart = deviceCommandHandler.GetDataPart(fullResponse);

                return dataPart != null ? Encoding.UTF8.GetString(dataPart) : null;
            }
            catch (EndOfStreamException)
            {
                throw new InvalidOperationException("The device did not respond correctly.");
            }
        }
        /// <summary>
        /// Send command to device using <seealso cref="AksCommand"></seealso>
        /// </summary>
        /// <param name="readerId">Reader id (150,151 etc.)</param>
        /// <param name="commandId">AKS command id</param>
        /// <param name="commandParam">Parameter for given command</param>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns>Device response as string</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<string?> SendRawCommandAsync(byte readerId, AksCommand commandId, string commandParam = "", CancellationToken cancellationToken = default) =>
            SendRawCommandAsync(readerId, (byte)commandId, commandParam, cancellationToken);

        /// <summary>
        /// Send command to device using <seealso cref="AksCommand"></seealso>
        /// </summary>
        /// <param name="readerId">Reader id (150,151 etc.)</param>
        /// <param name="commandId">AKS command id</param>
        /// <param name="commandParam">Parameter for given command</param>
        /// <returns>Device response as string</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public string? SendRawCommand(byte readerId, AksCommand commandId, string commandParam = "") =>
            SendRawCommand(readerId, (byte)commandId, commandParam);

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            aksDevice.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
