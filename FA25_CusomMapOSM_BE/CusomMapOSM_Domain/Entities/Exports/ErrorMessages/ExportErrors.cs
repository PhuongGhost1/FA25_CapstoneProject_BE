using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Exports.ErrorMessages;

public class ExportErrors
{
    public const string ExportNotFound = "Export not found";
    public const string ExportAlreadyExists = "Export already exists";
    public const string ExportNotValid = "Export is not valid";
}

public class ExportTypeErrors
{
    public const string ExportTypeNotFound = "Export type not found";
    public const string ExportTypeAlreadyExists = "Export type already exists";
    public const string ExportTypeNotValid = "Export type is not valid";
}
