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
    
    // Error
    [Message("Không tìm thấy cuộc trò chuyện hoặc bạn không phải thành viên trong nhóm.", "group_error_01")]
    ConversationNotFound,
    [Message("Không có quyền để thực hiện thao tác này", "group_error_02")]
    ConversationAccessDenied,
    [Message("Thành viên đã tồn tại", "group_error_03")]
    GroupMemberAlreadyExist,
    [Message("Thành viên không tồn tại trong nhóm", "group_error_04")]
    GroupMemberNotExists,
    [Message("Không có quyền để xóa thành viên này", "group_error_05")]
    CannotRemoveMember,
    [Message("Không thể gỡ quyền chủ nhóm", "group_error_06")]
    CannotDemoteOwner,
    [Message("Cuộc trò chuyện này không khả dụng với bạn", "group_error_07")]
    YouNotConversationParticipant,
    
    //Validator
    [Message("Không có gì thay đổi", "conversation_validation_01")]
    NothingChanges,
    [Message("Tên không được trùng với tên cũ", "conversation_validation_02")]
    SameAsCurrentName
    
}