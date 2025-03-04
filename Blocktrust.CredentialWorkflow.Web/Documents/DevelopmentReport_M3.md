# Development Report for Milestone 3

The focus of this milestone was primarily around the Validation Action and continued testing and refinement. Since we already completed more components in the last milestone than initially planned, this milestone could be completed in relatively short succession. These are components that are currently working. For Triggers:
- The different variants of the HTTP trigger,
- The recurring Timer (not demoed or explained yet),
- The Form Trigger (also not demoed or explained yet),
- The Wallet-Interaction (DIDComm) Trigger (new).
- The manual Trigger isn’t implemented yet.

For the actions, we currently support:
- Issuing of W3C Credentials (demoed in the last milestone),
- The Verification of W3C Credentials (new),
- The Validation of W3C Credentials (new),
- The Custom Validation (new),
- DIDComm Messages (demoed in the last milestone),
- Email (new).

A few Actions are not implemented yet but are planned as next steps:
- The HTTP-Request Actions,
- DID Resolution.

And some Actions are not implemented yet, which in general still pose questions on their usefulness and/or implementation details, like:
- DID Updates (creating, updating, or deleting did:prism DIDs),
- OOB (Out-of-band connections),
- AnonCreds and SD-VCs,
- and maybe JWT-Tokens apart from JWT-VCs.

While they are all technically possible, the question lies in actual use cases. Just implementing things because we put them in the upcoming milestone is possible but somewhat questionable. While we do believe that there is a use case for these Actions, it is still unclear how someone would indeed like to use them. This is a question that came up multiple times over the project overall, even with the existing elements: The current state of interest in SSI projects in general is not very high and clearly has decreased since the “restructuring” of the Atala PRISM team. This has also had an indirect effect on this project, since we don’t see many potential users and, therefore, use cases right now inside the Cardano ecosystem. With DID-based solutions not being pushed by IOG and their very slow progress toward DID integration into the Lace Wallet, interest is declining, and therefore, ideas for useful integrations are too. While we still can complete the technical requirements and goals we have set for ourselves, it is a bit of a blind flight for very specific features. A good example of this is the DIDComm integration. We have been working with DIDComm protocols for over 2.5 years now, and we have a good understanding of it. But to demo it, we still rely on a wallet we completed over 1.5 years ago because all other ways of demoing it in a visual and somewhat understandable way are either lackluster or nonexistent. The result is that we can’t target known use cases with our implementation. A future, potentially popular use case might require a very specific detail we just don’t know yet, or users would have to make some workaround on their end to call our platform based on the very specific inputs or outputs we require. Since we obviously can’t accommodate every potential future use case and detail, we have to make constant assumptions about what one might need. Sometimes they are easy and obvious; in other cases, these are hunches. In any case, the development would have been more straightforward if the demand for DID-based solutions had been higher already, so we could simply react to market demand. This way, we’ll likely have to adjust more at some later stage when the project is already completed.

Nonetheless, we completed all the technical requirements for this milestone, with hopefully reasonable assumptions about how the platform could be used. An example of this issue is the Validation Actions: We assume that the potential user would like to verify a W3C VC (as in the acceptance criteria). What might be the case is that the user would like to verify an OpenBadge 3.0 Credential (which is based on the W3C VC Data Model 2.0) but has additional properties. Doing this with the current Validation Action is mostly possible but, depending on the case, quite complicated. There are also surely things that can’t be validated with the current patterns. If we knew that most of the possible future use cases would be around Open Badges, we could already shift the focus to that. So overall, it is very much a balancing act between what we have written in the acceptance criteria, what we see as potential demand, and how much it makes sense to invest in technical edge cases one might never use.

From a technical perspective, we are generally satisfied with the architecture and the UI, but there are still several issues we have to look into. The test coverage is overall okay, but we still anticipate a wide range of errors that will occur with real data. Since this platform processes a lot of unfiltered user input, there are many checks to perform to make sure that data is in an expected state and doesn’t cause crashes along the way when multiple actions are chained.

We hope these comments give a bit of insight into what we are currently doing and how things are progressing. We expect the next milestone will also be completed rather quickly within the next 4-8 weeks.

