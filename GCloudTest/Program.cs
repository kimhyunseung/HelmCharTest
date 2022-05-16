using Google.Api.Gax.ResourceNames;
using Google.Cloud.RecaptchaEnterprise.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RecaptchaEnterpriseServiceClient client = RecaptchaEnterpriseServiceClient.Create();
            ProjectName projectName = ProjectName.FromProject("");
            var assmnt = new Assessment();
            assmnt.Event.SiteKey = "";
            assmnt.Event.Token = "";

            client.CreateAssessment(projectName, assmnt);
        }
    }
}
