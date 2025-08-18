namespace CanCakar.AKSReader.Enums
{
    /// <summary>
    /// Device protocol types
    /// </summary>
    public enum AksDeviceProtocol
    {
        /// <summary>
        /// Client mode: you need to query for cards with sending commands to device
        /// </summary>
        Client,
        /// <summary>
        /// Server mode: the device will automatically write to input buffer when a card is read
        /// <para>Note: Some tcp devices require online work type (<seealso cref="AksDeviceWorkType.Online"/>) for this behaviour</para>
        /// </summary>
        Server
    }
}
