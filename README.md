<p align="center">
  <img src="https://<YOUR-LOGO-URL>" alt="Booklir Logo" width="200" />
</p>

# Booklir

> **ASP.NET MVC**–based trip & tour booking platform with user authentication, admin dashboard, itinerary management, gallery images, and seamless payments via Stripe.

---

## 📖 Table of Contents

1. [About](#about)  
2. [Features](#features)  
3. [Tech Stack](#tech-stack)  
4. [Demo Video](#demo-video)  
5. [Getting Started](#getting-started)  
   - [Prerequisites](#prerequisites)  
   - [Installation](#installation)  
6. [Configuration](#configuration)  
7. [Usage](#usage)  
8. [Project Structure](#project-structure)  
9. [Contributing](#contributing)  
10. [License](#license)  
11. [Contact](#contact)  

---

## 🧐 About

Booklir is a full‑stack web application that lets travelers browse and book trips, manage itineraries, and make payments. Administrators have a rich dashboard to manage trips, destinations, categories, and view analytics via Chart.js.

---

## ✨ Features

- **User Authentication & Authorization**  
  - ASP.NET Identity with roles (Admin, User)  
  - “BookingOwner” policy ensures users can only modify their own bookings  

- **Trip Management**  
  - Create, read, update, delete (CRUD) for trips, including gallery images & primary image selector  
  - Itinerary editor with dynamic add/remove days  

- **Gallery & Uploads**  
  - Multi‑image upload with live preview (max. 3 images/trip, 2 MB each)  
  - Razor View Components for popular destinations and notifications  

- **Booking Flow**  
  - View detailed trip page with date picker, traveler count, itinerary  
  - Stripe integration for secure payments  

- **Admin Panel**  
  - AJAX‑powered dashboards with SweetAlert, Toastr, Chart.js  
  - Manage users, trips, destinations, categories, bookings in real time  

- **Email Notifications**  
  - SendGrid to send booking confirmations, contact form messages, newsletter subscriptions  

- **Responsive & Modern UI**  
  - Bootstrap 5, custom CSS variables (`:root` theme), smooth scroll & scroll‑to‑top  

---

## 🛠 Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8.0)  
- **ORM:** Entity Framework Core (SQL Server)  
- **Mapping:** AutoMapper  
- **Storage:** Local file system (for uploads)  
- **Payments:** Stripe.net  
- **Email:** SendGrid, RazorLight for templating  
- **UI:** Bootstrap 5, jQuery, SweetAlert2, Toastr, Chart.js  
- **JSON:** Newtonsoft.Json  
- **PDF Reports:** PdfSharpCore  
- **CLI tools:** EF Core Tools, CsvHelper  

---

## 🎥 Demo Video

Watch a quick walkthrough of the main features:

<video width="640" controls>
  <source src="https://onedrive.live.com/download?cid=<YOUR-CID>&resid=<RESID>&authkey=<AUTHKEY>" type="video/mp4">
  Your browser does not support the video tag.
</video>

> Tip: Hover over the gallery, click itinerary days to see details, and try the Stripe checkout form.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)  
- SQL Server (LocalDB or full instance)  
- A [Stripe account](https://stripe.com) for API keys  
- A [SendGrid account](https://sendgrid.com) (optional, for email)  

---

### Installation

1. **Clone the repo**  
   ```bash
   git clone https://github.com/JassimIbrahim-JI/Booklir.git
   cd BooklirSolution
