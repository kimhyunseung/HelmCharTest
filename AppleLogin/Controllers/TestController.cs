using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppleLogin.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TT()
        {
            return View();
        }

    
        public ActionResult Authorize()
        {
            var url = "https://appleid.apple.com/auth/authorize";

            //Required
            var clientId = "com.webzen.mocaa.nativesample.signin";        //Native
            //var clientId = "com.webzen.mocaa.nativesample.signin";          //Unity
            var redirectUrl = "https://alpha-kr-mapi.webzen.com/v2/account/appleid/callback";
            var responseType = "code";

            //Optinal
            string scope = null;
            var response_mode = "query";      //Must Be Response Mode fragment or form_post
            var state = "TEST STATE";
            var nonce = "TEST NONCE";

            var postUrl = string.Format(
                @"{0}?"
                + "client_id={1}"
                + "&redirect_uri={2}"
                + "&response_type={3}"
                + "&scope={4}"
                + "&response_mode={5}"
                + "&state={6}"
                + "&nonce={7}",

                url,
                clientId,
                redirectUrl,
                responseType,
                scope,
                response_mode,
                state,
                nonce);

            return Redirect(postUrl);
        }
    }
}