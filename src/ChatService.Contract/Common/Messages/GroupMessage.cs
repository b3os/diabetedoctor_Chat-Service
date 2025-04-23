namespace ChatService.Contract.Common.Messages;

public enum GroupMessage
{
    [Message("Tạo nhóm thành công", "GROUP_200")]
    CreatedGroupSuccessfully,
    [Message("Cập nhật thành công", "group02")]
    UpdatedGroupSuccessfully,
    [Message("Xóa nhóm thành công", "group03")]
    DeletedGroupSuccessfully,
    //Exception
    [Message("Nhóm không tồn tại", "group04")]
    GroupNotFound,
}