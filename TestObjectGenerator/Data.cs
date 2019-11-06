using System;
using System.Runtime.InteropServices;

namespace TestObjectGenerator
{
    public static class Data
    {
        public static string ReadText(String columnName)
        {
            string returnValue = null;
            int rowCount = 0;
            int columnCount = 0;

            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            var xlWorkBook = xlApp.Workbooks.Open(@"C:\Users\akusai01\source\repos\TestObjectGenerator\TestData.csv", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            var xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.Item[1];

            var range = xlWorkSheet.UsedRange;
            rowCount = range.Rows.Count;
            columnCount = range.Columns.Count;


            for (int c = 1; c <= columnCount; c++)
            {
                if (xlWorkSheet.Cells[1, c].Value2 != null)
                {
                    if (xlWorkSheet.Cells[1, c].Value2.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        int randomRowIndex = new Random().Next(2, rowCount);
                        returnValue = xlWorkSheet.Cells[randomRowIndex, c].Value2.ToString();
                    }
                }
            }
            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
            return returnValue;

        }
    }
}
