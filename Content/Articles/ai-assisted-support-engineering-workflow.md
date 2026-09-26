---
title: "How I Built an AI-Assisted Support Engineering Workflow for Troubleshooting Production Websites Safely"
subtitle: "A Systems-First Approach to Legacy Web Platforms, Local Case Isolation, Immutable Baselines, and Human-Gated Deployment"
date: "2026-09-26T22:30:00Z"
author: "Rodney A. Chery"
tags: ["Support Engineering", "PowerShell", "WinSCP", "Classic ASP", "Git", "Agentic Architecture", "SRE", "Web Performance"]
description: "A comprehensive systems architecture review of how I engineered an isolated, reproducible, AI-assisted troubleshooting workflow for production dealership websites using PowerShell, WinSCP, Git, and filesystem-based agent orchestration."
---

# How I Built an AI-Assisted Support Engineering Workflow for Troubleshooting Production Websites Safely
### *A Systems-First Approach to Legacy Web Platforms, Local Case Isolation, Immutable Baselines, and Human-Gated Deployment*

---

## 1. Introduction

In enterprise web operations, production support engineering is frequently treated as an emergency triage desk rather than a disciplined software engineering discipline. Nowhere is this tension more acute than in high-volume, multi-tenant digital dealership platforms. 

When supporting hundreds of live automotive and commercial dealership websites, an engineer encounters a diverse architectural landscape: legacy Classic ASP (`.asp`) backends with deeply nested Server-Side Includes (`<!--#include file="..." -->`), ADODB database connections, COM object integrations, custom CSS/LESS stylesheets with conflicting cascade rules, bespoke jQuery plugins, dynamically rendered inventory Vehicle Detail Pages (VDP) and Search Results Pages (SRP), third-party tracking scripts, and third-party inventory syndication APIs.

When a customer reports an issue—such as a broken call-to-action button, a distorted mobile navigation menu, a missing document fee field on an inventory card, or an improperly rendered canonical link—the historical impulse across many support teams is ad-hoc, reactive intervention: open a remote SFTP client, browse the live server, edit an active file in place, save, refresh the browser, and hope nothing broke.

This article details the production support engineering architecture I designed and implemented to replace that reckless cycle. By combining deterministic PowerShell automation, the WinSCP .NET assembly, immutable filesystem baselines, local Git change tracking, structured filesystem-based agent orchestration (Interpretable Context Methodology), and strict human-in-the-loop deployment boundaries, I transformed ad-hoc troubleshooting into a deterministic, reproducible, and verifiable engineering pipeline.

Crucially, this system does not unleash an autonomous AI agent to manipulate production infrastructure. Instead, it embeds large language models (LLMs) strictly as read-only code reviewers and analytical assistants within a rigid, human-gated state machine.

---

## 2. Why Production Support Needed an Engineering Workflow

The traditional, undisciplined approach to production web support can be summarized as:

```
Remote Server ──► In-Place File Edit ──► Remote Save ──► Hard Refresh ──► "Hope It Works"
```

In modern software reliability engineering (SRE), this pattern is considered an anti-pattern fraught with critical operational hazards:

1. **Catastrophic In-Place Failure:** An accidental syntax error in a shared Classic ASP header or a single unclosed brace in a global LESS stylesheet immediately triggers a 500 Internal Server Error or an unstyled layout across the entire dealership domain.
2. **Irreversible State Loss:** Remote files edited in place overwrite the only existing copy of the code on disk. If the change breaks rendering in an unexpected browser viewport, there is no one-command rollback mechanism.
3. **Hidden Cascade & Blast Radius:** Legacy platforms rely heavily on shared assets. A stylesheet modified to fix padding on a contact form may silently collapse the grid on 2,500 active Vehicle Detail Pages. Without local diffing and dependency tracing, the blast radius is unknowable until customers complain.
4. **Environment Confusion:** When juggling multiple browser tabs and SFTP sessions, it is shockingly easy for an engineer to intend to edit a staging environment (`Stage`) but inadvertently upload changes directly to the live cluster (`Live`).
5. **Epistemic Drift & Hallucination Risk:** When an engineer pastes raw, fragmented snippets into a chat-based LLM without architectural boundaries, the AI hallucinates explanations based on incomplete context, suggesting sweeping refactors that break undocumented server dependencies.
6. **Zero Auditability and Forensic Traceability:** When a ticket closes, there is no durable, version-controlled artifact showing the exact lines changed, the baseline state prior to intervention, the specific diagnostic evidence uncovered, or the human justification for the patch.

Production troubleshooting should not begin with editing production files. Every support ticket must become an isolated, reproducible, version-controlled, and verifiable engineering workspace.

---

## 3. System Architecture

The workflow decouples into two strict operational planes: the **Control Plane** (which handles ticket metadata, environmental verification, confirmation gates, and agentic stage boundaries) and the **Data Plane** (which handles the actual website assets, scripts, stylesheets, and markup).

### High-Level Architectural Flow

```
                         ┌────────────────────────┐
                         │  Support Ticket / Case │
                         └───────────┬────────────┘
                                     │
                                     ▼
                         ┌────────────────────────┐
                         │ PowerShell Case Loader │
                         └───────────┬────────────┘
                                     │
                    Explicit environment confirmation
                                     │
                                     ▼
                         ┌────────────────────────┐
                         │      SFTP / WinSCP     │
                         │ Remote Source Retrieval│
                         └───────────┬────────────┘
                                     │
                                     ▼
                    ┌─────────────────────────────────┐
                    │ Downloaded Local Source Snapshot│
                    └──────────────┬──────────────────┘
                                   │
                    ┌──────────────┴───────────────┐
                    │                              │
                    ▼                              ▼
        ┌─────────────────────┐         ┌─────────────────────┐
        │ Immutable Baseline  │         │ Active Case Workspace│
        └─────────────────────┘         └──────────┬──────────┘
                                                   │
                                                   ▼
                                        ┌─────────────────────┐
                                        │ Git Repository       │
                                        │ Initial Baseline     │
                                        └──────────┬──────────┘
                                                   │
                                                   ▼
                                     ┌──────────────────────────┐
                                     │ Agentic Stage Workspace  │
                                     └──────────┬───────────────┘
                                                │
           ┌────────────────────────────────────┼─────────────────────────────────┐
           │                                    │                                 │
           ▼                                    ▼                                 ▼
       Intake                             Investigation                      Validation
           │                                    │                                 │
           ▼                                    ▼                                 ▼
      Reproduction ──► Root Cause ──► Remediation Plan ──► Implementation ──► Diff Review
                                                                                  │
                                                                                  ▼
                                                                    Human Deployment Decision
                                                                                  │
                                                                                  ▼
                                                                        Approved Remote Change
```

