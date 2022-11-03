using SoftJail.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class OfficerXmlImportModel
    {
        [XmlElement("Name")]
        [MinLength(3)]
        [MaxLength(30)]
        [Required]
        public string Name { get; set; }

        [Range(0,double.MaxValue)]
        public decimal Money { get; set; }

        [EnumDataType(typeof(Position))]
        public string Position { get; set; }

        [EnumDataType(typeof(Weapon))]
        public string Weapon { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public PrisonerXmlImputModel[] Prisoners { get; set; }
    }

  // <Officer>
  // <Name>Minerva Kitchingman</Name>
  // <Money>2582</Money>
  // <Position>Invalid</Position>
  // <Weapon>ChainRifle</Weapon>
  // <DepartmentId>2</DepartmentId>
  // <Prisoners>
  //   <Prisoner id = "15" />
  // </ Prisoners >
  /// Officer >



}
