---
title: "PrimeCare EMR"
description: "A custom, HIPAA and HITECH-compliant Electronic Medical Record system engineered to strict FHIR standards."
author: Rodney Chery
date: 2026-06-04
tags: C#, ASP.NET CORE BLAZOR, MICROSOFT AZURE, HL7 FHIR, FIRELY .NET SDK
image: /images/project_thumbnails/Screenshot 2026-06-01 181913.webp
---

# Redefining the EMR Experience

PrimeCare EMR is a custom SaaS platform built for **Prime Medical Group**. Traditional EMRs are notorious for being cluttered, slow, and mentally exhausting for medical providers to use. 
This project set out to solve the "cognitive load" problem in clinical software by providing a streamlined, lightning-fast interface.

## Interoperability at the Core
Built from the ground up using the **Firely .NET SDK**, the backend communicates natively using the **HL7 FHIR** standard. This ensures the system can seamlessly interoperate with external hospital networks, labs, and pharmacies without writing fragile custom translation layers.

## Innovative Features
- **Intelligent Omnibox:** Similar to a modern web search engine, the omnibox allows providers to instantly pull up patients, lab results, or billing codes using natural language fragments, entirely bypassing complex navigation menus.
- **Dynamic Drill-Down Calendar:** A scheduling interface built in Blazor that allows staff to view the clinic at a macro level and drill down into 15-minute appointment slots instantly.
- **Decluttered Clinical Dashboard:** A UI that aggressively hides non-essential information, ensuring that the patient's critical data is front-and-center during the encounter.

## Tech Stack
- **Frontend/Backend:** Blazor Web App (Interactive Server / WebAssembly Hybrid)
- **Cloud Infrastructure:** Microsoft Azure (App Service, Azure SQL with Transparent Data Encryption)
- **Compliance:** Full HIPAA and HITECH compliance, complete with immutable audit logs and Role-Based Access Control (RBAC).
