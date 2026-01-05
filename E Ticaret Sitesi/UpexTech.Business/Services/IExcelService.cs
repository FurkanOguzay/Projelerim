namespace UpexTech.Business.Services
{
    public interface IExcelService
    {
        /// <summary>
        /// Liste verisini Excel formatında byte array'e dönüştürür
        /// </summary>
        byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName, Dictionary<string, string>? columnMappings = null);
    }
}
