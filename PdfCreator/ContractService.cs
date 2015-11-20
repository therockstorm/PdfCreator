using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace PdfCreator
{
    public class ContractService
    {
        private const string BucketName = "my-bucket";

        private readonly IAmazonS3 _client = new AmazonS3Client(RegionEndpoint.USWest2);

        public async Task<bool> UploadAsync(Contract contract)
        {
            string error = null;

            try
            {
                var response = await _client.PutObjectAsync(CreateUploadRequest(contract));
                if (response.HttpStatusCode != HttpStatusCode.OK) error = response.HttpStatusCode.ToString();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error == null;
        }

        public async Task<Stream> GetContractAsync(Contract contract)
        {
            using (var response = await _client.GetObjectAsync(CreateGetContractRequest(contract)))
            {
                return response.ResponseStream;
            }
        }

        private static PutObjectRequest CreateUploadRequest(Contract contract)
        {
            return new PutObjectRequest
            {
                BucketName = BucketName,
                Key = contract.TradeId.ToString(),
                FilePath = contract.Filepath,
                ContentType = "application/pdf"
            };
        }

        private static GetObjectRequest CreateGetContractRequest(Contract contract)
        {
            return new GetObjectRequest
            {
                BucketName = BucketName,
                Key = contract.TradeId.ToString()
            };
        }
    }
}