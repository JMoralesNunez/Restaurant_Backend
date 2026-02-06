namespace RestaurantBackend.Application.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Uploads an image from a stream to Cloudinary
    /// </summary>
    /// <param name="imageStream">The image stream</param>
    /// <param name="fileName">The file name</param>
    /// <returns>The secure URL of the uploaded image</returns>
    Task<(string Url, string PublicId)> UploadImageAsync(Stream imageStream, string fileName);

    /// <summary>
    /// Uploads an image from a base64 string to Cloudinary
    /// </summary>
    /// <param name="base64Image">The base64 encoded image</param>
    /// <param name="fileName">The file name</param>
    /// <returns>The secure URL of the uploaded image</returns>
    Task<(string Url, string PublicId)> UploadImageAsync(string base64Image, string fileName);

    /// <summary>
    /// Deletes an image from Cloudinary
    /// </summary>
    /// <param name="publicId">The public ID of the image to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteImageAsync(string publicId);
}
