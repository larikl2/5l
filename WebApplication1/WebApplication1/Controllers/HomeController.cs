using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string pattern)
        {
            List<SearchResultLine> result;
            using (kira2Entities db = new kira2Entities())
            {
                result = db.Car
                .Join(
                db.CarCustomer,
                s => s.id,
                sc => sc.id_cust,
                (s, sc) => new
                {
                    Mark = s.Mark,
                    id_s_c = sc.id,
                    id_c = sc.id_car
                }
            ).Join(
            db.Customer,
            sc => sc.id_c,
            c => c.id,
            (sc, c) => new SearchResultLine()
            {
                Mark = sc.Mark,
                Name = c.Name
            }
            ).ToList();
            }
            if (pattern == null)
            {
                ViewBag.SearchData = result;

                return View();
            }
            else
            {
                result = result.Where((p) => p.Mark.Contains(pattern)).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }
        public FileStreamResult GetWord()
        {
            List<SearchResultLine> result;
            using (kira2Entities db = new kira2Entities())
            {
                result = db.Car
                .Join(
                db.CarCustomer,
                s => s.id,
                sc => sc.id_cust,
                (s, sc) => new
                {
                    Mark = s.Mark,
                    id_s_c = sc.id,
                    id_c = sc.id_car
                }
            ).Join(
            db.Customer,
            sc => sc.id_c,
            c => c.id,
            (sc, c) => new SearchResultLine()
            {
                Mark = sc.Mark,
                Name = c.Name
            }
            ).ToList();

                string[,] data = new string[result.Count , 2];
                for (int i = 0; i < result.Count; i++)
                {
                    data[i, 0] = result[i].Mark;
                    data[i, 1] = result[i].Name;
                }
                MemoryStream memoryStream = GenerateWord(data);
                return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = "demo.docx"
                };
            }
        }
        public MemoryStream GenerateWord(string[,] data)
        {
            MemoryStream mStream = new MemoryStream();
            // Создаем документ
            WordprocessingDocument document =
            WordprocessingDocument.Create(mStream, WordprocessingDocumentType.Document,
            true);
            // Добавляется главная часть документа.
            MainDocumentPart mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());
            // Создаем таблицу.
            Table table = new Table();
            body.AppendChild(table);
            // Устанавливаем свойства таблицы(границы и размер).
            TableProperties props = new TableProperties(
            new TableBorders(
            new TopBorder
            {
                Val = new EnumValue<BorderValues>(BorderValues.Single),
                Size = 12
            },
            new BottomBorder
            {
                Val = new EnumValue<BorderValues>(BorderValues.Single),
                Size = 12
            },
            new LeftBorder
            {
                Val = new EnumValue<BorderValues>(BorderValues.Single),
                Size = 12
            },
            new RightBorder
            {
                Val = new EnumValue<BorderValues>(BorderValues.Single),
                Size = 12
            },
            new InsideHorizontalBorder
            {
                Val = new EnumValue<BorderValues>(BorderValues.Single),
                Size = 12
            },
            new InsideVerticalBorder
            {
                Val = new EnumValue<BorderValues>(BorderValues.Single),
                Size = 12
            }));
            // Назначаем свойства props объекту table
            table.AppendChild<TableProperties>(props);
            // Заполняем ячейки таблицы.
            for (var i = 0; i <= data.GetUpperBound(0); i++)
            {
                var tr = new TableRow();
                for (var j = 0; j <= data.GetUpperBound(1); j++)
                {

                    var tc = new TableCell();
                    tc.Append(new Paragraph(new Run(new Text(data[i, j]))));
                    // размер колонок определяется автоматически.
                    tc.Append(new TableCellProperties(
                        new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    tr.Append(tc);
                }
                table.Append(tr);
            }
            mainPart.Document.Save();
            document.Clone();
            mStream.Position = 0;
            return mStream;

        }
        public void DeleteRow(int delId)
        {
            kira2Entities db = new kira2Entities();
            var dat = db.CarCustomer.Where(x => x.id == delId).FirstOrDefault();
            db.CarCustomer.Remove(dat);
            db.SaveChanges();
        }
        public void AddRow(int carid, int customerid)
        {
            kira2Entities db = new kira2Entities();
            db.CarCustomer.Add(new CarCustomer() { id_cust = customerid, id_car = carid });
            db.SaveChanges();
        }
    }

}