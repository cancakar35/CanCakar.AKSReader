using CanCakar.AKSReader.Enums;
using System;
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
            byte[] input = Array.Empty<byte>();

            // Act
            var result = commandHandler.XORBytes(input);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void XorBytes_ReturnXorResult_WhenInputIsNotEmpty()
        {
            // Arrange
            byte[] input = new byte[] { 2, 255, 150, 10, 9, 8 };
            // Act
            var result = commandHandler.XORBytes(input);
            // Assert
            Assert.Equal(96, result);
        }

        [Fact]
        public void CreateBcc_ReturnsCorrectHexString_WhenInputIsNotEmpty()
        {
            // Arrange
            byte[] input = new byte[] { 2, 255, 150, 4, 10 };
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
            byte[] input = new byte[] { 1, 2 };
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
            byte commandId = (byte)AksCommand.AccessOperation;
            byte[] paramBytes = Encoding.UTF8.GetBytes("+12300019082025Pass");
            byte[] dataPart = new byte[paramBytes.Length + 1];
            dataPart[0] = commandId;
            paramBytes.CopyTo(dataPart, 1);
            byte[] expectedCommand = new byte[8 + paramBytes.Length];
            expectedCommand[0] = 2;
            expectedCommand[1] = 255;
            expectedCommand[2] = 150;
            expectedCommand[3] = (byte)(paramBytes.Length + 4);
            dataPart.CopyTo(expectedCommand, 4);
            expectedCommand[expectedCommand.Length - 3] = 55;
            expectedCommand[expectedCommand.Length - 2] = 50;
            expectedCommand[expectedCommand.Length - 1] = 3;
            // Act
            var result = commandHandler.CreateCommand(readerId, dataPart);
            // Assert
            Assert.Equal(expectedCommand, result);
        }

        [Fact]
        public void GetDataPart_ShouldReturnNull_WhenBufferDoesNotContainStxByte()
        {
            // Arrange
            byte[] buffer = new byte[] { 255, 150, 10, 3 };
            // Act
            var result = commandHandler.GetDataPart(buffer);
            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void GetDataPart_ShouldReturnNull_WhenBufferDoesNotContainEtxByte()
        {
            // Arrange
            byte[] buffer = new byte[] { 2, 255, 150, 10 };
            // Act
            var result = commandHandler.GetDataPart(buffer);
            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void GetDataPart_ShouldReturnDataPart_WhenBufferIsValid()
        {
            // Arrange
            byte[] buffer = new byte[] { 2, 255, 150, 4, 111, 48, 48, 3 };
            byte[] expected = new byte[] { 111 };
            // Act
            var result = commandHandler.GetDataPart(buffer);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }
    }
}
