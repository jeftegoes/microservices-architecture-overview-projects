using System.Drawing;
using System.Drawing.Imaging;

public class ImageUtility
{
    public byte[] ConvertToBytes(string imagePath)
    {
        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(imagePath, FileMode.Open))
        {
            fileStream.CopyTo(memoryStream);
        }

        var bytes = memoryStream.ToArray();
        return bytes;
    }

    public void FromBytesToImage(byte[] imageBytes, string fileName)
    {
        using (var ms = new MemoryStream(imageBytes))
        {
            var image = Image.FromStream(ms);
            image.Save(fileName + ".jpg", ImageFormat.Jpeg);
        }
    }
}