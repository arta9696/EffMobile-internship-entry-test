using AdvertisingPlatform.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
using Xunit;
using Assert = Xunit.Assert;

namespace AdvertisingPlatform.Tests
{

    public class AdvertisingPlatformTests
    {
        private readonly AdvertisingPlatformService _service;

        public AdvertisingPlatformTests()
        {
            _service = new AdvertisingPlatformService();
        }

        private IFormFile CreateTestFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "test", "test.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }

        [Fact]
        public async Task LoadPlatformsFromFile_ValidFile_ReturnsSuccess()
        {
            // Arrange
            var content = 
@"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd";
            var file = CreateTestFile(content);

            // Act
            var result = await _service.LoadPlatformsFromFile(file);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(4, result.LoadedPlatformsCount);
        }

        [Fact]
        public async Task LoadPlatformsFromFile_EmptyFile_ReturnsError()
        {
            // Arrange
            var file = CreateTestFile("");

            // Act
            var result = await _service.LoadPlatformsFromFile(file);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("empty", result.Message.ToLower());
        }

        [Fact]
        public async Task LoadPlatformsFromFile_NullFile_ReturnsError()
        {
            // Act
            var result = await _service.LoadPlatformsFromFile(null!);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("empty", result.Message.ToLower());
        }

        [Fact]
        public async Task SearchPlatforms_ExactMatch_ReturnsCorrectPlatforms()
        {
            // Arrange
            var content = 
@"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd";
            var file = CreateTestFile(content);
            await _service.LoadPlatformsFromFile(file);

            // Act
            var result = _service.SearchPlatforms("/ru/msk");

            // Assert
            Assert.Equal("/ru/msk", result.Location);
            Assert.Contains("Яндекс.Директ", result.Platforms);
            Assert.Contains("Газета уральских москвичей", result.Platforms);
            Assert.Equal(2, result.Platforms.Count);
        }

        [Fact]
        public async Task SearchPlatforms_NestedLocation_ReturnsParentPlatforms()
        {
            // Arrange
            var content = 
@"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd";
            var file = CreateTestFile(content);
            await _service.LoadPlatformsFromFile(file);

            // Act
            var result = _service.SearchPlatforms("/ru/svrd/revda");

            // Assert
            Assert.Equal("/ru/svrd/revda", result.Location);
            Assert.Contains("Яндекс.Директ", result.Platforms);
            Assert.Contains("Ревдинский рабочий", result.Platforms);
            Assert.Contains("Крутая реклама", result.Platforms);
            Assert.Equal(3, result.Platforms.Count);
        }

        [Fact]
        public async Task SearchPlatforms_RootLocation_ReturnsOnlyRootPlatforms()
        {
            // Arrange
            var content = 
@"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd";
            var file = CreateTestFile(content);
            await _service.LoadPlatformsFromFile(file);

            // Act
            var result = _service.SearchPlatforms("/ru");

            // Assert
            Assert.Equal("/ru", result.Location);
            Assert.Contains("Яндекс.Директ", result.Platforms);
            Assert.Single(result.Platforms);
        }

        [Fact]
        public void SearchPlatforms_EmptyLocation_ReturnsEmptyResult()
        {
            // Act
            var result = _service.SearchPlatforms("");

            // Assert
            Assert.Empty(result.Platforms);
        }

        [Fact]
        public void SearchPlatforms_NullLocation_ReturnsEmptyResult()
        {
            // Act
            var result = _service.SearchPlatforms(null!);

            // Assert
            Assert.Empty(result.Platforms);
        }

        [Fact]
        public async Task SearchPlatforms_LocationWithoutLeadingSlash_AddsSlash()
        {
            // Arrange
            var content = "Яндекс.Директ:/ru";
            var file = CreateTestFile(content);
            await _service.LoadPlatformsFromFile(file);

            // Act
            var result = _service.SearchPlatforms("ru");

            // Assert
            Assert.Equal("/ru", result.Location);
            Assert.Contains("Яндекс.Директ", result.Platforms);
        }

        [Fact]
        public async Task SearchPlatforms_NonExistentLocation_ReturnsEmpty()
        {
            // Arrange
            var content = "Яндекс.Директ:/ru";
            var file = CreateTestFile(content);
            await _service.LoadPlatformsFromFile(file);

            // Act
            var result = _service.SearchPlatforms("/us");

            // Assert
            Assert.Equal("/us", result.Location);
            Assert.Empty(result.Platforms);
        }

        [Fact]
        public async Task SearchPlatforms_ComplexHierarchy_ReturnsCorrectPlatforms()
        {
            // Arrange
            var content = 
@"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd";
            var file = CreateTestFile(content);
            await _service.LoadPlatformsFromFile(file);

            // Act
            var result = _service.SearchPlatforms("/ru/svrd");

            // Assert
            Assert.Equal("/ru/svrd", result.Location);
            Assert.Contains("Яндекс.Директ", result.Platforms);
            Assert.Contains("Крутая реклама", result.Platforms);
            Assert.Equal(2, result.Platforms.Count);
        }

        [Fact]
        public async Task LoadPlatformsFromFile_InvalidFormat_SkipsInvalidLines()
        {
            // Arrange
            var content = 
@"Яндекс.Директ:/ru
InvalidLine
Another:Invalid:Line:/ru/test
Normal Platform:/ru/normal";
            var file = CreateTestFile(content);

            // Act
            var result = await _service.LoadPlatformsFromFile(file);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.LoadedPlatformsCount);
        }

        [Fact]
        public async Task SearchPlatforms_AfterReload_UsesNewData()
        {
            // Arrange
            var content1 = "Platform1:/ru";
            var content2 = "Platform2:/ru";

            var file1 = CreateTestFile(content1);
            var file2 = CreateTestFile(content2);

            // Act
            await _service.LoadPlatformsFromFile(file1);
            var result1 = _service.SearchPlatforms("/ru");

            await _service.LoadPlatformsFromFile(file2);
            var result2 = _service.SearchPlatforms("/ru");

            // Assert
            Assert.Contains("Platform1", result1.Platforms);
            Assert.DoesNotContain("Platform2", result1.Platforms);

            Assert.Contains("Platform2", result2.Platforms);
            Assert.DoesNotContain("Platform1", result2.Platforms);
        }
    }
    
}