### Tripartite Operational Separation

The core philosophical foundation of the system is a strict division of responsibilities:

1. **Deterministic Automation (PowerShell & WinSCP):** Executes all mechanical, repetitive, and safety-critical operations—directory scaffolding, SSH host key validation, SFTP transport, baseline copying, Git repository initialization, and workspace metadata generation. Automation never makes heuristic judgments.
2. **Interpretive Analysis (AI / LLMs):** Acts as a read-only codebase investigator, dependency tracer, structural analyst, and diff reviewer. The AI operates within sandboxed directory scopes with explicit instructions.
3. **Authority and Judgment (The Support Engineer):** The human engineer retains exclusive control over case classification, hypothesis confirmation, actual code modifications, testing validation, and remote production deployment decisions.

---

## 4. Trust Boundaries

Establishing unambiguous trust boundaries prevents accidental data corruption and cognitive confusion during high-pressure outages.

```
┌────────────────────────────────────────────────────────────────────────┐
│                        AUTHORITATIVE REMOTE ZONE                       │
│                                                                        │
│   Production SFTP Host (Live)               Staging SFTP Host (Stage)   │
│   ┌───────────────────────────┐             ┌────────────────────────┐ │
│   │ /WebSites/dealership-live │             │ /WebSites/dealership-stg││
│   └─────────────┬─────────────┘             └────────────────────────┘ │
└─────────────────┼──────────────────────────────────────────────────────┘
                  │ (Read-Only Secure SFTP via WinSCP .NET)
                  ▼
══════════════════════════════════════════════════════════════════════════
               LOCAL ISOLATION BOUNDARY (No Automatic Sync)
══════════════════════════════════════════════════════════════════════════
┌────────────────────────────────────────────────────────────────────────┐
│                      DISCONNECTED LOCAL CASE ZONE                      │
│                                                                        │
│   ┌───────────────────────────┐             ┌────────────────────────┐ │
│   │   Immutable Baseline      │             │  Active Case Workspace │ │
│   │   (Read-Only Reference)   │             │  (Local Git Repo)      │ │
│   └───────────────────────────┘             └───────────┬────────────┘ │
│                                                         │              │
│                                                         ▼              │
│                                             ┌────────────────────────┐ │
│                                             │ AI Analysis Sandbox    │ │
│                                             │ (Scoped FS Stages)     │ │
│                                             └────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

### Core Axiom: `LOCAL COPY != PRODUCTION`

1. **Local Edits Do Not Touch Production:** The downloaded snapshot in the `site/` folder is completely severed from the remote server. Modifying a file locally transmits zero packets across the network.
2. **Local Success Does Not Guarantee Production Success:** A layout fix that renders cleanly under a local test harness may behave differently in production due to upstream reverse proxies, CDN edge caching, server-side cookies, or dynamic database states.
3. **Local Failure Does Not Disprove Production Issues:** If an engineer cannot immediately run a Classic ASP script locally due to a missing COM component, that failure does not invalidate the dealer's complaint; it merely indicates an environment dependency.

---

## 5. PowerShell Control Plane

To eliminate human error during initialization, the workflow is driven by dedicated PowerShell automation. Rather than having a single script with a `-Environment` flag (which invites muscle-memory execution errors), the tooling is split into distinct commands: `Get-LiveSite.ps1` and `Get-StageSite.ps1`.

### Case Bootstrap Invocation

```powershell
cd "C:\SupportTools\SiteWorkflow"

.\Get-LiveSite.ps1 `
    -CaseNumber "CASE-01700000" `
    -ClientName "dealer-acme" `
    -RemotePath "/WebSites/dealer-acme" `
    -OpenVSCode
```

### Argument Verification & Namespace Isolation

- **`-CaseNumber`**: Provides 1:1 correlation with the CRM/ticketing platform (e.g., Salesforce or Jira).
- **`-ClientName`**: Provides human-readable namespace isolation.
- **`-RemotePath`**: Identifies the authoritative root directory on the remote cluster.
- **`-OpenVSCode`**: Integrates the bootstrapped case workspace directly into the active editor instance upon completion.

The local case workspace is deterministically named using a composite identifier: `CASE-01700000__dealer-acme`. Using either the case number or client name alone creates collision vulnerabilities: a case number alone provides zero human-readable context during multi-case root-cause comparisons, while a client name alone collides when the same dealer submits multiple tickets across different months.

### The Operational Intent Confirmation Gate

Upon invocation, `Get-LiveSite.ps1` halts execution and requires the engineer to type an explicit verification token:

```
[SECURITY WARNING] You are targeting the LIVE PRODUCTION environment for dealer-acme.
Authoritative Remote Path: /WebSites/dealer-acme
To proceed with source retrieval, type 'DOWNLOAD-LIVE': DOWNLOAD-LIVE
```

This gate is intentionally disruptive. Authentication proves *who the operator is*; confirmation verifies *what the operator intends to do*. It stops accidental script execution resulting from clipboard mistakes, terminal history replays (`Up-Arrow + Enter`), or environment confusion.

---

## 6. SFTP and WinSCP Data Transfer Layer

Rather than relying on external command-line utilities with fragile string-based stdout parsing, the transport layer embeds the **WinSCP .NET Assembly** (`WinSCPnet.dll`).

```powershell
# Structural WinSCP .NET Session Configuration
Add-Type -Path "C:\SupportTools\WinSCP\WinSCPnet.dll"

$sessionOptions = New-Object WinSCP.SessionOptions -Property @{
    Protocol              = [WinSCP.Protocol]::Sftp
    HostName              = $Global:TargetSftpHost
    UserName              = $Credentials.UserName
    SecurePassword        = $Credentials.Password
    SshHostKeyFingerprint = "ssh-ed25519 255 SHA256:4x9...[VERIFIED_KEY]..."
}

