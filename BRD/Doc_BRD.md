# Business Requirements Document

## Product Goal

Build a simple, web-based Patient Management Application for a general physician to efficiently manage daily clinical activities such as appointment scheduling, patient records, complaints, diagnosis, and medication.

The goal is to reduce manual paperwork, improve consultation efficiency, and maintain accurate, easily accessible patient history.

---

## Users and Stakeholders

**Primary Users:**
- General Physician (Single User)

**Secondary Users:**
- None (Receptionist access not included in Phase 1)

**Stakeholders:**
- Clinic Owner (Doctor)
- Product Owner
- Development Team

---

## Problem Statement

General physicians in small clinics often rely on paper-based systems or fragmented tools to manage patient data and appointments. This results in:
- Slow patient lookup and history tracking  
- Risk of lost or incomplete records  
- Inefficient consultation workflow  
- Lack of structured medical records  

A lightweight, web-based solution is needed to streamline and digitize daily operations.

---

## Scope

The first version of the application will include:

- Web-based access (browser-based system)  
- Patient registration and profile management  
- Appointment scheduling and tracking  
- Recording patient complaints (symptoms)  
- Diagnosis documentation  
- Medication and prescription management  
- Printable prescriptions (with basic header, footer, and content)  
- Mandatory vitals capture (temperature, BP, pulse)  
- Patient visit history tracking  
- Basic search functionality  
- Data export (CSV/PDF)  

---

## Out of Scope

The following will NOT be included in the initial release:

- Receptionist or multi-user access  
- Billing and invoicing  
- Insurance processing  
- Integration with labs or pharmacies  
- AI-based diagnosis or recommendations  
- Offline functionality  
- Mobile application  
- Advanced analytics and reporting  
- Multi-doctor or multi-clinic support  
- Follow-up alerts/reminders  

---

## Success Criteria

- **SC-001** Doctor can complete a consultation record within 2–3 minutes  
- **SC-002** Patient search and history retrieval within 2–5 seconds  
- **SC-003** At least 80% reduction in paper usage  
- **SC-004** Smooth generation and printing of prescriptions  
- **SC-005** Successful export of data in CSV/PDF format  
- **SC-006** High usability with minimal training required  

---

## Functional Requirements

### Patient Management
- **FR-PAT-001** Add patient details  
- **FR-PAT-002** Edit patient details  
- **FR-PAT-003** View patient details  
- **FR-PAT-004** Capture:
  - Name  
  - Age / DOB  
  - Gender  
  - Contact details  
- **FR-PAT-005** Search patients by name or phone number  

---

### Appointment Management
- **FR-APT-001** Schedule appointments  
- **FR-APT-002** View daily appointment list  
- **FR-APT-003** Update appointment status:
  - Scheduled  
  - Completed  
  - Cancelled  
  - No-show  

---

### Consultation Workflow

#### Vitals Capture (Mandatory)
- **FR-VIT-001** Record for every consultation:
  - Temperature  
  - Blood Pressure  
  - Pulse  

---

#### Complaints
- **FR-CMP-001** Enter patient symptoms (free text)

---

#### Diagnosis
- **FR-DIA-001** Record diagnosis notes  

---

#### Medication / Prescription
- **FR-RX-001** Add medicines with:
  - Name  
  - Dosage  
  - Frequency  
  - Duration  
  - Instructions  

- **FR-RX-002** Generate **printable prescription** including:
  - Clinic/doctor header  
  - Patient details  
  - Vitals  
  - Diagnosis  
  - Medications  
  - Footer (basic notes/signature area)  

---

### Patient History
- **FR-HIS-001** View previous visits  
- **FR-HIS-002** Access:
  - Vitals  
  - Complaints  
  - Diagnosis  
  - Prescriptions  
- **FR-HIS-003** Filter by date  

---

### Search & Navigation
- **FR-SRC-001** Quick patient search  
- **FR-SRC-002** View recent patients  
- **FR-SRC-003** Easy navigation between patient profile and visits  

---

### Data Export
- **FR-EXP-001** Export patient or visit data as CSV  
- **FR-EXP-002** Export patient or visit data as PDF  

---

## Non-Functional Requirements

- **Usability:**
  - **NFR-USB-001** Simple, minimal UI optimized for fast data entry during consultations  

- **Performance:**
  - **NFR-PRF-001** Page load time < 2 seconds  
  - **NFR-PRF-002** Fast patient search and retrieval  

- **Reliability:**
  - **NFR-REL-001** No data loss  
  - **NFR-REL-002** Regular automated backups  

- **Security:**
  - **NFR-SEC-001** Secure login (single user authentication)  
  - **NFR-SEC-002** Data encryption (at rest and in transit)  

- **Scalability:**
  - **NFR-SCL-001** Designed for a single clinic with moderate patient volume  

- **Compatibility:**
  - **NFR-CMP-001** Works on modern web browsers (Chrome, Edge, Safari)  

---

## Open Questions

- None (all major product decisions defined for Phase 1)
