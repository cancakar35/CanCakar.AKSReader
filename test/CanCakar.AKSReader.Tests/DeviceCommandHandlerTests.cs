using System.Text;
using Xunit;

namespace CanCakar.AKSReader.Tests
{
    public class DeviceCommandHandlerTests
    {
        private readonly DeviceCommandHandler commandHandler;

        public DeviceCommandHandlerTests()
        {
            commandHandler = new DeviceCommandHandler();
        }

        [Fact]
        public void XorBytes_ReturnZero_WhenInputIsEmpty()
        {
            // Arrange
            byte[] input = [];

            // Act
            var result = commandHandler.XORBytes(input);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void XorBytes_ReturnXorResult_WhenInputIsNotEmpty()
        {
            // Arrange
            byte[] input = [2, 255, 150, 10, 9, 8];
            // Act
            var result = commandHandler.XORBytes(input);
            // Assert
            Assert.Equal(96, result);
        }

        [Fact]
        public void CreateBcc_ReturnsCorrectHexString_WhenInputIsNotEmpty()
        {
            // Arrange
            byte[] input = [2, 255, 150, 4, 10];
            string expectedBcc = "65";
            // Act
            var result = commandHandler.CreateBcc(input);
            // Assert
            Assert.Equal(expectedBcc, result);
        }

        [Fact]
        public void CreateBcc_ShouldAlwaysReturnTwoChar()
        {
            // Arrange
            byte[] input = [1, 2];
            string expectedBcc = "03";
            // Act
            var result = commandHandler.CreateBcc(input);
            // Assert
            Assert.Equal(expectedBcc, result);
        }

        [Fact]
        public void CreateCommand_ShouldReturnCorrectCommandStructure()
        {
            // Arrange
            byte readerId = 150;
            byte[] dataPart = [17, ..Encoding.UTF8.GetBytes("+12300019082025Pass")];
            byte[] expectedCommand = [2, 255, 150, (byte)(dataPart.Length + 3), .. dataPart, 55, 50, 3];
            // Act
            var result = commandHandler.CreateCommand(readerId, dataPart);
            // Assert
            Assert.Equal(expectedCommand, result);
        }

        [Fact]
        public void GetDataPart_ShouldReturnNull_WhenBufferDoesNotContainStxByte()
        {
            // Arrange
            byte[] buffer = [255, 150, 10, 3];
            // Act
            var result = commandHandler.GetDataPart(buffer);
            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void GetDataPart_ShouldReturnNull_WhenBufferDoesNotContainEtxByte()
        {
            // Arrange
            byte[] buffer = [2, 255, 150, 10];
            // Act
            var result = commandHandler.GetDataPart(buffer);
            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void GetDataPart_ShouldReturnDataPart_WhenBufferIsValid()
        {
            // Arrange
            byte[] buffer = [2, 255, 150, 4, 111, 48, 48, 3];
            byte[] expected = [111];
            // Act
            var result = commandHandler.GetDataPart(buffer);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }
    }
}
