﻿using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;
using AuthenticationManager = Usergrid.Sdk.Manager.AuthenticationManager;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class AuthenticationManagerTests
    {
        [Test]
        public void ShouldLoginWithClientCredentialsWithCorrectRequestBodyForAuthTypeClient()
        {
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken("accessToken");

            const string clientLoginId = "login";
            const string clientSecret = "secret";

            var authenticationManager = new AuthenticationManager(request);
            authenticationManager.Login(clientLoginId, clientSecret, AuthType.Organization);

            request
                .Received(1)
                .ExecuteJsonRequest<LoginResponse>(
                    Arg.Any<string>(),
                    Arg.Any<Method>(),
                    Arg.Is<ClientIdLoginPayload>(d => d.GrantType == "client_credentials" && d.ClientId == clientLoginId && d.ClientSecret == clientSecret));
        }

        [Test]
        public void ShouldLoginWithClientCredentialsWithCorrectRequestBodyForAuthTypeApplication()
        {
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken("accessToken");

            const string clientLoginId = "login";
            const string clientSecret = "secret";

            var authenticationManager = new AuthenticationManager(request);
            authenticationManager.Login(clientLoginId, clientSecret, AuthType.Application);

            request
                .Received(1)
                .ExecuteJsonRequest<LoginResponse>(
                    Arg.Any<string>(),
                    Arg.Any<Method>(),
                    Arg.Is<ClientIdLoginPayload>(d => d.GrantType == "client_credentials" && d.ClientId == clientLoginId && d.ClientSecret == clientSecret));
        }

        [Test]
        public void ShouldLoginWithUserCredentialsWithCorrectRequestBodyForAuthTypeUser()
        {
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken("accessToken");

            const string clientLoginId = "login";
            const string clientSecret = "secret";

            var authenticationManager = new AuthenticationManager(request);
            authenticationManager.Login(clientLoginId, clientSecret, AuthType.User);

            request
                .Received(1)
                .ExecuteJsonRequest<LoginResponse>(
                    Arg.Any<string>(),
                    Arg.Any<Method>(),
                    Arg.Is<UserLoginPayload>(d => d.GrantType == "password" && d.UserName == clientLoginId && d.Password == clientSecret));
        }

        [Test]
        public void ShouldNotMakeACallToEndPointWhenLoggingWithAuthTypeNone()
        {
            var request = Substitute.For<IUsergridRequest>();

            var authenticationManager = new AuthenticationManager(request);
            authenticationManager.Login(null, null, AuthType.None);

            request.DidNotReceiveWithAnyArgs().ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>());
        }

        [Test]
        [TestCase(null, AuthType.None)]
        [TestCase("accessToken1", AuthType.Application)]
        [TestCase("accessToken2", AuthType.Organization)]
        [TestCase("accessToken4", AuthType.User)]
        public void ShouldSetTheAccessToken(string accessToken, AuthType authType)
        {
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken(accessToken);

            var authenticationManager = new AuthenticationManager(request);
            authenticationManager.Login(null, null, authType);

            request.Received(1).AccessToken = accessToken;
        }

        [Test]
        public void ShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
        {
            const string invalidUsernameOrPassword = "Invalid username or password";
            const string invalidGrant = "invalid_grant";

            var restResponseContent = new UsergridError {Description = invalidUsernameOrPassword, Error = invalidGrant};
            var restResponse = Helpers.SetUpRestResponseWithContent<LoginResponse>(HttpStatusCode.BadRequest, restResponseContent);
            var request = Helpers.SetUpUsergridRequestWithRestResponse(restResponse);

            var authenticationManager = new AuthenticationManager(request);
            try
            {
                authenticationManager.Login(null, null, AuthType.Organization);
                throw new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual(invalidGrant, e.ErrorCode);
                Assert.AreEqual(invalidUsernameOrPassword, e.Message);
            }
        }
    }
}