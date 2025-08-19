using CanCakar.AKSReader.Communication;
using Moq;
using Xunit;

namespace CanCakar.AKSReader.Tests
{
    public class ReaderTests
    {
        private readonly Mock<IAksDeviceCommunication> mockDevice;
        private readonly IDeviceCommandHandler commandHandler;
        private readonly Reader reader;

        public ReaderTests()
        {
            mockDevice = new Mock<IAksDeviceCommunication>();
            commandHandler = new DeviceCommandHandler();
            reader = new Reader(mockDevice.Object, commandHandler);
        }

        [Fact]
        public void IsConnected_ReturnsDeviceConnectionStatus()
        {
            mockDevice.Setup(x => x.IsConnected).Returns(true);

            Assert.True(reader.IsConnected);

            mockDevice.Setup(x => x.IsConnected).Returns(false);
            Assert.False(reader.IsConnected);
        }

        [Fact]
        public void Connect_CallsDeviceConnect()
        {
            reader.Connect();
            mockDevice.Verify(x => x.Connect(), Times.Once);
        }

        [Fact]
        public async Task ConnectAsync_CallsDeviceConnectAsync()
        {
            await reader.ConnectAsync();
            mockDevice.Verify(x => x.ConnectAsync(), Times.Once);
        }

        [Fact]
        public async Task SendRawCommandAsync_WithValidCommand_ReturnsStringResponse()
        {
            // Arrange
            byte readerId = 150;
            byte commandId = 10;
            byte[] commandBytes = [2, 255, 150, 4, 10, 54, 53, 3];
            byte[] response = [2, 150, 255, 4, 111, 48, 48, 3];

            mockDevice.Setup(x => x.WriteAsync(It.Is<byte[]>(x=>x.SequenceEqual(commandBytes)), CancellationToken.None))
                .Returns(Task.CompletedTask);

            mockDevice.Setup(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None))
                .Callback<byte[], int, int, CancellationToken>((buffer, offset, count, token) => {
                    Array.Copy(response, 0, buffer, offset, count);
                    response = [.. response.Skip(count)];
                })
                .Returns(Task.CompletedTask);

            // Act
            string? result = await reader.SendRawCommandAsync(readerId, commandId);

            // Assert
            Assert.Equal("o", result);
            mockDevice.Verify(x => x.WriteAsync(commandBytes, default), Times.Once);
            mockDevice.Verify(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None), Times.AtLeastOnce);
        }
    }
}