$session = New-Object WinSCP.Session
$session.Open($sessionOptions)
```

### SSH Host Key Verification vs. Credential Authentication

A critical security principle implemented in this layer is strict host key fingerprint pinning. Authentication credentials prove the client's identity to the server; the SSH host key fingerprint proves the server's identity to the client. Pinning the fingerprint directly in the session configuration guarantees that traffic cannot be intercepted by an unauthorized intermediary or misconfigured gateway.

Under no circumstances are plain-text passwords or private keys stored in `.ps1` files, configuration dictionaries, or AI instructions. Credentials are gathered at runtime via secure OS dialogs (`Get-Credential`) and retained only in volatile memory during the transfer session.

### The Physics of the 20,000-File Transfer

A legacy dealership website often contains 15,000 to 30,000 discrete files: nested includes, localized graphics, inventory icons, and CSS stylesheets. Transferring 20,000 files totaling 800 MB takes significantly longer than transferring a single 800 MB zip archive. 

Each file transfer over SFTP requires:
1. SSH channel packet negotiation
2. Remote directory lookup and attribute retrieval (`SSH_FXP_LSTAT`)
3. File handle allocation (`SSH_FXP_OPEN`)
4. Data chunk packet acknowledgments
5. Handle closure and timestamp preservation (`SSH_FXP_CLOSE`)

Because `GetFiles()` operates synchronously, terminal output may appear unresponsive for several minutes. To verify ongoing data transfer without interrupting the process, engineers use separate telemetry queries:

```powershell
# External verification of active download directory
Get-ChildItem -Path "C:\Cases\CASE-01700000__dealer-acme\raw_download" -Recurse | 
    Measure-Object -Property Length -Sum
```

---

## 7. Download State Machine

The case bootstrapping lifecycle is managed as an explicit, unidirectional finite state machine.

```
       [ NEW ]
          │
          ▼
 [ ENVIRONMENT_VERIFIED ]
          │
          ▼
[ REMOTE_PATH_VERIFIED ]
          │
          ▼
 [ DOWNLOAD_APPROVED ]  ◄── (Confirmation gate: DOWNLOAD-LIVE)
          │
          ▼
   [ TRANSFERRING ]     ◄── (WinSCP SFTP synchronous stream)
          │
          ▼
 [ DOWNLOAD_COMPLETE ]
          │
          ▼
 [ BASELINE_CREATED ]   ◄── (Untouched copy saved to archive)
          │
          ▼
 [ GIT_INITIALIZED ]    ◄── (Initial baseline commit created)
          │
          ▼
[ AGENTIC_WORKSPACE ]   ◄── (Scaffold stages & context files)
          │
          ▼
[ READY_FOR_ANALYSIS ]
```

Enforcing strict state progression prevents invalid transitions:
- An engineer cannot begin **IMPLEMENTATION** without an initialized **GIT** repository and baseline.
- An interruption during **TRANSFERRING** fails closed, preventing partial, corrupted source trees from being promoted to a baseline.
- A failure during Git initialization halts the sequence before agentic stages can be scaffolded.

---

## 8. Immutable Baselines

Once the download is verified, the automation creates two identical copies before any tool, editor, or agent touches the filesystem:

```
Authoritative Remote Source
            │
            ▼
   Downloaded Raw Tree
            │
            ├──────────────────────► [ Immutable Baseline Archive ]
            │                        C:\Baselines\CASE-01700000__dealer-acme_BASELINE
            │
            ▼
 [ Active Case Workspace ]
 C:\Cases\CASE-01700000__dealer-acme\site
```

The **Immutable Baseline** is a read-only directory containing exclusively the downloaded website assets. It does not contain documentation, agent instructions, test scripts, or Git metadata.

### Why Dual Redundancy (Filesystem Baseline + Git Baseline) Matters

One might ask: *If the case is placed into a Git repository, why maintain a separate filesystem baseline?*

This represents deliberate defense-in-depth:
1. **Resilience Against Git Mistakes:** An engineer or automated tool experimenting with `git reset --hard` or `git clean -fdx` can accidentally purge untracked files or destroy local commits. An external, read-only filesystem baseline remains untouchable.
2. **Zero-Tooling Diffing:** Any standard visual diffing utility (Beyond Compare, WinMerge, VS Code diff) can immediately compare the active `site/` folder against the baseline folder without navigating Git reflogs.
3. **Forensic Integrity:** If a dispute arises regarding whether a bug was introduced during support remediation or was pre-existing in production, the timestamped immutable baseline provides definitive, tamper-evident proof of the production state prior to intervention.

---

## 9. Git as a Change-Control Layer

Inside the active case directory, the `site/` tree is immediately initialized as a standalone Git repository. The initial commit captures the raw, untouched state:

```powershell
Set-Location "C:\Cases\CASE-01700000__dealer-acme"
git init
git add site/
git commit -m "chore(baseline): initial production snapshot for CASE-01700000"
```

In this architecture, Git is not used as a remote collaborative code host (like GitHub or GitLab). It functions as a **local case-scoped change ledger**.

### The Diagnostic Power of Git Commands in Support

- `git status`: Instantly reveals whether any unexpected files were modified.
- `git diff --name-only`: Generates the exact list of modified files, ensuring that scope expansion is caught immediately.
- `git diff`: Provides line-by-line inspection of changes before any file is uploaded back to the dealership server.

```
Expected Scope:   site/includes/footer.asp
Observed Scope:   site/includes/footer.asp
                  site/css/global.less
                  site/web.config   <-- ALERT: Scope expansion detected!
