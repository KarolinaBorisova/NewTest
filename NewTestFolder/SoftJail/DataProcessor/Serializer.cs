namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners.Where(p => ids.Contains(p.Id))
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.FullName,
                    CellNumber = x.Cell.CellNumber,
                    Officers = x.PrisonerOfficers.Select(o => new
                        {
                           OfficerName = o.Officer.FullName,
                           Department = o.Officer.Department.Name
                        })
                            .OrderBy(x => x.OfficerName)
                            .ToList(),
                    TotalOfficerSalary = decimal.Parse(x.PrisonerOfficers
                    .Sum(x => x.Officer.Salary)
                    .ToString("F2"))
                    
                })
                .OrderBy(p=>p.Name)
                .ThenBy(p=>p.Id)
                .ToList();

            string result = JsonConvert.SerializeObject(prisoners, Formatting.Indented);
            return result;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            
            List<string> prisonersNameList = prisonersNames
                .Split(",",StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var prisoners = context.Prisoners
                .Where(p => prisonersNameList.Contains(p.FullName))
                .Select(x => new PrisonerXmlExportModel
                {
                    Id = x.Id,
                    Name = x.FullName,
                    IncarcerationDate = x.IncarcerationDate.ToString("yyyy-MM-dd"),
                    EncryptedMessages = x.Mails.Select(m=> new MessageXmlModel
                    {
                        Description =string.Join("", m.Description.Reverse()) 
                    })
                    .ToArray()
                })
                .OrderBy(x=>x.Name)
                .ThenBy(x=>x.Id)
                .ToArray();


            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrisonerXmlExportModel[]), new XmlRootAttribute("Prisoners"));
            var sw = new StringWriter();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(sw, prisoners, ns);
            return sw.ToString();
           
        }
    }
}