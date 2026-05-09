# Resend Email Setup

This backend uses Resend as the only transactional email provider.

## Required configuration

- `Resend:ApiKey`
- `Resend:FromEmail` (verified sender/domain in Resend)
- `Resend:FromName` (optional, defaults to `MiNegocioCR`)

Environment variables supported:

- `RESEND_API_KEY`
- `Resend__FromEmail`
- `Resend__FromName`

## Notes

- `POST /api/auth/test-email` sends using `IEmailService` (Resend implementation).
- Forgot password emails are sent by `SendPasswordResetEmail(...)` through Resend.
- Do not commit real API keys in `appsettings.json` or `launchSettings.json`.
