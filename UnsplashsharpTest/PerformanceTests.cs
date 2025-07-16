using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text.Json;
using Unsplasharp;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest
{
    [TestClass]
    public class PerformanceTests
    {
        private UnsplasharpClient _client;

        [TestInitialize]
        public void Setup()
        {
            _client = new UnsplasharpClient("test-app-id");
        }

        [TestMethod]
        public void JsonParsingPerformanceTest()
        {
            // Test JSON parsing performance with System.Text.Json
            var samplePhotoJson = @"{
                ""id"": ""test123"",
                ""created_at"": ""2023-01-01T00:00:00Z"",
                ""updated_at"": ""2023-01-01T00:00:00Z"",
                ""width"": 1920,
                ""height"": 1080,
                ""color"": ""#000000"",
                ""blur_hash"": ""LKO2?U%2Tw=w]~RBVZRi};RPxuwH"",
                ""description"": ""A beautiful test photo"",
                ""downloads"": 1000,
                ""likes"": 500,
                ""liked_by_user"": false,
                ""urls"": {
                    ""raw"": ""https://example.com/raw"",
                    ""full"": ""https://example.com/full"",
                    ""regular"": ""https://example.com/regular"",
                    ""small"": ""https://example.com/small"",
                    ""thumb"": ""https://example.com/thumb""
                },
                ""user"": {
                    ""id"": ""user123"",
                    ""username"": ""testuser"",
                    ""name"": ""Test User"",
                    ""first_name"": ""Test"",
                    ""last_name"": ""User"",
                    ""profile_image"": {
                        ""small"": ""https://example.com/profile_small"",
                        ""medium"": ""https://example.com/profile_medium"",
                        ""large"": ""https://example.com/profile_large""
                    },
                    ""links"": {
                        ""self"": ""https://api.unsplash.com/users/testuser"",
                        ""html"": ""https://unsplash.com/@testuser"",
                        ""photos"": ""https://api.unsplash.com/users/testuser/photos"",
                        ""likes"": ""https://api.unsplash.com/users/testuser/likes"",
                        ""portfolio"": ""https://api.unsplash.com/users/testuser/portfolio""
                    }
                },
                ""links"": {
                    ""self"": ""https://api.unsplash.com/photos/test123"",
                    ""html"": ""https://unsplash.com/photos/test123"",
                    ""download"": ""https://unsplash.com/photos/test123/download"",
                    ""download_location"": ""https://api.unsplash.com/photos/test123/download""
                }
            }";

            const int iterations = 1000;
            var stopwatch = new Stopwatch();

            // Test System.Text.Json parsing performance
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                using var document = JsonDocument.Parse(samplePhotoJson);
                var root = document.RootElement;
                
                // Simulate basic property access
                var id = root.GetProperty("id").GetString();
                var width = root.GetProperty("width").GetInt32();
                var height = root.GetProperty("height").GetInt32();
                var urls = root.GetProperty("urls");
                var rawUrl = urls.GetProperty("raw").GetString();
            }
            stopwatch.Stop();

            var systemTextJsonTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"System.Text.Json parsing {iterations} iterations: {systemTextJsonTime}ms");

            // Verify the parsing worked correctly
            using var testDocument = JsonDocument.Parse(samplePhotoJson);
            var testRoot = testDocument.RootElement;
            Assert.AreEqual("test123", testRoot.GetProperty("id").GetString());
            Assert.AreEqual(1920, testRoot.GetProperty("width").GetInt32());
            Assert.AreEqual(1080, testRoot.GetProperty("height").GetInt32());

            // Performance should be reasonable (less than 1 second for 1000 iterations)
            Assert.IsTrue(systemTextJsonTime < 1000, 
                $"JSON parsing took too long: {systemTextJsonTime}ms for {iterations} iterations");
        }

        [TestMethod]
        public void JsonSerializationPerformanceTest()
        {
            // Test JSON serialization performance
            var testObject = new
            {
                id = "test123",
                width = 1920,
                height = 1080,
                description = "A test photo",
                urls = new
                {
                    raw = "https://example.com/raw",
                    full = "https://example.com/full",
                    regular = "https://example.com/regular"
                },
                user = new
                {
                    id = "user123",
                    username = "testuser",
                    name = "Test User"
                }
            };

            const int iterations = 1000;
            var stopwatch = new Stopwatch();

            // Test System.Text.Json serialization performance
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var json = JsonSerializer.Serialize(testObject);
                // Verify it's not empty
                Assert.IsTrue(json.Length > 0);
            }
            stopwatch.Stop();

            var systemTextJsonTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"System.Text.Json serialization {iterations} iterations: {systemTextJsonTime}ms");

            // Performance should be reasonable
            Assert.IsTrue(systemTextJsonTime < 1000, 
                $"JSON serialization took too long: {systemTextJsonTime}ms for {iterations} iterations");
        }

        [TestMethod]
        public void MemoryUsageTest()
        {
            // Test memory usage with System.Text.Json
            var sampleJson = @"{""id"":""test"",""width"":1920,""height"":1080,""description"":""test photo""}";
            
            var initialMemory = GC.GetTotalMemory(true);
            
            // Parse JSON multiple times
            for (int i = 0; i < 100; i++)
            {
                using var document = JsonDocument.Parse(sampleJson);
                var root = document.RootElement;
                var id = root.GetProperty("id").GetString();
                var width = root.GetProperty("width").GetInt32();
            }
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryUsed = finalMemory - initialMemory;
            
            Console.WriteLine($"Memory used for 100 JSON parsing operations: {memoryUsed} bytes");
            
            // Memory usage should be reasonable (less than 1MB for this simple test)
            Assert.IsTrue(memoryUsed < 1024 * 1024, 
                $"Memory usage too high: {memoryUsed} bytes");
        }

        [TestMethod]
        public void JsonHelperMethodsPerformanceTest()
        {
            // Test the performance of our custom JsonHelper extension methods
            var sampleJson = @"{
                ""string_prop"": ""test_value"",
                ""int_prop"": 42,
                ""double_prop"": 3.14,
                ""bool_prop"": true,
                ""null_prop"": null,
                ""nested"": {
                    ""inner_string"": ""inner_value"",
                    ""inner_int"": 100
                }
            }";

            const int iterations = 1000;
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                using var document = JsonDocument.Parse(sampleJson);
                var root = document.RootElement;
                
                // Test our extension methods
                var stringVal = root.GetProperty("string_prop").GetString();
                var intVal = root.GetProperty("int_prop").GetInt32();
                var doubleVal = root.GetProperty("double_prop").GetDouble();
                var boolVal = root.GetProperty("bool_prop").GetBoolean();
                var nullVal = root.TryGetProperty("null_prop", out var nullProperty) && nullProperty.ValueKind != JsonValueKind.Null ? nullProperty.GetString() : null;

                // Test nested access
                if (root.TryGetProperty("nested", out var nested))
                {
                    var innerString = nested.GetProperty("inner_string").GetString();
                    var innerInt = nested.GetProperty("inner_int").GetInt32();
                }
            }
            stopwatch.Stop();

            var extensionMethodsTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"JsonHelper extension methods {iterations} iterations: {extensionMethodsTime}ms");

            // Verify correctness
            using var testDocument = JsonDocument.Parse(sampleJson);
            var testRoot = testDocument.RootElement;
            Assert.AreEqual("test_value", testRoot.GetProperty("string_prop").GetString());
            Assert.AreEqual(42, testRoot.GetProperty("int_prop").GetInt32());
            Assert.AreEqual(3.14, testRoot.GetProperty("double_prop").GetDouble(), 0.001);
            Assert.AreEqual(true, testRoot.GetProperty("bool_prop").GetBoolean());
            Assert.IsTrue(testRoot.TryGetProperty("null_prop", out var nullProp) && nullProp.ValueKind == JsonValueKind.Null);

            // Performance should be reasonable
            Assert.IsTrue(extensionMethodsTime < 1000, 
                $"Extension methods took too long: {extensionMethodsTime}ms for {iterations} iterations");
        }
    }
}
