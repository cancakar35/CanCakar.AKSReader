// Licensed under the MIT License.
// https://opensource.org/license/MIT


using CanCakar.AKSReader;
using CanCakar.AKSReader.Enums;
using System.Net;
using TCPDeviceSample;


// Using in memory for sample. Use a database for real scenarios.
List<Attendance> attandenceRecords = [];
Dictionary<string, string> myUsers = new()
{
    {"005D9DEA","CAN CAKAR" }
};

const int readerId = 150;
const string deviceCode = "I-1";

using var reader = new Reader(IPAddress.Parse("192.168.1.139"), 1001);

await reader.ConnectAsync();

// set device work type as online (you can use offline or on_off as well but you need to send persons to device)
string? setAsOnlineResp = await reader.SendRawCommandAsync(readerId, AksCommand.SetDeviceWorkType, $"{(byte)AksDeviceWorkType.Online}");
if (setAsOnlineResp != "o")
{
    Console.WriteLine("Failed to set the device work type");
}
// set device protocol as client
string? setAsClientResp = await reader.SendRawCommandAsync(readerId, AksCommand.WriteDeviceProtocol, $"{(byte)AksDeviceProtocol.Client}");
if (setAsClientResp != "o")
{
    Console.WriteLine("Failed to set the device protocol");
}

Console.WriteLine("Listening the device...");

while (reader.IsConnected)
{
    try
    {
        string? readCardResp = await reader.SendRawCommandAsync(readerId, AksCommand.ReadCard);
        if (readCardResp is null || readCardResp.Length == 0) continue;

        // discard offline logs to avoid deadlocks
        if (readCardResp[0] == 'd')
        {
            // if you are using offline or on_off mode, persist attendance logs before discarding
            await reader.SendRawCommandAsync(readerId, AksCommand.DeviceLogHandled);
            continue;
        }

        if (readCardResp[0] == 'b')
        {
            string cardId = readCardResp[3..];
            if (myUsers.TryGetValue(cardId, out string? personName))
            {
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm} {cardId} {personName} Pass");
                string? openDoorResp = await reader.SendRawCommandAsync(readerId, AksCommand.AccessOperation, $"+{DateTime.Now:HHmmssddMMyyyy}{personName}");
                if (openDoorResp != "o")
                {
                    Console.WriteLine("Failed to open door");
                    continue;
                }
                attandenceRecords.Add(new Attendance { CardId = cardId, Date = DateTime.Now, DeviceCode = deviceCode });
            }
            else
            {
                Console.WriteLine($"{cardId} UNKNOWN CARD");
                string? sendAccessErrorResp = await reader.SendRawCommandAsync(readerId, AksCommand.AccessOperation, $"-{DateTime.Now:HHmmssddMMyyyy}UNKNOWN CARD");
            }
        }

        await Task.Delay(100);
    }
    catch (TimeoutException)
    {
    }
    catch (Exception ex)
    {
        // use proper logging in real scenarios
        Console.WriteLine($"An error occured: {ex.Message}");
    }
}

Console.WriteLine("Connection lost");
