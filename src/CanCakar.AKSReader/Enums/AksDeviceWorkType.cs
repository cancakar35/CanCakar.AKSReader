namespace CanCakar.AKSReader.Enums
{
    /// <summary>
    /// Device work types
    /// </summary>
    public enum AksDeviceWorkType
    {
        /// <summary>
        /// Server decides whether open the door/turnstile or not
        /// </summary>
        Online = 1,
        /// <summary>
        /// The device opens the door/turnstile depending on whether the card exists and has authorization in its memory.
        /// </summary>
        Offline = 2,
        /// <summary>
        /// <para>
        /// If there is a connection with server, server decides whether open the door/turnstile or not
        /// <br /> Note that, the server must constantly query the device
        /// </para>
        /// <para> If there is no connection, the device opens the door/turnstile depending on whether the card exists and has authorization in its memory.</para>
        /// </summary>
        OnOff = 3
    }
}
