namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var output = new StringBuilder();
            var departments = JsonConvert.DeserializeObject<ICollection<DepartmentJsonImportModel>>(jsonString);

            foreach (var departmentJson in departments)
            {
                if (!IsValid(departmentJson) || departmentJson.Cells.Count()==0 ||
                    departmentJson.Cells.Any(x=>x.CellNumber<1))
                {
                    output.AppendLine("Invalid Data");
                    continue;
                }

               /* foreach (var cellJson in departmentJson.Cells)
                {
                    if (!IsValid(cellJson))
                    {
                        output.AppendLine("Invalid Data");
                        continue;
                    }
                }
               */

                var department = new Department
                {
                    Name = departmentJson.Name,
                    Cells = departmentJson.Cells.Select(x => new Cell
                    {
                        CellNumber = x.CellNumber,
                        HasWindow = x.HasWindow
                    }
                    ).ToArray()
                };

                context.Add(department);
                context.SaveChanges();
                output.AppendLine($"Imported {departmentJson.Name} with {departmentJson.Cells.Count()} cells");
                
            };
            return output.ToString();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var output = new StringBuilder();

            var prisoners = JsonConvert.DeserializeObject<IEnumerable<PrisonerJsonImportModel>>(jsonString);

            foreach (var prisonerJson in prisoners)
            {
                if (!IsValid(prisonerJson) || !prisonerJson.Mails.All(IsValid))
                {
                    output.AppendLine("Invalid Data");
                    continue;
                }
                var parsedDateIncarcerationdate = DateTime
                    .TryParseExact(prisonerJson.IncarcerationDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateIncarceration);

                var parsedDateReleaseDate = DateTime
                   .TryParseExact(prisonerJson.ReleaseDate, "dd/MM/yyyy",
                   CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateRelease);

                if (!parsedDateIncarcerationdate )
                {
                    output.AppendLine("Invalid Data");
                    continue;
                }

                var prisoner = new Prisoner
                {
                    FullName = prisonerJson.FullName,
                    Nickname = prisonerJson.Nickname,
                    Age = prisonerJson.Age,
                    IncarcerationDate = dateIncarceration,
                    ReleaseDate = dateRelease ,
                    Bail = prisonerJson.Bail,
                    CellId = prisonerJson.CellId,
                    Mails = prisonerJson.Mails.Select(m => new Mail
                    {
                        Description = m.Description,
                        Sender = m.Sender,
                        Address = m.Address
                    }).ToList(),
                };

                context.Prisoners.Add(prisoner);
                context.SaveChanges();

                output.AppendLine($"Imported {prisonerJson.FullName} {prisonerJson.Age} years old");
            }

            return output.ToString();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var output = new StringBuilder();

            var xmlSerialaizer = new XmlSerializer(typeof(OfficerXmlImportModel[]),
                new XmlRootAttribute("Officers"));

            var officers = (OfficerXmlImportModel[])xmlSerialaizer.Deserialize(
                new StringReader(xmlString));

            foreach (var officerXml in officers)
            {
                if (!IsValid(officerXml) )
                {
                    output.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer
                {
                    FullName = officerXml.Name,
                    Salary = officerXml.Money,
                    Position = Enum.Parse<Position>(officerXml.Position),
                    Weapon = Enum.Parse<Weapon>(officerXml.Weapon),
                    DepartmentId = officerXml.DepartmentId,
                    OfficerPrisoners = officerXml.Prisoners.Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id
                    })
                    .ToList()
                   
                };

                context.Add(officer);
                output.AppendLine($"Imported {officerXml.Name} ({officerXml.Prisoners.Count()} prisoners)");


            };

            context.SaveChanges();
            return output.ToString();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}