using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using HospitalInventoryManagement.BL.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing; // Wordprocessing açık kalacak

namespace HospitalInventoryManagement.BL.Service
{
    public class FileProcessingService : IFileProccesingService
    {
        public string ConvertWordToFormattedHtml(string filePath)
        {
            using (var wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                return ConvertBodyToFormattedHtml(body);
            }
        }

        public void UpdateWordDocumentFromHtml(string filePath, string htmlContent)
        {
            using (var wordDoc = WordprocessingDocument.Open(filePath, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                body.RemoveAllChildren();

                var paragraphs = htmlContent.Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var paragraph in paragraphs)
                {
                    var p = new Paragraph(new Run(new Text(paragraph.Trim())));
                    body.AppendChild(p);
                }
                wordDoc.MainDocumentPart.Document.Save();

            }
        }

        private string ConvertBodyToFormattedHtml(Body body)
        {
            var htmlBuilder = new StringBuilder();

            foreach (var element in body.ChildElements)
            {
                if (element is Paragraph paragraphElement)
                {
                    string paragraphStyle = "text-align: left;";
                    var alignmentValue = paragraphElement.ParagraphProperties?.Justification?.Val;

                    if (alignmentValue != null)
                    {
                        if (alignmentValue.Value == JustificationValues.Center)
                            paragraphStyle = "text-align: center;";
                        else if (alignmentValue.Value == JustificationValues.Right)
                            paragraphStyle = "text-align: right;";
                    }

                    htmlBuilder.Append($"<p style='{paragraphStyle}'>");

                    foreach (var run in paragraphElement.Elements<Run>())
                    {
                        var bold = run.RunProperties?.Bold != null ? "font-weight: bold;" : "";
                        var italic = run.RunProperties?.Italic != null ? "font-style: italic;" : "";
                        var fontSize = run.RunProperties?.FontSize?.Val != null
                            ? $"font-size: {run.RunProperties.FontSize.Val}px;"
                            : "";

                        var textStyle = $"{bold} {italic} {fontSize}";

                        foreach (var text in run.Elements<Text>())
                        {
                            htmlBuilder.Append($"<span style='{textStyle}'>{text.Text}</span>");
                        }
                    }

                    htmlBuilder.Append("</p>");
                }
                else if (element is Table tableElement)
                {
                    htmlBuilder.Append("<table style='border-collapse: collapse; width: 50%; margin: 0 auto; border: 1px solid black;'>");

                    foreach (var row in tableElement.Elements<TableRow>())
                    {
                        htmlBuilder.Append("<tr>");
                        foreach (var cell in row.Elements<TableCell>())
                        {
                            string cellStyle = "border: 1px solid black; padding: 5px; text-align: left;";
                            int colSpan = 1;
                            int rowSpan = 1;

                            var gridSpan = cell.TableCellProperties?.GridSpan?.Val;
                            if (gridSpan != null)
                                colSpan = int.Parse(gridSpan.InnerText);

                            var vMerge = cell.TableCellProperties?.VerticalMerge;
                            if (vMerge != null && vMerge.Val != null)
                            {
                                if (vMerge.Val.Value == MergedCellValues.Restart)
                                    rowSpan = GetRowSpan(cell);
                                else if (vMerge.Val.Value == MergedCellValues.Continue)
                                    continue;
                            }

                            htmlBuilder.Append($"<td style='{cellStyle}' colspan='{colSpan}' rowspan='{rowSpan}'>");

                            var combinedText = new StringBuilder();

                            foreach (var cellParagraph in cell.Elements<Paragraph>())
                            {
                                foreach (var run in cellParagraph.Elements<Run>())
                                {
                                    foreach (var text in run.Elements<Text>())
                                    {
                                        combinedText.Append(text.Text.Trim() + " ");
                                    }
                                }
                            }

                            var cellContent = combinedText.ToString().Trim();
                            if (cellContent.Contains("Fatura Tarihi") || cellContent.Contains("Fatura Numarası"))
                            {
                                cellContent = cellContent.Replace("Fatura Tarihi", "").Replace("Fatura Numarası", "").Trim();
                            }

                            htmlBuilder.Append(cellContent);
                            htmlBuilder.Append("</td>");
                        }
                        htmlBuilder.Append("</tr>");
                    }

                    htmlBuilder.Append("</table>");
                }
            }

            return htmlBuilder.ToString();
        }

        private int GetRowSpan(TableCell cell)
        {
            var rowSpan = 1;
            var nextCell = cell;

            while (nextCell != null)
            {
                var vMerge = nextCell.TableCellProperties?.VerticalMerge;
                if (vMerge == null || vMerge.Val == null || vMerge.Val.Value != MergedCellValues.Continue)
                    break;

                rowSpan++;
                nextCell = GetNextCellInRow(nextCell);
            }

            return rowSpan; 
        }

        private TableCell GetNextCellInRow(TableCell currentCell)
        {
            var row = currentCell.Ancestors<TableRow>().FirstOrDefault();
            if (row == null) return null;

            var cells = row.Elements<TableCell>().ToList();
            var index = cells.IndexOf(currentCell);

            return index >= 0 && index < cells.Count - 1 ? cells[index + 1] : null;
        }
    }
}
