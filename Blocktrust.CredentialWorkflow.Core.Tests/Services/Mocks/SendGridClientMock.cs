using SendGrid;
using SendGrid.Helpers.Mail;
using Moq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Services.Mocks
{
    public class SendGridClientMock
    {
        public static Mock<ISendGridClient> CreateMock(HttpStatusCode responseStatusCode = HttpStatusCode.Accepted)
        {
            var mockResponse = new Mock<Response>(MockBehavior.Strict);
            mockResponse.Setup(r => r.StatusCode).Returns(responseStatusCode);
            mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(responseStatusCode == HttpStatusCode.Accepted || responseStatusCode == HttpStatusCode.OK);
            
            var mockClient = new Mock<ISendGridClient>();
            mockClient
                .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);
                
            return mockClient;
        }
    }
}