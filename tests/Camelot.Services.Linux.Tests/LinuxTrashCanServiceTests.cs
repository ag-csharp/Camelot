using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Operations;
using Camelot.Services.Environment.Interfaces;
using Camelot.Services.Linux.Interfaces.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Camelot.Services.Linux.Tests
{
    public class LinuxTrashCanServiceTests
    {
        private const string FilePath = "/home/file.txt";
        private const string HomePath = "/home/camelot";
        private const string FileName = "file.txt";
        private const string MetaData = "metadata";

        private readonly AutoMocker _autoMocker;

        public LinuxTrashCanServiceTests()
        {
            _autoMocker = new AutoMocker();
        }

        [Theory]
        [InlineData("/", "/home/camelot/.local/share/Trash/info/file.txt.trashinfo", "/home/camelot/.local/share/Trash/files/")]
        [InlineData("/test", "/test/.Trash-42/info/file.txt.trashinfo", "/test/.Trash-42/files/")]
        public async Task TestMoveToTrash(string volume, string metadataPath, string newFilePath)
        {
            var now = DateTime.UtcNow;
            _autoMocker
                .Setup<IDriveService, DriveModel>(m => m.GetFileDrive(It.IsAny<string>()))
                .Returns(new DriveModel {RootDirectory = volume});
            var isCallbackCalled = false;
            _autoMocker
                .Setup<IOperationsService>(m => m.MoveAsync(It.IsAny<IReadOnlyDictionary<string, string>>()))
                .Callback<IReadOnlyDictionary<string, string>>(d => 
                    isCallbackCalled = d.ContainsKey(FilePath) && d[FilePath] == newFilePath);
            _autoMocker
                .Setup<IPathService, string>(m => m.GetFileName(FilePath))
                .Returns(FileName);
            _autoMocker
                .Setup<IPathService, string>(m => m.Combine(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((a, b) => $"{a}/{b}");
            _autoMocker
                .Setup<IFileService, bool>(m => m.CheckIfExists(FilePath))
                .Returns(true);
            _autoMocker
                .Setup<IFileService>(m => m.WriteTextAsync(metadataPath, MetaData))
                .Verifiable();
            _autoMocker
                .Setup<IEnvironmentService, string>(m => m.GetEnvironmentVariable("UID"))
                .Returns("42");
            _autoMocker
                .Setup<IEnvironmentService, string>(m => m.GetEnvironmentVariable("HOME"))
                .Returns(HomePath);
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(It.IsAny<string>()))
                .Returns(true);
            _autoMocker
                .Setup<IDateTimeProvider, DateTime>(m => m.Now)
                .Returns(now);
            var builderMock = new Mock<ILinuxRemovedFileMetadataBuilder>();
            builderMock
                .Setup(m => m.WithFilePath(FilePath))
                .Returns(builderMock.Object)
                .Verifiable();
            builderMock
                .Setup(m => m.WithRemovingDateTime(now))
                .Returns(builderMock.Object)
                .Verifiable();
            builderMock
                .Setup(m => m.Build())
                .Returns(MetaData);
            _autoMocker
                .Setup<ILinuxRemovedFileMetadataBuilderFactory, ILinuxRemovedFileMetadataBuilder>(m => m.Create())
                .Returns(builderMock.Object);

            var linuxTrashCanService = _autoMocker.CreateInstance<LinuxTrashCanService>();
            await linuxTrashCanService.MoveToTrashAsync(new[] {FilePath}, CancellationToken.None);

            Assert.True(isCallbackCalled);
            _autoMocker
                .Verify<IFileService>(m => m.WriteTextAsync(metadataPath, MetaData), Times.Once);
            builderMock
                .Verify(m => m.WithFilePath(FilePath), Times.Once);
            builderMock
                .Verify(m => m.WithRemovingDateTime(now), Times.Once);
        }
    }
}