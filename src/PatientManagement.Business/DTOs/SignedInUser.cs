namespace PatientManagement.Business.DTOs;

// The account details the web layer needs to sign a user in. No password or hash is included.
public sealed record SignedInUser(int Id, string Username);
