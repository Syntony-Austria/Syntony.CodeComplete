# Runbook: the mail key is out

Trigger: the key turns up in a commit, a log, a ticket or a report from outside.

## Roles (one person each, even if there are only two of you)

- Lead: keeps the whole picture, decides, keeps the log. Does not type.
- Hands: changes the key and checks the result.
- Word: tells the team and the mail provider; asks who else must know.

## Steps

1. Note the time. From now on every action goes into the log with its time.
2. Can the key read personal data? Involve data protection now. In the EU, the
   supervisory authority must hear of it without undue delay, where feasible
   within 72 hours (GDPR Art. 33), unless it is unlikely to result in a risk.
3. Create a second key at the mail provider. Do not revoke the old one yet.
4. Put the new key into the secret store. Restart the mail service.
5. Check: a test mail goes out, the log shows the new key name.
6. Revoke the old key at the provider. Only now.
7. Ask the provider what was sent with the old key, and since when.
8. Close the leak: file, history, log. Rotating comes first, deleting second.
9. Review within a week: how did it get out, and which check was missing?

Last drill: none yet. Next drill: write a date here, then keep it.