```

If an editor plugin or language server automatically reformats `web.config` or touches `global.less`, `git status` flags the change immediately, preventing inadvertent production regressions.

---

## 10. Filesystem as Agent Architecture

A core innovation of this workflow is the concept of **Folder Structure as Agent Architecture**, derived from the Interpretable Context Methodology (ICM).

When developers attempt to use AI for large codebase troubleshooting, they often construct monolithic prompts: *"Here is our entire website and a ticket about a button bug. Fix it, test it, and tell me what changed."* This invariably fails due to context saturation, loss of precision, hallucinated dependencies, and uncontrolled changes.

Instead of a monolithic prompt, the case workspace is scaffolded into discrete engineering stages:

```
CASE-01700000__dealer-acme/
│
├── CONTEXT.md                  # Global Case Identification & Safety Invariants
├── CLAUDE.md                   # Agent Operational Boundaries & Tool Definitions
├── workspace.json              # Machine-readable environment metadata
├── .agentic-workspace          # Reentrancy marker
├── .git/                       # Local change tracking
│
├── site/                       # Active website codebase
│   ├── default.asp
│   ├── includes/
│   ├── css/
│   └── js/
│
├── case-input/                 # Raw ticket context from customer
│   ├── ticket.md
│   └── reproduction-notes.md
│
├── _config/                    # Corporate engineering guidelines
│   ├── support-rules.md
│   ├── safety-rules.md
│   ├── classic-asp-rules.md
│   └── testing-standards.md
│
└── stages/                     # Sequential analysis pipeline
    ├── 01_intake/
    │   ├── CONTEXT.md
    │   └── output/
    ├── 02_reproduction/
    │   ├── CONTEXT.md
    │   └── output/
    ├── 03_investigation/
    │   ├── CONTEXT.md
    │   ├── references/
    │   └── output/
    ├── 04_root-cause-analysis/
    │   ├── CONTEXT.md
    │   └── output/
    ├── 05_remediation-plan/
    │   ├── CONTEXT.md
    │   └── output/
    ├── 06_implementation/
    │   ├── CONTEXT.md
    │   └── output/
    ├── 07_validation/
    │   ├── CONTEXT.md
    │   └── output/
    └── 08_diff-review/
        ├── CONTEXT.md
        └── output/
```

In this architecture, the filesystem functions as:
1. **Workflow State:** The active stage directory reflects the exact progress of the case.
2. **Context Boundary:** An LLM executing analysis for a specific stage is provided *only* the context files relevant to that stage.
3. **Audit Trail:** Every stage deposits a markdown report into its `output/` directory, creating a permanent record of reasoning.
4. **Human Verification Barrier:** Moving from one stage to the next requires human review of the generated output.

---

## 11. Context Hierarchy

Context is structured hierarchically, mirroring lexical scope in computer science:

```
┌────────────────────────────────────────────────────────┐
│ Global Scope: Root CONTEXT.md & workspace.json          │
│ (Case ID, Client, Authoritative Source, Invariants)     │
└───────────────────────────┬────────────────────────────┘
                            │ Inherited by
                            ▼
┌────────────────────────────────────────────────────────┐
│ Operational Scope: CLAUDE.md & _config/ Rules          │
│ (Tool limits, Classic ASP constraints, Safety rules)   │
└───────────────────────────┬────────────────────────────┘
                            │ Inherited by
                            ▼
┌────────────────────────────────────────────────────────┐
│ Stage Scope: stages/XX_name/CONTEXT.md                 │
│ (Stage-specific goals, permitted actions, deliverables)│
└───────────────────────────┬────────────────────────────┘
                            │ Consumes
                            ▼
┌────────────────────────────────────────────────────────┐
│ Artifact Scope: output/ from Previous Stages           │
│ (Verified findings, reproduction steps, plan)          │
└────────────────────────────────────────────────────────┘
```

This strict scoping prevents context contamination. An agent tasked with Root Cause Analysis cannot skip ahead and start rewriting JavaScript files because the `04_root-cause-analysis/CONTEXT.md` explicitly denies file-write capabilities.

---

## 12. The Eight Troubleshooting Stages

| Stage | Name | Input | Primary Objective | Output Deliverable | Write Permitted? |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **01** | **Intake** | `case-input/ticket.md` | Synthesize symptom, affected URL, device scope, and customer expectations into a formal engineering problem statement. | `01_intake/output/case-summary.md` | **NO** |
| **02** | **Reproduction** | `case-summary.md`, `site/` | Attempt local reproduction across desktop, tablet, and mobile viewports; document environmental blockers. | `02_reproduction/output/reproduction-report.md` | **NO** |
| **03** | **Investigation** | `reproduction-report.md`, `site/` | Trace code execution: map `#include` trees, inspect CSS specificity cascades, identify event listeners. | `03_investigation/output/investigation-findings.md` | **NO** |
| **04** | **Root Cause Analysis** | `investigation-findings.md` | Formulate a verifiable causal chain explaining *why* the defect occurs; cite line numbers and DOM nodes. | `04_root-cause-analysis/output/root-cause-analysis.md` | **NO** |
| **05** | **Remediation Plan** | `root-cause-analysis.md` | Design the smallest possible safe patch; define blast radius, regression risks, and rollback instructions. | `05_remediation-plan/output/remediation-plan.md` | **NO** |
| **06** | **Implementation** | `remediation-plan.md`, `site/` | Apply the approved patch to the local codebase. Enforce surgical edits; forbid unrelated refactoring. | `06_implementation/output/implementation-notes.md` | **YES (Local Only)** |
| **07** | **Validation** | `remediation-plan.md`, `site/` | Test the fix against the primary defect; execute regression tests across responsive breakpoints and pseudo-states. | `07_validation/output/validation-results.md` | **NO** |
| **08** | **Diff Review** | `git diff`, `git status` | Perform automated second-engineer code review; audit scope alignment, syntax safety, and unintended modifications. | `08_diff-review/output/diff-review.md` | **NO** |

---

## 13. Evidence-Based AI Analysis

To maintain technical credibility, AI models operating within the investigation and RCA stages must follow an **Evidence-Based Reasoning Protocol**.

### The Epistemic Classification Rule

The model must categorize every statement into one of four epistemic tiers:
1. **CONFIRMED:** Directly verified in code or DOM evidence (e.g., *"Line 42 of `includes/nav.asp` sets `class='mobile-hidden'`"*).
2. **INFERRED:** Logically deduced from code structure but not directly observable in static assets (e.g., *"The variable `session("DealerRegion")` appears to dictate whether the pricing banner renders"*).
3. **HYPOTHESIS:** A plausible explanation that requires runtime testing to confirm.
4. **UNKNOWN:** Missing information, such as server-side database records, IIS URL Rewrite rules, or external CDN configurations.

This protocol prevents hallucinations from masquerading as factual findings, allowing the support engineer to immediately identify assumptions that require manual validation.

---

## 14. Human Authority and Least Privilege

AI agents within this architecture are bounded by strict capability constraints:

