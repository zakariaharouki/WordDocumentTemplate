using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
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
            // Populate empty entries for dynamic form binding
            ResumeData.Experiences.Add(new ResumeData.Experience());
            ResumeData.Educations.Add(new ResumeData.Education());
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "WordTemplate.docx");
            var outputFileName = $"{ResumeData.FullName}_Resume.docx";

            //  STEP 1: Load file content into MemoryStream
            byte[] fileBytes = System.IO.File.ReadAllBytes(templatePath);
            using MemoryStream templateStream = new MemoryStream(fileBytes);

            //  STEP 2: Load the template into Syncfusion document
            using WordDocument document = new WordDocument(templateStream, FormatType.Docx);

            //  STEP 3: Replace placeholders
            document.Replace("{{FullName}}", ResumeData.FullName ?? "", true, true);
            document.Replace("{{Title}}", ResumeData.Title ?? "", true, true);
            document.Replace("{{City}}", ResumeData.City ?? "", true, true);
            document.Replace("{{Phone}}", ResumeData.Phone ?? "", true, true);
            document.Replace("{{Email}}", ResumeData.Email ?? "", true, true);
            document.Replace("{{Objective}}", ResumeData.Objective ?? "", true, true);
            document.Replace("{{Skills}}", ResumeData.Skills ?? "", true, true);
            document.Replace("{{Experiences}}", FormatExperiences(), true, true);
            document.Replace("{{Educations}}", FormatEducations(), true, true);
            document.Replace("{{CurrentDate}}", DateTime.Now.ToString("D"), true, true);

            //  STEP 4: Save to stream
            MemoryStream outputStream = new MemoryStream();
            document.Save(outputStream, FormatType.Docx);
            outputStream.Position = 0;

            //  STEP 5: Return the generated file
            return File(outputStream,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                outputFileName);
        }



        private string FormatExperiences()
        {
            return string.Join("\n\n", ResumeData.Experiences
                .Where(e => !string.IsNullOrWhiteSpace(e.Company))
                .Select(e => $"{e.Company} | {e.Position}\t{e.TimePeriod}\n" +
                             string.Join("\n", e.Responsibilities.Select(r => $"• {r}"))));
        }

        private string FormatEducations()
        {
            return string.Join("\n\n", ResumeData.Educations
                .Where(e => !string.IsNullOrWhiteSpace(e.University))
                .Select(e => $"{e.University}, {e.Major}\t{e.Year}\nDegree: {e.Degree}"));
        }
    }
}
