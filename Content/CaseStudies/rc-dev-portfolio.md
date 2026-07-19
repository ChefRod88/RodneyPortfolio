---
title: "RC DEV PORTFOLIO"
description: "Full-stack portfolio with client portal, invoicing, Stripe payments, and CI/CD pipeline. Built to production standards with rate limiting, EF Core, and ASP.NET Core 9."
author: Rodney Chery
date: 2026-06-01
tags: C#, ASP.NET CORE 9, ENTITY FRAMEWORK CORE, MICROSOFT SQL SERVER, MICROSOFT AZURE, STRIPE API, GITHUB ACTIONS
---

# Architecture & Overview

The RC Dev Portfolio is an enterprise-grade web application built to showcase software engineering capabilities. It features a complete client portal, integrated invoicing via the Stripe API, and robust protection against abuse using ASP.NET Core Rate Limiting.

## Technical Highlights
- **Framework:** ASP.NET Core 9 Razor Pages
- **Database:** Microsoft SQL Server with Entity Framework Core
- **Cloud:** Deployed to Microsoft Azure App Services
- **CI/CD:** Automated builds and deployments via GitHub Actions
- **Payments:** Secure invoicing handled by the Stripe API

## Challenges & Solutions
The primary challenge was ensuring the application remains lightweight while supporting complex backend processes like role-based authentication and secure payments. 
By utilizing Clean Architecture and the latest .NET 9 performance enhancements (like compiled models for EF Core), the system achieves sub-100ms response times globally.
