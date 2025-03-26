﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal
{
    public class TerminalErrorsTest
    {
        [Fact]
        public void AssertErrorsAreValid()
        {
            typeof(TerminalErrors).Should().HaveConstantCount(20);

            TerminalErrors.ConnectionClosed.Should().Be("connection_closed");
            TerminalErrors.InvalidCommand.Should().Be("invalid_command");
            TerminalErrors.InvalidConfiguration.Should().Be("invalid_configuration");
            TerminalErrors.InvalidOption.Should().Be("invalid_option");
            TerminalErrors.InvalidArgument.Should().Be("invalid_argument");
            TerminalErrors.DuplicateOption.Should().Be("duplicate_option");
            TerminalErrors.InvalidRequest.Should().Be("invalid_request");
            TerminalErrors.UnsupportedOption.Should().Be("unsupported_option");
            TerminalErrors.UnsupportedCommand.Should().Be("unsupported_command");
            TerminalErrors.UnsupportedArgument.Should().Be("unsupported_argument");
            TerminalErrors.ServerError.Should().Be("server_error");
            TerminalErrors.MissingOption.Should().Be("missing_option");
            TerminalErrors.MissingClaim.Should().Be("missing_claim");
            TerminalErrors.MissingArgument.Should().Be("missing_argument");
            TerminalErrors.MissingCommand.Should().Be("missing_command");
            TerminalErrors.MissingIdentity.Should().Be("missing_identity");
            TerminalErrors.RequestCanceled.Should().Be("request_canceled");
            TerminalErrors.InvalidLicense.Should().Be("invalid_license");
            TerminalErrors.UnauthorizedAccess.Should().Be("unauthorized_access");
            TerminalErrors.InvalidDeclaration.Should().Be("invalid_declaration");
        }
    }
}