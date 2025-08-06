using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Layers.ErrorMessages;

public class LayerErrors
{
    public const string LayerNotFound = "Layer not found.";
    public const string LayerInvalid = "Layer is invalid.";
    public const string LayerAlreadyExists = "Layer already exists.";
    public const string LayerNameEmpty = "Layer name cannot be empty.";
    public const string LayerTypeInvalid = "Layer type is invalid.";
    public const string LayerSourceNotFound = "Layer source not found.";
    public const string LayerSourceInvalid = "Layer source is invalid.";
}

public class LayerSourceErrors
{
    public const string LayerSourceNotFound = "Layer source not found.";
    public const string LayerSourceInvalid = "Layer source is invalid.";
    public const string LayerSourceAlreadyExists = "Layer source already exists.";
    public const string LayerSourceNameEmpty = "Layer source name cannot be empty.";
    public const string LayerSourceUrlEmpty = "Layer source URL cannot be empty.";
}

public class LayerTypeErrors
{
    public const string LayerTypeNotFound = "Layer type not found.";
    public const string LayerTypeInvalid = "Layer type is invalid.";
    public const string LayerTypeAlreadyExists = "Layer type already exists.";
    public const string LayerTypeNameEmpty = "Layer type name cannot be empty.";
    public const string LayerTypeDescriptionEmpty = "Layer type description cannot be empty.";
}