```
┌────────────────────────────────────────────────────────┐
│                      READ AUTHORITY                    │
│   AI may inspect local files, parse ASTs, and analyze   │
│   templates within the local case directory.            │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                     WRITE AUTHORITY                    │
│   AI may ONLY write code locally during Stage 06       │
│   (Implementation), under explicit human supervision.   │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                    REMOTE AUTHORITY                    │
│   ABSOLUTELY ZERO. AI cannot connect to SFTP, deploy   │
│   code, modify production files, or alter live data.   │
└────────────────────────────────────────────────────────┘
```

The system operates under the principle of **least privilege**: an agent is granted only the permissions necessary for its current stage. If an agent is performing root cause analysis, write access to the filesystem is revoked. Remote production deployment is an exclusively human operational act.

---

## 15. Classic ASP Local Reproduction

A significant technical hurdle when supporting legacy web platforms is that modern frontend developer servers (such as VS Code Live Server or Vite) cannot execute Classic Active Server Pages (`.asp`).

```
Request: http://localhost:5500/default.asp
Live Server Response: Raw ASP source code delivered to browser!
Result: <% @Language="VBScript" %> rendered as plain text; includes unparsed.
```

To achieve functional local execution, the architecture accommodates local **Internet Information Services (IIS)** or **IIS Express** configured with the ASP feature enabled:

```
Browser ──► localhost:8080 ──► IIS / IIS Express
                                     │
                                     ▼
                              asp.dll (ISAPI)
                                     │
                  ┌──────────────────┴──────────────────┐
                  ▼                                     ▼
        VBScript Runtime Engine               Server-Side Includes
       (Executes <% ... %> code)             (<!--#include file="..." -->)
                  │                                     │
                  ▼                                     ▼
        Rendered HTML / CSS / JS Delivered to Browser DOM
```

### Navigating Legacy Server Dependencies

When full local IIS execution is obstructed by missing external dependencies (e.g., custom COM dlls, proprietary database drivers, internal middleware), the workflow categorizes issues into:
- **Client-Side Reproducible:** Issues rooted in CSS specificity, LESS compilation, DOM manipulation, responsive breakpoints, or vanilla JavaScript. These are 100% testable locally using static mock wrappers.
- **Environment-Dependent:** Issues requiring live database states, server variables (`Request.ServerVariables("HTTP_X_FORWARDED_FOR")`), or proprietary payment gateways. These are investigated statically locally and validated in a dedicated staging environment prior to production deployment.

---

## 16. Frontend and LESS Investigation

In dealership website platforms, CSS defects frequently stem from **selector specificity collisions** and **LESS inheritance overrides**.

### The Specificity Trap

A common support request involves a call-to-action button failing to display the dealer's requested brand color:

```css
/* Deeply nested platform rule in global.less */
#header .header-container .header-contact .header-phone .header-phone__link:hover {
    color: #333333 !important;
}
```

An inexperienced engineer might attempt to fix this by adding another `!important` rule to a localized stylesheet:

```css
/* Anti-Pattern: Specificity War */
.dealer-custom-phone a {
    color: #ff0000 !important;
}
```

Because the platform stylesheet has higher specificity (`1-3-0` vs `0-1-1`), the local rule is ignored, leading to developer frustration.

### Safe Override Methodology

During **Stage 03 (Investigation)**, the engineer and AI inspect the compiled CSS cascade via browser developer tools and trace it back to the source `.less` definitions. The **Remediation Plan (Stage 05)** mandates:
1. Target the exact component scope without inflating global specificity.
2. Mirror the necessary parent container selector without using `!important` unless overriding an immutable vendor asset.
3. Validate responsive breakpoint behavior across standard automotive mobile breakpoints (`320px`, `375px`, `414px`, `768px`, `1024px`, `1280px`).

---

## 17. Dynamic DOM and Inventory Systems

Dealership inventory search pages (SRP) and vehicle detail pages (VDP) rarely exist as static HTML. They rely on asynchronous DOM rendering engines that dynamically populate vehicle specifications, pricing summaries, and disclaimers.

### The Idempotency Principle in Support Scripts

When a ticket requires injecting custom presentation logic (such as repositioning a documentation fee or displaying an estimated monthly payment), support scripts must be **idempotent**—they must produce the exact same outcome whether executed once or ten times.

Because dynamic inventory platforms may re-render sections of the DOM during filtering or pagination, support scripts must guard against duplicate element injection:

```javascript
// Idempotent DOM Injection Pattern
(function() {
    const COMPONENT_CONTAINER_ID = 'rc-custom-doc-fee-display';
    
    function injectDocFee() {
        // Guard clause: Prevent duplicate injection
        if (document.getElementById(COMPONENT_CONTAINER_ID)) {
            return;
        }

        const sourceFeeElement = document.querySelector('.liUnit.LiInvdoc-fee .unitValue');
        const targetContainer = document.querySelector('.pricing-summary-card');

        if (!sourceFeeElement || !targetContainer) {
            return; // Target not yet available in DOM
        }

        const feeValue = sourceFeeElement.textContent.trim();
        
        const feeElement = document.createElement('div');
        feeElement.id = COMPONENT_CONTAINER_ID;
        feeElement.className = 'custom-fee-line-item';
        feeElement.innerHTML = `
            <span class="fee-label">Documentation Fee:</span>
            <span class="fee-amount">${feeValue}</span>
        `;

        targetContainer.appendChild(feeElement);
    }

    // Execute on DOMContentLoaded and observe dynamic AJAX re-renders
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', injectDocFee);
    } else {
        injectDocFee();
    }

    const observer = new MutationObserver(function() {
        injectDocFee();
    });

    const targetNode = document.getElementById('inventory-detail-root');
    if (targetNode) {
        observer.observe(targetNode, { childList: true, subtree: true });
    }
})();
```

---

## 18. Idempotency and Failure Recovery

A reliable automation script must be safely re-executable without corrupting previously initialized state.

### Workspace Reentrancy Protection

If `Get-LiveSite.ps1` is executed against an existing case workspace, it checks for the presence of the `.agentic-workspace` marker:

