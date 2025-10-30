namespace CusomMapOSM_Domain.Entities.Maps.Enums;

/// <summary>
/// Enum định nghĩa các trạng thái của Story Map
/// </summary>
public enum MapStatusEnum
{
    /// <summary>
    /// Nháp - Map đang được chỉnh sửa, chưa sẵn sàng để publish
    /// </summary>
    Draft = 1,
    
    /// <summary>
    /// Đang xem xét - Map đã được submit để review trước khi publish
    /// </summary>
    UnderReview = 2,
    
    /// <summary>
    /// Đã publish - Map đã được publish và có thể truy cập công khai (nếu IsPublic = true)
    /// </summary>
    Published = 3,
    
    /// <summary>
    /// Đã unpublish - Map đã được publish nhưng sau đó bị unpublish
    /// </summary>
    Unpublished = 4,
    
    /// <summary>
    /// Đã lưu trữ - Map đã bị archive và không còn hoạt động
    /// </summary>
    Archived = 5
}

