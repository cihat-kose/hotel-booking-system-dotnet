# Pre-publication audit

**Decision: keep the repository private.** This revision does not grant document rights, remove historical copies, rewrite history, add a license, merge into `master`, or change visibility.

## Scope and method

Baseline: `9a92cd7588565bc6b963c891f3001c0c50defa5e`. Remote heads/tags were enumerated with `git ls-remote --heads --tags origin` after fetch: only `master`, no tags. All 12 reachable commits and all 26 unique file blobs were inspected, including earlier `HotellBookingSystem` paths, deleted/renamed files, author/committer metadata and messages. Code and documentation variants were read; secret/private-key/token, email and long-number patterns were checked without copying sensitive values into this report. Both unique PDFs were text-extracted and their metadata/attachments checked; the assignment's three embedded image objects were also inspected. No embedded attachments were present. `git fsck --full` reported no additional dangling objects.

This covers repository Git content and the proposed branch, not an assurance about GitHub caches, other people's clones, external backups or all possible secret formats. No automated pattern scan can prove absence of secrets or establish copyright permission.

## Findings

| Finding | Evidence and assessment | Required action before public release |
| --- | --- | --- |
| School assignment PDF | Nine-page school-authored specification with teacher attribution, course/dates, school branding and assessment/submission instructions. No redistribution grant found. Blob `2ef6199e6f3189c750dc3b4127ff586ab795d681`, present since the initial commit under renamed paths. | Removed from this branch's current tree. Obtain permission or remove every historical occurrence before publication. Keep a private submission archive. |
| Personal Git identity | Author and committer records contain the owner's personal email and name throughout baseline history. Address deliberately not repeated here. | Owner must decide whether that historical identity may be public; otherwise rewrite author/committer email to the verified GitHub noreply identity. New portfolio commits use noreply. |
| Payment-like literals | Three card-like literals and one phone-like literal occur in original test fixtures. Their repetitive structure and context suggest demo data, but ownership cannot be proven from code. Old Vipps output also displayed its full input; short card input could be displayed in full. | Current code collects/stores no card/phone data and fixtures contain none. Confirm historical literals are synthetic or remove old test blobs during a coordinated history cleanup. Do not paste values into public reports. |
| Example guest data | Names are Norwegian sample names; all code/test email addresses use `example.com`. | Current app explicitly labels fixtures as demo data. No confirmed real guest/customer dataset found. |
| Original UML PDF | One generated class diagram, no personal/payment values found. Original AI record describes UML generation. It lists obsolete payment fields/public mutations. | Removed as stale output and replaced by editable `architecture.md`; historical copy remains. No claim is made about third-party redistribution rights beyond inspection. |
| Credentials | No API tokens, passwords, private keys, connection credentials or confirmed real payment records found in inspected blobs. | Recheck the final refs immediately before publication; rotate/revoke any real credential if later discovered. |
| License | No project license or school-document redistribution grant was present. | No license added by assumption. Owner chooses terms only for material they have the right to license. |

## File/reference decisions

References were searched before removal. The application README linked the assignment PDF, old UML and `AI_Prompts.md`; earlier root READMEs also contained broken `HotellBookingSystem` links. The root README now links current architecture and AI disclosures. The assignment requires a UML image/PDF and complete AI prompt documentation for **submission**, but does not require publishing the school's own specification on GitHub. This portfolio is not presented as a fresh assessment submission; Mermaid and the short AI summary do not claim to satisfy those exact submission formats.

- Consolidated both READMEs into one root README; removed the nested copy after replacing its content/links.
- Replaced original `AI_Prompts.md` with the owner's requested short `docs/ai-usage.md`. Original disclosure remains recoverable from the baseline commit; preserve it privately if history is rewritten.
- Removed school assignment PDF and obsolete UML PDF from this branch after checking references and requirements.
- Removed `SimpleTests.cs` only after migrating its five scenarios; startup invocation is replaced by a normal entry point and independent tests.
- No current source/document links depend on the old GitHub repository URL. Historical URLs/names are not rewritten by a repository rename.

## Safe resolution and impact

Deleting files in this PR only removes them from the latest tree. **Merging this PR and switching visibility would still expose old PDFs, test literals and commit email.** Renaming the repository does not change this.

Preferred low-risk option: keep this full submission/history private and publish a separate clean-history portfolio snapshot after owner review, retaining student attribution and a truthful AI disclosure. That loses public access to the original commit history but avoids force-pushing or exposing the school PDF.

If the same repository must become public: first create a private backup; then plan a coordinated history rewrite across **every branch/tag**, including this PR branch and all historical filename variants. Remove the assignment PDF by blob/path, resolve payment-like fixtures, and optionally replace author/committer emails. Re-audit from a fresh clone. This changes commit IDs, requires force-pushing/recloning, may invalidate signatures and disrupt PR references. GitHub PR refs/caches and existing clones can retain old content; contact GitHub Support where necessary. Do not perform that destructive rewrite as part of this PR.

Public visibility remains a separate final owner-approved step after these findings are resolved. A repository rename and description update are compatible with keeping the repository private.
