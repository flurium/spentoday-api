# Authentication and Autherization

## Refresh only schema

I desided to create own authentication solution to have benefits.
In standart JWT auth we have access token and refresh token. Refresh token is required
to keep user authenticated after page reload and stored in secure, http-only cookie.

We removed access token and rely only on refresh token instead. It can be send in cookie
and in header, so we can use it for different puproses. In database we store only version
of user not full sessions. Session information is stored inside of token. The version is
checked on each request, so if we change password, we also change version and all tokens
are revoked.

To implement this we have `RefreshOnlyAuthenticationHandler` which do all logic to check
whether user is authenticated or not.

Now for web we store token inside of cookie, so we need also use Custom Header Protection
to prevent CSRF.

## Custom Header Protection

`CustomHeaderProtectionMiddleware` requires custom header in all requests to secure endpoints.

Identifying whether endpoint requires authentication or not relies on `AuthorizeAttribute`
By the way `.RequireAuthorization()` adds AuthorizeAttribute to the endpoint,
so don't worry if you use Minimal Api.

## AllowAnonymous Ban

`AllowAnonymous` is `banned` in our codebase. Because somehow it overrides Authorize
attribute in any position. So I created one more class AllowAnonymousAttribute in same
namespace as existing and throw error in it constructor. So you can't start application
if you use AllwoAnonymous.
