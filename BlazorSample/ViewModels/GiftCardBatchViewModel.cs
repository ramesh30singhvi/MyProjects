using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class GiftCardBatchViewModel : IGiftCardBatchViewModel
    {
        private IProductService _productService;

        public GiftCardBatchViewModel(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<AddBusinessGiftCardBatchResponse> AddBusinessGiftCardBatch(BusinessGiftCardBatchRequestModel model)
        {
            return await _productService.AddBusinessGiftCardBatch(model);
        }

        public async Task<BusinessGiftCardBatchListResponse> GetBusinessGiftCardBatchList(int businessId)
        {
            return await _productService.GetBusinessGiftCardBatchList(businessId);
        }

        public MemoryStream CreateBatchNumbersExcel(List<BusinessGiftCardBatchNumberModel> businessGiftCardBatchNumbers)
        {
            //Create an instance of ExcelEngine.
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;

                //Create a workbook.
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                //Initialize DataTable and data get from SampleDataTable method.
                DataTable table = BindDataTable(businessGiftCardBatchNumbers);

                //Export data from DataTable to Excel worksheet.
                worksheet.ImportDataTable(table, true, 1, 1);

                worksheet.UsedRange.AutofitColumns();

                //Save the document as a stream and return the stream.
                using (MemoryStream stream = new MemoryStream())
                {
                    //Save the created Excel document to MemoryStream.
                    workbook.SaveAs(stream);
                    return stream;
                }
            }
        }

        private DataTable BindDataTable(List<BusinessGiftCardBatchNumberModel> businessGiftCardBatchNumbers)
        {
            DataTable batchNumbers = new DataTable();

            batchNumbers.Columns.Add("NUMBER");
            batchNumbers.Columns.Add("PIN", typeof(int));
            batchNumbers.Columns.Add("EXPIRATION", typeof(DateTime));
            batchNumbers.Columns.Add("STATUS");

            foreach (var number in businessGiftCardBatchNumbers)
            {
                batchNumbers.Rows.Add(number.CardNumber, number.CardPin, number.ExpirationDate, number.NumberStatusText);
            }

            return batchNumbers;
        }
    }
}
