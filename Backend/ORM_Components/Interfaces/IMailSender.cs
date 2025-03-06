using ORM_Components.DTO.MailDtos;

namespace ORM_Components.Interfaces
{
    public interface IMailSender
    {
        Task SendEmailAsync(EmailDto emailDto);
    }
}
