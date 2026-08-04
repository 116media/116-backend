# High — No test authenticates with a token the application issued

Every authenticated integration test carries a token the test project minted for
itself, validated by parameters the test project installed over the production
ones. The application's own token issuer is never on the path between login and a
protected endpoint, so the entire authentication contract — claim types, claim
values, issuer, audience, signing key — is untested end to end.

## The problem

The test suite mints its own tokens:

```csharp
// tests/Integration/Common/Extensions/HttpClientExtensions.cs:111-141
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.ValidSecret));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
...
var descriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddHours(1),
    SigningCredentials = credentials,
    Issuer = Jwt.ValidIssuer,
    Audience = Jwt.ValidAudience,
};
```

And the fixture replaces the host's validation so those tokens are accepted:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:137-156
private static void OverrideJwtAuthentication(IServiceCollection services)
{
    services.PostConfigure<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ...
                ValidIssuer = Jwt.ValidIssuer,
                ValidAudience = Jwt.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.ValidSecret)),
                ClockSkew = TimeSpan.Zero,
            };
        }
    );
}
```

Note that this is a whole-object replacement, not a tweak: whatever the production
`AddJwtAuthentication` configured is discarded, including any validation rule the
test constants happen not to reproduce.

`DefaultRequestHeaders.Authorization` is assigned in exactly five places, all of
them inside `HttpClientExtensions.cs` (lines 46, 56, 69, 81, and the `null` clear at
89). No test file anywhere writes a login-response token into a request header.

Tokens the application really issued are touched in 14 places across six test
files, and every one of them is a shape check or an expiry comparison:

```csharp
// tests/Integration/Workflows/AuthenticationFlowTests.cs:33-35
signupBody.AccessToken.Should().NotBeNullOrEmpty();
signupBody.RefreshToken.Should().NotBeNullOrEmpty();
signupBody.AccessToken.Split('.').Should().HaveCount(3);
```

That block belongs to a test named
`SignUpAndLogin_ShouldGrantAccessToProtectedEndpoints`
(`AuthenticationFlowTests.cs:16`). The test signs up, asserts the token has three
dot-separated segments, and asserts a user row exists. It never logs in, and it
never calls a protected endpoint. The name describes a test that does not exist.

## Why it matters

`JwtService` is the only thing that produces credentials in production, and nothing
proves that what it produces is accepted by the pipeline that consumes them. Three
concrete regressions would ship green today:

**A dropped session claim.** `JwtService.cs:52` emits
`JwtClaimsConstants.SessionId`. Handlers that validate the session against the
database read it. Delete that line and every test still passes, because the test
minter adds its own session claim at `HttpClientExtensions.cs:131`.

**A renamed role claim.** `JwtService.cs:98` emits roles as `ClaimTypes.Role`.
Change it to a short `"role"` claim type and authorization policies break in
production while the tests, which mint `ClaimTypes.Role` themselves, stay green.

**A mismatched issuer or audience.** The host's real validation parameters are
overwritten before any request runs, so a production `JWT_ISSUER` that no longer
matches what `JwtService` stamps is invisible.

There is also a claim the test minter does not emit at all. `JwtService` adds a
permissions claim when the user has permissions:

```csharp
// src/Modules/Identity/Identity/Infrastructure/Services/JwtService.cs:116-122
permissionClaims.Add(
    new Claim(
        type: JwtClaimsConstants.Permissions,
        JsonSerializer.Serialize(value: permissionsList),
        valueType: JsonClaimValueTypes.JsonArray
    )
);
```

No test token carries it, so any authorization rule that ever starts reading
permissions from the token would be untestable through the existing helpers, and
the serialization format of that claim is asserted nowhere.

Finally, the helpers hardcode role names as strings —
`client.AuthenticateAs(User.SuperAdminId, "SuperAdmin")` at
`HttpClientExtensions.cs:21`, and the same for `"Admin"` and `"Visitor"` at :29 and
:37 — while production compares against `nameof(EnumCoreUserRole.SuperAdmin)` (see
`src/Modules/Identity/Identity/Application/Roles/Specifications/UserRoleSpecifications.cs:19`).
Renaming an enum member would leave the tests passing against the old string.

**This is not primarily a test-authoring failure.** It is downstream of
[02-environment-divergence.md](02-environment-divergence.md): `.env` clobbers
`JWT_SECRET` after the fixture sets it, so the host signs with a secret the test
project cannot know. A real round trip is impossible until that is fixed, and the
`OverrideJwtAuthentication` doc comment says so in as many words.

## The fix

Fix `02` first, then delete the override and add the round trip the suite is
missing.

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs — before

builder.ConfigureTestServices(services =>
{
    ReplaceDbContexts(services);
    StubExternalServices(services);
    OverrideJwtAuthentication(services);

    if (DisableRateLimits)
    {
        DisableRateLimiting(services);
    }
});

// after — the host validates with the parameters production configured
builder.ConfigureTestServices(services =>
{
    ReplaceDbContexts(services);
    StubExternalServices(services);

    if (DisableRateLimits)
    {
        DisableRateLimiting(services);
    }
});
```

