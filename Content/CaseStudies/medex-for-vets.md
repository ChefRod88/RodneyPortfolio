---
title: "MEDEX FOR VETS PORTAL"
description: "A HIPAA-compliant patient and provider portal designed to allow veterans to prepare for their Independent Medical Exams (IME) and authorize MedEx staff to audit records."
author: Rodney Chery
date: 2026-06-05
tags: C#, .NET 10, ASP.NET CORE BLAZOR, MICROSOFT AZURE BLOB STORAGE, HIPAA COMPLIANCE, DOCUSIGN API, CALENDLY API
image: /images/project_thumbnails/Black and White Minimalist Aesthetic Photo Collage Fashion Collection Instagram Post.webp
---

# Streamlining Veteran Care Logistics

MedEx for Vets provides Independent Medical Exams (IME) for veterans seeking disability claims. They needed a secure, dual-sided portal that allowed veterans to easily upload their medical histories while allowing internal clinical staff to audit the records securely.

## Dual-Portal Architecture
The solution involved building two distinct interfaces over a shared, highly secure database context:
1. **The Claimant Portal:** An intuitive, mobile-first interface designed for veterans. It integrates with the **DocuSign eSignature API** so veterans can legally authorize MedEx to review their records without printing physical forms. It also utilizes the **Calendly API V2** to let veterans self-schedule their exams based on provider availability.
2. **The Provider/Admin Ops Portal:** A dense, data-rich Blazor Web App where clinical staff can review uploaded documents, assign cases to specific doctors, and track the status of exams.

## Secure Document Storage
Because the application handles highly sensitive military and medical records, all document uploads are streamed directly to **Azure Blob Storage** rather than being held in server memory. 
Access to these documents is controlled via short-lived Shared Access Signatures (SAS) generated on-the-fly by the backend, ensuring files are never permanently exposed to the public internet.

## Tech Stack
- **Framework:** .NET 10 Blazor Web App
- **Database:** Azure SQL Database
- **Storage:** Azure Blob Storage
- **Integrations:** DocuSign API, Calendly API V2, Firely .NET SDK (for FHIR mapping).
