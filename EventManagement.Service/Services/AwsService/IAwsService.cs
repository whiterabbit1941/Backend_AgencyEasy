using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using EventManagement.Domain;

namespace EventManagement.Service
{
    public interface IAwsService
    {
        string ResizeImage(string imageUrl, int height, int width);
        Task<string> UploadImageToAws(string directory, string base64, string fileName);
    }
}

