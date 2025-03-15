namespace ICSharpCode.SharpZipLib.Zip.Compression
{
	/// <summary>
	/// Contiene información sobre un bloque de compresión DEFLATE.
	/// </summary>
	public class DeflateBlockInfo
	{
		/// <summary>
		/// Tipo de bloque (STORED_BLOCK, STATIC_TREES, DYN_TREES)
		/// </summary>
		public int BlockType { get; set; }
        
		/// <summary>
		/// Datos originales sin comprimir
		/// </summary>
		public byte[] OriginalData { get; set; }
        
		/// <summary>
		/// Datos comprimidos generados
		/// </summary>
		public byte[] CompressedData { get; set; }
        
		/// <summary>
		/// Indica si este es el último bloque de la secuencia
		/// </summary>
		public bool IsLastBlock { get; set; }
        
		/// <summary>
		/// Tamaño de los datos originales
		/// </summary>
		public int OriginalSize => OriginalData?.Length ?? 0;
        
		/// <summary>
		/// Tamaño de los datos comprimidos
		/// </summary>
		public int CompressedSize => CompressedData?.Length ?? 0;
        
		/// <summary>
		/// Ratio de compresión (original/comprimido)
		/// </summary>
		public float CompressionRatio => OriginalSize == 0 ? 0 : (float)OriginalSize / CompressedSize;
	}
}
