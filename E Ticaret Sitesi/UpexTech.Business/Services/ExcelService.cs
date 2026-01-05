using ClosedXML.Excel;
using System.Reflection;

namespace UpexTech.Business.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName, Dictionary<string, string>? columnMappings = null)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Property'leri al
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Başlık satırını oluştur
            for (int i = 0; i < properties.Length; i++)
            {
                var propertyName = properties[i].Name;
                var headerName = columnMappings != null && columnMappings.ContainsKey(propertyName)
                    ? columnMappings[propertyName]
                    : propertyName;

                worksheet.Cell(1, i + 1).Value = headerName;
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Veri satırlarını ekle
            int rowIndex = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item);
                    
                    if (value != null)
                    {
                        // Decimal formatı için özel işlem
                        if (properties[i].PropertyType == typeof(decimal) || 
                            properties[i].PropertyType == typeof(decimal?))
                        {
                            worksheet.Cell(rowIndex, i + 1).Value = Convert.ToDecimal(value);
                            worksheet.Cell(rowIndex, i + 1).Style.NumberFormat.Format = "#,##0.00";
                        }
                        else if (properties[i].PropertyType == typeof(int) || 
                                properties[i].PropertyType == typeof(int?))
                        {
                            worksheet.Cell(rowIndex, i + 1).Value = Convert.ToInt32(value);
                            worksheet.Cell(rowIndex, i + 1).Style.NumberFormat.Format = "#,##0";
                        }
                        else
                        {
                            worksheet.Cell(rowIndex, i + 1).Value = value.ToString();
                        }
                    }
                }
                rowIndex++;
            }

            // Kolonları otomatik genişlet
            worksheet.Columns().AdjustToContents();

            // Excel'i byte array'e dönüştür
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
