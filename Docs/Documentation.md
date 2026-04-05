# Documentation

## AI Agent Context Docs

- Documentation used to provide further context for AI agents lives in `/Docs/AgentContext/*`.
- For example, `/Docs/AgentContext/2026-03-06-mvp-connected-device-loop.md`).
- Only meant to be read by AI agents and do NOT need to be written in a way that is easily digestible by humans.
- They should be focused on providing all of the necessary context for the agent to complete the associated Wave/Feature/Task.
- Make use of markdown links to other docs and resources as much as possible to avoid unnecessary repetition and to ensure that the agent has access to all relevant information.
- As plans or designs changed and/or tasks are completed/updated, these docs should be updated to reflect the current state of the project. This is important to ensure that AI agents have the most up-to-date information when working on their assigned tasks.
- Organizational subfolders can and should be used if multiple context documents are related to a single logical unit (e.g Wave/Feature/Task/etc).
- Record context, options considered, decisions made, rationale, unresolved questions, next steps, etc.; any/all information that will help an AI agent make informed decisions and implementations.
## Planning Docs

- Human-facing documentation for things like brainstorming sessions, active design discussions, Wave/Feature/Task implementation plans, etc. live in `/Docs/Planning/*`.
- Meant to contain information about **known upcoming or active, in-progress** Waves/Features/Tasks that are not contained to single GitHub issues/PRs, but are more overarching, long-term.
- Should be kept up-to-date as the Wave/Feature/Task progresses in order to reflect the most current state of the in-progress planning/status.
## Truth Docs

- High level documentation about the app, its design (UX and technical), documentation, etc. lives in `/Docs/Truth/*`.
- Meant to be the **official and reigning** sources of truth for current expected app and technical design, user experience behavior, art direction, etc.
- Should not be updated during planning/brainstorming phases or discussions; only update once a final ruling decision has been agreed upon.
- If any conflicts exist between docs in and outside of `/Docs/Truth/*`, docs inside the folder should be adhered to.
### App Design Doc

- Also referred to as ADD
- Ruling source of truth for app features and expected behavior, UI wireframes and mockups, etc..
- Should be *extremely* detailed as it is the main source that developers will reference for implementation of features and systems.
- Can break out mechanics and systems into separate documents if necessary to keep the document size down; any such linked documents should have the same expectations for level of detail and be considered of the same authority as this doc.
### Technical Design Doc
- Also referred to as TDD
- Ruling source of truth for the app's high-level technical and architecture design and choices.
- Should outline the app's tech stack
- Should describe reasonings and choices for the chosen technologies, software, and versions and provide guidance/rules/expectations for any upgrade plans.
## Developer Docs

- Documentation for human developers lives in `/Docs/Dev/*`.
- Meant to contain lower-level information about the current development state of the project; for example: full details about architecture, developer environment setup instructions, supplemental documentation when inline code docs (e.g. C# XML, JSDoc) are not sufficient, coding standards and best practices, etc.
- Should be written in a way that is easily digestible by humans and should provide clear and concise instructions.
## Historical Context Docs

- Documentation for historical information lives in `/Docs/HistoricalContext/*`.
- Meant to contain any documentation that is now either obsolete, contains completed planning/brainstorming, etc. but is still desired to keep active for historical context.
- Not everything needs to be kept; most planning and supplemental docs for Waves/Features/Tasks/etc. should just be deleted after the final decisions have been added to the **Truth** docs and/or the Wave/Feature/Task has been completed, as applicable; their history is still available then through version control history. Docs should only be moved here for convenience of not having to dig too much for decisions on recurring questions.
- Mostly here for AI agents, but also useful for humans.
- Docs should only be **moved** here as they get phased out; no new docs should be created here.