using System.ComponentModel.DataAnnotations;

namespace WordDocumentTemplate.models
{

    public class ResumeData
    {
        // Personal Information
        public string FullName { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // Objective
        public string Objective { get; set; }

        // Experience (supports multiple entries)
        public List<Experience> Experiences { get; set; } = new List<Experience>();

        // Education (supports multiple entries)
        public List<Education> Educations { get; set; } = new List<Education>();

        // Skills & Certifications
        public string Skills { get; set; }


        // Experience Sub-Class
        public class Experience
        {
            public string Company { get; set; }
            public string Position { get; set; }
            public string TimePeriod { get; set; } // "20XX - 20XX"
            public List<string> Responsibilities { get; set; } = new List<string>();

            public override string ToString()
            {
                return $"{Company} | {Position}\t{TimePeriod}\n" +
                       string.Join("\n", Responsibilities.Select(r => $"• {r}"));
            }
        }

        // Education Sub-Class
        public class Education
        {
            public string University { get; set; }
            public string Major { get; set; }
            public string Degree { get; set; }
            public string Year { get; set; }

            public override string ToString()
            {
                return $"{University}, {Major}\t{Year}\n" +
                       $"Degree: {Degree}";
            }
        }
    }
}
