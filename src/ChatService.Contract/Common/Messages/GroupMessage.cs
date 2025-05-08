namespace ChatService.Contract.Common.Messages;

public enum GroupMessage
{
    [Message("Tạo nhóm thành công", "group01")]
    CreatedGroupSuccessfully,
    [Message("Cập nhật thành công", "group02")]
    UpdatedGroupSuccessfully,
    [Message("Xóa nhóm thành công", "group03")]
    DeletedGroupSuccessfully,
    [Message("Phân quyền quản trị viên nhóm thành công", "group04")]
    PromoteAdminGroupSuccessfully,
    [Message("Thêm thành viên thành công", "group05")]
    AddMemberToGroupSuccessfully,
    [Message("Xóa thành viên thành công", "group06")]
    RemoveMemberFromGroupSuccessfully,
    
    //Exception
    [Message("Nhóm không tồn tại", "group_error_01")]
    GroupNotFound,
    [Message("Không có quyền truy cập nhóm", "group_error_02")]
    GroupAccessDenied,
    [Message("Thành viên đã tồn tại", "group_error_03")]
    GroupMemberAlreadyExists
}