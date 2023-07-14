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

Now for web we store token inside of cookie, so we need also use Double Submit Cookie to
prevent CSRF. Just send same value inside of cookie and header.

## Double Submit Token

`DoubleSubmitTokenMiddleware` requires double submit token for all endpoints that require
authentication.
Identifying whether endpoint requires authentication or not relies on `AuthorizeAttribute`
and `AllowAnonymousAttribute`. **So you need to use them.** By the way `.RequireAuthorization()`
adds AuthorizeAttribute to the endpoint, so don't worry if you use Minimal Api.