```powershell
$WorkspaceMarker = Join-Path $CaseDirectory ".agentic-workspace"

if (Test-Path $WorkspaceMarker) {
    Write-Warning "Case workspace already exists: $CaseDirectory"
    $response = Read-Host "Workspace detected. (O)pen in VS Code, (R)esume transfer, or (A)bort? [O/R/A]"
    switch ($response.ToUpper()) {
        "O" { code $CaseDirectory; exit 0 }
        "R" { Resume-Transfer -CaseDir $CaseDirectory }
        default { Write-Host "Aborted."; exit 1 }
    }
}
```

### Safe Resumption Logic

If an SFTP transfer drops at 14,000 of 20,000 files, the engineer should not be forced to start from scratch. The tooling evaluates the transfer log, compares local vs. remote file lists, and resumes synchronization seamlessly without invalidating existing baseline files.

---

## 19. Observability and Performance Telemetry

Operations that run without feedback erode engineering trust. The PowerShell automation instruments every phase with execution timers:

```
============================================================
CASE WORKSPACE INITIALIZATION TELEMETRY: CASE-01700000
============================================================
[TELEMETRY] Metadata Validation ........... 00:00:02
[TELEMETRY] SFTP Handshake & Auth ......... 00:00:04
[TELEMETRY] Recursive Source Download ..... 00:04:18 (18,421 files, 642 MB)
[TELEMETRY] Immutable Baseline Creation ... 00:00:28
[TELEMETRY] Git Initialization & Commit ... 00:00:14
[TELEMETRY] Agentic Stage Scaffolding ..... 00:00:02
------------------------------------------------------------
TOTAL PIPELINE DURATION: ................. 00:05:08
EFFECTIVE TRANSFER SPEED: ................. 2.48 MB/s (59.7 files/s)
STATUS: READY FOR INVESTIGATION
============================================================
```

Tracking these metrics allows the team to pinpoint operational bottlenecks—such as degraded SFTP throughput, network latency spikes, or excessive filesystem fragmentation.

---

## 20. Performance Engineering for Massive Codebases

For dealer networks whose assets include tens of thousands of customer-uploaded vehicle photos, PDFs, and brochures, downloading the entire website tree for every ticket is inefficient.

### Transfer Profiles: `Full` vs. `SourceOnly`

To optimize transfer times, the tooling introduces transfer profiles:

```powershell
.\Get-LiveSite.ps1 -CaseNumber "CASE-01700000" -ClientName "dealer-acme" -Profile "SourceOnly"
```

A `SourceOnly` transfer applies an exclusion mask during the WinSCP synchronization pass:

```
Exclude: *.jpg, *.jpeg, *.png, *.gif, *.webp, *.pdf, *.mp4, *.zip
Include: *.asp, *.inc, *.html, *.htm, *.css, *.less, *.js, *.json, *.xml
```

*Tradeoff Consideration:* Excluding media assets reduces download time from five minutes to thirty seconds. However, if the reported bug involves an broken background image URL or a distorted picture element, missing assets can impair local visual inspection. The engineer must select the profile matching the nature of the ticket.

### Multithreaded Local Copying with Robocopy

While remote SFTP transfers are constrained by SSH round-trip latency, local baseline copying is constrained by disk I/O. The automation replaces PowerShell's single-threaded `Copy-Item` with multithreaded `robocopy.exe`:

```powershell
robocopy $SourceDir $BaselineDir /E /MT:8 /R:1 /W:1 /NP /NFL /NDL
```

Leveraging 8 concurrent threads reduces local snapshot duplication time by over 70% on standard SSD storage.

---

## 21. Security Model and Sensitive Data Safeguards

Operating on live production websites requires strict adherence to cybersecurity standards:

1. **Credential Isolation:** Credentials are never written to disk, committed to Git, or exposed to LLM context files.
2. **Secret Scrubbing Before AI Analysis:** Production configuration files (`web.config`, connection string includes) may contain database connection strings or third-party API tokens. Before ingesting source code into an LLM context, automated regex sanitizers scrub sensitive key-value pairs:
   ```regex
   (connectionString|password|pwd|secret|apiKey)\s*=\s*["'][^"']+["']
   ```
3. **No Direct Remote Write Access for AI:** LLMs cannot initiate network sockets or invoke remote execution tools. All agentic actions are strictly confined to the local filesystem.

---

## 22. Common Failure Modes and Mitigations

| Failure Mode | Root Cause | Detection Mechanism | Mitigation Strategy |
| :--- | :--- | :--- | :--- |
| **Wrong Environment Downloaded** | Engineer accidentally targeted Live instead of Staging. | Explicit prompt gate; mismatch between ticket URL and `workspace.json`. | Separate scripts (`Get-LiveSite` vs `Get-StageSite`); mandatory typed confirmation phrase. |
| **Silent SFTP Drop** | Network timeout during massive directory traversal. | Exit code verification; file count mismatch against remote directory index. | Script halts on WinSCP transfer error; provides safe resumption command without re-downloading. |
| **Accidental In-Place Live Edit** | Engineer opened remote file directly in SFTP client. | Absence of Git commit history; unmonitored file change on remote cluster. | SFTP credentials restricted; production files accessible locally *only* via isolated case workspace. |
| **Scope Expansion / Creep** | IDE auto-formatter modified whitespace across 40 files. | `git status` and `git diff --name-only` flag unexpected modifications. | Git baseline acts as a deployment blocker; review stage rejects any diff containing untouched files. |
| **AI Suggests Disruptive Global Refactor** | LLM lacks visibility into shared platform dependencies. | Diff review stage flags broad selector changes or global CSS rules. | Stage 05 (Remediation Plan) enforces surgical patches; engineer rejects non-scoped code suggestions. |
| **Classic ASP Include Failure** | Local IIS missing virtual directory mapping for virtual includes. | HTTP 500 error: *Active Server Pages error 'ASP 0126'*. | Map virtual include paths in local IIS site bindings; isolate client-side logic from server-side rendering. |
| **Dynamic Script Re-run Duplication** | Inventory AJAX filter re-executes script on DOM mutation. | Duplicate pricing elements appear on vehicle cards during live testing. | Enforce idempotency guard clauses (`if (document.getElementById(ID)) return;`) in all custom scripts. |

---

## 23. Tradeoffs of the Architecture

No architectural pattern is without costs. Implementing this system introduces deliberate engineering tradeoffs:

