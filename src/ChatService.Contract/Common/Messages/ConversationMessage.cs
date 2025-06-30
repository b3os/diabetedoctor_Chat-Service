namespace ChatService.Contract.Common.Messages;

public enum ConversationMessage
{
    [Message("Tạo nhóm thành công", "group01")]
    CreatedGroupSuccessfully,
    [Message("Cập nhật thành công", "group02")]
    UpdatedGroupSuccessfully,
    [Message("Xóa nhóm thành công", "group03")]
    DeletedGroupSuccessfully,
    [Message("Phân quyền quản trị viên nhóm thành công", "group04")]
    PromoteMemberToAdminSuccessfully,
    [Message("Gỡ quyền quản trị viên nhóm thành công", "group05")]
    DemoteAdminToMemberSuccessfully,
    [Message("Thêm thành viên thành công", "group06")]
    AddMemberToGroupSuccessfully,
    [Message("Xóa thành viên thành công", "group07")]
    RemoveMemberFromGroupSuccessfully,
    [Message("Thêm bác sĩ thành công", "group08")]
    AddDoctorToGroupSuccessfully,
    
    // Error
    [Message("Không tìm thấy cuộc trò chuyện hoặc bạn không phải thành viên trong nhóm.", "group_error_01")]
    ConversationNotFound,
    [Message("Không có quyền để thực hiện thao tác này", "group_error_02")]
    ConversationAccessDenied,
    [Message("Thành viên đã tồn tại hoặc đã bị cấm", "group_error_03")]
    GroupMemberAlreadyExistedOrBanned,
    [Message("Thành viên không tồn tại trong nhóm", "group_error_04")]
    GroupMemberNotExists,
    [Message("Không có quyền để xóa thành viên này", "group_error_05")]
    CannotRemoveMember,
    [Message("Không thể gỡ quyền chủ nhóm", "group_error_06")]
    CannotDemoteOwner,
    [Message("Cuộc trò chuyện này đã đóng", "group_error_07")]
    ThisConversationIsClosed,
    [Message("Thành viên này đang bị cấm", "group_error_08")]
    MemberIsBanned,
    [Message("Thành viên này đã tồn tại", "group_error_09")]
    MemberAlreadyExisted,
    [Message("Bạn đã bị cấm khỏi nhóm này", "group_error_10")]
    YouAreBanned,
    [Message("Bạn đã là thành viên trong nhóm", "group_error_11")]
    YouAlreadyInGroup,
    
    
    //Validator
    [Message("Không có gì thay đổi", "conversation_validation_01")]
    NothingChanges,
    [Message("Tên không được trùng với tên cũ", "conversation_validation_02")]
    SameAsCurrentName
    
}