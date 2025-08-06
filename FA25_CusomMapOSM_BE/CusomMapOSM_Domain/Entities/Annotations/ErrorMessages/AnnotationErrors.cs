using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Annotations.ErrorMessages;

public class AnnotationErrors
{
    public const string AnnotationNotFound = "Annotation not found";
    public const string AnnotationAlreadyExists = "Annotation already exists";
    public const string AnnotationNotValid = "Annotation is not valid";
}

public class AnnotationTypeErrors
{
    public const string AnnotationTypeNotFound = "Annotation type not found";
    public const string AnnotationTypeAlreadyExists = "Annotation type already exists";
    public const string AnnotationTypeNotValid = "Annotation type is not valid";
}
