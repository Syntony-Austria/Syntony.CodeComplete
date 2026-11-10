# Runbook: the mail key is out

Trigger: the key turns up in a commit, a log, a ticket or a report from outside.

## Roles (one person each, even if there are only two of you)

- Lead: keeps the whole picture, decides, keeps the log. Does not type.
- Hands: changes the key and checks the result.
- Word: tells the team and the mail provider; asks who else must know.

## Steps

1. Note the time. From now on every action goes into the log with its time.
2. Create a second key at the mail provider. Do not revoke the old one yet.
3. Put the new key into the secret store. Restart the mail service.
4. Check: a test mail goes out, the log shows the new key name.
5. Revoke the old key at the provider. Only now.
6. Ask the provider what was sent with the old key, and since when.
7. Close the leak: file, history, log. Rotating comes first, deleting second.
8. Review within a week: how did it get out, and which check was missing?

Last drill: none yet. Next drill: write a date here, then keep it.
