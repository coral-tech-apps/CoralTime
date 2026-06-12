using Microsoft.AspNetCore.Http;

namespace CoralTime.Common.Services
{
    public interface IImageService
    {
        string GetUrlAvatar(int memberId);

        string GetUrlIcon(int memberId);

        string UploadImage(IFormFile uploadedFile);

        void SaveImagesFromDbToFolder();
    }
}