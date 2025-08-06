using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Bookmarks.ErrorMessages;

public class BookmarkErrors
{
    public const string BookmarkNotFound = "Bookmark not found";
    public const string BookmarkAlreadyExists = "Bookmark already exists";
    public const string BookmarkNotValid = "Bookmark is not valid";
}

public class DataSourceBookmarkErrors
{
    public const string DataSourceBookmarkNotFound = "Data source bookmark not found";
    public const string DataSourceBookmarkAlreadyExists = "Data source bookmark already exists";
    public const string DataSourceBookmarkNotValid = "Data source bookmark is not valid";
}
