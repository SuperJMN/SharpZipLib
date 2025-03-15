using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ICSharpCode.SharpZipLib.Tests.Base
{
	[TestClass]
	public class BlockTests
	{
		private byte[] CreateTestData(int size)
		{
			var data = new byte[size];
			// Rellenamos el array con datos aleatorios, usando una semilla fija para tener resultados deterministas en el test
			new Random(42).NextBytes(data);
			return data;
		}


		[TestMethod]
		public void TestBlockCaptureIntegrity()
		{
			// Arrange
			byte[] testData = CreateTestData(20000); // 20KB de datos de prueba
			var capturedBlocks = new List<DeflateBlockInfo>();
			byte[] compressedData;

			// Act - Fase 1: Comprimir y capturar bloques
			using (var memoryStream = new MemoryStream())
			{
				var deflater = new Deflater(Deflater.DEFAULT_COMPRESSION);
				// Suscribirse al observable para capturar cada bloque creado
				deflater.BlockCreated.Subscribe(block => capturedBlocks.Add(block));

				using (var deflaterStream = new DeflaterOutputStream(memoryStream, deflater))
				{
					deflaterStream.Write(testData, 0, testData.Length);
					deflaterStream.Finish();
				}

				compressedData = memoryStream.ToArray();
			}

			// Act - Fase 2: Descomprimir el stream completo para verificar la integridad global
			byte[] decompressedData;
			using (var memoryStream = new MemoryStream(compressedData))
			using (var inflaterStream = new InflaterInputStream(memoryStream))
			using (var resultStream = new MemoryStream())
			{
				inflaterStream.CopyTo(resultStream);
				decompressedData = resultStream.ToArray();
			}

			// Assert - Verificar que la descompresión global produce los datos originales
			Assert.AreEqual(testData.Length, decompressedData.Length,
				"El tamaño de los datos descomprimidos debe coincidir con el original.");
			CollectionAssert.AreEqual(testData, decompressedData,
				"Los datos descomprimidos globalmente deben ser idénticos a los originales.");

			// Comprobaciones adicionales de seguridad sobre los bloques capturados

			// 1. La suma de OriginalSize de todos los bloques debe coincidir con el tamaño total de testData.
			int totalOriginalSize = capturedBlocks.Sum(b => b.OriginalSize);
			Assert.AreEqual(testData.Length, totalOriginalSize,
				"La suma de OriginalSize en los bloques no coincide con el tamaño total de los datos originales.");

			// 2. Verificar que exactamente un bloque está marcado como último.
			int lastBlockCount = capturedBlocks.Count(b => b.IsLastBlock);
			Assert.AreEqual(1, lastBlockCount, "Debe haber exactamente un bloque marcado como último.");

			// 3. Verificar que el último bloque capturado tenga IsLastBlock = true.
			Assert.IsTrue(capturedBlocks.Last().IsLastBlock,
				"El último bloque en la lista debe estar marcado como último.");

			// 4. Comprobar que cada bloque tenga un BlockType válido.
			foreach (var block in capturedBlocks)
			{
				Assert.IsTrue(
					block.BlockType == DeflaterConstants.STORED_BLOCK ||
					block.BlockType == DeflaterConstants.STATIC_TREES ||
					block.BlockType == DeflaterConstants.DYN_TREES,
					$"Tipo de bloque incorrecto: {block.BlockType}"
				);
			}

			// 5. (Opcional) Verificar que la suma de CompressedSize de los bloques sea coherente con el tamaño total del stream comprimido.
			int totalCompressedSize = capturedBlocks.Sum(b => b.CompressedSize);
			Assert.IsTrue(totalCompressedSize <= compressedData.Length,
				"La suma de CompressedSize de los bloques debe ser menor o igual al tamaño total del stream comprimido.");

			// Mostrar estadísticas para debugging
			Console.WriteLine($"Total bloques capturados: {capturedBlocks.Count}");
			Console.WriteLine($"Suma de OriginalSize: {totalOriginalSize} bytes");
			Console.WriteLine($"Suma de CompressedSize: {totalCompressedSize} bytes");
			Console.WriteLine($"Tamaño del stream comprimido: {compressedData.Length} bytes");
		}
	}
}