- **Local Storage Footprint:** Maintaining a local website snapshot, an immutable baseline, and Git history consumes between 500 MB and 2 GB per case. On a workstation handling dozens of cases weekly, automated case archival and cleanup routines are required.
- **Initialization Latency:** Bootstrapping a case requires two to five minutes of initial transfer and scaffolding time. While longer than opening a file immediately over SFTP, this upfront investment prevents catastrophic outages that cost hours of downtime.
- **Process Discipline Overhead:** Engineers must resist the temptation to bypass stages for "quick fixes." The eight-stage workflow requires structured documentation, which is rewarded with flawless repeatability and auditability.

---

## 24. Future Architecture and Enhancements

The next iterations of this platform will introduce deeper automated validations:

```
Ticket Received (CRM)
       │
       ▼
Automated Case Bootstrap & Remote Manifest Checksum Validation
       │
       ▼
Selective Delta Transfer (SHA-256 Hash Matching)
       │
       ▼
Local IIS Container Provisioning (Automated Virtual Directory Mapping)
       │
       ▼
Playwright Headless Browser Visual Regression Testing
       │
       ▼
Automated Lighthouse SEO / Accessibility (a11y) Verification
       │
       ▼
AI Diff & Compliance Review
       │
       ▼
Human-Gated Production Release & Auto-Generated Case Audit Record
```

Planned capabilities include:
- **Remote Manifest Checksums:** Querying a remote manifest to transfer only files modified since the last known baseline.
- **Automated Visual Regression Diffing:** Running headless Playwright tests locally to capture pixel-by-pixel visual diffs across viewports before and after patch application.
- **Automated Case Closeout Records:** Compiling all stage outputs, Git diffs, and validation screenshots into a single signed PDF report attached directly to the CRM case ticket.

---

## 25. Engineering Lessons Learned

1. **Automation Handles Mechanics; Humans Handle Decisions:** The greatest productivity gains did not come from attempting to automate code generation. They came from automating the tedious, error-prone mechanical tasks: folder scaffolding, SFTP downloading, baseline archiving, and Git repository setup.
2. **Context Isolation Is Essential for AI Utility:** Large language models produce their best engineering output when restricted to narrow, focused tasks with explicit epistemic boundaries. A model asked to *only* trace an include tree or *only* review a Git diff provides exceptional value without hallucination risk.
3. **Traceability Creates Engineering Confidence:** Knowing that every change is captured in a local Git diff, backed by an immutable baseline, allows an engineer to investigate legacy codebases boldly and safely.
4. **Wording Governs Operational Mindset:** Eliminating phrases like *"I fixed production"* and replacing them with *"The local remediation has been validated and is ready for controlled deployment"* enforces operational humility and technical rigor.

---

## 26. Conclusion

Artificial intelligence in software engineering is most transformative not when it is granted autonomous authority to manipulate production environments, but when it is embedded inside a disciplined, human-governed engineering system.

By establishing deterministic PowerShell automation, immutable baselines, local Git tracking, filesystem-based agent orchestration, and strict human deployment boundaries, this architecture elevates production website support from chaotic in-place triage into a repeatable, auditable, and safe software engineering discipline.

---

### Executive Summary

This architecture review presents an isolated, reproducible support engineering workflow designed to troubleshoot production multi-tenant dealership websites. By eliminating reckless in-place SFTP file modifications, the system leverages PowerShell and WinSCP .NET to pull authoritative remote sources into local, version-controlled case workspaces. An untouched, immutable filesystem baseline is preserved alongside a local Git repository before any code is inspected. Using the Interpretable Context Methodology (ICM), troubleshooting is partitioned into eight sequential, filesystem-based stages (Intake through Diff Review). Large language models operate under least-privilege constraints as read-only analytical assistants and diff reviewers, strictly barred from remote access or autonomous deployment. The engineer retains absolute authority over implementation, testing, and production deployment, creating an auditable, safe, and disciplined operational pipeline.

---

### LinkedIn Announcement Post

🚀 **Moving from "Edit In Place & Hope" to Disciplined Production Support Engineering**

In high-volume web support—especially across legacy platforms featuring Classic ASP, nested includes, complex LESS stylesheets, and dynamic inventory systems—the traditional habit of opening a live file over SFTP, modifying it in place, and hitting refresh is an operational hazard waiting to happen.

I designed and implemented an engineering-first troubleshooting workflow that brings software reliability engineering (SRE) rigor to support operations:

🔹 **Deterministic Automation:** Dedicated PowerShell scripts and the WinSCP .NET assembly pull remote code into isolated, case-specific local workspaces with mandatory confirmation gates (`DOWNLOAD-LIVE`).  
🔹 **Immutable Baselines & Git Ledger:** An untouched baseline snapshot is preserved before a local Git repository tracks every single modification, giving instant before-and-after diff visibility.  
🔹 **Filesystem as Agent Architecture:** Using Interpretable Context Methodology (ICM), troubleshooting is structured across 8 discrete engineering stages (from Problem Intake to Root Cause Analysis and Diff Review).  
🔹 **Least-Privilege AI Collaboration:** LLMs act as read-only code investigators and diff reviewers—never as autonomous production operators. AI investigates and recommends; the human engineer verifies, tests, and deploys.

Read the complete systems design review on my portfolio:  
👉 https://www.rodneyachery.com/Articles/ai-assisted-support-engineering-workflow

#SoftwareEngineering #SupportEngineering #SRE #PowerShell #WebPerformance #DevOps #SystemDesign #ClassicASP #TechArchitecture

---

### Alternative Technical Titles

1. *Architecting an Isolated, Git-Backed Troubleshooting Pipeline for Multi-Tenant Legacy Web Platforms*
2. *Filesystem-Based Agent Orchestration and Immutable Baselines in Production Support Engineering*
3. *Engineering Safety into Production Web Triage: A PowerShell, WinSCP, and Git Operational Runbook*
4. *Beyond In-Place SFTP: Designing a Zero-Blast-Radius Troubleshooting Framework for Legacy Systems*
5. *Governed AI in Production Operations: Embedding LLMs as Sandboxed Reviewers in Local Case Workspaces*

---

### Key Takeaways

