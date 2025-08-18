namespace CanCakar.AKSReader.Enums
{
    /// <summary>
    /// Common reader commands. (Some commands may not be available on usb devices like ACS-503-USB)
    /// </summary>
    public enum AksCommand
    {
        /// <summary>
        /// If device response is 'o' it means device is ok
        /// </summary>
        CheckDeviceStatus = 10,
        /// <summary>
        /// Device Response after this command can be:
        /// <para>a00 if no card is present</para>
        /// <para>
        /// b00 with card id (hex) if card is present
        /// the two 0's indicate the status of the input ports, and the next eight characters indicate the card ID. (e.g. b00abc12345)
        /// </para>
        /// <para>
        /// d00 with card id, date and time info. If your device protocol is offline or on_off you may see this response.
        /// This response indicates the device has access logs you should handle.
        /// Example Response: d0009320505150825005DA58C0101000001 <br/>
        /// [d00 + 093205 (time HH:mm:ss) + 05150825 (date {dayOfWeekNumber}dd.MM.yy) + 005DA58C (8 char hex card id) + ... rest is device person info]
        /// <br/>you should send the <seealso cref="DeviceLogHandled"/> (111) command after this response.
        /// </para>
        /// </summary>
        ReadCard = 11,
        /// <summary>
        /// Set Device Orientation. Param: IN: 1, OUT: 2, IN_OUT: 3
        /// </summary>
        SetDeviceOrientation = 13,
        /// <summary>
        /// Grant or deny access.
        /// <para>
        /// Grant access (open door/turnstile). Param : +HHmmssddMMyyyyYour_Message_Here
        /// (replace datetime format and message part)
        /// </para>
        /// <para>
        /// To Deny access (long beep, write error message): -HHmmssddMMyyyyYour_Message
        /// </para>
        /// </summary>
        AccessOperation = 17,
        /// <summary>
        /// Set device datetime. Param: HHmmssXXddMMyy where XX is the 2 digit day of week number (e.g. 01 for Monday and 07 for Sunday)
        /// </summary>
        SetDeviceDateTime = 21,
        /// <summary>
        /// Get device datetime
        /// Response format: cHHmmssXXddMMyy where XX is 2 digit day of week number and c is the response prefix
        /// </summary>
        GetDeviceDateTime = 22,
        /// <summary>
        /// Set device work type. Param: <seealso cref="AksDeviceWorkType"/> (1 for online, 2 for offline, 3 for on_off)
        /// </summary>
        SetDeviceWorkType = 24,
        /// <summary>
        /// Add a new card to the device. Param: 8 char hex card id + type + expiration date(ddMMyy) + card holder name
        /// <br/>Example: ECAB297901311225CAN CAKAR
        /// <para>Device response: 'o' if the operation is successful</para>
        /// </summary>
        SendCard = 31,
        /// <summary>
        /// Remove all cards from device
        /// </summary>
        ClearAllCards = 32,
        /// <summary>
        /// Delete given card id from device. Param: 8 char hex card id
        /// </summary>
        DeleteCard = 33,
        /// <summary>
        /// Select card for mifare operations
        /// </summary>
        SelectCard = 52,
        /// <summary>
        /// Log in to the sector. The first parameter is the Key Type (A or B). The second parameter (0-F) is the sector number, and the third parameter is the 12 hex character key.
        /// <para>FFFFFFFFFFFF is the default key</para>
        /// <para>You must send SelectCard command before this command</para>
        /// </summary>
        CardLogin = 54,
        /// <summary>
        /// The first parameter (0-3) is the block number. This command must be executed after CardLogin (54) command.
        /// <para>Device response: h if operation failed or 32 byte of hex block data with 'g' prefix</para>
        /// </summary>
        ReadDataHex = 55,
        /// <summary>
        /// The first parameter (0-3) is the block number. This command must be executed after CardLogin (54) command.
        /// <para>Device response: h if operation failed or 8 byte hex block data with 'i' prefix</para>
        /// </summary>
        ReadValue = 56,
        /// <summary>
        /// The first parameter (0-3) is the block number. The next 32 bytes of hex data are the data to be written to the block. This command must be executed after CardLogin (54) command.
        /// <para>Device response: 'o' if the operation is successful, otherwise 'h'</para>
        /// </summary>
        WriteDataHex = 58,
        /// <summary>
        /// The first parameter (0-3) is the block number. The next 8 bytes of hex data are the data to be written to the block. This command must be executed after CardLogin (54) command.
        /// <para>Device response: 'o' if the operation is successful, otherwise 'h'</para>
        /// </summary>
        WriteValue = 59,
        /// <summary>
        /// The first parameter (0-3) is the block number. The next 8 bytes of hex data are the data to be incremented.
        /// <para>Device response: 'o' if the operation is successful, otherwise 'h'</para>
        /// <para>Important: There must be data previously written in the relevant block</para>
        /// </summary>
        Increment = 62,
        /// <summary>
        /// The first parameter (0-3) is the block number. The next 8 bytes of hex data are the data to be decremented.
        /// <para>Device response: 'o' if the operation is successful, otherwise 'h'</para>
        /// <para>Important: There must be data previously written in the relevant block</para>
        /// </summary>
        Decrement = 63,
        /// <summary>
        /// The first parameter (0-3) is the block number. The next 16 bytes of hex data to be written to ascii block
        /// <para>Device response: 'o' if the operation is successful, otherwise 'h'</para>
        /// </summary>
        WriteDataString = 70,
        /// <summary>
        /// The first parameter (0-3) is the block number.
        /// <para>Device response: 16 byte ascii block data with 'g' prefix if the operation is successful, otherwise 'h'</para>
        /// </summary>
        ReadDataString = 71,
        /// <summary>
        /// Set device protocol. Param: <seealso cref="AksDeviceProtocol"/> (1 for server, 0 for client) 
        /// <para>Device response: 'o' if the operation is successful, otherwise 'h'</para>
        /// </summary>
        WriteDeviceProtocol = 101,
        /// <summary>
        /// Send this command after you recive d.... response from device.
        /// </summary>
        DeviceLogHandled = 111,
        /// <summary>
        /// Get number of cards stored on device
        /// </summary>
        CardCount = 248,
        /// <summary>
        /// Get access log count stored on device
        /// </summary>
        LogCount = 249,
        /// <summary>
        /// Clears all access logs on device. Param: 'DEL'.
        /// </summary>
        ClearAllAccessLogs = 250,
    }
}