`HttpClientExtensions` can stay — hand-minted tokens are the right tool for the
malformed-credential tests it already serves (`AuthenticateWithoutSessionClaim`,
`AuthenticateWithMalformedUserId`). What it must stop being is the *only* way a
test ever authenticates. Add the missing test and give it the name the current one
borrows:

```csharp
// tests/Integration/Workflows/AuthenticationFlowTests.cs — after
[Fact]
public async Task SignUpAndLogin_ShouldGrantAccessToProtectedEndpoints()
{
    await SeedAsync<IdentityDbContext>(context =>
        context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor)))
    );

    Client.ClearAuthentication();
    Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

    string email = $"flow-{Guid.NewGuid():N}@test.com";
    var signupRequest = new PublicSignUpRequest(
        Email: email,
        UserName: $"u{Guid.NewGuid():N}"[..10],
        Password: TestAuth.ValidPassword
    );

    HttpResponseMessage signup = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);
    signup.StatusCode.Should().Be(HttpStatusCode.Created);

    await using (IdentityDbContext seedContext = CreateDbContext<IdentityDbContext>())
    {
        UserEntity user = await seedContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await seedContext.SaveChangesAsync();
    }

    HttpResponseMessage login = await Client.PostAsJsonAsync(
        Routes.Public.Auth.Login(),
        new PublicLoginRequest(Credentials: email, Password: TestAuth.ValidPassword)
    );
    login.StatusCode.Should().Be(HttpStatusCode.OK);

    PublicLoginMobileResponse body = await login.ReadAsAsync<PublicLoginMobileResponse>();

    // The whole point: the credential under test is the one the application issued.
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.AccessToken);

    HttpResponseMessage protectedResponse = await Client.GetAsync(Routes.Public.Me.Profile());

    protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    PublicGetOwnProfileResponse profile = await protectedResponse.ReadAsAsync<PublicGetOwnProfileResponse>();
    profile.User.Email.Should().Be(email, "the endpoint resolved the caller from the issued token's claims");
}
```

`PublicGetOwnProfileEndpointV1Tests.cs:12-23` already makes exactly this assertion
against `Routes.Public.Me.Profile()` — it reads the caller's id and email back out
of the response. The only thing it lacks is a credential the application produced.
The new test is that same assertion with the real token substituted in.

The last assertion is what makes the test able to fail. A 200 alone would still
pass if the endpoint ignored the caller; reading the caller's own email back proves
the subject claim survived issuance, transport, validation, and handler resolution.

One such test is enough. Every other test can keep using the minted-token helpers,
because once this test exists, the claim contract itself has a guard.

## The principle

**At least one test must exercise every credential the system issues, through the
same path a client would use.** Shape checks on a token — non-empty, three
segments — assert properties of base64 encoding, not properties of this
application. They pass for any JWT ever produced by anyone.

The broader rule: when a fixture rewrites a security control so that test-produced
inputs are accepted, the control is no longer under test. That is sometimes a
necessary trade, but it must be a recorded one with a compensating test, not a
silent default applied to all 1,879 tests.

## Checklist

- [ ] [02-environment-divergence.md](02-environment-divergence.md) fixed, so the
      host's signing key is the one the fixture set
- [ ] `ApiFixture.OverrideJwtAuthentication` deleted and removed from
      `ConfigureTestServices`
- [ ] One test logs in, sets the returned `AccessToken` as the bearer header, calls
      a protected endpoint, and asserts the resolved caller identity
- [ ] `HttpClientExtensions` role arguments use `nameof(EnumCoreUserRole.*)` rather
      than string literals
- [ ] Minted test tokens carry `JwtClaimsConstants.Permissions` in the same JSON
      array shape `JwtService` produces, or the helper documents why they do not
- [ ] `SignUpAndLogin_ShouldGrantAccessToProtectedEndpoints` either accesses a
      protected endpoint or is renamed to what it actually asserts
