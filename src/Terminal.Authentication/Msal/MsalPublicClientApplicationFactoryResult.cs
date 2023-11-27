/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Identity.Client;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// The <see cref="IMsalPublicClientApplicationFactory"/> result.
    /// </summary>
    public class MsalPublicClientApplicationFactoryResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="application"></param>
        public MsalPublicClientApplicationFactoryResult(IClientApplicationBase application)
        {
            Application = application;
        }

        /// <summary>
        /// The client application.
        /// </summary>
        public IClientApplicationBase Application { get; }

        /// <summary>
        /// Casts the <see cref="Application"/> to the specified <see cref="IClientApplicationBase"/> type.
        /// </summary>
        /// <typeparam name="TApp">The client application type.</typeparam>
        public TApp As<TApp>() where TApp : IClientApplicationBase
        {
            return (TApp)Application;
        }
    }
}
