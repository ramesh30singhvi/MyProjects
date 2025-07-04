using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string FullName { get; set; }

        public int FacilityId { get; set; }

        public int? PatientId { get; set; }

        public int? FacId { get; set; }

        public DateTime? AdmissionDate { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Gender { get; set; }

        public string PatientStatus { get; set; }

        public string MedicaidNumber { get; set; }

        public string MedicalRecordNumber { get; set; }

        public string MedicareBeneficiaryIdentifier { get; set; }

        public string MedicareNumber { get; set; }
        
        public int? RoomID { get; set; }

        public string RoomDesc { get; set; }

        public int? OmpId { get; set; }

        public int? UnitId { get; set; }

        public string UnitDesc { get; set; }

        public DateTime? DischargeDate { get; set; }

        public DateTime? DeathDateTime { get; set; }

        public string EthnicityDesc { get; set; }

        public IReadOnlyCollection<ProgressNote> ProgressNotes { get; set; }
    }
}
