using Profisys_Programming_Task.Model;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Profisys_Programming_Task.Service.Export
{
    internal class DocumentPdfExportService
    {
        public void Export(Documents document, List<DocumentItems> items, string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Content().Column(col =>
                    {
                        // Nagłówek
                        col.Item().Text($"Document #{document.Id}")
                            .FontSize(20).Bold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Cell().Text("Type:").Bold();
                            table.Cell().Text(document.Type);

                            table.Cell().Text("First Name:").Bold();
                            table.Cell().Text(document.FirstName);

                            table.Cell().Text("Last Name:").Bold();
                            table.Cell().Text(document.LastName);

                            table.Cell().Text("City:").Bold();
                            table.Cell().Text(document.City);

                            table.Cell().Text("Date:").Bold();
                            table.Cell().Text(document.Date.ToString("dd.MM.yyyy"));
                        });

                        col.Item().PaddingTop(24).Text("Items").FontSize(16).Bold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn(); 
                            });

                            table.Cell().Background("#EEEEEE").Padding(4).Text("Product").Bold();
                            table.Cell().Background("#EEEEEE").Padding(4).Text("Quantity").Bold();
                            table.Cell().Background("#EEEEEE").Padding(4).Text("Price").Bold();
                            table.Cell().Background("#EEEEEE").Padding(4).Text("Tax Rate").Bold();
                            table.Cell().Background("#EEEEEE").Padding(4).Text("Total").Bold();

                            foreach (var item in items)
                            {
                                decimal total = (decimal)(item.Quantity * item.Price * (1 + item.TaxRate / 100));

                                table.Cell().Padding(4).Text(item.Product);
                                table.Cell().Padding(4).Text(item.Quantity.ToString());
                                table.Cell().Padding(4).Text($"{item.Price:F2}");
                                table.Cell().Padding(4).Text($"{item.TaxRate}%");
                                table.Cell().Padding(4).Text($"{total:F2}");
                            }

                            decimal grandTotal = (decimal)items.Sum(i => i.Quantity * i.Price * (1 + i.TaxRate / 100));
                            table.Cell().ColumnSpan(4).Padding(4).AlignRight().Text("Total:").Bold();
                            table.Cell().Padding(4).Text($"{grandTotal:F2}").Bold();
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
