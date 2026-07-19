---
title: "Modernizing a Legacy Healthcare Portal"
description: "How we migrated a 10-year-old monolithic EHR interface to a modern, decoupled React and ASP.NET Core architecture, improving page load speeds by 80%."
author: Rodney Chery
date: 2026-05-12
tags: ASP.NET Core, React, Healthcare, EHR Integration, Cloud Architecture
---

# The Challenge

Our client, a mid-sized regional healthcare provider, was struggling with a legacy monolithic web portal. Built nearly a decade ago, the system was heavily tightly-coupled, making it nearly impossible to introduce new features. 

The most critical issues were:
1. **Performance:** Page load times regularly exceeded 5 seconds.
2. **Security:** The underlying framework was approaching End-of-Life (EOL), posing serious HIPAA compliance risks.
3. **Developer Velocity:** Deployments required massive downtime windows and manual database patching.

# The Solution

We executed a phased migration using the **Strangler Fig Pattern**. Instead of a risky "big bang" rewrite, we slowly replaced legacy components with a modern, decoupled architecture.

## Architecture & Tech Stack
- **Backend:** C# ASP.NET Core Web API (Minimal APIs)
- **Frontend:** React with TypeScript and Vite
- **Database:** PostgreSQL on AWS RDS
- **Infrastructure:** Containerized via Docker, deployed to AWS ECS via GitHub Actions.

## Phase 1: API Gateway & Authentication
We first placed an API Gateway in front of the legacy system. We migrated authentication to **Microsoft Entra ID** (formerly Azure AD), giving providers secure Single Sign-On (SSO) and Multi-Factor Authentication (MFA). 

## Phase 2: Decoupling the Frontend
We built a new React frontend for the most critical workflow: Patient Intake. The API Gateway routed new requests to our ASP.NET Core microservice, while legacy pages fell back to the old monolith. 

## Phase 3: Data Migration & EOL
Over 8 months, we migrated the remaining modules—Scheduling, Billing, and Charting—into independent services. 

# The Results

By the end of the project, we entirely decommissioned the legacy monolith.

- **80% Reduction in Load Times:** Moving to a modern SPA dropped average load times from 5s to under 1s.
- **Zero-Downtime Deployments:** Containerized CI/CD pipelines meant features could ship during business hours without interrupting clinical workflows.
- **Full Compliance:** The new system passed all third-party security audits and HIPAA assessments with zero critical findings.