- **Isolate Before You Inspect:** Never perform troubleshooting or code analysis directly on a live production server. Always establish an isolated, local case workspace.
- **Preserve Immutable Baselines:** Maintain an untouched filesystem copy of the production state prior to initializing tools or version control to ensure absolute forensic traceability.
- **Enforce Local Change Control:** Utilize Git as a local, case-scoped change ledger to maintain visibility over every line modified and immediately flag scope expansion.
- **Partition AI Context via Filesystem Stages:** Prevent LLM hallucinations and cognitive drift by breaking troubleshooting into sequential, scoped stages with explicit deliverables.
- **Enforce Human Authority:** Restrict AI to read-only investigative and review roles. The human engineer must remain the sole operator authorized to modify code and execute production deployments.

---

### Glossary of Architecture Terminology

- **Baseline:** An exact, pristine snapshot of the production codebase captured immediately upon download and preserved without modification as a reference point.
- **Blast Radius:** The scope and extent of potential downstream damage or unintended visual/functional breakage across other pages or shared components resulting from a code change.
- **Idempotency:** A software design property where an operation or script can be executed multiple times without producing different results or unintended side effects beyond the initial application.
- **SFTP (SSH File Transfer Protocol):** A secure network protocol operating over an SSH transport layer used to manage and transfer files between remote servers and local clients.
- **Host Key:** A cryptographic key used by an SSH/SFTP server to prove its identity to connecting clients, preventing man-in-the-middle attacks.
- **Control Plane:** The subsystem and metadata responsible for routing, safety checks, confirmation gates, environment verification, and workflow state management.
- **Data Plane:** The subsystem responsible for handling the actual payload—website assets, markup, scripts, stylesheets, and templates.
- **Local Reproduction:** The ability to simulate, recreate, and observe a reported production defect within a local developer environment.
- **Root Cause Analysis (RCA):** The systematic engineering process of identifying the fundamental, causal flaw responsible for a defect, as opposed to merely observing its visible symptoms.
- **Immutable Snapshot:** A read-only filesystem directory containing the original production files, sealed against accidental modification or deletion.
- **Agentic Workspace:** A structured directory environment where directory layouts, configuration files, and scoped context documents define the execution boundaries of an AI assistant.
- **Context Isolation:** The architectural practice of limiting the information and file scope provided to an LLM to only what is necessary for its immediate task, preventing cognitive saturation and hallucination.
- **Human-in-the-Loop (HITL):** A governance model where automated systems and AI agents cannot proceed through critical state transitions without explicit human review and approval.
- **Diff Review:** The formal inspection of a line-level code comparison (`git diff`) against the intended remediation plan prior to any deployment action.

---

### Why This Is Support Engineering Rather Than Traditional Help Desk Support

Traditional IT help desk support is generally focused on issue logging, user account resets, ticket routing, and surface-level triage based on canned response runbooks. When a technical bug arises, traditional support escalates the ticket to an engineering team and waits.

In contrast, **Support Engineering** operates directly at the intersection of production systems, software engineering, and infrastructure operations. A support engineer:
1. **Performs Deep Source Code Investigation:** Reads, traces, and diagnoses defects across complex multi-tier codebases (Classic ASP, JavaScript, CSS/LESS, SQL).
2. **Understands Production System Dependencies:** Analyzes how reverse proxies, IIS web servers, CDN edge caches, and database connection strings interact with frontend rendering.
3. **Executes Scientific Debugging:** Deconstructs complex user reports into reproducible steps, traces causal chains, and isolates root causes using runtime inspection and DOM profiling.
4. **Applies Software Engineering Discipline:** Leverages version control, automated scripting, defensive programming, and rigorous regression testing rather than applying unversioned manual hotfixes.
5. **Authors Production-Grade Patches:** Designs, tests, and deploys minimal, targeted code fixes that adhere to clean architecture and component isolation principles.
6. **Protects Operational Reliability:** Analyzes blast radius, prevents regressions, and maintains zero-downtime deployment safety across thousands of live client websites.

---

### What Makes This an Agentic Architecture

The system qualifies as an **Agentic Architecture** because it transforms passive large language models into active, task-directed problem solvers governed by structured environmental constraints:

1. **The Filesystem Acts as Workflow State:** Rather than relying on volatile conversational memory or proprietary vector database states, the directory tree itself represents the agent's progress through the state machine.
2. **Context Documents Function as Scoped Instruction Sets:** Files like `03_investigation/CONTEXT.md` define deterministic system prompts, input prerequisites, forbidden operations, and required output schemas for that specific phase.
3. **Artifacts Act as Inter-Stage Messages:** The markdown files generated in each stage's `output/` folder serve as immutable messages passed to subsequent stages, ensuring that later reasoning builds strictly upon verified evidence.
4. **Tool Access Defines Capability Boundaries:** In early stages, the agent is granted strictly read-only file access. Write access is conditionally unlocked only during implementation, and network/remote access is permanently withheld.
5. **Humans Function as Deterministic Approval Gates:** The agent cannot transition between stages or claim completion without satisfying the acceptance criteria enforced by the human engineer.

---

### Engineering Principles I Applied

- **Least Privilege:** Granted AI models and automated processes only the minimal filesystem and network permissions necessary to fulfill their immediate stage duties.
- **Separation of Concerns:** Strictly isolated the control plane (workflow metadata and confirmations) from the data plane (dealership website code).
- **Deterministic Automation:** Used PowerShell and WinSCP for predictable, repeatable mechanical tasks while reserving AI for nuanced, interpretive code analysis.
- **Fail-Fast Behavior:** Designed scripts to halt immediately upon detecting invalid environments, checksum discrepancies, or transfer errors.
- **Immutable State:** Preserved untouched, read-only baseline copies of production code to guarantee recovery and provide an unimpeachable reference point.
- **Reproducibility:** Ensured that any engineer given the case folder can inspect the exact code, baseline, reasoning artifacts, and diff that led to a resolution.
- **Explicit State Transitions:** Structured the troubleshooting lifecycle as an auditable finite state machine with prohibited invalid transitions.
- **Comprehensive Observability:** Instrumented all automation phases with precise timers and metrics to ensure complete operational transparency.
- **Human Governance:** Positioned the human engineer as the sole decision-maker for all code changes, test validations, and production deployments.
- **Controlled Blast Radius:** Restricted all remediations to the smallest possible surgical edits, forbidding speculative refactoring during support hotfixes.
