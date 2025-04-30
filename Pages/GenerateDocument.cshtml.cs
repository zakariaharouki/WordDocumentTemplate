using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenXmlPowerTools;
using WordDocumentTemplate.models;

namespace WordDocumentTemplate.Pages
{
    public class GenerateDocumentModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        public GenerateDocumentModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        [BindProperty]
        public ResumeData ResumeData { get; set; } = new ResumeData();

        public void OnGet()
        {
            // Initialize with empty entries
            ResumeData.Experiences.Add(new ResumeData.Experience());
            ResumeData.Educations.Add(new ResumeData.Education());
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var templatePath = System.IO.Path.Combine(_env.ContentRootPath, "Templates", "WordTemplate.docx");
            var outputPath = System.IO.Path.Combine(_env.WebRootPath, "generated-docs", $"{ResumeData.FullName}_Resume.docx");

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));

            using (var templateStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                var doc = new OpenXmlMemoryStreamDocument(templateStream);
                using (var wordDocument = doc.GetWordprocessingDocument())
                {
                    var body = wordDocument.MainDocumentPart.Document.Body;
                    var replacements = new Dictionary<string, string>
                    {
                        {"{{FullName}}", ResumeData.FullName},
                        {"{{Title}}", ResumeData.Title},
                        {"{{City}}", ResumeData.City},
                        {"{{Phone}}", ResumeData.Phone},
                        {"{{Email}}", ResumeData.Email},
                        {"{{Objective}}", ResumeData.Objective},
                        {"{{Skills}}", ResumeData.Skills},
                        {"{{Experiences}}", FormatExperiences()},
                        {"{{Educations}}", FormatEducations()},
                        {"{{CurrentDate}}", DateTime.Now.ToString("d")}
                    };

                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        foreach (var run in paragraph.Elements<Run>())
                        {
                            foreach (var text in run.Elements<Text>())
                            {
                                foreach (var replacement in replacements)
                                {
                                    if (text.Text.Contains(replacement.Key))
                                    {
                                        text.Text = text.Text.Replace(replacement.Key, replacement.Value);
                                    }
                                }
                            }
                        }
                    }

                    wordDocument.MainDocumentPart.Document.Save();
                }

                doc.SaveAs(outputStream);
            }

            return PhysicalFile(outputPath,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"{ResumeData.FullName}_Resume.docx");
        }

        private string FormatExperiences()
        {
            return string.Join("\n\n", ResumeData.Experiences
                .Select(e => $"{e.Company} | {e.Position}\t{e.TimePeriod}\n" +
                             string.Join("\n", e.Responsibilities.Select(r => $"• {r}"))));
        }

        private string FormatEducations()
        {
            return string.Join("\r\n\r\n", ResumeData.Educations
         .Select(e => $"{e.University}, {e.Major}\t{e.Year}\r\nDegree: {e.Degree}"));
        }
    }

    public class OpenXmlMemoryStreamDocument : IDisposable
    {
        private readonly MemoryStream _documentStream;

        public OpenXmlMemoryStreamDocument(Stream stream)
        {
            _documentStream = new MemoryStream();
            stream.CopyTo(_documentStream);
        }

        public WordprocessingDocument GetWordprocessingDocument()
        {
            return WordprocessingDocument.Open(_documentStream, true);
        }

        public void SaveAs(Stream stream)
        {
            _documentStream.Seek(0, SeekOrigin.Begin);
            _documentStream.CopyTo(stream);
        }

        public void Dispose()
        {
            _documentStream?.Dispose();
        }
    }
}
