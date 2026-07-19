---
title: "PRIMEMEDICALDOCTORS.COM"
description: "HIPAA-aware patient booking platform for a self-pay outpatient practice accepting attorney lien (personal injury) cases."
author: Rodney Chery
date: 2026-06-03
tags: C#, ASP.NET CORE 10, TWILIO SENDGRID, MICROSOFT AZURE, HTML5, CSS3, JS
image: /images/project_thumbnails/Screenshot 2026-06-02 071941.webp
---

# Secure Outpatient Booking

Prime Medical Doctors required a robust booking system to handle two distinct patient tracks: Self-Pay and Attorney Lien (Personal Injury). 
The platform was architected to safely capture sensitive patient intake information without risking a HIPAA breach.

## Innovative "Zero-Persistence" PHI Architecture
To drastically reduce the compliance footprint and liability, the application features a **"Zero-Persistence"** PHI model. 
All Protected Health Information (PHI) entered by patients flows through the application purely in-memory via strongly-typed C# records. It is *never* persisted to a backend database server-side.

Instead, the data is instantly compiled and dispatched via secure transactional email channels directly to the clinic's encrypted inbox.

## Key Features
- **Two-Track Wizard:** Dynamic forms that adapt based on whether the patient is self-pay or represented by legal counsel.
- **Transactional Communications:** 9 unique transactional email templates powered by Twilio SendGrid for confirmations, reminders, and cancellations.
- **Internationalization (i18n):** Full support for English and Spanish (EN/ES) switching without page reloads.
- **Infrastructure:** Deployed on Azure App Service (West US 3) with a fully automated GitHub Actions CI/CD pipeline.
