using System.Net.Http.Headers;
using Newtonsoft.Json;

var imagePath = "Images/avengers.jpg";

var urlAdress = "https://localhost:6001/api/Faces";

var imageUtility = new ImageUtility();

var bytes = imageUtility.ConvertToBytes(imagePath);

var faceList = new List<byte[]>();

var byteContent = new ByteArrayContent(bytes);
byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

using (var httpClient = new HttpClient())
{
    using (var client = await httpClient.PostAsync(urlAdress, byteContent))
    {
        var response = await client.Content.ReadAsStringAsync();
        faceList = (JsonConvert.DeserializeObject<Tuple<List<byte[]>, Guid>>(response)).Item1;
    }
}

if (faceList.Count > 0)
{
    for (int i = 0; i < faceList.Count; i++)
    {
        imageUtility.FromBytesToImage(faceList[i], "face_" + i);
    }
}