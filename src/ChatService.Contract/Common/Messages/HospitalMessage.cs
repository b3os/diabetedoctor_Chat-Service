namespace ChatService.Contract.Common.Messages;

public enum HospitalMessage
{
    [Message("Tên bệnh viện đã tồn tại", "hospital01")]
    HospitalNameExistException,

    [Message("Tạo bệnh viện đã tồn tại", "hospital02")]
    CreateHospitalSuccessfully,
}
